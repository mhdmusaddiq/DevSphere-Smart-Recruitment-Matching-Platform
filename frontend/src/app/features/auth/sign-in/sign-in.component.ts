import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal
} from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';
import { finalize } from 'rxjs';

import {
  PublicRole
} from '../../../core/auth/auth.models';
import {
  AuthService
} from '../../../core/auth/auth.service';
import {
  safeInternalReturnUrl
} from '../../../core/auth/safe-return-url';
import {
  SessionService
} from '../../../core/auth/session.service';
import {
  BrandWordmarkComponent
} from '../../../shared/brand/brand-wordmark.component';

const ROLE_HOME: Record<PublicRole, string> = {
  JobSeeker: '/seeker/dashboard',
  Employer: '/employer/dashboard',
  Admin: '/admin/dashboard'
};

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BrandWordmarkComponent
  ],
  templateUrl: './sign-in.component.html',
  styleUrl: './sign-in.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SignInComponent {
  private readonly authService = inject(AuthService);
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isSubmitting = signal(false);
  readonly passwordVisible = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly form = new FormGroup({
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
        Validators.required
      ]
    })
  });

  togglePasswordVisibility(): void {
    this.passwordVisible.update(value => !value);
  }

  submit(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.form.controls.email.setValue(
      this.form.controls.email.value.trim()
    );

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitError.set(null);
    this.isSubmitting.set(true);

    const formValue = this.form.getRawValue();

    this.authService
      .login({
        email: formValue.email.trim(),
        password: formValue.password
      })
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({
        next: user => {
          if (user.accountState === 'PendingEmailVerification') {
            this.session.clear();

            void this.router.navigate(
              ['/verify-email']
            );

            return;
          }

          if (user.accountState === 'Disabled') {
            this.session.clear();

            this.submitError.set(
              'We couldn’t sign you in with those details. Check your email and password, or use account recovery.'
            );

            return;
          }

          const requestedReturnUrl =
            this.route.snapshot.queryParamMap.get('returnUrl');

          const safeReturnUrl =
            safeInternalReturnUrl(requestedReturnUrl);

          const destination =
            requestedReturnUrl &&
            safeReturnUrl === requestedReturnUrl
              ? safeReturnUrl
              : ROLE_HOME[user.role];

          void this.router.navigateByUrl(destination);
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
    if (error.status === 401) {
      return 'We couldn’t sign you in with those details. Check your email and password, or use account recovery.';
    }

    if (error.status === 429) {
      return 'Too many sign-in attempts. Please wait a moment and try again.';
    }

    if (error.status === 0) {
      return 'We couldn’t reach AptLens. Check your connection and try again.';
    }

    if (error.status >= 500) {
      return 'AptLens is having trouble signing you in right now. Please try again.';
    }

    return 'We couldn’t sign you in right now. Please try again.';
  }
}
