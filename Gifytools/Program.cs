using Gifytools.Bll;
using Gifytools.Database;
using Gifytools.ProcessQueue;
using Gifytools.Settings;
using Microsoft.EntityFrameworkCore;
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
