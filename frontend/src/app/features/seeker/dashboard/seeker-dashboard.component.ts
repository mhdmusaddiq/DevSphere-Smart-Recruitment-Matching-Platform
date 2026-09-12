import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable, catchError, forkJoin, map, of } from 'rxjs';

import { SessionService } from '../../../core/auth/session.service';
import { CompanyMonogramComponent } from '../../../shared/avatar/company-monogram.component';
import { ErrorStateComponent } from '../../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../../shared/states/loading-state.component';
import {
  ApplicationReadiness,
  CandidateDashboardSummary,
  CandidateProfileView,
  ContactRequestView,
  JobApplicationView,
  NotificationView,
  VacancyView
} from '../data/seeker.models';
import { SeekerApiService } from '../data/seeker-api.service';

interface LoadResult<T> {
  data: T;
  failed: boolean;
}

interface NextStep {
  eyebrow: string;
  title: string;
  description: string;
  action: string;
  route: string;
}

@Component({
  selector: 'app-seeker-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    CompanyMonogramComponent,
    ErrorStateComponent,
    LoadingStateComponent
  ],
  templateUrl: './seeker-dashboard.component.html',
  styleUrl: './seeker-dashboard.component.css'
})
export class SeekerDashboardComponent implements OnInit {
  private readonly api = inject(SeekerApiService);
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);

  loading = true;
  dashboardFailed = false;
  rolesFailed = false;
  applicationsFailed = false;
  notificationsFailed = false;
  contactsFailed = false;

  summary: CandidateDashboardSummary = {
    candidateId: '',
    applicationCount: 0,
    unreadNotificationCount: 0,
    pendingContactRequestCount: 0
  };
  readiness: ApplicationReadiness | null = null;
  profile: CandidateProfileView | null = null;
  openRoles: VacancyView[] = [];
  recentApplications: JobApplicationView[] = [];
  notifications: NotificationView[] = [];
  pendingContact: ContactRequestView | null = null;

  searchQuery = '';
  searchLocation = '';

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;

    forkJoin({
      summary: this.safe(
        this.api.getDashboardSummary(),
        this.summary
      ),
      readiness: this.safe<ApplicationReadiness | null>(
        this.api.getApplicationReadiness(),
        null
      ),
      profile: this.safe<CandidateProfileView | null>(
        this.api.getCandidateProfile(),
        null
      ),
      roles: this.safe(
        this.api.getVacancies({ page: 1, pageSize: 4 }),
        [] as VacancyView[]
      ),
      applications: this.safe(
        this.api.getApplications(),
        [] as JobApplicationView[]
      ),
      contacts: this.safe(
        this.api.getContactRequests(),
        [] as ContactRequestView[]
      ),
      notifications: this.safe(
        this.api.getNotifications(),
        [] as NotificationView[]
      )
    }).subscribe(result => {
      this.summary = result.summary.data;
      this.readiness = result.readiness.data;
      this.profile = result.profile.data;
      this.openRoles = result.roles.data.slice(0, 4);
      this.recentApplications = [...result.applications.data]
        .sort((a, b) => b.submittedAtUtc.localeCompare(a.submittedAtUtc))
        .slice(0, 4);
      this.pendingContact = result.contacts.data.find(
        request => request.status.toLowerCase() === 'pending'
      ) ?? null;
      this.notifications = [...result.notifications.data]
        .sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc))
        .slice(0, 4);

      this.dashboardFailed = result.summary.failed;
      this.rolesFailed = result.roles.failed;
      this.applicationsFailed = result.applications.failed;
      this.contactsFailed = result.contacts.failed;
      this.notificationsFailed = result.notifications.failed;
      this.loading = false;
    });
  }

  findJobs(): void {
    const queryParams: Record<string, string> = {};
    const q = this.searchQuery.trim();
    const location = this.searchLocation.trim();

    if (q) {
      queryParams['q'] = q;
    }

    if (location) {
      queryParams['location'] = location;
    }

    void this.router.navigate(['/jobs'], { queryParams });
  }

  get displayName(): string {
    return this.profile?.fullName?.trim()
      || this.session.user()?.displayName?.trim()
      || 'Job Seeker';
  }

  get greeting(): string {
    const hour = new Date().getHours();

    if (hour < 12) {
      return 'Good morning';
    }

    if (hour < 18) {
      return 'Good afternoon';
    }

    return 'Good evening';
  }

  get nextStep(): NextStep {
    if (this.readiness && !this.readiness.profileReady) {
      return {
        eyebrow: 'YOUR NEXT STEP',
        title: 'Complete your career profile',
        description: 'Add the factual profile details required before an application can be ready.',
        action: 'Open profile',
        route: '/seeker/profile'
      };
    }

    if (this.readiness && !this.readiness.resumeReady) {
      return {
        eyebrow: 'YOUR NEXT STEP',
        title: 'Add your current CV',
        description: 'Upload or select the ResumeVersion you want the server to use for future applications.',
        action: 'Upload CV',
        route: '/seeker/cv'
      };
    }

    if (this.summary.pendingContactRequestCount > 0) {
      return {
        eyebrow: 'YOUR NEXT STEP',
        title: 'Review a contact request',
        description: 'An employer is waiting for your decision before contact details can be disclosed.',
        action: 'Review request',
        route: '/seeker/contact-requests'
      };
    }

    if (this.summary.unreadNotificationCount > 0) {
      return {
        eyebrow: 'YOUR NEXT STEP',
        title: 'Read your latest updates',
        description: 'Review unread recruitment notifications from the platform.',
        action: 'View updates',
        route: '/seeker/notifications'
      };
    }

    return {
      eyebrow: 'YOUR NEXT STEP',
      title: 'Explore published roles',
      description: 'Your current readiness signals do not require an immediate action.',
      action: 'Find jobs',
      route: '/jobs'
    };
  }

  isCalculated(application: JobApplicationView): boolean {
    return application.frozenAssessmentStatus === 'Calculated'
      && application.displayCompatibility !== null;
  }

  experienceLabel(months: number): string {
    if (months < 12) {
      return `${months} month${months === 1 ? '' : 's'}`;
    }

    const years = Math.floor(months / 12);
    const rest = months % 12;
    return rest === 0 ? `${years} year${years === 1 ? '' : 's'}` : `${years}y ${rest}m`;
  }

  salaryLabel(vacancy: VacancyView): string {
    if (vacancy.salaryMin === null && vacancy.salaryMax === null) {
      return 'Salary not listed';
    }

    const format = (value: number) => new Intl.NumberFormat('en-LK', {
      style: 'currency',
      currency: 'LKR',
      maximumFractionDigits: 0
    }).format(value);

    if (vacancy.salaryMin !== null && vacancy.salaryMax !== null) {
      return `${format(vacancy.salaryMin)}–${format(vacancy.salaryMax)}`;
    }

    return format(vacancy.salaryMin ?? vacancy.salaryMax ?? 0);
  }

  private safe<T>(source: Observable<T>, fallback: T): Observable<LoadResult<T>> {
    return source.pipe(
      map(data => ({ data, failed: false })),
      catchError(() => of({ data: fallback, failed: true }))
    );
  }
}