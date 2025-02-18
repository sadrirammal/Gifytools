﻿using Gifytools.Database.Entities;

namespace Gifytools.Bll;

public interface IVideoToGifService
{
    Task ConvertToGif(string inputPath, string outputPath, int fps = 15, int width = 720);
    Task<string> UploadVideo(IFormFile videoFile);
    Task<string> ConvertToGif(string inputPath, string fileName, GifConversionOptions options);

    Task<string> ConvertToGif(ConversionRequestEntity entity);

}