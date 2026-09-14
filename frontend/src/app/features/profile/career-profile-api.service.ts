import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL, apiUrl } from '../../core/config/api-base-url';
import {
  ApplicationReadiness,
  CandidateProfilePayload,
  CandidateSkillRecord,
  CertificationRecord,
  EducationRecord,
  LanguageRecord,
  LicenceRecord,
  ProfileReadiness,
  ProjectRecord,
  WorkExperienceRecord
} from './profile.models';

@Injectable({ providedIn: 'root' })
export class CareerProfileApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getProfile(): Observable<CandidateProfilePayload | null> {
    return this.http.get<CandidateProfilePayload | null>(
      apiUrl(this.baseUrl, '/profile/candidate')
    );
  }

  createProfile(payload: CandidateProfilePayload): Observable<CandidateProfilePayload> {
    return this.http.post<CandidateProfilePayload>(
      apiUrl(this.baseUrl, '/profile/candidate'),
      payload
    );
  }

  updateProfile(payload: CandidateProfilePayload): Observable<CandidateProfilePayload> {
    return this.http.put<CandidateProfilePayload>(
      apiUrl(this.baseUrl, '/profile/candidate'),
      payload
    );
  }

  getSkills(): Observable<CandidateSkillRecord[]> {
    return this.http.get<CandidateSkillRecord[]>(
      apiUrl(this.baseUrl, '/profile/candidate/skills')
    );
  }

  addSkill(name: string): Observable<CandidateSkillRecord> {
    return this.http.post<CandidateSkillRecord>(
      apiUrl(this.baseUrl, '/profile/candidate/skills'),
      { name }
    );
  }

  deleteSkill(skillId: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/profile/candidate/skills/${skillId}`)
    );
  }

  getProfileReadiness(): Observable<ProfileReadiness> {
    return this.http.get<ProfileReadiness>(
      apiUrl(this.baseUrl, '/profile/candidate/readiness')
    );
  }

  getApplicationReadiness(): Observable<ApplicationReadiness> {
    return this.http.get<ApplicationReadiness>(
      apiUrl(this.baseUrl, '/profile/candidate/application-readiness')
    );
  }

  getWorkExperiences(): Observable<WorkExperienceRecord[]> {
    return this.http.get<WorkExperienceRecord[]>(
      apiUrl(this.baseUrl, '/candidate-career/work-experiences')
    );
  }

  addWorkExperience(payload: Partial<WorkExperienceRecord>): Observable<WorkExperienceRecord> {
    return this.http.post<WorkExperienceRecord>(
      apiUrl(this.baseUrl, '/candidate-career/work-experiences'),
      payload
    );
  }

  updateWorkExperience(id: string, payload: Partial<WorkExperienceRecord>): Observable<WorkExperienceRecord> {
    return this.http.put<WorkExperienceRecord>(
      apiUrl(this.baseUrl, `/candidate-career/work-experiences/${id}`),
      payload
    );
  }

  deleteWorkExperience(id: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/candidate-career/work-experiences/${id}`)
    );
  }

  getEducation(): Observable<EducationRecord[]> {
    return this.http.get<EducationRecord[]>(
      apiUrl(this.baseUrl, '/candidate-career/education')
    );
  }

  addEducation(payload: Partial<EducationRecord>): Observable<EducationRecord> {
    return this.http.post<EducationRecord>(
      apiUrl(this.baseUrl, '/candidate-career/education'),
      payload
    );
  }

  updateEducation(id: string, payload: Partial<EducationRecord>): Observable<EducationRecord> {
    return this.http.put<EducationRecord>(
      apiUrl(this.baseUrl, `/candidate-career/education/${id}`),
      payload
    );
  }

  deleteEducation(id: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/candidate-career/education/${id}`)
    );
  }

  getCertifications(): Observable<CertificationRecord[]> {
    return this.http.get<CertificationRecord[]>(
      apiUrl(this.baseUrl, '/candidate-career/certifications')
    );
  }

  addCertification(payload: Partial<CertificationRecord>): Observable<CertificationRecord> {
    return this.http.post<CertificationRecord>(
      apiUrl(this.baseUrl, '/candidate-career/certifications'),
      payload
    );
  }

  updateCertification(id: string, payload: Partial<CertificationRecord>): Observable<CertificationRecord> {
    return this.http.put<CertificationRecord>(
      apiUrl(this.baseUrl, `/candidate-career/certifications/${id}`),
      payload
    );
  }

  deleteCertification(id: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/candidate-career/certifications/${id}`)
    );
  }

  getProjects(): Observable<ProjectRecord[]> {
    return this.http.get<ProjectRecord[]>(
      apiUrl(this.baseUrl, '/candidate-career/projects')
    );
  }

  addProject(payload: Partial<ProjectRecord>): Observable<ProjectRecord> {
    return this.http.post<ProjectRecord>(
      apiUrl(this.baseUrl, '/candidate-career/projects'),
      payload
    );
  }

  updateProject(id: string, payload: Partial<ProjectRecord>): Observable<ProjectRecord> {
    return this.http.put<ProjectRecord>(
      apiUrl(this.baseUrl, `/candidate-career/projects/${id}`),
      payload
    );
  }

  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/candidate-career/projects/${id}`)
    );
  }

  getLanguages(): Observable<LanguageRecord[]> {
    return this.http.get<LanguageRecord[]>(
      apiUrl(this.baseUrl, '/candidate-career/languages')
    );
  }

  addLanguage(payload: Partial<LanguageRecord>): Observable<LanguageRecord> {
    return this.http.post<LanguageRecord>(
      apiUrl(this.baseUrl, '/candidate-career/languages'),
      payload
    );
  }

  updateLanguage(id: string, payload: Partial<LanguageRecord>): Observable<LanguageRecord> {
    return this.http.put<LanguageRecord>(
      apiUrl(this.baseUrl, `/candidate-career/languages/${id}`),
      payload
    );
  }

  deleteLanguage(id: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/candidate-career/languages/${id}`)
    );
  }

  getLicences(): Observable<LicenceRecord[]> {
    return this.http.get<LicenceRecord[]>(
      apiUrl(this.baseUrl, '/candidate-career/licences')
    );
  }

  addLicence(payload: Partial<LicenceRecord>): Observable<LicenceRecord> {
    return this.http.post<LicenceRecord>(
      apiUrl(this.baseUrl, '/candidate-career/licences'),
      payload
    );
  }

  updateLicence(id: string, payload: Partial<LicenceRecord>): Observable<LicenceRecord> {
    return this.http.put<LicenceRecord>(
      apiUrl(this.baseUrl, `/candidate-career/licences/${id}`),
      payload
    );
  }

  deleteLicence(id: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/candidate-career/licences/${id}`)
    );
  }
}