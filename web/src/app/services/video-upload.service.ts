import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GifConversionOptions } from '../models/gif-converter-options.model';
import { EMPTY, interval, map, Observable, switchMap, takeWhile, tap, timeout } from 'rxjs';
import { ConversionStatusEnum } from '../models/conversion-status.enum';

@Injectable({
  providedIn: 'root'
})
export class VideoUploadService {

  // private api: string = "https://localhost:7143/api/gif";
  private api: string = "https://api.gifytools.com/api/gif";

  constructor(private http: HttpClient) { }

  // Schedule the conversion request
  conversionRequest(postmodel: GifConversionOptions): Observable<string> {
    const formData = new FormData();

    // Append fields manually
    formData.append('SetFps', postmodel.SetFps.toString());
    formData.append('Fps', postmodel.Fps.toString());
    formData.append('Width', postmodel.Width.toString());
    formData.append('SetStartTime', postmodel.SetStartTime.toString());
    formData.append('StartTime', postmodel.StartTime || '');
    formData.append('SetEndTime', postmodel.SetEndTime.toString());
    formData.append('EndTime', postmodel.EndTime || '');
    formData.append('SetSpeed', postmodel.SetSpeed.toString());
    formData.append('SpeedMultiplier', postmodel.SpeedMultiplier.toString());
    formData.append('SetCrop', postmodel.SetCrop.toString());
    formData.append('CropX', postmodel.CropX.toString());
    formData.append('CropY', postmodel.CropY.toString());
    formData.append('CropWidth', postmodel.CropWidth.toString());
    formData.append('CropHeight', postmodel.CropHeight.toString());
    formData.append('SetReverse', postmodel.SetReverse.toString());
    formData.append('SetWatermark', postmodel.SetWatermark.toString());
    formData.append('WatermarkText', postmodel.WatermarkText || '');
    formData.append('WatermarkFont', postmodel.WatermarkFont || '');
    formData.append('SetCompression', postmodel.SetCompression.toString());
    formData.append('CompressionLevel', postmodel.CompressionLevel.toString());
    formData.append('SetReduceFrames', postmodel.SetReduceFrames.toString());
    formData.append('FrameSkipInterval', postmodel.FrameSkipInterval.toString());

    // Append file separately
    if (postmodel.VideoFile) {
      formData.append('VideoFile', postmodel.VideoFile, postmodel.VideoFile.name);
    }

    return this.http.post<string>(`${this.api}/ConversionRequest`, formData);
  }

  // Check conversion status
  getConversionStatus(conversionId: string): Observable<{ conversionStatus: number }> {
    return this.http.get<{ conversionStatus: number, gifOutputPath?: string }>(
      `${this.api}/ConversionRequest/${conversionId}/status`
    );
  }

  // Download GIF file
  downloadGif(conversionId: string): Observable<Blob> {
    return this.http.get(`${this.api}/ConversionRequest/${conversionId}/download`, {
      responseType: 'blob'
    });
  }

  pollConversionStatus(conversionId: string, intervalMs = 5000): Observable<Blob> {
    console.log(`Starting polling for conversion ID: ${conversionId}`);
  
    return interval(intervalMs).pipe(
      switchMap(() => this.getConversionStatus(conversionId).pipe(
        tap(status => console.log(`Polled status:`, status))
      )),
      takeWhile(status => status.conversionStatus !== ConversionStatusEnum.Completed &&
                          status.conversionStatus !== ConversionStatusEnum.Failed, true),
      switchMap(status => {
        console.log(`Checking status: ${status.conversionStatus}`);
  
        if (status.conversionStatus === ConversionStatusEnum.Completed) {
          return this.downloadGif(conversionId).pipe(
            tap(blob => console.log('✅ GIF successfully downloaded!'))
          );
        } else if (status.conversionStatus === ConversionStatusEnum.Failed) {
          console.log(`❌ Conversion failed.`);
          return EMPTY; // Return an empty observable instead of an unsubscribed Observable
        }
  
        return EMPTY; // Continue polling without emitting unnecessary values
      })
    );
  }
  
}
