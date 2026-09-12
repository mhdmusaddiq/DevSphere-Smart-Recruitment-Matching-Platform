import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  convertToParamMap,
  provideRouter
} from '@angular/router';
import { of } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerRankedApplicantsComponent } from './employer-ranked-applicants.component';

describe('EmployerRankedApplicantsComponent', () => {
  let fixture: ComponentFixture<EmployerRankedApplicantsComponent>;
  let component: EmployerRankedApplicantsComponent;

  const api = jasmine.createSpyObj<EmployerApiService>(
    'EmployerApiService',
    ['getVacancy', 'getVacancyApplications']
  );

  const vacancy = {
    id: 'vacancy-1',
    title: 'Software Engineer'
  };

  const applicants = [
    {
      applicationId: 'application-1',
      candidateId: 'candidate-1',
      vacancyId: 'vacancy-1',
      appliedAt: '2026-09-12T10:00:00Z',
      status: 'Applied',
      rawCompatibility: 87.25,
      matchScore: 87.3,
      assessmentStatus: 'Calculated',
      eligibility: 'MeetsBaseline',
      eligibilityReason: null,
      highTierAggregate: 90,
      mediumTierAggregate: 85,
      coverage: 100,
      matchedSkills: ['Angular'],
      missingSkills: [],
      missingInputs: [],
      families: []
    }
  ];

  beforeEach(async () => {
    api.getVacancy.calls.reset();
    api.getVacancyApplications.calls.reset();

    api.getVacancy.and.returnValue(of(vacancy as any));
    api.getVacancyApplications.and.returnValue(
      of(applicants as any)
    );

    await TestBed.configureTestingModule({
      imports: [EmployerRankedApplicantsComponent],
      providers: [
        provideRouter([]),
        {
          provide: EmployerApiService,
          useValue: api
        },
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

    fixture = TestBed.createComponent(
      EmployerRankedApplicantsComponent
    );

    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads vacancy and server-ranked applicants', () => {
    expect(api.getVacancy)
      .toHaveBeenCalledWith('vacancy-1');

    expect(api.getVacancyApplications)
      .toHaveBeenCalledWith('vacancy-1');

    expect(component.applicants.length).toBe(1);
  });

  it('preserves backend applicant order', () => {
    expect(component.applicants[0].applicationId)
      .toBe('application-1');
  });

  it('uses server compatibility without client calculation', () => {
    expect(component.applicants[0].matchScore)
      .toBe(87.3);
  });
});