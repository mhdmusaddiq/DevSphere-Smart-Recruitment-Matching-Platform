import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CandidateProfile,
  CandidateSkill,
  EducationRecord,
  WorkExperience
} from '../models/candidate-profile.model';

@Injectable({ providedIn: 'root' })
export class CandidateProfileApiService {
  private readonly api = 'https://localhost:7097/api';

  constructor(private readonly http: HttpClient) {}

  getProfile(): Observable<CandidateProfile | null> {
    return this.http.get<CandidateProfile | null>(
      `${this.api}/profile/candidate`
    );
  }

  createProfile(profile: CandidateProfile): Observable<CandidateProfile> {
    return this.http.post<CandidateProfile>(
      `${this.api}/profile/candidate`,
      profile
    );
  }

  updateProfile(profile: CandidateProfile): Observable<CandidateProfile> {
    return this.http.put<CandidateProfile>(
      `${this.api}/profile/candidate`,
      profile
    );
  }

  getSkills(): Observable<CandidateSkill[]> {
    return this.http.get<CandidateSkill[]>(
      `${this.api}/profile/candidate/skills`
    );
  }

  addSkill(name: string): Observable<CandidateSkill> {
    return this.http.post<CandidateSkill>(
      `${this.api}/profile/candidate/skills`,
      { name }
    );
  }

  deleteSkill(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.api}/profile/candidate/skills/${id}`
    );
  }

  getWorkExperiences(): Observable<WorkExperience[]> {
    return this.http.get<WorkExperience[]>(
      `${this.api}/candidate-career/work-experiences`
    );
  }

  addWorkExperience(
    item: Partial<WorkExperience>
  ): Observable<WorkExperience> {
    return this.http.post<WorkExperience>(
      `${this.api}/candidate-career/work-experiences`,
      item
    );
  }

  updateWorkExperience(
    id: string,
    item: Partial<WorkExperience>
  ): Observable<WorkExperience> {
    return this.http.put<WorkExperience>(
      `${this.api}/candidate-career/work-experiences/${id}`,
      item
    );
  }

  deleteWorkExperience(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.api}/candidate-career/work-experiences/${id}`
    );
  }

  getEducation(): Observable<EducationRecord[]> {
    return this.http.get<EducationRecord[]>(
      `${this.api}/candidate-career/education`
    );
  }

  addEducation(
    item: Partial<EducationRecord>
  ): Observable<EducationRecord> {
    return this.http.post<EducationRecord>(
      `${this.api}/candidate-career/education`,
      item
    );
  }

  updateEducation(
    id: string,
    item: Partial<EducationRecord>
  ): Observable<EducationRecord> {
    return this.http.put<EducationRecord>(
      `${this.api}/candidate-career/education/${id}`,
      item
    );
  }

  deleteEducation(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.api}/candidate-career/education/${id}`
    );
  }
}
