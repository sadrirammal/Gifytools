import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { VideoUploadService } from '../../../services/video-upload.service';
import { Observable } from 'rxjs';
import { ConversionStatusEnum } from '../../../models/conversion-status.enum';

@Component({
  selector: 'app-loading-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-modal.component.html',
  styleUrl: './loading-modal.component.scss'
})
export class LoadingModalComponent implements OnInit {
  
  @Input() conversionId!: string;

  isLoading: boolean = true;
  generatedGifUrl: string | null = null;
  conversionErrorMessage: string = '';
  conversionStatus: ConversionStatusEnum = ConversionStatusEnum.Pending;

  constructor(
    private bsModalRef: BsModalRef,
    private videoUploadService: VideoUploadService
  ) { }

  ngOnInit(): void {
    if (!this.conversionId) {
      this.bsModalRef.hide();
      return;
    }

    // Start polling the conversion status
    this.videoUploadService.pollConversionStatus(this.conversionId).subscribe({
      next: (blob) => {
        this.generatedGifUrl = window.URL.createObjectURL(blob); // Create a URL for the GIF
        console.log('Generated GIF URL:', this.generatedGifUrl);
        this.conversionStatus = ConversionStatusEnum.Completed;
      },
      error: (err) => {
        console.error('Error while polling:', err);
        this.conversionStatus = ConversionStatusEnum.Failed;
        this.conversionErrorMessage = 'Conversion failed. Please try again.';
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  downloadGif() {
    if (!this.generatedGifUrl) {
      console.error('No GIF available to download.');
      return;
    }

    const a = document.createElement('a');
    a.href = this.generatedGifUrl;
    a.download = 'converted.gif';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  close() {
    this.bsModalRef.hide();
  }
}
