import { Routes } from '@angular/router';

import { guestOnlyGuard } from '../core/guards/guest-only.guard';

export const publicRoutes: Routes = [
  { path: 'jobs', loadComponent: () => import('../features/jobs/jobs.component').then(component => component.JobsComponent) },
  {
    path: 'jobs/:vacancyId',
    loadComponent: () =>
      import('../features/jobs/job-detail/job-detail.component')
        .then(component => component.JobDetailComponent),
    data: { pageId: 'S03', pageName: 'Job details' }
  },
 {
  path: 'login',
  loadComponent: () =>
    import('../features/auth/sign-in/sign-in.component')
      .then(component => component.SignInComponent),
  canActivate: [guestOnlyGuard],
  data: {
    pageId: 'A02',
    pageName: 'Sign in'
  }
},
{
  path: 'register',
  loadComponent: () =>
    import('../features/auth/register/register.component')
      .then(component => component.RegisterComponent),
  canActivate: [guestOnlyGuard],
  data: {
    pageId: 'A03',
    pageName: 'Create account'
  }
},
  {
    path: 'verify-email',
    loadComponent: () =>
      import('../features/auth/verify-email/verify-email.component')
        .then(component => component.VerifyEmailComponent),
    data: { pageId: 'A04', pageName: 'Verify email' }
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('../features/auth/password-recovery/forgot-password.component')
        .then(component => component.ForgotPasswordComponent),
    data: { pageId: 'A05', pageName: 'Forgot password' }
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('../features/auth/password-recovery/reset-password.component')
        .then(component => component.ResetPasswordComponent),
    data: { pageId: 'A05', pageName: 'Reset password' }
  }
];
