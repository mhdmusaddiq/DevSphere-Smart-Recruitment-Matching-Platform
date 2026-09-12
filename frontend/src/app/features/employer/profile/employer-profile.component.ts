import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize } from 'rxjs';

import { SessionService } from '../../../core/auth/session.service';
import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerProfile } from '../data-access/employer.models';

@Component({
  selector: 'app-employer-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './employer-profile.component.html',
  styleUrl: './employer-profile.component.css'
})
export class EmployerProfileComponent implements OnInit {
  private readonly employerApi = inject(EmployerApiService);
  readonly session = inject(SessionService);
  private readonly formBuilder = inject(FormBuilder);

  readonly profileForm = this.formBuilder.nonNullable.group({
    companyName: ['', [Validators.required, Validators.maxLength(200)]],
    contactEmail: ['', [Validators.required, Validators.email]],
    website: [''],
    location: ['']
  });

  loading = true;
  submitting = false;
  profileExists = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.employerApi.getProfile()
      .pipe(finalize(() => {
        this.loading = false;
      }))
      .subscribe({
        next: (profile) => {
          this.profileExists = profile !== null;

          if (profile) {
            this.profileForm.reset(profile);
          }
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(
            error,
            'We could not load your employer profile.'
          );
        }
      });
  }

  submit(): void {
    if (this.profileForm.invalid || this.submitting) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const profile: EmployerProfile = this.profileForm.getRawValue();

    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request$ = this.profileExists
      ? this.employerApi.updateProfile(profile)
      : this.employerApi.createProfile(profile);

    request$
      .pipe(finalize(() => {
        this.submitting = false;
      }))
      .subscribe({
        next: (savedProfile) => {
          this.profileExists = true;
          this.profileForm.reset(savedProfile);
          this.successMessage = 'Employer profile saved successfully.';
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(
            error,
            'We could not save your employer profile.'
          );
        }
      });
  }

  private getErrorMessage(
    error: HttpErrorResponse,
    fallback: string
  ): string {
    if (error.status === 0) {
      return 'Unable to reach the server. Please try again.';
    }

    if (error.status === 401) {
      return 'Your session is no longer valid. Please sign in again.';
    }

    if (error.status === 403) {
      return 'You do not have permission to manage this employer profile.';
    }

    return fallback;
  }
}
