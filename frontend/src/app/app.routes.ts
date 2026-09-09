import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'candidate/jobs'
  },
  {
    path: 'candidate/profile',
    loadComponent: () =>
      import('./features/profile/profile.component')
        .then(m => m.ProfileComponent)
  },
  {
    path: 'candidate/resume',
    loadComponent: () =>
      import('./features/resume/resume.component')
        .then(m => m.ResumeComponent)
  },
  {
    path: 'candidate/jobs',
    loadComponent: () =>
      import('./features/jobs/jobs.component')
        .then(m => m.JobsComponent)
  },
  {
    path: 'candidate/applications',
    loadComponent: () =>
      import('./features/applications/applications.component')
        .then(m => m.ApplicationsComponent)
  },
  {
    path: '**',
    redirectTo: 'candidate/jobs'
  }
];
