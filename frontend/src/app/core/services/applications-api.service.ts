import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  ApplicationStatusHistory,
  JobApplication
} from '../models/application.model';

@Injectable({
  providedIn: 'root'
})
export class ApplicationsApiService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = 'https://localhost:7097/api';

  apply(vacancyId: string): Observable<JobApplication> {
    return this.http.post<JobApplication>(
      `${this.apiUrl}/applications`,
      {
        vacancyId,
        candidateId: '',
        status: ''
      }
    );
  }

  getMine(): Observable<JobApplication[]> {
    return this.http.get<JobApplication[]>(
      `${this.apiUrl}/applications/candidate`
    );
  }

  getHistory(
    applicationId: string
  ): Observable<ApplicationStatusHistory[]> {
    return this.http.get<ApplicationStatusHistory[]>(
      `${this.apiUrl}/applications/${applicationId}/status-history`
    );
  }
}