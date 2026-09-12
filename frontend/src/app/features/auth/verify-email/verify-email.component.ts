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
import {
  ActivatedRoute,
  RouterLink
} from '@angular/router';
import { finalize } from 'rxjs';

import {
  API_BASE_URL,
  apiUrl
} from '../../../core/config/api-base-url';

interface VerifyEmailResponse {
  message: string;
}

interface ResendVerificationResponse {
  message: string;
  developmentCode?: string;
}

type StatusKind = 'info' | 'success' | 'error';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VerifyEmailComponent {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly route = inject(ActivatedRoute);

  readonly emailControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.email]
  });

  readonly codeControls = Array.from(
    { length: 6 },
    () =>
      new FormControl('', {
        nonNullable: true,
        validators: [Validators.pattern(/^\d?$/)]
      })
  );

  readonly knownEmail = signal(false);
  readonly isSubmitting = signal(false);
  readonly isResending = signal(false);
  readonly verified = signal(false);
  readonly codeTouched = signal(false);
  readonly statusMessage = signal<string | null>(null);
  readonly statusKind = signal<StatusKind>('info');
  readonly developmentCode = signal<string | null>(null);

  constructor() {
    const email =
      this.route.snapshot.queryParamMap
        .get('email')
        ?.trim() ?? '';

    if (email) {
      this.emailControl.setValue(email);
      this.knownEmail.set(true);
    }
  }

  get normalizedEmail(): string {
    return this.emailControl.value.trim();
  }

  get code(): string {
    return this.codeControls
      .map(control => control.value)
      .join('');
  }

  get hasCompleteCode(): boolean {
    return /^\d{6}$/.test(this.code);
  }

  get maskedEmail(): string {
    const email = this.normalizedEmail;
    const [local, domain] = email.split('@');

    if (!local || !domain) {
      return email;
    }

    const visible =
      local.length <= 2
        ? local.charAt(0)
        : local.slice(0, 2);

    return `${visible}${'*'.repeat(
      Math.max(3, local.length - visible.length)
    )}@${domain}`;
  }

  changeEmail(): void {
    this.knownEmail.set(false);
    this.statusMessage.set(null);
    this.developmentCode.set(null);
  }

  onDigitInput(
    event: Event,
    index: number
  ): void {
    const input = event.target as HTMLInputElement;
    const digit =
      input.value.replace(/\D/g, '').slice(-1);

    this.codeControls[index].setValue(
      digit,
      { emitEvent: false }
    );

    input.value = digit;

    if (digit && index < 5) {
      this.focusRelative(input, index + 1);
    }
  }

  onDigitKeydown(
    event: KeyboardEvent,
    index: number
  ): void {
    if (
      event.key === 'Backspace' &&
      !this.codeControls[index].value &&
      index > 0
    ) {
      this.focusRelative(
        event.target as HTMLInputElement,
        index - 1
      );
    }
  }

  onCodePaste(event: ClipboardEvent): void {
    const digits =
      event.clipboardData
        ?.getData('text')
        .replace(/\D/g, '')
        .slice(0, 6) ?? '';

    if (!digits) {
      return;
    }

    event.preventDefault();

    this.codeControls.forEach((control, index) => {
      control.setValue(digits[index] ?? '');
    });

    const host = event.currentTarget as HTMLElement;
    const inputs =
      host.querySelectorAll<HTMLInputElement>(
        'input[data-code-digit]'
      );

    inputs[Math.min(digits.length, 6) - 1]?.focus();
  }

  submitVerification(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.emailControl.setValue(
      this.normalizedEmail
    );
    this.emailControl.markAsTouched();
    this.codeTouched.set(true);

    if (
      this.emailControl.invalid ||
      !/^\d{6}$/.test(this.code)
    ) {
      return;
    }

    this.isSubmitting.set(true);
    this.statusMessage.set(null);
    this.developmentCode.set(null);

    this.http
      .post<VerifyEmailResponse>(
        apiUrl(this.baseUrl, '/auth/verify-email'),
        {
          email: this.normalizedEmail,
          code: this.code
        }
      )
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: response => {
          this.verified.set(true);
          this.statusKind.set('success');
          this.statusMessage.set(response.message);
        },
        error: (error: HttpErrorResponse) => {
          this.statusKind.set('error');
          this.statusMessage.set(
            this.verifyErrorMessage(error)
          );
        }
      });
  }

  resendVerification(): void {
    if (this.isResending()) {
      return;
    }

    this.emailControl.setValue(
      this.normalizedEmail
    );
    this.emailControl.markAsTouched();

    if (this.emailControl.invalid) {
      return;
    }

    this.isResending.set(true);
    this.statusMessage.set(null);
    this.developmentCode.set(null);

    this.http
      .post<ResendVerificationResponse>(
        apiUrl(
          this.baseUrl,
          '/auth/resend-verification'
        ),
        { email: this.normalizedEmail }
      )
      .pipe(
        finalize(() => {
          this.isResending.set(false);
        })
      )
      .subscribe({
        next: response => {
          this.statusKind.set('info');
          this.statusMessage.set(response.message);
          this.developmentCode.set(
            response.developmentCode ?? null
          );
        },
        error: (error: HttpErrorResponse) => {
          this.statusKind.set('error');
          this.statusMessage.set(
            this.transportErrorMessage(error)
          );
        }
      });
  }

  private focusRelative(
    input: HTMLInputElement,
    targetIndex: number
  ): void {
    const host = input.parentElement;
    const inputs =
      host?.querySelectorAll<HTMLInputElement>(
        'input[data-code-digit]'
      );

    inputs?.[targetIndex]?.focus();
  }

  private verifyErrorMessage(
    error: HttpErrorResponse
  ): string {
    if (error.status === 400) {
      return 'The verification challenge is invalid or expired.';
    }

    return this.transportErrorMessage(error);
  }

  private transportErrorMessage(
    error: HttpErrorResponse
  ): string {
    if (error.status === 429) {
      return 'Too many requests. Please wait a moment and try again.';
    }

    if (error.status === 0) {
      return 'We couldn’t reach AptLens. Check your connection and try again.';
    }

    if (error.status >= 500) {
      return 'AptLens is having trouble processing this request right now. Please try again.';
    }

    return 'We couldn’t complete this request right now. Please try again.';
  }
}
