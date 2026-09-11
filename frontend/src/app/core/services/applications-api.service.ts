import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  ApplicationStatusHistory,
  JobApplication
} from '../models/application.model';
import { API_BASE_URL, apiUrl } from '../config/api-base-url';

@Injectable({
  providedIn: 'root'
})
export class ApplicationsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  apply(vacancyId: string): Observable<JobApplication> {
    return this.http.post<JobApplication>(
      apiUrl(this.baseUrl, '/applications'),
      {
        vacancyId,
        candidateId: '',
        status: ''
      }
    );
  }

  getMine(): Observable<JobApplication[]> {
    return this.http.get<JobApplication[]>(
      apiUrl(this.baseUrl, '/applications/candidate')
    );
  }

  getHistory(
    applicationId: string
  ): Observable<ApplicationStatusHistory[]> {
    return this.http.get<ApplicationStatusHistory[]>(
      apiUrl(this.baseUrl, `/applications/${applicationId}/status-history`)
    );
  }
}
