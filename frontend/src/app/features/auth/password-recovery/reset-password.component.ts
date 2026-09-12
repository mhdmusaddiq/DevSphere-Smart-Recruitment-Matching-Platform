import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal
} from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
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

interface ResetPasswordResponse {
  message: string;
}

interface IdentityError {
  code?: string;
  description?: string;
}

const STRONG_PASSWORD =
  /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{15,}$/;

const passwordsMatch: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const password =
    control.get('newPassword')?.value;
  const confirm =
    control.get('confirmPassword')?.value;

  if (!password || !confirm) {
    return null;
  }

  return password === confirm
    ? null
    : { passwordMismatch: true };
};

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './password-recovery.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResetPasswordComponent {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly route = inject(ActivatedRoute);

  readonly form = new FormGroup(
    {
      email: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.email
        ]
      }),
      newPassword: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.pattern(STRONG_PASSWORD)
        ]
      }),
      confirmPassword: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      })
    },
    { validators: passwordsMatch }
  );

  readonly codeControls = Array.from(
    { length: 6 },
    () =>
      new FormControl('', {
        nonNullable: true,
        validators: [Validators.pattern(/^\d?$/)]
      })
  );

  readonly isSubmitting = signal(false);
  readonly completed = signal(false);
  readonly codeTouched = signal(false);
  readonly passwordVisible = signal(false);
  readonly confirmPasswordVisible = signal(false);
  readonly statusMessage = signal<string | null>(null);
  readonly serverErrors = signal<string[]>([]);

  constructor() {
    const email =
      this.route.snapshot.queryParamMap
        .get('email')
        ?.trim() ?? '';

    if (email) {
      this.form.controls.email.setValue(email);
    }
  }

  get code(): string {
    return this.codeControls
      .map(control => control.value)
      .join('');
  }

  get hasCompleteCode(): boolean {
    return /^\d{6}$/.test(this.code);
  }

  get password(): string {
    return this.form.controls.newPassword.value;
  }

  hasMinLength(): boolean {
    return this.password.length >= 15;
  }

  hasUppercase(): boolean {
    return /[A-Z]/.test(this.password);
  }

  hasLowercase(): boolean {
    return /[a-z]/.test(this.password);
  }

  hasNumber(): boolean {
    return /\d/.test(this.password);
  }

  hasSymbol(): boolean {
    return /[^A-Za-z0-9]/.test(this.password);
  }

  togglePassword(
    field: 'newPassword' | 'confirmPassword'
  ): void {
    if (field === 'newPassword') {
      this.passwordVisible.update(value => !value);
      return;
    }

    this.confirmPasswordVisible.update(
      value => !value
    );
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

  submit(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.form.controls.email.setValue(
      this.form.controls.email.value.trim()
    );
    this.form.markAllAsTouched();
    this.codeTouched.set(true);

    if (
      this.form.invalid ||
      !/^\d{6}$/.test(this.code)
    ) {
      return;
    }

    this.isSubmitting.set(true);
    this.statusMessage.set(null);
    this.serverErrors.set([]);

    const value = this.form.getRawValue();

    this.http
      .post<ResetPasswordResponse>(
        apiUrl(this.baseUrl, '/auth/reset-password'),
        {
          email: value.email,
          code: this.code,
          newPassword: value.newPassword
        }
      )
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: response => {
          this.completed.set(true);
          this.statusMessage.set(response.message);
        },
        error: (error: HttpErrorResponse) => {
          this.completed.set(false);
          this.statusMessage.set(
            this.errorMessage(error)
          );
          this.serverErrors.set(
            this.extractServerErrors(error)
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

  private errorMessage(
    error: HttpErrorResponse
  ): string {
    if (
      error.status === 400 &&
      error.error?.message ===
        'The new password does not meet password requirements.'
    ) {
      return 'The new password does not meet password requirements.';
    }

    if (error.status === 400) {
      return 'The password recovery challenge is invalid or expired.';
    }

    if (error.status === 429) {
      return 'Too many requests. Please wait a moment and try again.';
    }

    if (error.status === 0) {
      return 'We couldn’t reach AptLens. Check your connection and try again.';
    }

    if (error.status >= 500) {
      return 'AptLens is having trouble resetting your password right now. Please try again.';
    }

    return 'We couldn’t reset your password right now. Please try again.';
  }

  private extractServerErrors(
    error: HttpErrorResponse
  ): string[] {
    const errors: unknown = error.error?.errors;

    if (!Array.isArray(errors)) {
      return [];
    }

    return errors
      .map(item => {
        if (typeof item === 'string') {
          return item;
        }

        if (
          typeof item === 'object' &&
          item !== null
        ) {
          const description =
            (item as IdentityError).description;

          return typeof description === 'string'
            ? description
            : null;
        }

        return null;
      })
      .filter(
        (item): item is string =>
          typeof item === 'string'
      );
  }
}
