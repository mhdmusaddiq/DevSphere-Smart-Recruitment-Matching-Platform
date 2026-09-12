import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  convertToParamMap,
  provideRouter
} from '@angular/router';
import { of } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerApplicantDetailComponent } from './employer-applicant-detail.component';

describe('EmployerApplicantDetailComponent', () => {
  let fixture: ComponentFixture<EmployerApplicantDetailComponent>;
  let component: EmployerApplicantDetailComponent;

  const api = jasmine.createSpyObj<EmployerApiService>(
    'EmployerApiService',
    ['getVacancyApplications']
  );

  const applicant = {
    applicationId: 'application-1',
    candidateId: 'candidate-1',
    vacancyId: 'vacancy-1',
    appliedAt: '2026-09-12T10:00:00Z',
    status: 'Applied',
    rawCompatibility: 91.25,
    matchScore: 91.3,
    assessmentStatus: 'Calculated',
    eligibility: 'MeetsBaseline',
    eligibilityReason: 'AllMandatoryRequirementsSatisfied',
    highTierAggregate: 95,
    mediumTierAggregate: 85,
    coverage: 100,
    matchedSkills: ['Angular'],
    missingSkills: [],
    missingInputs: [],
    families: []
  };

  beforeEach(async () => {
    api.getVacancyApplications.calls.reset();
    api.getVacancyApplications.and.returnValue(
      of([applicant] as any)
    );

    await TestBed.configureTestingModule({
      imports: [EmployerApplicantDetailComponent],
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
                applicationId: 'application-1'
              }),
              queryParamMap: convertToParamMap({
                vacancyId: 'vacancy-1'
              })
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(
      EmployerApplicantDetailComponent
    );

    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads employer-authorized vacancy applications', () => {
    expect(api.getVacancyApplications)
      .toHaveBeenCalledWith('vacancy-1');
  });

  it('selects the requested application', () => {
    expect(component.applicant?.applicationId)
      .toBe('application-1');
  });

  it('preserves server-authored compatibility evidence', () => {
    expect(component.applicant?.matchScore).toBe(91.3);
    expect(component.applicant?.eligibility)
      .toBe('MeetsBaseline');
  });
});