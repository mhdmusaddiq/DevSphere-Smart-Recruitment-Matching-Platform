import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Vacancy } from '../models/vacancy.model';
import { MatchResult } from '../models/match-result.model';
import { API_BASE_URL, apiUrl } from '../config/api-base-url';

@Injectable({
  providedIn: 'root'
})
export class JobsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getVacancies(): Observable<Vacancy[]> {
    return this.http.get<Vacancy[]>(
      apiUrl(this.baseUrl, '/vacancies')
    );
  }

  getMatch(vacancyId: string): Observable<MatchResult> {
    return this.http.get<MatchResult>(
      apiUrl(this.baseUrl, `/matching/vacancies/${vacancyId}`)
    );
  }
}
