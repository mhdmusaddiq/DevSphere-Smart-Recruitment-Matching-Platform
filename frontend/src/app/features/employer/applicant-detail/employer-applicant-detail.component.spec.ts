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
    [
      'getEmployerApplication',
      'getApplicationSnapshot',
      'getApplicationStatusHistory',
      'getEmployerContactRequests',
      'updateApplicationStatus',
      'sendContactRequest',
      'downloadApplicationResume'
    ]
  );

  const applicant = {
    applicationId: 'application-1',
    candidateId: 'candidate-1',
    candidateDisplayName: 'A. Mendis',
    vacancyId: 'vacancy-1',
    vacancyTitle: 'Senior Frontend Engineer',
    submittedAtUtc: '2026-09-12T10:00:00Z',
    status: 'UnderReview',
    assessmentStatus: 1,
    displayCompatibility: 91.3,
    eligibility: 1,
    eligibilityReason: 'AllMandatoryRequirementsSatisfied',
    matchedSkills: ['Angular'],
    missingSkills: [],
    missingInputs: [],
    families: [],
    resumeVersionId: 'resume-version-1',
    capturedAtUtc: '2026-09-12T10:00:01Z'
  };

  beforeEach(async () => {
    api.getEmployerApplication.calls.reset();
    api.getApplicationSnapshot.calls.reset();
    api.getApplicationStatusHistory.calls.reset();
    api.getEmployerContactRequests.calls.reset();

    api.getEmployerApplication.and.returnValue(
      of(applicant as any)
    );
    api.getApplicationSnapshot.and.returnValue(
      of({
        id: 'snapshot-1',
        jobApplicationId: 'application-1',
        coverage: 100,
        capturedAtUtc: '2026-09-12T10:00:01Z'
      } as any)
    );
    api.getApplicationStatusHistory.and.returnValue(
      of([
        {
          id: 'history-1',
          jobApplicationId: 'application-1',
          previousStatus: 'Submitted',
          newStatus: 'UnderReview',
          changedByUserId: 'employer-1',
          changedAtUtc: '2026-09-12T10:05:00Z',
          notes: 'Status updated.'
        }
      ] as any)
    );
    api.getEmployerContactRequests.and.returnValue(
      of([])
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
                vacancyId: 'vacancy-1',
                applicationId: 'application-1'
              }),
              queryParamMap: convertToParamMap({})
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

  it('loads the dedicated employer application detail contract', () => {
    expect(api.getEmployerApplication)
      .toHaveBeenCalledWith('application-1');

    expect(api.getApplicationSnapshot)
      .toHaveBeenCalledWith('application-1');

    expect(api.getApplicationStatusHistory)
      .toHaveBeenCalledWith('application-1');

    expect(api.getEmployerContactRequests)
      .toHaveBeenCalled();

    expect(component.applicant?.candidateDisplayName)
      .toBe('A. Mendis');
  });

  it('accepts canonical vacancy context from route params', () => {
    expect(component.vacancyId)
      .toBe('vacancy-1');
  });

  it('maps numeric assessment enums and preserves server score', () => {
    expect(component.assessmentLabel(1))
      .toBe('Calculated');
    expect(component.eligibilityLabel(1))
      .toBe('MeetsBaseline');
    expect(component.applicant?.displayCompatibility)
      .toBe(91.3);
    expect(component.hasCalculatedScore())
      .toBeTrue();
  });

  it('uses backend application status transitions only', () => {
    expect(component.availableStatusTransitions)
      .toEqual(['Shortlisted', 'Rejected']);
  });

  it('does not expose candidate email without server disclosure', () => {
    component.contactRequest = {
      id: 'contact-1',
      jobApplicationId: 'application-1',
      employerId: 'employer-1',
      candidateId: 'candidate-1',
      status: 'Accepted',
      vacancyId: 'vacancy-1',
      vacancyTitle: 'Senior Frontend Engineer',
      companyId: 'company-1',
      companyName: 'Ceylon Digital Labs',
      candidateDisplayName: 'A. Mendis',
      employerDisplayName: 'N. Perera',
      canDiscloseContact: false,
      candidateEmail: 'candidate@example.test'
    };

    fixture.detectChanges();

    expect(
      fixture.nativeElement.textContent
    ).not.toContain('candidate@example.test');
  });
});
