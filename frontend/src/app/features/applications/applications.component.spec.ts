/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { SeekerWorkflowApiService } from '../seeker/data/seeker-workflow-api.service';
import { SeekerApplication } from '../seeker/data/seeker-workflow.models';
import { ApplicationsComponent } from './applications.component';

const APPLICATIONS: SeekerApplication[] = [
  {
    id: '1', candidateId: 'candidate', vacancyId: 'v1',
    vacancyTitle: 'Frontend Engineer', vacancyLocation: 'Colombo',
    workMode: 'Hybrid', companyId: 'c1', companyName: 'Ceylon Digital Labs',
    submittedAtUtc: '2026-09-08T10:42:00Z', status: 'UnderReview',
    frozenAssessmentStatus: 'Calculated', displayCompatibility: 82.5,
    eligibility: 'Eligible', resumeVersionId: 'r1', capturedAtUtc: '2026-09-08T10:42:00Z'
  },
  {
    id: '2', candidateId: 'candidate', vacancyId: 'v2',
    vacancyTitle: 'QA Engineer', vacancyLocation: 'Kandy',
    workMode: 'Remote', companyId: 'c2', companyName: 'Quality Labs',
    submittedAtUtc: '2026-09-07T10:42:00Z', status: 'Rejected',
    frozenAssessmentStatus: 'NotCalculated', displayCompatibility: null,
    eligibility: 'Unknown', resumeVersionId: null, capturedAtUtc: null
  }
];

describe('ApplicationsComponent S04', () => {
  let api: jasmine.SpyObj<SeekerWorkflowApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<SeekerWorkflowApiService>(
      'SeekerWorkflowApiService',
      ['getApplications', 'getResume', 'withdraw']
    );

    api.getApplications.and.returnValue(of(APPLICATIONS));
    api.getResume.and.returnValue(of({
      id: 'resume', candidateProfileId: 'profile', currentVersionId: 'r1',
      versions: [{
        id: 'r1', versionNumber: 3, originalFileName: 'cv.pdf',
        storageKey: 'private/key', contentType: 'application/pdf',
        fileSizeBytes: 100, isCurrent: true
      }]
    }));

    await TestBed.configureTestingModule({
      imports: [ApplicationsComponent],
      providers: [
        provideRouter([]),
        { provide: SeekerWorkflowApiService, useValue: api }
      ]
    }).compileComponents();
  });

  it('uses local workflow-status filters without inventing counts', () => {
    const fixture = TestBed.createComponent(ApplicationsComponent);
    fixture.detectChanges();

    fixture.componentInstance.setFilter('Active');
    expect(fixture.componentInstance.filteredApplications.map(item => item.id))
      .toEqual(['1']);
  });

  it('renders numeric compatibility only for Calculated frozen truth', () => {
    const fixture = TestBed.createComponent(ApplicationsComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.isCalculated(APPLICATIONS[0])).toBeTrue();
    expect(fixture.componentInstance.isCalculated(APPLICATIONS[1])).toBeFalse();
  });

  it('allows withdrawal only from the backend-supported non-terminal statuses', () => {
    const fixture = TestBed.createComponent(ApplicationsComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.canWithdraw(APPLICATIONS[0])).toBeTrue();
    expect(fixture.componentInstance.canWithdraw(APPLICATIONS[1])).toBeFalse();
  });

  it('does not request resume metadata when there are no applications to resolve', () => {
    api.getApplications.and.returnValue(of([]));

    const fixture = TestBed.createComponent(ApplicationsComponent);
    fixture.detectChanges();

    expect(api.getResume).not.toHaveBeenCalled();
  });
});