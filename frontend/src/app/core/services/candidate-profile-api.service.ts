import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CandidateProfile,
  CandidateSkill,
  EducationRecord,
  WorkExperience
} from '../models/candidate-profile.model';
import { API_BASE_URL, apiUrl } from '../config/api-base-url';

@Injectable({ providedIn: 'root' })
export class CandidateProfileApiService {
  private readonly baseUrl = inject(API_BASE_URL);

  constructor(private readonly http: HttpClient) {}

  getProfile(): Observable<CandidateProfile | null> {
    return this.http.get<CandidateProfile | null>(
      apiUrl(this.baseUrl, '/profile/candidate')
    );
  }

  createProfile(profile: CandidateProfile): Observable<CandidateProfile> {
    return this.http.post<CandidateProfile>(
      apiUrl(this.baseUrl, '/profile/candidate'),
      profile
    );
  }

  updateProfile(profile: CandidateProfile): Observable<CandidateProfile> {
    return this.http.put<CandidateProfile>(
      apiUrl(this.baseUrl, '/profile/candidate'),
      profile
    );
  }

  getSkills(): Observable<CandidateSkill[]> {
    return this.http.get<CandidateSkill[]>(
      apiUrl(this.baseUrl, '/profile/candidate/skills')
    );
  }

  addSkill(name: string): Observable<CandidateSkill> {
    return this.http.post<CandidateSkill>(
      apiUrl(this.baseUrl, '/profile/candidate/skills'),
      { name }
    );
  }

  deleteSkill(id: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/profile/candidate/skills/${id}`)
    );
  }

  getWorkExperiences(): Observable<WorkExperience[]> {
    return this.http.get<WorkExperience[]>(
      apiUrl(this.baseUrl, '/candidate-career/work-experiences')
    );
  }

  addWorkExperience(
    item: Partial<WorkExperience>
  ): Observable<WorkExperience> {
    return this.http.post<WorkExperience>(
      apiUrl(this.baseUrl, '/candidate-career/work-experiences'),
      item
    );
  }

  updateWorkExperience(
    id: string,
    item: Partial<WorkExperience>
  ): Observable<WorkExperience> {
    return this.http.put<WorkExperience>(
      apiUrl(this.baseUrl, `/candidate-career/work-experiences/${id}`),
      item
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

  addEducation(
    item: Partial<EducationRecord>
  ): Observable<EducationRecord> {
    return this.http.post<EducationRecord>(
      apiUrl(this.baseUrl, '/candidate-career/education'),
      item
    );
  }

  updateEducation(
    id: string,
    item: Partial<EducationRecord>
  ): Observable<EducationRecord> {
    return this.http.put<EducationRecord>(
      apiUrl(this.baseUrl, `/candidate-career/education/${id}`),
      item
    );
  }

  deleteEducation(id: string): Observable<void> {
    return this.http.delete<void>(
      apiUrl(this.baseUrl, `/candidate-career/education/${id}`)
    );
  }
}
