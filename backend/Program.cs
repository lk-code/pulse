using Microsoft.EntityFrameworkCore;
using Pulse.Api.Data;
using Pulse.Api.Endpoints;
using Pulse.Api.Services;
using Serilog;

var dataPath = Environment.GetEnvironmentVariable("DATA_PATH") ?? "/data";
var logsPath = Path.Combine(dataPath, "logs");

Directory.CreateDirectory(dataPath);
Directory.CreateDirectory(logsPath);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(logsPath, "pulse-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Pulse.Api");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var dbPath = Path.Combine(dataPath, "pulse.db");

    builder.Services.AddDbContext<PulseDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));

    builder.Services.AddSingleton<ScanStateManager>();
    builder.Services.AddSingleton<LibraryScannerService>();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PulseDbContext>();
        db.Database.EnsureCreated();
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0}ms";
    });

    app.UseCors();

    app.MapLibraryEndpoints();
    app.MapTrackEndpoints();
    app.MapPlaylistEndpoints();
    app.MapProgressEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Pulse.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
