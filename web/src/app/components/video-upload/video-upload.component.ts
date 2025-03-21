// video-upload.component.ts
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Component, Inject, PLATFORM_ID } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LoadingModalComponent } from '../modals/loading-modal/loading-modal.component';
import { GifConversionOptions } from '../../models/gif-converter-options.model';
import { VideoUploadService } from '../../services/video-upload.service';
import { BsModalService, BsModalRef, ModalModule } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-video-upload',
  standalone: true,
  imports: [FormsModule, CommonModule, LoadingModalComponent],
  templateUrl: './video-upload.component.html',
  styleUrl: './video-upload.component.scss'
})
export class VideoUploadComponent implements AfterViewInit {
  constructor(@Inject(PLATFORM_ID) private platformId: object, 
  private videoUploadService: VideoUploadService, private modalService: BsModalService) {}

  async ngAfterViewInit(): Promise<void> {
    if (isPlatformBrowser(this.platformId)) {
      try {
        // ✅ Dynamically import Bootstrap (use default import)
        const bootstrap = await import('bootstrap');

        // ✅ Ensure tooltips work
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        tooltipTriggerList.forEach((tooltipTriggerEl) => {
          new bootstrap.Tooltip(tooltipTriggerEl);
        });
      } catch (error) {
        console.error("Bootstrap failed to load:", error);
      }
    }
  }

  selectedFile: File | null = null;
  isConverting: boolean = false;
  conversionId: string | null = null;
  conversionStatus: string = '';

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

  clearFile() {
    this.selectedFile = null;
    this.isConverting = false;
    this.conversionStatus = '';
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

    this.options.VideoFile = this.selectedFile; 
    this.videoUploadService.conversionRequest(this.options).subscribe({
      next: (id) => {
        this.conversionId = id;
        this.modalService.show(LoadingModalComponent, { initialState: { conversionId: id } });
      },
      error: (error) => {
        console.error('Upload error:', error);
        //TODO: add toast
      }
    }).add(() => {
      this.isConverting = false;
      this.selectedFile = null;
    });
  }
}