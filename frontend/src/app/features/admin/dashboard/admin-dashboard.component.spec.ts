/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import {
  of,
  throwError
} from 'rxjs';

import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminDashboardComponent
} from './admin-dashboard.component';

describe('AdminDashboardComponent', () => {
  let api: jasmine.SpyObj<AdminApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<AdminApiService>(
      'AdminApiService',
      [
        'getDashboard',
        'getPendingCompanyVerifications',
        'getAuditEvents'
      ]
    );

    api.getDashboard.and.returnValue(
      of({
        totalUsers: 8,
        activeUsers: 7,
        disabledUsers: 1,
        adminUsers: 1,
        employerUsers: 2,
        jobSeekerUsers: 5
      })
    );
    api.getPendingCompanyVerifications
      .and.returnValue(of([]));
    api.getAuditEvents.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [AdminDashboardComponent],
      providers: [
        provideRouter([]),
        {
          provide: AdminApiService,
          useValue: api
        }
      ]
    }).compileComponents();
  });

  it('loads dashboard, PendingReview queue and five audit events', () => {
    const fixture =
      TestBed.createComponent(AdminDashboardComponent);

    fixture.detectChanges();

    expect(api.getDashboard).toHaveBeenCalled();
    expect(
      api.getPendingCompanyVerifications
    ).toHaveBeenCalled();
    expect(api.getAuditEvents)
      .toHaveBeenCalledOnceWith(5);

    expect(fixture.componentInstance.metrics()?.totalUsers)
      .toBe(8);
  });

  it('renders a recoverable error when the overview request fails', () => {
    api.getDashboard.and.returnValue(
      throwError(() => new Error('network'))
    );

    const fixture =
      TestBed.createComponent(AdminDashboardComponent);

    fixture.detectChanges();

    expect(
      fixture.componentInstance.errorMessage()
    ).toContain('could not load');
    expect(fixture.componentInstance.loading())
      .toBeFalse();
  });
});
