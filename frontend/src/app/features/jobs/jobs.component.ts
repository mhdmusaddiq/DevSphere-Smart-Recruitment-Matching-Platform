import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { SessionService } from '../../core/auth/session.service';
import { CompanyMonogramComponent } from '../../shared/avatar/company-monogram.component';
import { EmptyStateVisualComponent } from '../../shared/states/empty-state-visual.component';
import { ErrorStateComponent } from '../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../shared/states/loading-state.component';
import { SeekerApiService } from '../seeker/data/seeker-api.service';
import { VacancyFilters, VacancyView } from '../seeker/data/seeker.models';

type DiscoveryPreset = 'all' | 'best' | 'newest' | 'remote' | 'onsite' | 'hybrid';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    CompanyMonogramComponent,
    EmptyStateVisualComponent,
    ErrorStateComponent,
    LoadingStateComponent
  ],
  templateUrl: './jobs.component.html',
  styleUrl: './jobs.component.css'
})
export class JobsComponent implements OnInit {
  private readonly api = inject(SeekerApiService);
  private readonly session = inject(SessionService);
  private readonly route = inject(ActivatedRoute);

  readonly pageSize = 8;
  vacancies: VacancyView[] = [];
  loading = true;
  errorMessage = '';
  noticeMessage = '';
  page = 1;
  hasMore = false;
  mobileFiltersOpen = false;
  preset: DiscoveryPreset = 'all';

  filters: VacancyFilters = {
    q: '',
    location: '',
    skill: '',
    workMode: '',
    employmentType: '',
    page: 1,
    pageSize: this.pageSize
  };

  ngOnInit(): void {
    this.filters.q = this.route.snapshot.queryParamMap.get('q') ?? '';
    this.filters.location = this.route.snapshot.queryParamMap.get('location') ?? '';
    this.search(true);
  }

  get isJobSeeker(): boolean {
    const user = this.session.user();
    return user?.role === 'JobSeeker' && user.accountState === 'Active' && user.emailVerified;
  }

  get activeFilters(): Array<{ key: keyof VacancyFilters; label: string }> {
    const chips: Array<{ key: keyof VacancyFilters; label: string }> = [];
    if (this.filters.q) chips.push({ key: 'q', label: `Search: ${this.filters.q}` });
    if (this.filters.location) chips.push({ key: 'location', label: `Location: ${this.filters.location}` });
    if (this.filters.skill) chips.push({ key: 'skill', label: `Skill: ${this.filters.skill}` });
    if (this.filters.workMode) chips.push({ key: 'workMode', label: `Work mode: ${this.filters.workMode}` });
    if (this.filters.employmentType) chips.push({ key: 'employmentType', label: `Type: ${this.filters.employmentType}` });
    return chips;
  }

  selectPreset(preset: DiscoveryPreset): void {
    this.preset = preset;
    this.noticeMessage = '';

    if (preset === 'remote') this.filters.workMode = 'Remote';
    else if (preset === 'onsite') this.filters.workMode = 'On-site';
    else if (preset === 'hybrid') this.filters.workMode = 'Hybrid';
    else this.filters.workMode = '';

    if (preset === 'best' && !this.isJobSeeker) {
      this.vacancies = [];
      this.loading = false;
      this.errorMessage = '';
      this.noticeMessage = 'Sign in with an active Job Seeker account to see server-ranked Best Match results.';
      return;
    }

    this.search(true);
  }

  search(resetPage = true): void {
    if (resetPage) {
      this.page = 1;
    }

    this.loading = true;
    this.errorMessage = '';
    this.noticeMessage = '';

    const bestMatch = this.preset === 'best';
    this.api.getVacancies({
      ...this.filters,
      q: this.filters.q?.trim(),
      location: this.filters.location?.trim(),
      skill: this.filters.skill?.trim(),
      page: this.page,
      pageSize: this.pageSize
    }, bestMatch).subscribe({
      next: rows => {
        this.vacancies = rows;
        this.hasMore = rows.length === this.pageSize;
        this.loading = false;
        this.mobileFiltersOpen = false;
      },
      error: (error: HttpErrorResponse) => {
        if (bestMatch && (error.status === 401 || error.status === 403)) {
          this.vacancies = [];
          this.errorMessage = error.status === 401
            ? 'Sign in to see your Best Match results.'
            : 'Your current account state cannot use personalized matching yet.';
          this.loading = false;
          return;
        }

        if (bestMatch && error.status >= 500) {
          this.preset = 'all';
          this.noticeMessage = 'Best Match is temporarily unavailable. Showing public published roles instead.';
          this.loadPublicFallback();
          return;
        }

        this.errorMessage = 'Published jobs could not be loaded. Please try again.';
        this.loading = false;
      }
    });
  }

  previousPage(): void {
    if (this.page <= 1 || this.loading) return;
    this.page -= 1;
    this.search(false);
  }

  nextPage(): void {
    if (!this.hasMore || this.loading) return;
    this.page += 1;
    this.search(false);
  }

  loadMore(): void {
    if (!this.hasMore || this.loading) return;

    const nextPage = this.page + 1;
    this.loading = true;
    this.errorMessage = '';

    this.api.getVacancies({
      ...this.filters,
      page: nextPage,
      pageSize: this.pageSize
    }, this.preset === 'best').subscribe({
      next: rows => {
        this.vacancies = [...this.vacancies, ...rows];
        this.page = nextPage;
        this.hasMore = rows.length === this.pageSize;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'More jobs could not be loaded.';
        this.loading = false;
      }
    });
  }

  removeFilter(key: keyof VacancyFilters): void {
    if (key === 'q' || key === 'location' || key === 'skill' || key === 'workMode' || key === 'employmentType') {
      this.filters[key] = '';
    }
    if (key === 'workMode') this.preset = 'all';
    this.search(true);
  }

  clearFilters(): void {
    this.filters = {
      q: '', location: '', skill: '', workMode: '', employmentType: '',
      page: 1, pageSize: this.pageSize
    };
    this.preset = 'all';
    this.search(true);
  }

  isCalculated(vacancy: VacancyView): boolean {
    return vacancy.assessmentStatus === 'Calculated'
      && vacancy.displayCompatibility !== null;
  }

  experienceLabel(months: number): string {
    if (months < 12) return `${months} month${months === 1 ? '' : 's'}`;
    const years = Math.floor(months / 12);
    const rest = months % 12;
    return rest ? `${years}y ${rest}m` : `${years} year${years === 1 ? '' : 's'}`;
  }

  salaryLabel(vacancy: VacancyView): string {
    const min = vacancy.salaryMin;
    const max = vacancy.salaryMax;
    if (min === null && max === null) return '';
    const format = (value: number) => new Intl.NumberFormat('en-LK', {
      style: 'currency', currency: 'LKR', maximumFractionDigits: 0
    }).format(value);
    if (min !== null && max !== null) return `${format(min)}–${format(max)}`;
    return format(min ?? max ?? 0);
  }

  private loadPublicFallback(): void {
    this.api.getVacancies({
      ...this.filters,
      page: 1,
      pageSize: this.pageSize
    }, false).subscribe({
      next: rows => {
        this.page = 1;
        this.vacancies = rows;
        this.hasMore = rows.length === this.pageSize;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Published jobs could not be loaded.';
        this.loading = false;
      }
    });
  }
}