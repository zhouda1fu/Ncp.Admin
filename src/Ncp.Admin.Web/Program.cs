using FastEndpoints;
using FastEndpoints.Swagger;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.BackgroundJobs;
using Ncp.Admin.Web.Application.Services.Notification;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.BusinessAdapters;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using Ncp.Admin.Web.Clients;
using Ncp.Admin.Web.Middleware;
using Ncp.Admin.Web.Services;
using Ncp.Admin.Web.Services.SystemLogs;
using Ncp.Admin.Web.Utils;
using NetCorePal.Extensions.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prometheus;
using Refit;
using Serilog;
using Serilog.Formatting.Json;
using StackExchange.Redis;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    // SignalR（浏览器 WebSocket）会把 JWT 放在查询参数 access_token 中；默认 MaxRequestLineSize=8192 易触发 414，
    // 进而表现为控制台「CORS / 连接失败」（错误响应常不带 Access-Control-Allow-Origin）。
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestLineSize = 65536;
    });

    builder.AddServiceDefaults();

    #region SignalR

    builder.Services.AddHealthChecks();
    builder.Services.AddMvc()
        .AddNewtonsoftJson(options => { options.SerializerSettings.AddNetCorePalJsonConverters(); });
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, Ncp.Admin.Web.Application.Hubs.NameUserIdProvider>();
    builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IHubFilter, Ncp.Admin.Web.Application.Hubs.UserSessionHubFilter>();

    #endregion

    #region Prometheus监控

    builder.Services.AddHealthChecks().ForwardToPrometheus();
    builder.Services.AddHttpClient(Options.DefaultName)
        .UseHttpClientMetrics();

    #endregion

    #region 身份认证

    builder.AddRedisClient("Redis");

    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis("DataProtection-Keys");

    builder.Services.AddMemoryCache();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();
    builder.Services.Configure<WeChatOfficialAccountOptions>(
        builder.Configuration.GetSection(WeChatOfficialAccountOptions.SectionName));
    builder.Services.Configure<SystemLogOptions>(
        builder.Configuration.GetSection(SystemLogOptions.SectionName));

    builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
    builder.Services.AddSingleton<IRefreshTokenGenerator, DefaultRefreshTokenGenerator>();
    builder.Services.AddSingleton<IUserSessionService, UserSessionService>();

    builder.Services.Configure<AppConfiguration>(builder.Configuration.GetSection("AppConfiguration"));
    var appConfig = builder.Configuration.GetSection("AppConfiguration").Get<AppConfiguration>() ?? new AppConfiguration { JwtIssuer = "netcorepal", JwtAudience = "netcorepal" };

    builder.Services.AddAuthentication().AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidAudience = appConfig.JwtAudience;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidIssuer = appConfig.JwtIssuer;
        options.TokenValidationParameters.ValidateIssuer = true;
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notification"))
                {
                    ctx.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                var userIdString = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!long.TryParse(userIdString, out var userIdValue))
                {
                    ctx.Fail("Invalid user identity.");
                    return;
                }

                var dbContext = ctx.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var userAvailable = await dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(u =>
                        u.Id == new UserId(userIdValue)
                        && !u.IsResigned
                        && u.Status == 1
                        && u.IsActive,
                        ctx.HttpContext.RequestAborted);
                if (!userAvailable)
                {
                    ctx.Fail("Current user is disabled, resigned, or does not exist.");
                    return;
                }

                var sessionId = ctx.Principal?.FindFirstValue(UserSessionClaimTypes.SessionId);
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    ctx.HttpContext.Items[UserSessionAuthenticationReasons.HeaderName] =
                        UserSessionAuthenticationReasons.SessionReplaced;
                    ctx.Fail("Current session is no longer valid.");
                    return;
                }

                try
                {
                    var sessionService = ctx.HttpContext.RequestServices.GetRequiredService<IUserSessionService>();
                    if (!await sessionService.IsCurrentAsync(userIdValue, sessionId))
                    {
                        ctx.HttpContext.Items[UserSessionAuthenticationReasons.HeaderName] =
                            UserSessionAuthenticationReasons.SessionReplaced;
                        ctx.Fail("Current session is no longer valid.");
                    }
                }
                catch (RedisException)
                {
                    ctx.HttpContext.Items[UserSessionAuthenticationReasons.HeaderName] =
                        UserSessionAuthenticationReasons.SessionStoreUnavailable;
                    ctx.Fail("Session store is unavailable.");
                }
            },
            OnChallenge = async ctx =>
            {
                if (ctx.HttpContext.Items.TryGetValue(
                        UserSessionAuthenticationReasons.HeaderName,
                        out var reasonValue)
                    && reasonValue is string reason)
                {
                    ctx.HandleResponse();
                    ctx.Response.Headers[UserSessionAuthenticationReasons.HeaderName] = reason;
                    ctx.Response.StatusCode = reason == UserSessionAuthenticationReasons.SessionStoreUnavailable
                        ? StatusCodes.Status503ServiceUnavailable
                        : StatusCodes.Status401Unauthorized;
                    await ctx.Response.CompleteAsync();
                }
            }
        };
    });
    builder.Services.AddNetCorePalJwt().AddRedisStore();

    #endregion

    #region CORS

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:5666", "http://localhost:5173", "http://localhost:3000" };

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders(UserSessionAuthenticationReasons.HeaderName)
                  .AllowCredentials();
        });
    });

    #endregion

    #region Controller

    builder.Services.AddControllers().AddNetCorePalSystemTextJson();

    #endregion

    #region FastEndpoints

    builder.Services
        .AddFastEndpoints(o => o.IncludeAbstractValidators = true)
        .AddIdempotency();

    builder.Services.SwaggerDocument(settings =>
    {
        settings.DocumentSettings = s =>
        {
            s.Title = "Ncp.AdminAPI接口文档";
            s.Version = "v1";
            s.Description = "Ncp.AdminAPI接口文档";
            s.UseControllerSummaryAsTagDescription = true;
        };
        settings.EnableJWTBearerAuth = true;
    });

    builder.Services.Configure<JsonOptions>(o =>
    {
        o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.SerializerOptions.AddNetCorePalJsonConverters();
    });

    #endregion

    #region 模型验证器

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    builder.Services.AddKnownExceptionErrorModelInterceptor();
    builder.Services.AddTransient<
        IPipelineBehavior<Ncp.Admin.Web.Application.Commands.Workflows.StartWorkflowCommand, WorkflowInstanceId>,
        Ncp.Admin.Web.Application.Commands.Workflows.StartWorkflowDuplicateBusinessKeyBehavior>();

    #endregion

    #region Query

    builder.Services.AddQueries(Assembly.GetExecutingAssembly());

    builder.Services.AddScoped<IWorkflowAssigneeResolver>(sp => sp.GetRequiredService<WorkflowAssigneeResolverQuery>());
    builder.Services.AddScoped<WorkflowOutgoingTaskService>();
    builder.Services.AddScoped<WorkflowRuntimeRecordService>();
    builder.Services.AddScoped<IWorkflowVisibilityService, WorkflowVisibilityService>();
    builder.Services.AddScoped<WorkflowTaskOperationAuthorizer>();
    builder.Services.AddScoped<WorkflowTaskVisibilityPolicy>();
    builder.Services.AddScoped<IWorkflowTaskVisibilityPolicy>(sp => sp.GetRequiredService<WorkflowTaskVisibilityPolicy>());
    builder.Services.AddScoped<WorkflowApprovalAssignmentService>();
    builder.Services.AddScoped<IWorkflowApprovalAssignmentService>(sp => sp.GetRequiredService<WorkflowApprovalAssignmentService>());
    builder.Services.AddScoped<WorkflowDefinitionAssigneeConfigValidator>();
    builder.Services.AddScoped<WorkflowDefinitionIdentityCatalogBuilder>();
    builder.Services.AddScoped<WorkflowDefinitionIdentityRemapper>();
    builder.Services.AddScoped<WorkflowDefinitionExportService>();
    builder.Services.AddScoped<WorkflowDefinitionCacheInvalidator>();
    builder.Services.AddScoped<WorkflowGraphCompiler>();
    builder.Services.AddScoped<WorkflowGraphRuntimeService>();
    builder.Services.AddScoped<WorkflowBusinessAdapterDispatcher>();
    builder.Services.AddScoped<WorkflowConditionFieldsProvider>();
    builder.Services.AddScoped<WorkflowStartAssigneeGate>();
    builder.Services.AddScoped<IWorkflowBusinessAdapter, CreateUserWorkflowBusinessAdapter>();
    builder.Services.AddScoped<RecurringJobManagementService>();

    builder.Services.AddHttpClient(WeChatOfficialAccountClient.HttpClientName, client =>
    {
        client.BaseAddress = new Uri("https://api.weixin.qq.com/");
        client.Timeout = TimeSpan.FromSeconds(20);
    });

    #endregion

    #region 基础设施

    builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);
    builder.Services.AddCustomEntityRepositories(typeof(ApplicationDbContext).Assembly);
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"), npgsql =>
        {
            npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.ConfigureWarnings(w =>
                w.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
        }
    });

    var fileStorageProvider = builder.Configuration.GetValue<string>("FileStorage:Provider") ?? "Local";
    if (string.Equals(fileStorageProvider, "MinIO", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.Configure<MinioFileStorageOptions>(builder.Configuration.GetSection(MinioFileStorageOptions.SectionName));
        builder.Services.AddScoped<MinioFileStorageService>();
        builder.Services.AddScoped<IFileStorageService>(sp =>
            new Ncp.Admin.Web.Application.Services.Files.LegacyFilesFileStorageService(
                sp.GetRequiredService<MinioFileStorageService>(),
                sp.GetRequiredService<IWebHostEnvironment>()));
    }
    else
    {
        builder.Services.Configure<LocalFileStorageOptions>(builder.Configuration.GetSection(LocalFileStorageOptions.SectionName));
        builder.Services.AddScoped<LocalFileStorageService>();
        builder.Services.AddScoped<IFileStorageService>(sp =>
            new Ncp.Admin.Web.Application.Services.Files.LegacyFilesFileStorageService(
                sp.GetRequiredService<LocalFileStorageService>(),
                sp.GetRequiredService<IWebHostEnvironment>()));
    }

    builder.Services.AddScoped<IWeChatAccessTokenProvider, WeChatAccessTokenProvider>();
    builder.Services.AddScoped<IWeChatOfficialAccountClient, WeChatOfficialAccountClient>();
    builder.Services.AddScoped<IWeChatBindingService, WeChatBindingService>();
    builder.Services.AddScoped<NotificationNavigationResolver>();
    builder.Services.AddScoped<INotificationLinkResolver, NotificationLinkResolver>();
    builder.Services.AddScoped<INotificationPushBuffer, NotificationPushBuffer>();
    builder.Services.AddScoped<INotificationChannel, SignalRNotificationSender>();
    builder.Services.AddScoped<INotificationChannel, WeChatNotificationSender>();
    builder.Services.AddScoped<CompositeNotificationSender>();
    builder.Services.AddScoped<INotificationSender, DeferredNotificationSender>();

    builder.Services.AddUnitOfWork<ApplicationDbContext>();
    builder.Services.AddSingleton<OperationLogChannel>();
    builder.Services.AddHostedService<OperationLogBackgroundService>();
    builder.Services.AddSingleton<SystemLogChannel>();
    builder.Services.AddSingleton<SystemLogDatabase>();
    builder.Services.AddSingleton<ILoggerProvider, SystemLogLoggerProvider>();
    builder.Services.AddHostedService<SystemLogBackgroundService>();

    builder.Services.AddRedisLocks();
    builder.Services.AddContext().AddEnvContext().AddDataPermissionContext().AddCapContextProcessor();
    builder.Services.AddNetCorePalServiceDiscoveryClient();
    builder.Services.AddIntegrationEvents(typeof(Program))
        .UseCap<ApplicationDbContext>(b =>
        {
            b.RegisterServicesFromAssemblies(typeof(Program));
            b.AddContextIntegrationFilters();
        });

    builder.Services.AddCap(x =>
    {
        x.UseNetCorePalStorage<ApplicationDbContext>();
        x.JsonSerializerOptions.AddNetCorePalJsonConverters();
        x.ConsumerThreadCount = Environment.ProcessorCount;
        x.UseRabbitMQ(p =>
        {
            var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
            if (!string.IsNullOrEmpty(connectionString))
            {
                var uri = new Uri(connectionString);
                p.HostName = uri.Host;
                p.Port = uri.Port;
                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var userInfo = uri.UserInfo.Split(':');
                    p.UserName = userInfo[0];
                    if (userInfo.Length > 1)
                    {
                        p.Password = userInfo[1];
                    }
                }
                if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
                {
                    p.VirtualHost = uri.AbsolutePath.TrimStart('/');
                }
            }
            else
            {
                builder.Configuration.GetSection("RabbitMQ").Bind(p);
            }
        });
        x.UseDashboard();
    });

    #endregion

    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
            .AddCommandLockBehavior()
            .AddKnownExceptionValidationBehavior()
            .AddOpenBehavior(typeof(NotificationPushAfterUnitOfWorkBehavior<,>))
            .AddUnitOfWorkBehaviors());

    #region 多环境支持与服务注册发现

    builder.Services.AddMultiEnv(envOption => envOption.ServiceName = "Abc.Template")
        .UseMicrosoftServiceDiscovery();
    builder.Services.AddConfigurationServiceEndpointProvider();

    #endregion

    #region 远程服务客户端配置

    var jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
    jsonSerializerSettings.AddNetCorePalJsonConverters();
    var ser = new NewtonsoftJsonContentSerializer(jsonSerializerSettings);
    var settings = new RefitSettings(ser);
    builder.Services.AddRefitClient<IUserServiceClient>(settings)
        .ConfigureHttpClient(client =>
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("https+http://user:8080")!))
        .AddMultiEnvMicrosoftServiceDiscovery()
        .AddStandardResilienceHandler();

    #endregion

    #region Jobs

    builder.Services.AddHangfire(x => { x.UseRedisStorage(builder.Configuration.GetConnectionString("Redis")); });
    builder.Services.AddHangfireServer();

    #endregion

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var migrateLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Database.Migrate");
        await dbContext.Database.MigrateAsync();
    }

    using (var seedScope = app.Services.CreateScope())
    {
        var dbContext = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seedLogger = seedScope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Database.Seed");
        var passwordHasher = seedScope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformAdminSeeder.EnsureSeededAsync(dbContext, passwordHasher, seedLogger, CancellationToken.None);
    }

    app.UseKnownExceptionHandler();

    app.UseStaticFiles();
    app.UseCors();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseContext();
    app.UseMiddleware<DataPermissionContextMiddleware>();

    #region Scalar

    app.UseOutputCache();
    app.UseFastEndpoints(c =>
    {
        c.Endpoints.Configurator = ep =>
        {
            ep.PreProcessor<Ncp.Admin.Web.Processors.OperationLogGlobalPreProcessor>(FastEndpoints.Order.Before);
            ep.PostProcessor<Ncp.Admin.Web.Processors.OperationLogGlobalPostProcessor>(FastEndpoints.Order.After);
        };
    });
    app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");
    app.MapScalarApiReference("scalar", options =>
    {
        options.WithOpenApiRoutePattern("/openapi/v1.json");
    });

    #endregion

    #region SignalR

    app.MapHub<Ncp.Admin.Web.Application.Hubs.NotificationHub>("/notification");

    #endregion

    app.UseHttpMetrics();
    app.MapMetrics();
    app.MapDefaultEndpoints();

    app.MapGet("/code-analysis", () =>
    {
        var html = VisualizationHtmlBuilder.GenerateVisualizationHtml(
            CodeFlowAnalysisHelper.GetResultFromAssemblies(typeof(Program).Assembly,
                typeof(ApplicationDbContext).Assembly,
                typeof(Ncp.Admin.Domain.AggregatesModel.UserAggregate.User).Assembly)
        );
        return Results.Content(html, "text/html; charset=utf-8");
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard();
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用程序意外终止。");
}
finally
{
    await Log.CloseAndFlushAsync();
}

#pragma warning disable S1118
public partial class Program
#pragma warning restore S1118
{
}
