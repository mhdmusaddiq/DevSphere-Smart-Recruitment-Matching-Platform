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

  it('loads PendingReview company verifications', () => {
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

  it('loads verification detail without exposing a storage route', () => {
    service
      .getCompanyVerification('verification 1')
      .subscribe();

    const request = http.expectOne(
      'https://api.example.test/api/admin/company-verifications/verification%201'
    );

    expect(request.request.method).toBe('GET');
    request.flush({
      id: 'verification 1',
      companyId: null,
      companyName: 'Example',
      status: 'PendingReview',
      evidenceStorageKey: 'private/path',
      notes: '',
      submittedAtUtc: '2026-09-12T00:00:00Z',
      reviewedAtUtc: null,
      reviewedByUserId: null
    });
  });

  it('downloads evidence only through the task-scoped protected route', () => {
    service
      .downloadCompanyVerificationEvidence('verification-1')
      .subscribe();

    const request = http.expectOne(
      'https://api.example.test/api/admin/company-verifications/verification-1/evidence'
    );

    expect(request.request.method).toBe('GET');
    expect(request.request.responseType).toBe('blob');
    request.flush(new Blob(['evidence']));
  });

  it('reviews a verification with only the backend decision field', () => {
    service
      .reviewCompanyVerification(
        'verification-1',
        'Verified'
      )
      .subscribe();

    const request = http.expectOne(
      'https://api.example.test/api/admin/company-verifications/verification-1/review'
    );

    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      decision: 'Verified'
    });

    request.flush({
      id: 'verification-1',
      companyId: null,
      companyName: 'Example',
      status: 'Verified',
      evidenceStorageKey: 'private/path',
      notes: '',
      submittedAtUtc: '2026-09-12T00:00:00Z',
      reviewedAtUtc: '2026-09-12T01:00:00Z',
      reviewedByUserId: 'admin-1'
    });
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

  it('loads all skill concepts including inactive records', () => {
    service.getSkillConcepts(true).subscribe();

    const request = http.expectOne(req =>
      req.url ===
        'https://api.example.test/api/admin/catalogue/skills' &&
      req.params.get('includeInactive') === 'true'
    );

    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('updates alias status without inventing extra governance fields', () => {
    service
      .setSkillAliasStatus('alias-1', false)
      .subscribe();

    const request = http.expectOne(
      'https://api.example.test/api/admin/catalogue/aliases/alias-1/status'
    );

    expect(request.request.body).toEqual({
      isActive: false
    });

    request.flush({
      id: 'alias-1',
      skillConceptId: 'skill-1',
      skillConceptName: 'Angular',
      alias: 'AngularJS',
      isActive: false,
      skillConceptIsActive: true
    });
  });
});
