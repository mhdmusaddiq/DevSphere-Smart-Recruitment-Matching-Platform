/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../../../core/config/api-base-url';
import { SeekerWorkflowApiService } from './seeker-workflow-api.service';

describe('SeekerWorkflowApiService S04/S05/S08/S09 contracts', () => {
  let service: SeekerWorkflowApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        SeekerWorkflowApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://example.test/api' }
      ]
    });

    service = TestBed.inject(SeekerWorkflowApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('uses candidate application list and immutable history endpoints', () => {
    const id = '11111111-1111-1111-1111-111111111111';

    service.getApplications().subscribe();
    service.getApplication(id).subscribe();
    service.getSnapshot(id).subscribe();
    service.getStatusHistory(id).subscribe();
    service.getWorkflow(id).subscribe();

    const list = http.expectOne('https://example.test/api/applications/candidate');
    const detail = http.expectOne(`https://example.test/api/applications/candidate/${id}`);
    const snapshot = http.expectOne(`https://example.test/api/applications/${id}/snapshot`);
    const history = http.expectOne(`https://example.test/api/applications/${id}/status-history`);
    const workflow = http.expectOne(`https://example.test/api/candidate/applications/${id}/workflow`);

    for (const request of [list, detail, snapshot, history, workflow]) {
      expect(request.request.method).toBe('GET');
      request.flush({});
    }
  });

  it('withdraws through the candidate application endpoint', () => {
    const id = '22222222-2222-2222-2222-222222222222';

    service.withdraw(id).subscribe();

    const request = http.expectOne(`https://example.test/api/applications/${id}/withdraw`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({});
    request.flush({});
  });

  it('updates only supported candidate contact decisions', () => {
    const id = '33333333-3333-3333-3333-333333333333';

    service.updateContactStatus(id, 'Revoked').subscribe();

    const request = http.expectOne(`https://example.test/api/contact-requests/${id}/status`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ status: 'Revoked' });
    request.flush({});
  });

  it('marks an individual notification read without inventing a bulk endpoint', () => {
    const id = '44444444-4444-4444-4444-444444444444';

    service.markNotificationRead(id).subscribe();

    const request = http.expectOne(`https://example.test/api/notifications/${id}/read`);
    expect(request.request.method).toBe('PUT');
    request.flush({ message: 'Notification marked as read.' });
  });

  it('downloads the exact historical resume version', () => {
    const id = '55555555-5555-5555-5555-555555555555';

    service.downloadResumeVersion(id).subscribe();

    const request = http.expectOne(`https://example.test/api/candidates/resume/versions/${id}/download`);
    expect(request.request.method).toBe('GET');
    expect(request.request.responseType).toBe('blob');
    request.flush(new Blob(['pdf'], { type: 'application/pdf' }));
  });
});