import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal
} from '@angular/core';
import {
  FormControl,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import {
  API_BASE_URL,
  apiUrl
} from '../../../core/config/api-base-url';

interface ForgotPasswordResponse {
  message: string;
  developmentCode?: string;
}

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './password-recovery.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForgotPasswordComponent {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  readonly emailControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.email]
  });

  readonly isSubmitting = signal(false);
  readonly processed = signal(false);
  readonly statusMessage = signal<string | null>(null);
  readonly developmentCode = signal<string | null>(null);

  get normalizedEmail(): string {
    return this.emailControl.value.trim();
  }

  submit(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.emailControl.setValue(this.normalizedEmail);
    this.emailControl.markAsTouched();

    if (this.emailControl.invalid) {
      return;
    }

    this.isSubmitting.set(true);
    this.processed.set(false);
    this.statusMessage.set(null);
    this.developmentCode.set(null);

    this.http
      .post<ForgotPasswordResponse>(
        apiUrl(this.baseUrl, '/auth/forgot-password'),
        { email: this.normalizedEmail }
      )
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: response => {
          this.processed.set(true);
          this.statusMessage.set(response.message);
          this.developmentCode.set(
            response.developmentCode ?? null
          );
        },
        error: (error: HttpErrorResponse) => {
          this.statusMessage.set(
            this.errorMessage(error)
          );
        }
      });
  }

  private errorMessage(
    error: HttpErrorResponse
  ): string {
    if (error.status === 429) {
      return 'Too many requests. Please wait a moment and try again.';
    }

    if (error.status === 0) {
      return 'We couldn’t reach AptLens. Check your connection and try again.';
    }

    if (error.status >= 500) {
      return 'AptLens is having trouble processing account recovery right now. Please try again.';
    }

    return 'We couldn’t process account recovery right now. Please try again.';
  }
}
