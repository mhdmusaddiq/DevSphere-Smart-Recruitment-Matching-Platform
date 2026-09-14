import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL, apiUrl } from '../../core/config/api-base-url';
import { Resume } from '../../core/models/resume.model';

@Injectable({ providedIn: 'root' })
export class CvApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getResume(): Observable<Resume> {
    return this.http.get<Resume>(
      apiUrl(this.baseUrl, '/candidates/resume')
    );
  }

  uploadVersion(file: File): Observable<Resume> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<Resume>(
      apiUrl(this.baseUrl, '/candidates/resume/versions'),
      formData
    );
  }

  setCurrentVersion(versionId: string): Observable<Resume> {
    return this.http.put<Resume>(
      apiUrl(this.baseUrl, `/candidates/resume/versions/${versionId}/current`),
      {}
    );
  }

  downloadVersion(versionId: string): Observable<HttpResponse<Blob>> {
    return this.http.get(
      apiUrl(this.baseUrl, `/candidates/resume/versions/${versionId}/download`),
      {
        observe: 'response',
        responseType: 'blob'
      }
    );
  }
}