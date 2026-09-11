import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Resume } from '../models/resume.model';
import { API_BASE_URL, apiUrl } from '../config/api-base-url';

@Injectable({ providedIn: 'root' })
export class ResumeApiService {
  private readonly baseUrl = inject(API_BASE_URL);

  constructor(private readonly http: HttpClient) {}

  getResume(): Observable<Resume> {
    return this.http.get<Resume>(apiUrl(this.baseUrl, '/candidates/resume'));
  }

  uploadVersion(file: File): Observable<Resume> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<Resume>(
      apiUrl(this.baseUrl, '/candidates/resume/versions'),
      formData
    );
  }

  downloadVersion(
    versionId: string
  ): Observable<HttpResponse<Blob>> {
    return this.http.get(
      apiUrl(this.baseUrl, `/candidates/resume/versions/${versionId}/download`),
      {
        observe: 'response',
        responseType: 'blob'
      }
    );
  }
}
