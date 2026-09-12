/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../../core/config/api-base-url';
import { CvApiService } from './cv-api.service';

describe('CvApiService S07 contracts', () => {
  let service: CvApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CvApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://example.test/api' }
      ]
    });

    service = TestBed.inject(CvApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads resume versions and sets a selected version current', () => {
    service.getResume().subscribe();
    service.setCurrentVersion('version-2').subscribe();

    const list = http.expectOne('https://example.test/api/candidates/resume');
    const current = http.expectOne('https://example.test/api/candidates/resume/versions/version-2/current');

    expect(list.request.method).toBe('GET');
    expect(current.request.method).toBe('PUT');

    list.flush({ id: 'r1', candidateProfileId: 'c1', currentVersionId: null, versions: [] });
    current.flush({ id: 'r1', candidateProfileId: 'c1', currentVersionId: 'version-2', versions: [] });
  });

  it('uploads using multipart field file', () => {
    const file = new File(['%PDF-'], 'cv.pdf', { type: 'application/pdf' });
    service.uploadVersion(file).subscribe();

    const request = http.expectOne('https://example.test/api/candidates/resume/versions');
    expect(request.request.method).toBe('POST');
    expect(request.request.body instanceof FormData).toBeTrue();
    const uploadedFile = (request.request.body as FormData).get('file');

    expect(uploadedFile instanceof File).toBeTrue();

    const uploaded = uploadedFile as File;

    expect(uploaded.name).toBe('cv.pdf');
    expect(uploaded.type).toBe('application/pdf');
    expect(uploaded.size).toBe(file.size);

    request.flush({ id: 'r1', candidateProfileId: 'c1', currentVersionId: 'v1', versions: [] });
  });
});