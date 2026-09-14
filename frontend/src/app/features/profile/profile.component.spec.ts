/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { CareerProfileApiService } from './career-profile-api.service';
import { ProfileComponent } from './profile.component';

describe('ProfileComponent S06', () => {
  const api = jasmine.createSpyObj<CareerProfileApiService>(
    'CareerProfileApiService',
    [
      'getProfile',
      'getSkills',
      'getWorkExperiences',
      'getEducation',
      'getCertifications',
      'getLicences',
      'getLanguages',
      'getProjects',
      'getProfileReadiness',
      'getApplicationReadiness',
      'createProfile',
      'updateProfile',
      'addSkill',
      'deleteSkill',
      'addWorkExperience',
      'updateWorkExperience',
      'deleteWorkExperience',
      'addEducation',
      'updateEducation',
      'deleteEducation',
      'addCertification',
      'updateCertification',
      'deleteCertification',
      'addLicence',
      'updateLicence',
      'deleteLicence',
      'addLanguage',
      'updateLanguage',
      'deleteLanguage',
      'addProject',
      'updateProject',
      'deleteProject'
    ]
  );

  beforeEach(async () => {
    api.getProfile.and.returnValue(of({
      fullName: 'Maya K',
      location: 'Colombo',
      experienceMonths: 30,
      education: 'BSc IT',
      preferredWorkMode: 'Hybrid',
      preferredLocation: 'Colombo',
      willingToRelocate: false,
      preferredEmploymentType: 'Full-time',
      availabilityStatus: 'NoticePeriod',
      availableFrom: null,
      noticePeriodDays: 14
    }));
    api.getSkills.and.returnValue(of([]));
    api.getWorkExperiences.and.returnValue(of([]));
    api.getEducation.and.returnValue(of([]));
    api.getCertifications.and.returnValue(of([]));
    api.getLicences.and.returnValue(of([]));
    api.getLanguages.and.returnValue(of([]));
    api.getProjects.and.returnValue(of([]));
    api.getProfileReadiness.and.returnValue(of({
      isReady: true,
      missingItems: []
    }));
    api.getApplicationReadiness.and.returnValue(of({
      isReady: false,
      profileReady: true,
      resumeReady: false,
      currentResumeVersionId: null,
      missingItems: ['Current resume version']
    }));

    await TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [
        provideRouter([]),
        { provide: CareerProfileApiService, useValue: api }
      ]
    }).compileComponents();
  });

  it('uses FormArrays for required structured evidence editors', () => {
    const fixture = TestBed.createComponent(ProfileComponent);
    fixture.detectChanges();

    const component = fixture.componentInstance;

    expect(component.workArray).toBeDefined();
    expect(component.educationArray).toBeDefined();
    expect(component.certificationArray).toBeDefined();
    expect(component.profileForm.value.preferredWorkMode).toBe('Hybrid');
  });

  it('renders readiness as server truth without a fake percentage', () => {
    const fixture = TestBed.createComponent(ProfileComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Profile ready');
    expect(text).toContain('Resume required');
    expect(text).toContain('Current resume version');
    expect(text).not.toContain('%');
  });
});