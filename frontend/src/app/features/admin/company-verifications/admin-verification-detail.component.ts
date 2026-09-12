import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { DatePipe } from '@angular/common';
import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

import {
  CompanyMonogramComponent
} from '../../../shared/avatar/company-monogram.component';
import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminCompanyVerification,
  ApiProblem
} from '../data/admin.models';

@Component({
  selector: 'app-admin-verification-detail',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    CompanyMonogramComponent
  ],
  templateUrl: './admin-verification-detail.component.html',
  styleUrl: './admin-verification-detail.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminVerificationDetailComponent
  implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly verification =
    signal<AdminCompanyVerification | null>(null);

  readonly loading = signal(true);
  readonly missing = signal(false);
  readonly errorMessage =
    signal<string | null>(null);

  readonly evidenceLoading = signal(false);
  readonly evidenceError =
    signal<string | null>(null);

  readonly confirmVerify = signal(false);
  readonly reviewing = signal(false);
  readonly reviewMessage =
    signal<string | null>(null);
  readonly reviewError =
    signal<string | null>(null);

  get verificationId(): string {
    return (
      this.route.snapshot.paramMap
        .get('verificationId')
        ?.trim() ?? ''
    );
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    if (!this.verificationId) {
      this.missing.set(true);
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.missing.set(false);
    this.errorMessage.set(null);
    this.reviewError.set(null);
    this.confirmVerify.set(false);

    this.api
      .getCompanyVerification(
        this.verificationId
      )
      .pipe(
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: verification => {
          this.verification.set(verification);
        },
        error: (error: HttpErrorResponse) => {
          this.verification.set(null);

          if (error.status === 404) {
            this.missing.set(true);
            return;
          }

          this.errorMessage.set(
            'AptLens could not load this verification review. Try again.'
          );
        }
      });
  }

  downloadEvidence(): void {
    const current = this.verification();

    if (!current || this.evidenceLoading()) {
      return;
    }

    this.evidenceLoading.set(true);
    this.evidenceError.set(null);

    this.api
      .downloadCompanyVerificationEvidence(
        current.id
      )
      .pipe(
        finalize(() => {
          this.evidenceLoading.set(false);
        })
      )
      .subscribe({
        next: blob => {
          const url = URL.createObjectURL(blob);
          const anchor =
            document.createElement('a');

          anchor.href = url;
          anchor.download =
            `company-verification-${current.id}-evidence`;
          anchor.click();

          URL.revokeObjectURL(url);
        },
        error: (error: HttpErrorResponse) => {
          this.evidenceError.set(
            error.status === 404
              ? 'Protected evidence is unavailable. Do not make a decision until evidence can be reviewed.'
              : 'Evidence download failed. Retry before making a decision.'
          );
        }
      });
  }

  beginVerify(): void {
    if (
      this.verification()?.status !==
      'PendingReview'
    ) {
      return;
    }

    this.confirmVerify.set(true);
    this.reviewError.set(null);
  }

  cancelVerify(): void {
    this.confirmVerify.set(false);
  }

  verify(): void {
    const current = this.verification();

    if (
      !current ||
      current.status !== 'PendingReview' ||
      this.reviewing()
    ) {
      return;
    }

    this.reviewing.set(true);
    this.reviewError.set(null);
    this.reviewMessage.set(null);

    this.api
      .reviewCompanyVerification(
        current.id,
        'Verified'
      )
      .pipe(
        finalize(() => {
          this.reviewing.set(false);
        })
      )
      .subscribe({
        next: reviewed => {
          this.verification.set(reviewed);
          this.confirmVerify.set(false);
          this.reviewMessage.set(
            'Verification reviewed successfully. The server-authored status is shown below.'
          );
        },
        error: (error: HttpErrorResponse) => {
          this.confirmVerify.set(false);

          if (error.status === 404) {
            this.missing.set(true);
            this.verification.set(null);
            return;
          }

          if (error.status === 409) {
            const problem =
              error.error as ApiProblem | null;

            this.reviewError.set(
              problem?.message?.trim() ||
              'This verification is no longer pending. Reload the latest server state.'
            );
            return;
          }

          this.reviewError.set(
            'AptLens could not record this review. Reload the latest state and try again.'
          );
        }
      });
  }

  backToQueue(): void {
    void this.router.navigate([
      '/admin/company-verifications'
    ]);
  }

  statusLabel(status: string): string {
    return status
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/^./, value =>
        value.toUpperCase()
      );
  }
}
