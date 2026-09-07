import { Routes } from '@angular/router';
import { ProfileComponent } from './features/profile/profile.component';
import { ResumeComponent } from './features/resume/resume.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'profile',
    pathMatch: 'full'
  },
  {
    path: 'profile',
    component: ProfileComponent
  },
  {
    path: 'resume',
    component: ResumeComponent
  },
  {
    path: '**',
    redirectTo: 'profile'
  }
];
