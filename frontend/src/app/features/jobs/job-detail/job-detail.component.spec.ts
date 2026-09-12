/// <reference types="jasmine" />

import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { SessionService } from '../../../core/auth/session.service';
import { SeekerApiService } from '../../seeker/data/seeker-api.service';
import { VacancyView } from '../../seeker/data/seeker.models';
import { JobDetailComponent } from './job-detail.component';

describe('JobDetailComponent', () => {
  let api: jasmine.SpyObj<SeekerApiService>;
  const vacancy: VacancyView = {
    id: '11111111-1111-1111-1111-111111111111', companyId: null,
    companyName: 'AptLens Labs', companyVerificationStatus: 'Verified',
    title: 'Frontend Engineer', description: 'Build accessible Angular interfaces.',
    location: 'Colombo', workMode: 'Hybrid', employmentType: 'Full-time',
    requiredExperienceMonths: 24, minExperienceMonths: 24, maxExperienceMonths: 48,
    requiredEducation: 'Degree', salaryMin: null, salaryMax: null,
    closingDateUtc: null, publishedAtUtc: null, requiredSkills: [],
    lifecycleStatus: 'Published', isOpen: true, assessmentStatus: '', eligibility: '',
    rawCompatibility: null, displayCompatibility: null, highTierAggregate: null,
    mediumTierAggregate: null, coverage: null
  };

  beforeEach(async () => {
    api = jasmine.createSpyObj<SeekerApiService>('SeekerApiService', [
      'getVacancy', 'getMatch', 'getApplicationReadiness', 'getResume',
      'getApplyDecision', 'apply'
    ]);
    api.getVacancy.and.returnValue(of(vacancy));
    const user = signal(null);

    await TestBed.configureTestingModule({
      imports: [JobDetailComponent],
      providers: [
        provideRouter([]),
        { provide: SeekerApiService, useValue: api },
        { provide: SessionService, useValue: { user: user.asReadonly() } },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ vacancyId: vacancy.id })
            }
          }
        }
      ]
    }).compileComponents();
  });

  it('keeps public job detail public without calling Candidate-only APIs', () => {
    const fixture = TestBed.createComponent(JobDetailComponent);
    fixture.detectChanges();

    expect(api.getVacancy).toHaveBeenCalledWith(vacancy.id);
    expect(api.getMatch).not.toHaveBeenCalled();
    expect(api.getApplyDecision).not.toHaveBeenCalled();
  });

  it('maps backend numeric matching enums without calculating a client score', () => {
    const fixture = TestBed.createComponent(JobDetailComponent);
    const component = fixture.componentInstance;

    expect(component.assessmentLabel(1)).toBe('Calculated');
    expect(component.eligibilityLabel(4)).toBe('DoesNotMeetBaseline');
    expect(component.criterionStateLabel(5)).toBe('PendingVerification');
  });
});