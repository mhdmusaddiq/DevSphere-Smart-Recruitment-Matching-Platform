import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { Vacancy } from '../../core/models/vacancy.model';
import { MatchResult } from '../../core/models/match-result.model';
import { JobsApiService } from '../../core/services/jobs-api.service';
import { ApplicationsApiService } from '../../core/services/applications-api.service';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './jobs.component.html',
  styleUrl: './jobs.component.css'
})
export class JobsComponent implements OnInit {
  private readonly jobsApi = inject(JobsApiService);
  private readonly applicationsApi = inject(ApplicationsApiService);

  vacancies: Vacancy[] = [];
  searchTerm = '';

  loading = true;
  errorMessage = '';

  selectedVacancy: Vacancy | null = null;
  selectedMatch: MatchResult | null = null;

  matchLoadingId: string | null = null;

  ngOnInit(): void {
    this.loadVacancies();
  }

  get filteredVacancies(): Vacancy[] {
    const term = this.searchTerm.trim().toLowerCase();

    if (!term) {
      return this.vacancies;
    }

    return this.vacancies.filter(vacancy =>
      vacancy.title.toLowerCase().includes(term) ||
      vacancy.location.toLowerCase().includes(term) ||
      vacancy.description.toLowerCase().includes(term)
    );
  }

  loadVacancies(): void {
    this.loading = true;
    this.errorMessage = '';

    this.jobsApi.getVacancies().subscribe({
      next: vacancies => {
        this.vacancies = vacancies;
        this.loading = false;
      },
      error: () => {
        this.errorMessage =
          'Unable to load vacancies. Please make sure the API is running.';
        this.loading = false;
      }
    });
  }

  checkMatch(vacancy: Vacancy): void {
    this.selectedVacancy = vacancy;
    this.selectedMatch = null;
    this.matchLoadingId = vacancy.id;
    this.errorMessage = '';

    this.jobsApi.getMatch(vacancy.id).subscribe({
      next: result => {
        this.selectedMatch = result;
        this.matchLoadingId = null;
      },
      error: error => {
        this.matchLoadingId = null;

        if (error.status === 401 || error.status === 403) {
          this.errorMessage =
            'Please sign in as a Job Seeker to calculate your match.';
          return;
        }

        if (error.status === 404) {
          this.errorMessage =
            'Candidate profile or vacancy information was not found.';
          return;
        }

        this.errorMessage =
          'Unable to calculate the match right now.';
      }
    });
  }


  apply(vacancy: Vacancy): void {
    this.errorMessage = '';

    this.applicationsApi.apply(vacancy.id).subscribe({
      next: () => {
        this.errorMessage =
          'Application submitted successfully.';
      },
      error: error => {
        if (error.status === 409) {
          this.errorMessage =
            'You have already applied for this vacancy.';
          return;
        }

        if (error.status === 401 || error.status === 403) {
          this.errorMessage =
            'Please sign in as a Candidate to apply.';
          return;
        }

        this.errorMessage =
          'Unable to submit the application.';
      }
    });
  }
  closeMatch(): void {
    this.selectedVacancy = null;
    this.selectedMatch = null;
  }

  experienceLabel(months: number): string {
    if (months < 12) {
      return `${months} month${months === 1 ? '' : 's'}`;
    }

    const years = Math.floor(months / 12);
    const remainingMonths = months % 12;

    if (remainingMonths === 0) {
      return `${years} year${years === 1 ? '' : 's'}`;
    }

    return `${years}y ${remainingMonths}m`;
  }

  score(value: number): number {
    return Math.round(value);
  }
}
