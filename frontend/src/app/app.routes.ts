import { Routes } from '@angular/router';

import { adminRoutes } from './routes/admin.routes';
import { employerRoutes } from './routes/employer.routes';
import { publicRoutes } from './routes/public.routes';
import { seekerRoutes } from './routes/seeker.routes';
import { SystemStatusPageComponent } from './shared/routes/system-status-page.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'jobs' },
  ...publicRoutes,
  ...seekerRoutes,
  ...employerRoutes,
  ...adminRoutes,
  {
    path: '403',
    component: SystemStatusPageComponent,
    data: {
      code: '403',
      title: 'Access restricted',
      message: 'Your signed-in account does not have access to this page.'
    }
  },
  {
    path: '404',
    component: SystemStatusPageComponent,
    data: {
      code: '404',
      title: 'Page not found',
      message: 'The requested page does not exist or may have moved.'
    }
  },
  { path: '**', redirectTo: '404' }
];
