import { Route, Routes } from '@angular/router';

import { routes } from './app.routes';
import { adminRoutes } from './routes/admin.routes';
import { employerRoutes } from './routes/employer.routes';
import { publicRoutes } from './routes/public.routes';
import { seekerRoutes } from './routes/seeker.routes';

describe('Shared Core routes', () => {
  it('routes root to jobs and unknown paths to 404', () => {
    expect(routes.find(route => route.path === '')?.redirectTo).toBe('jobs');
    expect(routes.find(route => route.path === '**')?.redirectTo).toBe('404');
    expect(routes.some(route => route.path === '403')).toBeTrue();
    expect(routes.some(route => route.path === '404')).toBeTrue();
  });

  it('freezes the canonical route families', () => {
    expect(flatten(publicRoutes)).toEqual(jasmine.arrayContaining([
      '/jobs', '/jobs/:vacancyId', '/login', '/register',
      '/verify-email', '/forgot-password', '/reset-password'
    ]));
    expect(flatten(seekerRoutes)).toEqual(jasmine.arrayContaining([
      '/seeker/dashboard', '/seeker/applications',
      '/seeker/applications/:applicationId', '/seeker/profile',
      '/seeker/cv', '/seeker/contact-requests', '/seeker/notifications'
    ]));
    expect(flatten(employerRoutes)).toEqual(jasmine.arrayContaining([
      '/employer/dashboard', '/employer/profile', '/employer/company',
      '/employer/vacancies', '/employer/vacancies/:vacancyId/policy',
      '/employer/vacancies/:vacancyId/manage',
      '/employer/vacancies/:vacancyId/applicants',
      '/employer/applications/:applicationId',
      '/employer/contact-requests', '/employer/recruitment/:applicationId',
      '/employer/notifications'
    ]));
    expect(flatten(adminRoutes)).toEqual(jasmine.arrayContaining([
      '/admin/dashboard', '/admin/users', '/admin/company-verifications',
      '/admin/company-verifications/:verificationId', '/admin/catalogue',
      '/admin/audit-system'
    ]));
  });

  it('applies all three protected-family guard layers', () => {
    [seekerRoutes[0], employerRoutes[0], adminRoutes[0]].forEach(route => {
      expect(route.canActivate?.length).toBe(3);
    });
  });

  function flatten(source: Routes, prefix = ''): string[] {
    return source.flatMap((route: Route) => {
      const current = route.path ? `${prefix}/${route.path}` : prefix;
      return [current, ...flatten(route.children ?? [], current)];
    });
  }
});
