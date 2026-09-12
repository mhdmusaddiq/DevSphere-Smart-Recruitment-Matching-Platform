import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL, apiUrl } from '../../../core/config/api-base-url';
import { Resume } from '../../../core/models/resume.model';
import {
  ApplicationSnapshot,
  ApplicationStatusHistory,
  CandidateWorkflowSummary,
  ContactRequestView,
  NotificationView,
  SeekerApplication
} from './seeker-workflow.models';

@Injectable({ providedIn: 'root' })
export class SeekerWorkflowApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getApplications(): Observable<SeekerApplication[]> {
    return this.http.get<SeekerApplication[]>(
      apiUrl(this.baseUrl, '/applications/candidate')
    );
  }

  getApplication(applicationId: string): Observable<SeekerApplication> {
    return this.http.get<SeekerApplication>(
      apiUrl(this.baseUrl, `/applications/candidate/${applicationId}`)
    );
  }

  getSnapshot(applicationId: string): Observable<ApplicationSnapshot> {
    return this.http.get<ApplicationSnapshot>(
      apiUrl(this.baseUrl, `/applications/${applicationId}/snapshot`)
    );
  }

  getStatusHistory(applicationId: string): Observable<ApplicationStatusHistory[]> {
    return this.http.get<ApplicationStatusHistory[]>(
      apiUrl(this.baseUrl, `/applications/${applicationId}/status-history`)
    );
  }

  getWorkflow(applicationId: string): Observable<CandidateWorkflowSummary> {
    return this.http.get<CandidateWorkflowSummary>(
      apiUrl(this.baseUrl, `/candidate/applications/${applicationId}/workflow`)
    );
  }

  withdraw(applicationId: string): Observable<SeekerApplication> {
    return this.http.put<SeekerApplication>(
      apiUrl(this.baseUrl, `/applications/${applicationId}/withdraw`),
      {}
    );
  }

  getContactRequests(): Observable<ContactRequestView[]> {
    return this.http.get<ContactRequestView[]>(
      apiUrl(this.baseUrl, '/contact-requests/candidate')
    );
  }

  updateContactStatus(
    requestId: string,
    status: 'Accepted' | 'Declined' | 'Revoked'
  ): Observable<ContactRequestView> {
    return this.http.put<ContactRequestView>(
      apiUrl(this.baseUrl, `/contact-requests/${requestId}/status`),
      { status }
    );
  }

  getNotifications(): Observable<NotificationView[]> {
    return this.http.get<NotificationView[]>(
      apiUrl(this.baseUrl, '/notifications')
    );
  }

  markNotificationRead(notificationId: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      apiUrl(this.baseUrl, `/notifications/${notificationId}/read`),
      {}
    );
  }

  getResume(): Observable<Resume> {
    return this.http.get<Resume>(
      apiUrl(this.baseUrl, '/candidates/resume')
    );
  }

  downloadResumeVersion(versionId: string): Observable<HttpResponse<Blob>> {
    return this.http.get(
      apiUrl(this.baseUrl, `/candidates/resume/versions/${versionId}/download`),
      {
        observe: 'response',
        responseType: 'blob'
      }
    );
  }
}