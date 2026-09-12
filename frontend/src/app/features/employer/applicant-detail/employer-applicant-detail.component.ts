import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerRankedApplicant } from '../data-access/employer.models';

@Component({
  selector: 'app-employer-applicant-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './employer-applicant-detail.component.html',
  styleUrl: './employer-applicant-detail.component.css'
})
export class EmployerApplicantDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(EmployerApiService);

  applicationId = '';
  vacancyId = '';

  applicant: EmployerRankedApplicant | null = null;

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.applicationId =
      this.route.snapshot.paramMap.get('applicationId') ?? '';

    this.vacancyId =
      this.route.snapshot.queryParamMap.get('vacancyId') ?? '';

    if (!this.applicationId) {
      this.loading = false;
      this.errorMessage = 'Application identifier is missing.';
      return;
    }

    if (!this.vacancyId) {
      this.loading = false;
      this.errorMessage =
        'Vacancy context is required to load this applicant safely.';
      return;
    }

    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    this.api.getVacancyApplications(this.vacancyId).subscribe({
      next: applicants => {
        this.applicant =
          applicants.find(
            item => item.applicationId === this.applicationId
          ) ?? null;

        this.loading = false;

        if (!this.applicant) {
          this.errorMessage =
            'Applicant could not be found for this vacancy.';
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;

        this.errorMessage =
          error.status === 403
            ? 'You are not authorized to review this vacancy.'
            : 'Unable to load applicant detail.';
      }
    });
  }
}