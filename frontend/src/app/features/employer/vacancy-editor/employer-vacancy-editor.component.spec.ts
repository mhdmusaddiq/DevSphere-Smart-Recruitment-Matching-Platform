import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import {
  CompanyProfile,
  EmployerVacancy,
  VacancyPolicyAggregateDto
} from '../data-access/employer.models';
import { EmployerVacancyEditorComponent } from './employer-vacancy-editor.component';

describe('EmployerVacancyEditorComponent', () => {
  let fixture: ComponentFixture<EmployerVacancyEditorComponent>;
  let component: EmployerVacancyEditorComponent;
  let employerApi: jasmine.SpyObj<EmployerApiService>;

  const vacancyId = 'vacancy-1';

  const makeVacancy = (
    overrides: Partial<EmployerVacancy> = {}
  ): EmployerVacancy => ({
    id: vacancyId,
    companyId: 'company-1',
    companyName: 'AptLens',
    companyVerificationStatus: 'Verified',
    title: 'Software Engineer',
    description: 'Build product features.',
    location: 'Colombo',
    workMode: 'Hybrid',
    employmentType: 'FullTime',
    requiredExperienceMonths: 12,
    minExperienceMonths: 12,
    maxExperienceMonths: 36,
    requiredEducation: 'Bachelor degree',
    salaryMin: 100000,
    salaryMax: 180000,
    closingDateUtc: '2026-10-10T00:00:00Z',
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
    eligibility: 'Unknown',
    rawCompatibility: null,
    displayCompatibility: null,
    highTierAggregate: null,
    mediumTierAggregate: null,
    coverage: null,
    ...overrides
  });

  const makePolicy = (
    locked = false
  ): VacancyPolicyAggregateDto => ({
    revision: {
      id: 'revision-1',
      vacancyId,
      revisionNumber: 1,
      isCurrent: true,
      isMateriallyLocked: locked,
      materiallyLockedAtUtc: locked
        ? '2026-09-12T12:00:00Z'
        : null
    },
    families: [],
    requirements: [],
    alternativeSets: []
  });

  const companies: CompanyProfile[] = [
    {
      id: 'company-1',
      name: 'AptLens',
      description: null,
      website: null,
      location: 'Colombo',
      membershipStatus: 'Active',
      verificationStatus: 'Verified'
    }
  ];

  beforeEach(async () => {
    employerApi = jasmine.createSpyObj<EmployerApiService>(
      'EmployerApiService',
      [
        'getCompanies',
        'getVacancy',
        'getFullMatchingPolicy',
        'createVacancy',
        'updateVacancy',
        'updateMatchingPolicy',
        'publishVacancy'
      ]
    );

    employerApi.getCompanies.and.returnValue(of(companies));
    employerApi.getVacancy.and.returnValue(
      of(makeVacancy())
    );
    employerApi.getFullMatchingPolicy.and.returnValue(
      of(makePolicy())
    );

    await TestBed.configureTestingModule({
      imports: [EmployerVacancyEditorComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) =>
                  key === 'vacancyId'
                    ? vacancyId
                    : null
              }
            }
          }
        },
        {
          provide: EmployerApiService,
          useValue: employerApi
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(
      EmployerVacancyEditorComponent
    );
    component = fixture.componentInstance;
  });

  it('loads company, vacancy and policy data on init', () => {
    fixture.detectChanges();

    expect(employerApi.getCompanies)
      .toHaveBeenCalledTimes(1);
    expect(employerApi.getVacancy)
      .toHaveBeenCalledOnceWith(vacancyId);
    expect(employerApi.getFullMatchingPolicy)
      .toHaveBeenCalledOnceWith(vacancyId);

    expect(component.companies).toEqual(companies);
    expect(component.loadedVacancy?.id)
      .toBe(vacancyId);
    expect(component.policy?.revision.revisionNumber)
      .toBe(1);
  });

  it('updates an existing vacancy', () => {
    const vacancy = makeVacancy();

    component.loadedVacancy = vacancy;

    component.form.patchValue({
      companyId: 'company-1',
      title: vacancy.title,
      description: vacancy.description,
      location: vacancy.location,
      workMode: vacancy.workMode,
      employmentType: vacancy.employmentType,
      minExperienceMonths: 12,
      maxExperienceMonths: 36,
      requiredEducation: vacancy.requiredEducation,
      salaryMin: vacancy.salaryMin,
      salaryMax: vacancy.salaryMax,
      closingDateUtc: '2026-10-10T00:00'
    });

    component.requiredSkills.clear();
    component.requiredSkills.push(
      component['createSkillGroup'](
        'Angular',
        1
      )
    );

    employerApi.updateVacancy.and.returnValue(
      of(vacancy)
    );

    component.saveVacancy();

    expect(employerApi.updateVacancy)
      .toHaveBeenCalled();
    expect(component.successMessage)
      .toBe('Vacancy changes saved.');
  });

  it('blocks duplicate required skills', () => {
    component.form.patchValue({
      title: 'Software Engineer'
    });

    component.requiredSkills.clear();
    component.requiredSkills.push(
      component['createSkillGroup'](
        'Angular',
        1
      )
    );
    component.requiredSkills.push(
      component['createSkillGroup'](
        'angular',
        2
      )
    );

    component.saveVacancy();

    expect(employerApi.updateVacancy)
      .not.toHaveBeenCalled();
    expect(component.errorMessage)
      .toBe(
        'Duplicate required skills are not allowed.'
      );
  });

  it('saves the current matching policy aggregate', () => {
    component.loadedVacancy = makeVacancy();
    component.policy = makePolicy();

    employerApi.updateMatchingPolicy.and.returnValue(
      of(makePolicy())
    );

    component.savePolicy();

    expect(employerApi.updateMatchingPolicy)
      .toHaveBeenCalledOnceWith(
        vacancyId,
        {
          families: [],
          requirements: [],
          alternativeSets: []
        }
      );

    expect(component.policySuccessMessage)
      .toBe('Matching policy saved.');
  });

  it('does not save a materially locked policy', () => {
    component.loadedVacancy = makeVacancy({
      lifecycleStatus: 'Published'
    });
    component.policy = makePolicy(true);

    component.savePolicy();

    expect(employerApi.updateMatchingPolicy)
      .not.toHaveBeenCalled();
  });

  it('publishes a Draft vacancy', () => {
    const published = makeVacancy({
      lifecycleStatus: 'Published',
      isOpen: true,
      publishedAtUtc: '2026-09-12T12:00:00Z'
    });

    component.loadedVacancy = makeVacancy();

    employerApi.publishVacancy.and.returnValue(
      of(published)
    );

    component.publishVacancy();

    expect(employerApi.publishVacancy)
      .toHaveBeenCalledOnceWith(vacancyId);
    expect(component.loadedVacancy)
      .toEqual(published);
    expect(component.publishSuccessMessage)
      .toBe('Vacancy published successfully.');
  });

  it('reloads vacancy and policy after publish conflict', () => {
    const latest = makeVacancy();

    component.loadedVacancy = makeVacancy();

    employerApi.publishVacancy.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: {
              message: 'Publish blocked.'
            }
          })
      )
    );

    employerApi.getVacancy.and.returnValue(
      of(latest)
    );
    employerApi.getFullMatchingPolicy.and.returnValue(
      of(makePolicy())
    );

    component.publishVacancy();

    expect(employerApi.getVacancy)
      .toHaveBeenCalledWith(vacancyId);
    expect(employerApi.getFullMatchingPolicy)
      .toHaveBeenCalledWith(vacancyId);
    expect(component.publishErrorMessage)
      .toBe('Publish blocked.');
  });
});
