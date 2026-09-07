import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Resume } from '../models/resume.model';

@Injectable({ providedIn: 'root' })
export class ResumeApiService {
  private readonly api = 'https://localhost:7097/api/candidates/resume';

  constructor(private readonly http: HttpClient) {}

  getResume(): Observable<Resume> {
    return this.http.get<Resume>(this.api);
  }

  uploadVersion(file: File): Observable<Resume> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<Resume>(
      `${this.api}/versions`,
      formData
    );
  }

  downloadVersion(
    versionId: string
  ): Observable<HttpResponse<Blob>> {
    return this.http.get(
      `${this.api}/versions/${versionId}/download`,
      {
        observe: 'response',
        responseType: 'blob'
      }
    );
  }
}
