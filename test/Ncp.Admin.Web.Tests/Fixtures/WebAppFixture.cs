using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.Hosting;
using System; // 添加 System 命名空间
using System.Linq; // 添加 System.Linq 命名空间


namespace Ncp.Admin.Web.Tests.Fixtures;

public class WebAppFixture : AppFixture<Program>
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);
    private IDistributedApplicationTestingBuilder? _appHost;

    private DistributedApplication? _app;

    protected override async ValueTask PreSetupAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Ncp_Admin_TestAppHost>(cancellationToken);
        // Add Redis infrastructure
        var redis = builder.AddRedis("Redis");

        var databasePassword = builder.AddParameter("database-password", value: "123456@Abc", secret: true);
        // Add MySQL database infrastructure
        var mysql = builder.AddMySql("Database", password: databasePassword);

        var database =mysql.AddDatabase("MySql", "test");

        // Add RabbitMQ message queue infrastructure
        var rabbitmqPassword = builder.AddParameter("rabbitmq-password", value: "guest", secret: true);
        var rabbitmq = builder.AddRabbitMQ("rabbitmq", password: rabbitmqPassword);
        
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        _appHost = builder;
        _app = await builder.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(database.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(rabbitmq.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(redis.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        if (_app == null)
        {
            throw new InvalidOperationException("Distributed application not initialized");
        }

        // Get connection strings from Aspire resources
        SetConnectionString(a, "Redis", "ConnectionStrings:Redis");

        SetConnectionString(a, "MySql", "ConnectionStrings:MySql");

        SetConnectionString(a, "rabbitmq", "ConnectionStrings:rabbitmq");

        a.UseEnvironment("Development");
    }

    private void SetConnectionString(IWebHostBuilder builder, string resourceName, string configKey, params string[] alternativeNames)
    {
        var resource =  (IResourceWithConnectionString)_appHost!.Resources.First(r => r.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase) ||
                                alternativeNames.Any(n => r.Name.Equals(n, StringComparison.OrdinalIgnoreCase)));
        if (resource != null)
        {
            // Note: Using GetAwaiter().GetResult() is necessary here because ConfigureApp is synchronous
            // This is safe during test fixture initialization
            var connectionString = resource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.UseSetting(configKey, connectionString);
            }
        }
    }

    protected override async ValueTask TearDownAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
        await base.TearDownAsync();
    }
}