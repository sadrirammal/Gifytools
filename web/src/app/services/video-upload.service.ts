import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { GifConversionOptions } from '../models/gif-converter-options.model';
import { EMPTY, interval, map, Observable, of, switchMap, takeWhile, tap } from 'rxjs';
import { ConversionStatusEnum } from '../models/conversion-status.enum';

@Injectable({
  providedIn: 'root'
})
export class VideoUploadService {

  private api: string = "https://api.gifytools.com/api/gif";

  constructor(private http: HttpClient) { }

  conversionRequest(postmodel: GifConversionOptions): Observable<string> {
    const formData = new FormData();
    Object.entries(postmodel).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        formData.append(key, value.toString());
      }
    });

    if (postmodel.VideoFile) {
      formData.append('VideoFile', postmodel.VideoFile, postmodel.VideoFile.name);
    }

    return this.http.post<string>(`${this.api}/ConversionRequest`, formData);
  }

  getConversionStatus(conversionId: string): Observable<{ conversionStatus: number, gifOutputPath?: string }> {
    return this.http.get<{ conversionStatus: number, gifOutputPath?: string }>(
      `${this.api}/ConversionRequest/${conversionId}/status`
    );
  }

  downloadGif(conversionId: string): Observable<Blob> {
    return this.http.get(`${this.api}/ConversionRequest/${conversionId}/download`, {
      responseType: 'blob'
    });
  }

  pollConversionStatus(conversionId: string, intervalMs = 5000): Observable<{ status: ConversionStatusEnum, gifBlob?: Blob }> {
    return interval(intervalMs).pipe(
      switchMap(() => this.getConversionStatus(conversionId)),
      takeWhile(status => status.conversionStatus !== ConversionStatusEnum.Completed &&
                          status.conversionStatus !== ConversionStatusEnum.Failed, true),
      switchMap(status => {
        if (status.conversionStatus === ConversionStatusEnum.Completed) {
          return this.downloadGif(conversionId).pipe(
            map(blob => ({ status: ConversionStatusEnum.Completed, gifBlob: blob }))
          );
        } else if (status.conversionStatus === ConversionStatusEnum.Failed) {
          return of({ status: ConversionStatusEnum.Failed });
        }
        return of({ status: status.conversionStatus });
      })
    );
  }
}
