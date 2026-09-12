import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL, apiUrl } from '../../../core/config/api-base-url';
import {
  CompanyProfile,
  CompanyProfileRequest,
  CompanyVerificationResult,
  EmployerDashboard,
  EmployerProfile,
  EmployerVacancy,
  EmployerJobApplication,
  EmployerRankedApplicant,

  VacancyPolicyAggregateDto,
  MatchingPolicyAggregateUpdateRequest,
  MatchingPolicyRevisionDto,
  StoredFileDescriptor,
  SubmitCompanyVerificationRequest
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
  createVacancy(request: EmployerVacancy): Observable<EmployerVacancy> {
    return this.http.post<EmployerVacancy>(
      apiUrl(this.baseUrl, '/vacancies'),
      request
    );
  }

  updateVacancy(
    vacancyId: string,
    request: EmployerVacancy
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
}
