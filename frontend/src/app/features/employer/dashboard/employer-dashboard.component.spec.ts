/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';

import { EmployerApiService } from '../data-access/employer-api.service';
import {
  CompanyProfile,
  EmployerProfile,
  EmployerVacancy
} from '../data-access/employer.models';
import { EmployerDashboardComponent } from './employer-dashboard.component';

describe('EmployerDashboardComponent', () => {
  let component: EmployerDashboardComponent;
  let employerApi: jasmine.SpyObj<EmployerApiService>;

  const profile: EmployerProfile = {
    companyName: 'Example Ltd',
    contactEmail: 'employer@example.test',
    website: 'https://example.test',
    location: 'Colombo'
  };

  const verifiedCompany = {
    id: 'company-1',
    name: 'Example Ltd',
    description: null,
    website: 'https://example.test',
    location: 'Colombo',
    membershipStatus: 'Active',
    verificationStatus: 'Verified'
  } as CompanyProfile;

  const vacancy = {
    id: 'vacancy-1',
    lifecycleStatus: 'Published'
  } as EmployerVacancy;

  beforeEach(async () => {
    employerApi = jasmine.createSpyObj<EmployerApiService>(
      'EmployerApiService',
      [
        'getDashboard',
        'getProfile',
        'getCompanies',
        'getVacancies'
      ]
    );

    await TestBed.configureTestingModule({
      imports: [EmployerDashboardComponent],
      providers: [
        {
          provide: EmployerApiService,
          useValue: employerApi
        }
      ]
    }).compileComponents();

    const fixture =
      TestBed.createComponent(EmployerDashboardComponent);

    component = fixture.componentInstance;
  });

  it('prioritizes creating a company when no company exists', () => {
    component.profile = null;
    component.companies = [];
    component.vacancies = [];

    expect(component.nextAction.route)
      .toBe('/employer/company');

    expect(component.nextAction.label)
      .toBe('Create company');
  });

  it('prioritizes employer profile after a company exists', () => {
    component.profile = null;
    component.companies = [verifiedCompany];
    component.vacancies = [];

    expect(component.nextAction.route)
      .toBe('/employer/profile');

    expect(component.nextAction.label)
      .toBe('Complete profile');
  });

  it('requires verification for a draft company before vacancy creation', () => {
    component.profile = profile;
    component.companies = [
      {
        ...verifiedCompany,
        verificationStatus: 'Draft'
      }
    ];
    component.vacancies = [];

    expect(component.nextAction.route)
      .toBe('/employer/company');

    expect(component.nextAction.label)
      .toBe('Manage verification');
  });

  it('keeps a pending verification in company management', () => {
    component.profile = profile;
    component.companies = [
      {
        ...verifiedCompany,
        verificationStatus: 'PendingReview'
      }
    ];
    component.vacancies = [];

    expect(component.nextAction.route)
      .toBe('/employer/company');

    expect(component.nextAction.label)
      .toBe('View company');
  });

  it('keeps revoked membership in company management', () => {
    component.profile = profile;
    component.companies = [
      {
        ...verifiedCompany,
        membershipStatus: 'Revoked'
      }
    ];
    component.vacancies = [];

    expect(component.nextAction.route)
      .toBe('/employer/company');

    expect(component.nextAction.label)
      .toBe('Manage company');
  });

  it('offers vacancy creation only after profile and trust are ready', () => {
    component.profile = profile;
    component.companies = [verifiedCompany];
    component.vacancies = [];

    expect(component.nextAction.route)
      .toBe('/employer/vacancies');

    expect(component.nextAction.label)
      .toBe('Create vacancy');
  });

  it('prioritizes completing an existing draft vacancy', () => {
    component.profile = profile;
    component.companies = [verifiedCompany];
    component.vacancies = [
      {
        ...vacancy,
        id: 'draft-1',
        lifecycleStatus: 'Draft'
      }
    ];

    expect(component.nextAction.route)
      .toBe('/employer/vacancies/draft-1/policy');

    expect(component.nextAction.label)
      .toBe('Continue vacancy');
  });

  it('falls back to vacancy management when setup is complete', () => {
    component.profile = profile;
    component.companies = [verifiedCompany];
    component.vacancies = [vacancy];

    expect(component.nextAction.route)
      .toBe('/employer/vacancies');

    expect(component.nextAction.label)
      .toBe('Manage vacancies');
  });
});
