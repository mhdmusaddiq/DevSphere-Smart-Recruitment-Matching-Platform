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
  Router,
  RouterLink
} from '@angular/router';
import { finalize } from 'rxjs';

import {
  API_BASE_URL,
  apiUrl
} from '../../../core/config/api-base-url';
import {
  BrandWordmarkComponent
} from '../../../shared/brand/brand-wordmark.component';

type RegistrationRole = 'JobSeeker' | 'Employer';

interface RegisterResponse {
  email: string;
  displayName: string;
  accountState: string;
  emailVerificationRequired: boolean;
  developmentCode?: string;
  message?: string;
}

const STRONG_PASSWORD =
  /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{15,}$/;

const passwordsMatchValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const password = control.get('password')?.value;
  const confirmPassword =
    control.get('confirmPassword')?.value;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword
    ? null
    : { passwordMismatch: true };
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BrandWordmarkComponent
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterComponent {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly router = inject(Router);

  readonly isSubmitting = signal(false);
  readonly passwordVisible = signal(false);
  readonly confirmPasswordVisible = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly form = new FormGroup(
    {
      role: new FormControl<RegistrationRole>(
        'JobSeeker',
        {
          nonNullable: true,
          validators: [Validators.required]
        }
      ),

      displayName: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      }),

      email: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.email
        ]
      }),

      password: new FormControl('', {
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
    {
      validators: passwordsMatchValidator
    }
  );

  selectRole(role: RegistrationRole): void {
    this.form.controls.role.setValue(role);
    this.form.controls.role.markAsTouched();
  }

  togglePasswordVisibility(
    field: 'password' | 'confirmPassword'
  ): void {
    if (field === 'password') {
      this.passwordVisible.update(value => !value);
      return;
    }

    this.confirmPasswordVisible.update(
      value => !value
    );
  }

  submit(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.form.controls.displayName.setValue(
      this.form.controls.displayName.value.trim()
    );

    this.form.controls.email.setValue(
      this.form.controls.email.value.trim()
    );

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitError.set(null);
    this.isSubmitting.set(true);

    const value = this.form.getRawValue();

    this.http
      .post<RegisterResponse>(
        apiUrl(this.baseUrl, '/auth/register'),
        {
          displayName: value.displayName.trim(),
          email: value.email.trim(),
          password: value.password,
          role: value.role
        }
      )
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: response => {
          void this.router.navigate(
            ['/verify-email'],
            {
              queryParams: {
                email: response.email
              }
            }
          );
        },

        error: (error: HttpErrorResponse) => {
          this.submitError.set(
            this.getErrorMessage(error)
          );
        }
      });
  }

  private getErrorMessage(
    error: HttpErrorResponse
  ): string {
    if (
      error.status === 400 &&
      error.error?.message === 'Email already exists.'
    ) {
      return 'An account with this email already exists. Sign in or use account recovery.';
    }

    if (error.status === 400) {
      return 'We couldn’t create your account. Check your details and password requirements, then try again.';
    }

    if (error.status === 429) {
      return 'Too many registration attempts. Please wait a moment and try again.';
    }

    if (error.status === 0) {
      return 'We couldn’t reach AptLens. Check your connection and try again.';
    }

    if (error.status >= 500) {
      return 'AptLens is having trouble creating your account right now. Please try again.';
    }

    return 'We couldn’t create your account right now. Please try again.';
  }
}
