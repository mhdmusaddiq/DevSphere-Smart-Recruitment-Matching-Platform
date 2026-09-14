/// <reference types="jasmine" />

import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { SessionService } from '../../../core/auth/session.service';
import { SeekerApiService } from '../data/seeker-api.service';
import { SeekerDashboardComponent } from './seeker-dashboard.component';

describe('SeekerDashboardComponent', () => {
  let api: jasmine.SpyObj<SeekerApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<SeekerApiService>('SeekerApiService', [
      'getDashboardSummary', 'getApplicationReadiness', 'getCandidateProfile',
      'getVacancies', 'getApplications', 'getContactRequests', 'getNotifications'
    ]);

    api.getDashboardSummary.and.returnValue(of({
      candidateId: 'candidate-1', applicationCount: 4,
      unreadNotificationCount: 2, pendingContactRequestCount: 1
    }));
    api.getApplicationReadiness.and.returnValue(of({
      isReady: false, profileReady: true, resumeReady: false,
      currentResumeVersionId: null, missingItems: ['Current CV']
    }));
    api.getCandidateProfile.and.returnValue(of({
      fullName: 'Jeni Candidate', location: 'Colombo', experienceMonths: 24,
      education: 'BSc', preferredWorkMode: '', preferredLocation: '',
      willingToRelocate: false, preferredEmploymentType: '', availabilityStatus: '',
      availableFrom: null, noticePeriodDays: null
    }));
    api.getVacancies.and.returnValue(of([]));
    api.getApplications.and.returnValue(of([]));
    api.getContactRequests.and.returnValue(of([]));
    api.getNotifications.and.returnValue(of([]));

    const user = signal({
      userId: 'candidate-1', email: 'jeni@example.test', displayName: 'Jeni Candidate',
      role: 'JobSeeker' as const, accountState: 'Active' as const, emailVerified: true
    });

    await TestBed.configureTestingModule({
      imports: [SeekerDashboardComponent],
      providers: [
        provideRouter([]),
        { provide: SeekerApiService, useValue: api },
        { provide: SessionService, useValue: { user: user.asReadonly() } }
      ]
    }).compileComponents();
  });

  it('renders exactly the three backend dashboard counters', () => {
    const fixture = TestBed.createComponent(SeekerDashboardComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.summary-card').length).toBe(3);
    expect(fixture.componentInstance.summary.applicationCount).toBe(4);
    expect(fixture.componentInstance.summary.pendingContactRequestCount).toBe(1);
    expect(fixture.componentInstance.summary.unreadNotificationCount).toBe(2);
  });

  it('prioritizes the missing current CV as the next factual action', () => {
    const fixture = TestBed.createComponent(SeekerDashboardComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.nextStep.route).toBe('/seeker/cv');
  });
});