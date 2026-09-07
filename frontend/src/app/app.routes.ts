import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'jobs'
  },
  {
    path: 'jobs',
    loadComponent: () =>
      import('./features/jobs/jobs.component')
        .then(m => m.JobsComponent)
  },
  {
    path: '**',
    redirectTo: 'jobs'
  }
];