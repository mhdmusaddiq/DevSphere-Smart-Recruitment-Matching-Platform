import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  inject,
  OnInit
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize, forkJoin, switchMap } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import {
  CompanyProfile,
  CompanyProfileRequest,
  EmployerProfile
} from '../data-access/employer.models';

@Component({
  selector: 'app-employer-company',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './employer-company.component.html',
  styleUrl: './employer-company.component.css'
})
export class EmployerCompanyComponent implements OnInit {
  private readonly employerApi = inject(EmployerApiService);
  private readonly formBuilder = inject(FormBuilder);

  private static readonly MAX_EVIDENCE_BYTES = 5 * 1024 * 1024;

  readonly companyForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    website: [''],
    location: ['']
  });

  readonly verificationForm = this.formBuilder.nonNullable.group({
    notes: ['']
  });

  company: CompanyProfile | null = null;
  employerProfile: EmployerProfile | null = null;
  selectedEvidence: File | null = null;

  loading = true;
  savingCompany = false;
  submittingVerification = false;

  errorMessage = '';
  successMessage = '';
  evidenceError = '';

  ngOnInit(): void {
    this.loadCompany();
  }

  loadCompany(): void {
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    forkJoin({
      companies: this.employerApi.getCompanies(),
      profile: this.employerApi.getProfile()
    })
      .pipe(finalize(() => {
        this.loading = false;
      }))
      .subscribe({
        next: ({ companies, profile }) => {
          this.company = companies[0] ?? null;
          this.employerProfile = profile;

          if (this.company) {
            this.companyForm.reset({
              name: this.company.name,
              description: this.company.description ?? '',
              website: this.company.website ?? '',
              location: this.company.location ?? ''
            });
          }
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(
            error,
            'We could not load your company.'
          );
        }
      });
  }

  saveCompany(): void {
    if (this.companyForm.invalid || this.savingCompany) {
      this.companyForm.markAllAsTouched();
      return;
    }

    const value = this.companyForm.getRawValue();

    const request: CompanyProfileRequest = {
      name: value.name.trim(),
      description: this.optionalValue(value.description),
      website: this.optionalValue(value.website),
      location: this.optionalValue(value.location)
    };

    this.savingCompany = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request$ = this.company
      ? this.employerApi.updateCompany(this.company.id, request)
      : this.employerApi.createCompany(request);

    request$
      .pipe(finalize(() => {
        this.savingCompany = false;
      }))
      .subscribe({
        next: (company) => {
          this.company = company;
          this.companyForm.reset({
            name: company.name,
            description: company.description ?? '',
            website: company.website ?? '',
            location: company.location ?? ''
          });
          this.successMessage = 'Company details saved successfully.';
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(
            error,
            'We could not save your company.'
          );
        }
      });
  }

  onEvidenceSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    this.selectedEvidence = null;
    this.evidenceError = '';

    if (!file) {
      return;
    }

    if (file.type !== 'application/pdf') {
      this.evidenceError = 'Verification evidence must be a PDF file.';
      input.value = '';
      return;
    }

    if (file.size > EmployerCompanyComponent.MAX_EVIDENCE_BYTES) {
      this.evidenceError = 'Verification evidence must be 5 MiB or smaller.';
      input.value = '';
      return;
    }

    this.selectedEvidence = file;
  }

  submitVerification(): void {
    if (
      !this.company ||
      !this.selectedEvidence ||
      this.submittingVerification ||
      !this.canSubmitVerification
    ) {
      if (!this.selectedEvidence) {
        this.evidenceError = 'Choose a PDF verification document first.';
      }
      return;
    }

    const companyId = this.company.id;
    const file = this.selectedEvidence;
    const notes = this.verificationForm.controls.notes.value.trim();

    this.submittingVerification = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.evidenceError = '';

    this.employerApi.uploadFile(file)
      .pipe(
        switchMap((storedFile) =>
          this.employerApi.submitCompanyVerification(companyId, {
            evidenceStorageKey: storedFile.storageKey,
            notes
          })
        ),
        finalize(() => {
          this.submittingVerification = false;
        })
      )
      .subscribe({
        next: (result) => {
          if (this.company) {
            this.company = {
              ...this.company,
              verificationStatus: result.status
            };
          }

          this.selectedEvidence = null;
          this.verificationForm.reset();
          this.successMessage =
            'Verification evidence submitted successfully.';
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(
            error,
            'We could not submit your company verification.'
          );
        }
      });
  }

  get canSubmitVerification(): boolean {
    if (!this.company || !this.employerProfile) {
      return false;
    }

    if (this.company.membershipStatus === 'Revoked') {
      return false;
    }

    return !['Verified', 'PendingReview'].includes(
      this.company.verificationStatus
    );
  }

  get verificationGuidance(): string {
    if (!this.company) {
      return 'Create your company profile before submitting verification.';
    }

    if (!this.employerProfile) {
      return 'Complete your employer profile before submitting company verification.';
    }

    if (this.company.membershipStatus === 'Revoked') {
      return 'Verification submission is unavailable for a revoked membership.';
    }

    switch (this.company.verificationStatus) {
      case 'Verified':
        return 'Your company is verified. No new evidence is required.';
      case 'PendingReview':
        return 'Your verification evidence is currently pending review.';
      case 'NeedsMoreInformation':
        return 'Additional evidence can be submitted for review.';
      case 'Rejected':
        return 'You can submit new evidence for another review.';
      default:
        return 'Upload a PDF document to submit your company for verification.';
    }
  }

  private optionalValue(value: string): string | null {
    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
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
      return 'You do not have permission to manage this company.';
    }

    if (error.status === 409) {
      return typeof error.error?.message === 'string'
        ? error.error.message
        : 'This company verification cannot be submitted in its current state.';
    }

    if (typeof error.error?.message === 'string') {
      return error.error.message;
    }

    return fallback;
  }
}
