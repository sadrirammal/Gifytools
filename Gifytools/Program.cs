using Gifytools.Bll;
using Gifytools.Database;
using Gifytools.Diagnostics;
using Gifytools.Settings;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Plantup.Hangfire.Hangfire;
using Plantup.Swagger.Swagger;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin() 
                .AllowAnyHeader() 
                .AllowAnyMethod(); 
        });
});

//Add Systemd to configure logging in Systemd format and also do some other nice stuff to integrate is as a service
builder.Host.UseSystemd();

//add some logging to get away from Console.WriteLine()
builder.Logging.AddConsole();

//also add our logs to our OTEL endpoint
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

//Add OpenTelemetry to have some nice stats from our apps to view in something like the Aspire dashboard
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Gifytools", serviceVersion: "1.0.0")) //set the service name of our app
    .WithLogging(logging => 
        logging
            .AddConsoleExporter()
            .AddOtlpExporter())
    .WithTracing(tracing => 
        tracing
            .AddAspNetCoreInstrumentation() //collect default asp.net core traces
            .AddHangfireInstrumentation() //collect hangfire traces
            .AddNpgsql() //collect npgsql traces
            .AddSource("Gifytools") //collect our own traces
            .AddConsoleExporter() //export them to console (for local usage)
            .AddOtlpExporter()) //and export them using the OTLP format - you could use this to view the logs and metrics with the Aspire dashboard (see https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-otlp-example)
    .WithMetrics(metrics => 
         metrics
            .AddAspNetCoreInstrumentation() //add default asp.net core meters
            .AddNpgsqlInstrumentation() //add npgsql meters
            .AddMeter("Gifytools")//add our meters
            .AddConsoleExporter()
            .AddOtlpExporter()); 

builder.Services.AddSingleton<GifyMetrics>();

//Run pipeline
var appDbConnectionString = builder.Configuration.GetConnectionString("AppDbContextConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(appDbConnectionString));

builder.Services.AddPlantupHangfire(builder.Configuration, appDbConnectionString);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<FileSystemSettings>(builder.Configuration.GetSection("FileSystemSettings"));

builder.Services.AddTransient<IVideoToGifService, VideoToGifService>();

builder.Services.AddPlantupSwagger("GifyTools API", "1.0");

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100_000_000; // 100MB
});

var app = builder.Build();

app.UsePlantupHangfireDashboard(builder.Configuration);

// Configure the HTTP request pipeline.
app.UsePlantupSwaggerUi();

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();
