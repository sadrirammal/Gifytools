import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { VideoUploadComponent } from "./video-upload/video-upload.component";
import { provideHttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, VideoUploadComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'GifyTools';
}
