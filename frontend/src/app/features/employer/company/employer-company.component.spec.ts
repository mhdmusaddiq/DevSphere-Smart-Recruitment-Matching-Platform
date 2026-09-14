/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerCompanyComponent } from './employer-company.component';

describe('EmployerCompanyComponent', () => {
  let component: EmployerCompanyComponent;
  let employerApi: jasmine.SpyObj<EmployerApiService>;

  const employerProfile = {
    companyName: 'Example Ltd',
    contactEmail: 'employer@example.test',
    website: 'https://example.test',
    location: 'Colombo'
  };

  const company = {
    id: 'company-1',
    name: 'Example Ltd',
    description: null,
    website: 'https://example.test',
    location: 'Colombo',
    membershipStatus: 'Active',
    verificationStatus: 'Draft'
  } as any;

  beforeEach(async () => {
    employerApi = jasmine.createSpyObj<EmployerApiService>(
      'EmployerApiService',
      [
        'getCompanies',
        'getProfile',
        'createCompany',
        'updateCompany',
        'uploadFile',
        'submitCompanyVerification'
      ]
    );

    employerApi.getCompanies.and.returnValue(of([]));
    employerApi.getProfile.and.returnValue(of(null));

    await TestBed.configureTestingModule({
      imports: [EmployerCompanyComponent],
      providers: [
        {
          provide: EmployerApiService,
          useValue: employerApi
        }
      ]
    }).compileComponents();

    const fixture =
      TestBed.createComponent(EmployerCompanyComponent);

    component = fixture.componentInstance;
  });

  it('rejects non-PDF evidence', () => {
    const file = new File(
      ['text'],
      'evidence.txt',
      { type: 'text/plain' }
    );

    const input = document.createElement('input');
    Object.defineProperty(input, 'files', {
      value: [file]
    });

    component.onEvidenceSelected({
      target: input
    } as unknown as Event);

    expect(component.selectedEvidence).toBeNull();
    expect(component.evidenceError)
      .toBe('Verification evidence must be a PDF file.');
  });

  it('rejects PDF evidence larger than 5 MiB', () => {
    const file = new File(
      [new Uint8Array(5 * 1024 * 1024 + 1)],
      'large.pdf',
      { type: 'application/pdf' }
    );

    const input = document.createElement('input');
    Object.defineProperty(input, 'files', {
      value: [file]
    });

    component.onEvidenceSelected({
      target: input
    } as unknown as Event);

    expect(component.selectedEvidence).toBeNull();
    expect(component.evidenceError)
      .toBe('Verification evidence must be 5 MiB or smaller.');
  });

  it('accepts a PDF within the 5 MiB limit', () => {
    const file = new File(
      ['pdf'],
      'evidence.pdf',
      { type: 'application/pdf' }
    );

    const input = document.createElement('input');
    Object.defineProperty(input, 'files', {
      value: [file]
    });

    component.onEvidenceSelected({
      target: input
    } as unknown as Event);

    expect(component.selectedEvidence).toBe(file);
    expect(component.evidenceError).toBe('');
  });

  it('requires an employer profile before verification submission', () => {
    component.company = company;
    component.employerProfile = null;

    expect(component.canSubmitVerification).toBeFalse();
  });

  it('blocks verification for revoked membership', () => {
    component.company = {
      ...company,
      membershipStatus: 'Revoked'
    };
    component.employerProfile = employerProfile;

    expect(component.canSubmitVerification).toBeFalse();
  });

  it('blocks verification while pending review', () => {
    component.company = {
      ...company,
      verificationStatus: 'PendingReview'
    };
    component.employerProfile = employerProfile;

    expect(component.canSubmitVerification).toBeFalse();
  });

  it('blocks verification after company is verified', () => {
    component.company = {
      ...company,
      verificationStatus: 'Verified'
    };
    component.employerProfile = employerProfile;

    expect(component.canSubmitVerification).toBeFalse();
  });

  it('allows verification for a draft company with employer profile', () => {
    component.company = company;
    component.employerProfile = employerProfile;

    expect(component.canSubmitVerification).toBeTrue();
  });
});
