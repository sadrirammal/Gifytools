import { CommonModule } from '@angular/common';
import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VideoUploadService } from '../services/video-upload.service';
import { GifConversionOptions } from '../models/gif-converter-options.model';

@Component({
  selector: 'app-video-upload',
  standalone: true,
  imports: [FormsModule, CommonModule ],
  templateUrl: './video-upload.component.html',
  styleUrl: './video-upload.component.scss'
})
export class VideoUploadComponent {
  selectedFile: File | null = null;
  uploadProgress: number = 0;

  // Default Conversion Options (Match .NET Model)
  conversionOptions: GifConversionOptions = {
    SetFps: false,
    Fps: 15,
    Width: 720,
    SetReverse: false,
    SetStartTime: false,
    StartTime: null,
    SetEndTime: false,
    EndTime: null,
    SetSpeed: false,
    SpeedMultiplier: 1.0,
    SetCrop: false,
    CropX: 0,
    CropY: 0,
    CropWidth: 0,
    CropHeight: 0,
    SetWatermark: false,
    WatermarkText: '',
    WatermarkFont: '',
    SetCompression: false,
    CompressionLevel: 0,
    SetReduceFrames: false,
    FrameSkipInterval: 2
  };

  constructor(private videoUploadService: VideoUploadService) {}

  // Handle file selection
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  public generatedGifUrl: string = '';
  // Upload file to API
  onUpload() {
    if (!this.selectedFile) return;

    this.conversionOptions.VideoFile = this.selectedFile;

    this.videoUploadService.videoToGif(this.conversionOptions).subscribe({
      next: (blob) => {
        const objectURL = URL.createObjectURL(blob);
        this.generatedGifUrl = objectURL;
      },
      error: () => console.log("Something went wrong")});
  }
}
