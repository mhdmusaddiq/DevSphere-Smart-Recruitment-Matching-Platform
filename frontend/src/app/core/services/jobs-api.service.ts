import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Vacancy } from '../models/vacancy.model';
import { MatchResult } from '../models/match-result.model';

@Injectable({
  providedIn: 'root'
})
export class JobsApiService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = 'https://localhost:7097/api';

  getVacancies(): Observable<Vacancy[]> {
    return this.http.get<Vacancy[]>(
      `${this.apiUrl}/vacancies`
    );
  }

  getMatch(vacancyId: string): Observable<MatchResult> {
    return this.http.get<MatchResult>(
      `${this.apiUrl}/matching/vacancies/${vacancyId}`
    );
  }
}