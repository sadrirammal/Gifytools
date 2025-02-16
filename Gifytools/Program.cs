using Gifytools.Bll;
using Gifytools.Settings;
using Microsoft.AspNetCore.Http.Features;
using Plantup.Swagger.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

// Configure the HTTP request pipeline.
app.UsePlantupSwaggerUi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
