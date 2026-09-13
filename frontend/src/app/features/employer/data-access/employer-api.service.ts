import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL, apiUrl } from '../../../core/config/api-base-url';
import {
  CompanyProfile,
  CompanyProfileRequest,
  CompanyVerificationResult,
  ApplicationStatusTransitionRequest,
  EmployerDashboard,
  EmployerProfile,
  EmployerVacancy,
  EmployerVacancyUpsertRequest,
  EmployerJobApplication,
  EmployerRankedApplicant,
  EmployerApplicationDetail,
  EmployerApplicationSnapshot,
  EmployerApplicationStatusHistory,

  VacancyPolicyAggregateDto,
  MatchingPolicyAggregateUpdateRequest,
  MatchingPolicyRevisionDto,
  StoredFileDescriptor,
  SubmitCompanyVerificationRequest
} from './employer.models';

import {
  EmployerContactRequest,
  EmployerInterview,
  EmployerInterviewSlot,
  EmployerNotification,
  EmployerOffer,
  EmployerScorecard,
  EmployerTalentPoolEntry,
  EmployerWorkflowSummary
} from './employer.models';
@Injectable({
  providedIn: 'root'
})
export class EmployerApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getDashboard(): Observable<EmployerDashboard> {
    return this.http.get<EmployerDashboard>(
      apiUrl(this.baseUrl, '/dashboard/employer')
    );
  }

  getProfile(): Observable<EmployerProfile | null> {
    return this.http.get<EmployerProfile | null>(
      apiUrl(this.baseUrl, '/profile/employer')
    );
  }

  createProfile(profile: EmployerProfile): Observable<EmployerProfile> {
    return this.http.post<EmployerProfile>(
      apiUrl(this.baseUrl, '/profile/employer'),
      profile
    );
  }

  updateProfile(profile: EmployerProfile): Observable<EmployerProfile> {
    return this.http.put<EmployerProfile>(
      apiUrl(this.baseUrl, '/profile/employer'),
      profile
    );
  }

  getVacancies(): Observable<EmployerVacancy[]> {
    return this.http.get<EmployerVacancy[]>(
      apiUrl(this.baseUrl, '/vacancies/mine')
    );
  }

  getVacancy(vacancyId: string): Observable<EmployerVacancy> {
    return this.http.get<EmployerVacancy>(
      apiUrl(this.baseUrl, `/vacancies/${vacancyId}`)
    );
  }


  getVacancyApplications(
    vacancyId: string
  ): Observable<EmployerRankedApplicant[]> {
    return this.http.get<EmployerRankedApplicant[]>(
      apiUrl(this.baseUrl, `/applications/vacancy/${vacancyId}`)
    );
  }

  getEmployerApplication(
    applicationId: string
  ): Observable<EmployerApplicationDetail> {
    return this.http.get<EmployerApplicationDetail>(
      apiUrl(
        this.baseUrl,
        `/employer/applications/${applicationId}`
      )
    );
  }

  getApplicationSnapshot(
    applicationId: string
  ): Observable<EmployerApplicationSnapshot> {
    return this.http.get<EmployerApplicationSnapshot>(
      apiUrl(
        this.baseUrl,
        `/applications/${applicationId}/snapshot`
      )
    );
  }

  getApplicationStatusHistory(
    applicationId: string
  ): Observable<EmployerApplicationStatusHistory[]> {
    return this.http.get<EmployerApplicationStatusHistory[]>(
      apiUrl(
        this.baseUrl,
        `/applications/${applicationId}/status-history`
      )
    );
  }

  updateApplicationStatus(
    applicationId: string,
    status: string
  ): Observable<EmployerJobApplication> {
    const request: ApplicationStatusTransitionRequest = { status };

    return this.http.put<EmployerJobApplication>(
      apiUrl(
        this.baseUrl,
        `/applications/${applicationId}/status`
      ),
      request
    );
  }

  downloadApplicationResume(
    applicationId: string
  ): Observable<Blob> {
    return this.http.get(
      apiUrl(
        this.baseUrl,
        `/employer/applications/${applicationId}/resume`
      ),
      { responseType: 'blob' }
    );
  }
  createVacancy(
    request: EmployerVacancyUpsertRequest
  ): Observable<EmployerVacancy> {
    return this.http.post<EmployerVacancy>(
      apiUrl(this.baseUrl, '/vacancies'),
      request
    );
  }

  updateVacancy(
    vacancyId: string,
    request: EmployerVacancyUpsertRequest
  ): Observable<EmployerVacancy> {
    return this.http.put<EmployerVacancy>(
      apiUrl(this.baseUrl, `/vacancies/${vacancyId}`),
      request
    );
  }

  publishVacancy(vacancyId: string): Observable<EmployerVacancy> {
    return this.http.put<EmployerVacancy>(
      apiUrl(this.baseUrl, `/vacancies/${vacancyId}/publish`),
      {}
    );
  }

  closeVacancy(vacancyId: string): Observable<EmployerVacancy> {
    return this.http.put<EmployerVacancy>(
      apiUrl(this.baseUrl, `/vacancies/${vacancyId}/close`),
      {}
    );
  }

  getCurrentMatchingPolicy(
    vacancyId: string
  ): Observable<MatchingPolicyRevisionDto> {
    return this.http.get<MatchingPolicyRevisionDto>(
      apiUrl(
        this.baseUrl,
        `/vacancies/${vacancyId}/policy/current`
      )
    );
  }

  getFullMatchingPolicy(
    vacancyId: string
  ): Observable<VacancyPolicyAggregateDto> {
    return this.http.get<VacancyPolicyAggregateDto>(
      apiUrl(
        this.baseUrl,
        `/vacancies/${vacancyId}/policy/current/full`
      )
    );
  }

  updateMatchingPolicy(
    vacancyId: string,
    request: MatchingPolicyAggregateUpdateRequest
  ): Observable<VacancyPolicyAggregateDto> {
    return this.http.put<VacancyPolicyAggregateDto>(
      apiUrl(
        this.baseUrl,
        `/vacancies/${vacancyId}/policy/current`
      ),
      request
    );
  }
  getCompanies(): Observable<CompanyProfile[]> {
    return this.http.get<CompanyProfile[]>(
      apiUrl(this.baseUrl, '/companies/mine')
    );
  }

  createCompany(request: CompanyProfileRequest): Observable<CompanyProfile> {
    return this.http.post<CompanyProfile>(
      apiUrl(this.baseUrl, '/companies'),
      request
    );
  }

  updateCompany(
    companyId: string,
    request: CompanyProfileRequest
  ): Observable<CompanyProfile> {
    return this.http.put<CompanyProfile>(
      apiUrl(this.baseUrl, `/companies/${companyId}`),
      request
    );
  }

  uploadFile(file: File): Observable<StoredFileDescriptor> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<StoredFileDescriptor>(
      apiUrl(this.baseUrl, '/files'),
      formData
    );
  }
  submitCompanyVerification(
    companyId: string,
    request: SubmitCompanyVerificationRequest
  ): Observable<CompanyVerificationResult> {
    return this.http.post<CompanyVerificationResult>(
      apiUrl(this.baseUrl, `/companies/${companyId}/verification`),
      request
    );
  }

  getEmployerContactRequests(): Observable<EmployerContactRequest[]> {
    return this.http.get<EmployerContactRequest[]>(apiUrl(this.baseUrl, '/contact-requests/employer'));
  }
  sendContactRequest(applicationId:string): Observable<EmployerContactRequest> {
    return this.http.post<EmployerContactRequest>(apiUrl(this.baseUrl, `/contact-requests/applications/${applicationId}`), {});
  }
  cancelContactRequest(id:string): Observable<EmployerContactRequest> {
    return this.http.put<EmployerContactRequest>(apiUrl(this.baseUrl, `/contact-requests/${id}/employer-status`), {status:'Cancelled'});
  }
  getEmployerWorkflowSummary(applicationId:string): Observable<EmployerWorkflowSummary> {
    return this.http.get<EmployerWorkflowSummary>(apiUrl(this.baseUrl, `/employer-workflow/applications/${applicationId}/summary`));
  }
  createInterview(applicationId:string,notes:string): Observable<EmployerInterview> {
    return this.http.post<EmployerInterview>(apiUrl(this.baseUrl, '/employer-workflow/interviews'), {jobApplicationId:applicationId,notes});
  }
  updateInterviewStatus(id:string,status:'Completed'|'Cancelled'): Observable<EmployerInterview> {
    return this.http.put<EmployerInterview>(apiUrl(this.baseUrl, `/employer-workflow/interviews/${id}/status`), {status});
  }
  addInterviewSlot(id:string,startsAtUtc:string,endsAtUtc:string,locationOrMeetingUrl:string): Observable<EmployerInterviewSlot> {
    return this.http.post<EmployerInterviewSlot>(apiUrl(this.baseUrl, `/employer-workflow/interviews/${id}/slots`), {startsAtUtc,endsAtUtc,locationOrMeetingUrl});
  }
  createScorecard(applicationId:string,interviewId:string|null,overallRating:number,notes:string): Observable<EmployerScorecard> {
    return this.http.post<EmployerScorecard>(apiUrl(this.baseUrl, '/employer-workflow/scorecards'), {jobApplicationId:applicationId,interviewId,overallRating,notes});
  }
  createOffer(applicationId:string,offeredSalary:number|null,expiresAtUtc:string|null,notes:string): Observable<EmployerOffer> {
    return this.http.post<EmployerOffer>(apiUrl(this.baseUrl, '/employer-workflow/offers'), {jobApplicationId:applicationId,offeredSalary,expiresAtUtc,notes});
  }
  updateOfferStatus(id:string,status:'Extended'|'Accepted'|'Declined'|'Withdrawn'): Observable<EmployerOffer> {
    return this.http.put<EmployerOffer>(apiUrl(this.baseUrl, `/employer-workflow/offers/${id}/status`), {status});
  }
  addTalentPoolEntry(applicationId:string,hasCandidateConsent:boolean,notes:string): Observable<EmployerTalentPoolEntry> {
    return this.http.post<EmployerTalentPoolEntry>(apiUrl(this.baseUrl, '/employer-workflow/talent-pool'), {jobApplicationId:applicationId,hasCandidateConsent,notes});
  }
  getNotifications(): Observable<EmployerNotification[]> {
    return this.http.get<EmployerNotification[]>(apiUrl(this.baseUrl, '/notifications'));
  }
  markNotificationRead(id:string): Observable<{message:string}> {
    return this.http.put<{message:string}>(apiUrl(this.baseUrl, `/notifications/${id}/read`), {});
  }
}
