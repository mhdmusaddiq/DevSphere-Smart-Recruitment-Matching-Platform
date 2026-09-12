import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  convertToParamMap,
  provideRouter,
  Router
} from '@angular/router';
import { of, throwError } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerVacancyManagementComponent } from './employer-vacancy-management.component';

describe('EmployerVacancyManagementComponent', () => {
  let fixture: ComponentFixture<EmployerVacancyManagementComponent>;
  let component: EmployerVacancyManagementComponent;

  const vacancy = {
    id: 'vacancy-1',
    companyId: 'company-1',
    companyName: 'AptLens',
    companyVerificationStatus: 'Verified',
    title: 'Software Engineer',
    description: 'Build products',
    location: 'Colombo',
    workMode: 'Hybrid',
    employmentType: 'FullTime',
    requiredExperienceMonths: 12,
    minExperienceMonths: 12,
    maxExperienceMonths: 36,
    requiredEducation: 'Degree',
    salaryMin: null,
    salaryMax: null,
    closingDateUtc: null,
    publishedAtUtc: null,
    requiredSkills: [
      {
        name: 'Angular',
        weight: 1
      }
    ],
    lifecycleStatus: 'Draft',
    isOpen: false,
    assessmentStatus: 'NotAssessed',
    eligibility: '',
    rawCompatibility: null,
    matchScore: null,
    highTierAggregate: null,
    mediumTierAggregate: null,
    coverage: null
  };

  const policy = {
    id: 'policy-1',
    vacancyId: 'vacancy-1',
    revisionNumber: 1,
    isCurrent: true,
    isMateriallyLocked: false,
    materiallyLockedAtUtc: null
  };

  const applications = [
    {
      applicationId: 'application-1',
      candidateId: 'candidate-1',
      vacancyId: 'vacancy-1',
      vacancyTitle: 'Software Engineer',
      vacancyLocation: 'Colombo',
      workMode: 'Hybrid',
      companyId: 'company-1',
      companyName: 'AptLens',
      appliedAt: '2026-09-12T10:00:00Z',
      status: 'Submitted',
      assessmentStatus: 'Completed',
      matchScore: 82,
      eligibility: 'Eligible',
      eligibilityReason: null,
      highTierAggregate: null,
      mediumTierAggregate: null,
      coverage: 100,
      matchedSkills: ['Angular'],
      missingSkills: [],
      missingInputs: [],
      families: [],
      resumeVersionId: null,
      capturedAtUtc: null,
      applyDecision: null
    }
  ];

  const api = jasmine.createSpyObj<EmployerApiService>(
    'EmployerApiService',
    [
      'getVacancy',
      'getCurrentMatchingPolicy',
      'getVacancyApplications',
      'publishVacancy',
      'closeVacancy'
    ]
  );

  let router: Router;

  beforeEach(async () => {
    api.getVacancy.calls.reset();
    api.getCurrentMatchingPolicy.calls.reset();
    api.getVacancyApplications.calls.reset();
    api.publishVacancy.calls.reset();
    api.closeVacancy.calls.reset();
    api.getVacancy.and.returnValue(
      of(vacancy as any)
    );

    api.getCurrentMatchingPolicy.and.returnValue(
      of(policy as any)
    );

    api.getVacancyApplications.and.returnValue(
      of(applications as any)
    );

    await TestBed.configureTestingModule({
      imports: [
        EmployerVacancyManagementComponent
      ],
      providers: [
        {
          provide: EmployerApiService,
          useValue: api
        },
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({
                vacancyId: 'vacancy-1'
              })
            }
          }
        }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);

    fixture = TestBed.createComponent(
      EmployerVacancyManagementComponent
    );

    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('loads vacancy policy and applications', () => {
    expect(api.getVacancy)
      .toHaveBeenCalledWith('vacancy-1');

    expect(api.getCurrentMatchingPolicy)
      .toHaveBeenCalledWith('vacancy-1');

    expect(api.getVacancyApplications)
      .toHaveBeenCalledWith('vacancy-1');

    expect(component.applicationCount).toBe(1);
  });

  it('derives status counts from owned application list', () => {
    expect(
      component.countStatus('Submitted')
    ).toBe(1);

    expect(
      component.countStatus('Rejected')
    ).toBe(0);
  });

  it('navigates Draft vacancy to editor', () => {
    component.edit();

    expect(router.navigate).toHaveBeenCalledWith([
      '/employer/vacancies',
      'vacancy-1',
      'edit'
    ]);
  });

  it('publishes Draft vacancy', () => {
    api.publishVacancy.and.returnValue(
      of({
        ...vacancy,
        lifecycleStatus: 'Published'
      } as any)
    );

    component.publish();

    expect(api.publishVacancy)
      .toHaveBeenCalledWith('vacancy-1');

    expect(
      component.vacancy?.lifecycleStatus
    ).toBe('Published');
  });

  it('closes Published vacancy', () => {
    component.vacancy = {
      ...vacancy,
      lifecycleStatus: 'Published'
    } as any;

    api.closeVacancy.and.returnValue(
      of({
        ...vacancy,
        lifecycleStatus: 'Closed'
      } as any)
    );

    component.close();

    expect(api.closeVacancy)
      .toHaveBeenCalledWith('vacancy-1');

    expect(
      component.vacancy?.lifecycleStatus
    ).toBe('Closed');
  });

  it('reloads vacancy after lifecycle conflict', () => {
    api.publishVacancy.and.returnValue(
      throwError(() => ({
        status: 409
      }))
    );

    component.publish();

    expect(api.getVacancy)
      .toHaveBeenCalledTimes(2);

    expect(component.errorMessage)
      .toContain('reloaded');
  });
});