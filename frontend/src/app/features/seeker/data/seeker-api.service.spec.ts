/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../../../core/config/api-base-url';
import { SeekerApiService } from './seeker-api.service';

describe('SeekerApiService Batch A contracts', () => {
  let service: SeekerApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        SeekerApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://example.test/api' }
      ]
    });

    service = TestBed.inject(SeekerApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads exactly the candidate dashboard endpoint', () => {
    service.getDashboardSummary().subscribe();
    const request = http.expectOne('https://example.test/api/dashboard/candidate');
    expect(request.request.method).toBe('GET');
    request.flush({});
  });

  it('sends S02 public filters to the server', () => {
    service.getVacancies({
      q: 'Angular',
      location: 'Colombo',
      skill: 'TypeScript',
      workMode: 'Hybrid',
      employmentType: 'Full-time',
      page: 2,
      pageSize: 8
    }).subscribe();

    const request = http.expectOne(req =>
      req.url === 'https://example.test/api/vacancies'
      && req.params.get('q') === 'Angular'
      && req.params.get('location') === 'Colombo'
      && req.params.get('skill') === 'TypeScript'
      && req.params.get('workMode') === 'Hybrid'
      && req.params.get('employmentType') === 'Full-time'
      && req.params.get('page') === '2'
      && req.params.get('pageSize') === '8'
    );

    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('uses the server-owned Best Match endpoint', () => {
    service.getVacancies({ page: 1, pageSize: 8 }, true).subscribe();
    const request = http.expectOne(req => req.url === 'https://example.test/api/vacancies/best-match');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('uses the S03 vacancy, matching and ApplyDecision endpoints', () => {
    const vacancyId = '11111111-1111-1111-1111-111111111111';

    service.getVacancy(vacancyId).subscribe();
    service.getMatch(vacancyId).subscribe();
    service.getApplyDecision(vacancyId).subscribe();

    const vacancyRequest = http.expectOne(`https://example.test/api/vacancies/${vacancyId}`);
    const matchRequest = http.expectOne(`https://example.test/api/matching/vacancies/${vacancyId}`);
    const decisionRequest = http.expectOne(`https://example.test/api/applications/vacancies/${vacancyId}/apply-decision`);

    expect(vacancyRequest.request.method).toBe('GET');
    expect(matchRequest.request.method).toBe('GET');
    expect(decisionRequest.request.method).toBe('GET');

    vacancyRequest.flush({});
    matchRequest.flush({});
    decisionRequest.flush({});
  });

  it('submits only vacancyId and explicit baseline acknowledgement', () => {
    const vacancyId = '22222222-2222-2222-2222-222222222222';

    service.apply(vacancyId, true).subscribe();

    const request = http.expectOne('https://example.test/api/applications');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      vacancyId,
      baselineAcknowledged: true
    });
    request.flush({});
  });
});