import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VideoUploadService } from '../services/video-upload.service';
import { GifConversionOptions } from '../models/gif-converter-options.model';

@Component({
  selector: 'app-video-upload',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './video-upload.component.html',
  styleUrl: './video-upload.component.scss'
})
export class VideoUploadComponent {
  selectedFile: File | null = null;
  isConverting: boolean = false;
  conversionId: string | null = null;
  conversionStatus: string = '';
  generatedGifUrl: string = '';

  // Default Conversion Options
  options: GifConversionOptions = {
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
    FrameSkipInterval: 10,
  };

  constructor(private videoUploadService: VideoUploadService) {}

  clearFile() {
    this.selectedFile = null;
  }

  // Handle file selection
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onFileDropped(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    const file = event.dataTransfer?.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  // Upload file & start conversion
  onUpload() {
    if (!this.selectedFile) return;

    this.isConverting = true;
    this.conversionStatus = 'Uploading & converting...';
    this.options.VideoFile = this.selectedFile;

    this.videoUploadService.conversionRequest(this.options).subscribe({
      next: (id) => {
        this.conversionId = id;
        this.pollConversionStatus(id); // Start polling for conversion status
      },
      error: () => {
        this.conversionStatus = 'Conversion failed.';
        this.isConverting = false;
      }
    });
  }

  // Polling for conversion status
  pollConversionStatus(conversionId: string) {
    this.videoUploadService.pollConversionStatus(conversionId).subscribe({
      next: (gifUrl) => {
        if (gifUrl) {
          this.generatedGifUrl = gifUrl;
          this.conversionStatus = 'GIF is ready!';
          this.isConverting = false;
          this.downloadGif(conversionId);
        } else {
          this.conversionStatus = 'Processing...';
        }
      },
      error: (err) => {
        console.error('Polling failed:', err);
        this.conversionStatus = 'Conversion failed.';
        this.isConverting = false;
      }
    });
  }

  // Download the generated GIF
  downloadGif(conversionId: string) {
    this.videoUploadService.downloadGif(conversionId).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'converted.gif';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    });
  }
}
