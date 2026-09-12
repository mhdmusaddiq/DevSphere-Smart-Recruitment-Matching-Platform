/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../../core/config/api-base-url';
import { CareerProfileApiService } from './career-profile-api.service';

describe('CareerProfileApiService S06 contracts', () => {
  let service: CareerProfileApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CareerProfileApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://example.test/api' }
      ]
    });

    service = TestBed.inject(CareerProfileApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('uses candidate profile and readiness endpoints', () => {
    service.getProfile().subscribe();
    service.getProfileReadiness().subscribe();
    service.getApplicationReadiness().subscribe();

    const profile = http.expectOne('https://example.test/api/profile/candidate');
    const readiness = http.expectOne('https://example.test/api/profile/candidate/readiness');
    const applicationReadiness = http.expectOne('https://example.test/api/profile/candidate/application-readiness');

    expect(profile.request.method).toBe('GET');
    expect(readiness.request.method).toBe('GET');
    expect(applicationReadiness.request.method).toBe('GET');

    profile.flush({});
    readiness.flush({ isReady: false, missingItems: [] });
    applicationReadiness.flush({
      isReady: false,
      profileReady: false,
      resumeReady: false,
      currentResumeVersionId: null,
      missingItems: []
    });
  });

  it('uses the complete career evidence endpoint set', () => {
    service.getWorkExperiences().subscribe();
    service.getEducation().subscribe();
    service.getCertifications().subscribe();
    service.getProjects().subscribe();
    service.getLanguages().subscribe();
    service.getLicences().subscribe();

    const paths = [
      'work-experiences',
      'education',
      'certifications',
      'projects',
      'languages',
      'licences'
    ];

    for (const path of paths) {
      const request = http.expectOne(`https://example.test/api/candidate-career/${path}`);
      expect(request.request.method).toBe('GET');
      request.flush([]);
    }
  });

  it('keeps server taxonomy authoritative when adding a skill', () => {
    service.addSkill('Angular').subscribe();

    const request = http.expectOne('https://example.test/api/profile/candidate/skills');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ name: 'Angular' });
    request.flush({ id: 'skill-1', name: 'Angular' });
  });
});