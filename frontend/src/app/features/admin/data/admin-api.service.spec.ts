/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import {
  API_BASE_URL
} from '../../../core/config/api-base-url';
import { AdminApiService } from './admin-api.service';

describe('AdminApiService', () => {
  let service: AdminApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AdminApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: API_BASE_URL,
          useValue: 'https://api.example.test/api'
        }
      ]
    });

    service = TestBed.inject(AdminApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('loads the factual account dashboard', () => {
    service.getDashboard().subscribe();

    const request = http.expectOne(
      'https://api.example.test/api/admin/dashboard'
    );

    expect(request.request.method).toBe('GET');

    request.flush({
      totalUsers: 4,
      activeUsers: 3,
      disabledUsers: 1,
      adminUsers: 1,
      employerUsers: 1,
      jobSeekerUsers: 2
    });
  });

  it('loads only PendingReview company verifications', () => {
    service
      .getPendingCompanyVerifications()
      .subscribe();

    const request = http.expectOne(req =>
      req.url ===
        'https://api.example.test/api/admin/company-verifications' &&
      req.params.get('status') ===
        'PendingReview'
    );

    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('sends server-side user filters and pagination', () => {
    service
      .getUsers({
        q: '  admin@example.test  ',
        role: 'Admin',
        isActive: true,
        page: 2,
        pageSize: 25
      })
      .subscribe();

    const request = http.expectOne(req =>
      req.url ===
        'https://api.example.test/api/admin/users'
    );

    expect(request.request.params.get('q'))
      .toBe('admin@example.test');
    expect(request.request.params.get('role'))
      .toBe('Admin');
    expect(request.request.params.get('isActive'))
      .toBe('true');
    expect(request.request.params.get('page'))
      .toBe('2');
    expect(request.request.params.get('pageSize'))
      .toBe('25');

    request.flush({
      page: 2,
      pageSize: 25,
      totalCount: 0,
      items: []
    });
  });

  it('updates status using only the backend-owned boolean decision', () => {
    service
      .setUserStatus('user id/1', false)
      .subscribe();

    const request = http.expectOne(
      'https://api.example.test/api/admin/users/user%20id%2F1/status'
    );

    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      isActive: false
    });

    request.flush({
      userId: 'user id/1',
      email: 'user@example.test',
      displayName: 'User',
      role: 'JobSeeker',
      isActive: false,
      createdAtUtc: '2026-09-12T00:00:00Z'
    });
  });
});
