/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { SeekerWorkflowApiService } from '../data/seeker-workflow-api.service';
import { ApplicationDetailComponent } from './application-detail.component';

describe('ApplicationDetailComponent S05', () => {
  let api: jasmine.SpyObj<SeekerWorkflowApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<SeekerWorkflowApiService>(
      'SeekerWorkflowApiService',
      [
        'getApplication', 'getSnapshot', 'getStatusHistory', 'getWorkflow',
        'getContactRequests', 'getResume', 'withdraw', 'updateContactStatus',
        'downloadResumeVersion'
      ]
    );

    api.getApplication.and.returnValue(of({
      id: 'a1', candidateId: 'candidate', vacancyId: 'v1',
      vacancyTitle: 'Frontend Engineer', vacancyLocation: 'Colombo',
      workMode: 'Hybrid', companyId: 'c1', companyName: 'Ceylon Digital Labs',
      submittedAtUtc: '2026-09-08T10:42:00Z', status: 'UnderReview',
      frozenAssessmentStatus: 'Calculated', displayCompatibility: 82.5,
      eligibility: 'Eligible', resumeVersionId: 'r1',
      capturedAtUtc: '2026-09-08T10:42:00Z'
    }));
    api.getSnapshot.and.returnValue(of({
      id: 's1', jobApplicationId: 'a1', resumeVersionId: 'r1',
      matchingPolicyRevisionId: null, compatibilityScore: 82.5,
      rawCompatibilityScore: 82.5, displayCompatibilityScore: 82.5,
      highTierAggregateScore: null, mediumTierAggregateScore: null,
      coverage: 1, compatibilityStatus: 'Calculated',
      eligibilityStatus: 'Eligible', eligibilityReason: null, isEligible: true,
      applyDecision: 'Allowed', matchedSkillsJson: '["Angular","TypeScript"]',
      gapSkillsJson: '["Accessibility"]', evidenceSummaryJson: '{}',
      matchResultJson: '{}', candidateSnapshotJson: '{}',
      vacancySnapshotJson: '{}', capturedAtUtc: '2026-09-08T10:42:00Z'
    }));
    api.getStatusHistory.and.returnValue(of([]));
    api.getWorkflow.and.returnValue(of({
      jobApplicationId: 'a1', interviews: [], interviewSlots: [],
      offers: [], talentPoolEntries: []
    }));
    api.getContactRequests.and.returnValue(of([]));
    api.getResume.and.returnValue(of({
      id: 'resume', candidateProfileId: 'profile', currentVersionId: 'r1',
      versions: [{
        id: 'r1', versionNumber: 3, originalFileName: 'cv.pdf',
        storageKey: 'private/key', contentType: 'application/pdf',
        fileSizeBytes: 100, isCurrent: true
      }]
    }));

    await TestBed.configureTestingModule({
      imports: [ApplicationDetailComponent],
      providers: [
        provideRouter([]),
        { provide: SeekerWorkflowApiService, useValue: api },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ applicationId: 'a1' }) } }
        }
      ]
    }).compileComponents();
  });

  it('renders only immutable snapshot compatibility for application history truth', () => {
    const fixture = TestBed.createComponent(ApplicationDetailComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.frozenCompatibility).toBe(82.5);
    expect(fixture.componentInstance.matchedSkills).toEqual(['Angular', 'TypeScript']);
    expect(fixture.componentInstance.gapSkills).toEqual(['Accessibility']);
  });

  it('does not expose a live matching call from the application detail component', () => {
    const fixture = TestBed.createComponent(ApplicationDetailComponent);
    fixture.detectChanges();

    expect(api.getSnapshot).toHaveBeenCalledWith('a1');
    expect(api.getWorkflow).toHaveBeenCalledWith('a1');
  });
});