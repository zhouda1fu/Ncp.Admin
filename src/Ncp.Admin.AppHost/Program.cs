using Aspire.Hosting.JavaScript;

var builder = DistributedApplication.CreateBuilder(args);

//Enable Docker publisher
// builder.AddDockerComposeEnvironment("docker-env")
//     .WithDashboard(dashboard =>
//     {
//         dashboard.WithHostPort(8080)
//             .WithForwardedHeaders(enabled: true);
//     });

// Add Redis infrastructure
var redis = builder.AddRedis("Redis").WithRedisInsight();

var databasePassword = builder.AddParameter("database-password", value:"1234@Dev", secret: true);
// Add MySQL database infrastructure
var mysql = builder.AddMySql("Database", password: databasePassword)
    // Configure the container to store data in a volume so that it persists across instances.
    //.WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    //.WithLifetime(ContainerLifetime.Persistent)
    .WithPhpMyAdmin();

var mysqlDb = mysql.AddDatabase("MySql", "dev");

// Add RabbitMQ message queue infrastructure
var rabbitmqPassword = builder.AddParameter("rabbitmq-password", value: "guest", secret: true);
var rabbitmq = builder.AddRabbitMQ("rabbitmq", password: rabbitmqPassword)
    .WithManagementPlugin();

// Add web project with infrastructure dependencies
var web = builder.AddProject<Projects.Ncp_Admin_Web>("web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(mysqlDb)
    .WaitFor(mysqlDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    ;

var frontend = builder.AddJavaScriptApp("frontend", "../frontend")
    .WithPnpm()
    .WithExternalHttpEndpoints()
    .WithReference(web)
    .WaitFor(web);
frontend.WithDeveloperCertificateTrust(true);
frontend.WithEnvironment("VITE_API_BASE", web.GetEndpoint("http"));
//frontend.WithEnvironment("VITE_GLOB_API_URL", "http://localhost:5511");
frontend.WithExternalHttpEndpoints();

await builder.Build().RunAsync();
