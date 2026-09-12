import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import {
  EmployerRankedApplicant,
  EmployerVacancy
} from '../data-access/employer.models';

@Component({
  selector: 'app-employer-ranked-applicants',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './employer-ranked-applicants.component.html',
  styleUrl: './employer-ranked-applicants.component.css'
})
export class EmployerRankedApplicantsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(EmployerApiService);

  vacancyId = '';
  vacancy: EmployerVacancy | null = null;
  applicants: EmployerRankedApplicant[] = [];

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.vacancyId =
      this.route.snapshot.paramMap.get('vacancyId') ?? '';

    if (!this.vacancyId) {
      this.loading = false;
      this.errorMessage = 'Vacancy identifier is missing.';
      return;
    }

    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      vacancy: this.api.getVacancy(this.vacancyId),
      applicants: this.api.getVacancyApplications(this.vacancyId)
    }).subscribe({
      next: ({ vacancy, applicants }) => {
        this.vacancy = vacancy;

        // Preserve backend deterministic ranking order.
        this.applicants = applicants;

        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;

        this.errorMessage =
          error.status === 404
            ? 'Vacancy could not be found.'
            : 'Unable to load ranked applicants.';
      }
    });
  }
}