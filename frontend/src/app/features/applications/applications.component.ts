import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import {
  ApplicationStatusHistory,
  JobApplication
} from '../../core/models/application.model';
import {
  ApplicationsApiService
} from '../../core/services/applications-api.service';

@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './applications.component.html',
  styleUrl: './applications.component.css'
})
export class ApplicationsComponent implements OnInit {
  private readonly api = inject(ApplicationsApiService);

  applications: JobApplication[] = [];
  history: ApplicationStatusHistory[] = [];

  loading = true;
  historyLoading = false;

  errorMessage = '';

  selectedApplication: JobApplication | null = null;

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.loading = true;
    this.errorMessage = '';

    this.api.getMine().subscribe({
      next: applications => {
        this.applications = applications;
        this.loading = false;
      },
      error: error => {
        this.loading = false;

        if (error.status === 401 || error.status === 403) {
          this.errorMessage =
            'Please sign in as a Candidate to view your applications.';
          return;
        }

        this.errorMessage =
          'Unable to load your applications.';
      }
    });
  }

  openHistory(application: JobApplication): void {
    this.selectedApplication = application;
    this.history = [];
    this.historyLoading = true;

    this.api.getHistory(application.id).subscribe({
      next: history => {
        this.history = history;
        this.historyLoading = false;
      },
      error: () => {
        this.historyLoading = false;
        this.errorMessage =
          'Unable to load application history.';
      }
    });
  }

  closeHistory(): void {
    this.selectedApplication = null;
    this.history = [];
  }

  statusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }
}