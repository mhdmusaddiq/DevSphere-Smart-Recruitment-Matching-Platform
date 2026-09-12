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
