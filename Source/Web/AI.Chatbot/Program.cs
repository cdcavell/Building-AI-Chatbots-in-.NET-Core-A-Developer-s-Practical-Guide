using AI.Chatbot.Extensions;
using Serilog;
using System.Diagnostics;

var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddUserSecrets(typeof(Program).Assembly, optional: true)
    .Build();

Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateBootstrapLogger();

Log.Information("Starting host");

try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Information("Adding services to the container");
    var app = builder.ConfigureServices();

    Log.Information("Configuring the HTTP request pipeline");
    app.ConfigurePipeline();

    Log.Information("Starting application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception: {Message}", ex.Message);
    throw; // Re-throw the exception to ensure it is not swallowed
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

