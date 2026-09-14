import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL, apiUrl } from '../../../core/config/api-base-url';
import {
  ApplicationReadiness,
  ApplyDecisionView,
  CandidateDashboardSummary,
  CandidateProfileView,
  ContactRequestView,
  JobApplicationView,
  MatchResultView,
  NotificationView,
  ResumeView,
  VacancyFilters,
  VacancyView
} from './seeker.models';

@Injectable({ providedIn: 'root' })
export class SeekerApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getDashboardSummary(): Observable<CandidateDashboardSummary> {
    return this.http.get<CandidateDashboardSummary>(
      apiUrl(this.baseUrl, '/dashboard/candidate')
    );
  }

  getApplicationReadiness(): Observable<ApplicationReadiness> {
    return this.http.get<ApplicationReadiness>(
      apiUrl(this.baseUrl, '/profile/candidate/application-readiness')
    );
  }

  getCandidateProfile(): Observable<CandidateProfileView> {
    return this.http.get<CandidateProfileView>(
      apiUrl(this.baseUrl, '/profile/candidate')
    );
  }

  getVacancies(
    filters: VacancyFilters = {},
    bestMatch = false
  ): Observable<VacancyView[]> {
    let params = new HttpParams();

    const values: Array<[string, string | number | undefined]> = [
      ['q', filters.q],
      ['location', filters.location],
      ['skill', filters.skill],
      ['workMode', filters.workMode],
      ['employmentType', filters.employmentType],
      ['page', filters.page],
      ['pageSize', filters.pageSize]
    ];

    for (const [key, value] of values) {
      if (value !== undefined && String(value).trim() !== '') {
        params = params.set(key, String(value));
      }
    }

    const path = bestMatch ? '/vacancies/best-match' : '/vacancies';
    return this.http.get<VacancyView[]>(apiUrl(this.baseUrl, path), { params });
  }

  getVacancy(vacancyId: string): Observable<VacancyView> {
    return this.http.get<VacancyView>(
      apiUrl(this.baseUrl, `/vacancies/${vacancyId}`)
    );
  }

  getMatch(vacancyId: string): Observable<MatchResultView> {
    return this.http.get<MatchResultView>(
      apiUrl(this.baseUrl, `/matching/vacancies/${vacancyId}`)
    );
  }

  getApplyDecision(vacancyId: string): Observable<ApplyDecisionView> {
    return this.http.get<ApplyDecisionView>(
      apiUrl(this.baseUrl, `/applications/vacancies/${vacancyId}/apply-decision`)
    );
  }

  apply(
    vacancyId: string,
    baselineAcknowledged: boolean
  ): Observable<JobApplicationView> {
    return this.http.post<JobApplicationView>(
      apiUrl(this.baseUrl, '/applications'),
      { vacancyId, baselineAcknowledged }
    );
  }

  getApplications(): Observable<JobApplicationView[]> {
    return this.http.get<JobApplicationView[]>(
      apiUrl(this.baseUrl, '/applications/candidate')
    );
  }

  getContactRequests(): Observable<ContactRequestView[]> {
    return this.http.get<ContactRequestView[]>(
      apiUrl(this.baseUrl, '/contact-requests/candidate')
    );
  }

  getNotifications(): Observable<NotificationView[]> {
    return this.http.get<NotificationView[]>(
      apiUrl(this.baseUrl, '/notifications')
    );
  }

  getResume(): Observable<ResumeView> {
    return this.http.get<ResumeView>(
      apiUrl(this.baseUrl, '/candidates/resume')
    );
  }
}