import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GifConversionOptions } from '../models/gif-converter-options.model';
import { Observable, timeout } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class VideoUploadService {

  // private api: string = "https://localhost:7143/api/gif";
  private api: string = "https://api.gifytools.com/api/gif";

  constructor(private http: HttpClient) { }

  videoToGif(postmodel: GifConversionOptions): Observable<Blob>  {
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
  
    return this.http.post<Blob>(`${this.api}/videoToGif`, formData, {
      responseType: 'blob' as 'json'
    }).pipe(timeout(60000));
  }
  
}
