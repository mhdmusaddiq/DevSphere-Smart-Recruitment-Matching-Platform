/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { SessionService } from '../../../core/auth/session.service';
import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerProfileComponent } from './employer-profile.component';

describe('EmployerProfileComponent', () => {
  let component: EmployerProfileComponent;
  let employerApi: jasmine.SpyObj<EmployerApiService>;

  const profile = {
    companyName: 'Example Ltd',
    contactEmail: 'employer@example.test',
    website: 'https://example.test',
    location: 'Colombo'
  };

  const session = {
    user: () => ({
      userId: 'user-1',
      email: 'account@example.test',
      displayName: 'Employer User',
      role: 'Employer',
      accountState: 'Active',
      emailVerified: true
    })
  };

  beforeEach(async () => {
    employerApi = jasmine.createSpyObj<EmployerApiService>(
      'EmployerApiService',
      [
        'getProfile',
        'createProfile',
        'updateProfile'
      ]
    );

    employerApi.getProfile.and.returnValue(of(null));

    await TestBed.configureTestingModule({
      imports: [EmployerProfileComponent],
      providers: [
        {
          provide: EmployerApiService,
          useValue: employerApi
        },
        {
          provide: SessionService,
          useValue: session
        }
      ]
    }).compileComponents();

    const fixture =
      TestBed.createComponent(EmployerProfileComponent);

    component = fixture.componentInstance;
  });

  it('creates the employer profile page', () => {
    expect(component).toBeTruthy();
  });

  it('does not submit an invalid profile form', () => {
    component.submit();

    expect(component.profileForm.touched).toBeTrue();
    expect(employerApi.createProfile).not.toHaveBeenCalled();
    expect(employerApi.updateProfile).not.toHaveBeenCalled();
  });

  it('creates a profile when no profile exists', () => {
    employerApi.createProfile.and.returnValue(of(profile));

    component.profileExists = false;
    component.profileForm.setValue(profile);

    component.submit();

    expect(employerApi.createProfile)
      .toHaveBeenCalledWith(profile);

    expect(employerApi.updateProfile)
      .not.toHaveBeenCalled();

    expect(component.profileExists).toBeTrue();
    expect(component.successMessage)
      .toBe('Employer profile saved successfully.');
  });

  it('updates a profile when one already exists', () => {
    employerApi.updateProfile.and.returnValue(of(profile));

    component.profileExists = true;
    component.profileForm.setValue(profile);

    component.submit();

    expect(employerApi.updateProfile)
      .toHaveBeenCalledWith(profile);

    expect(employerApi.createProfile)
      .not.toHaveBeenCalled();

    expect(component.successMessage)
      .toBe('Employer profile saved successfully.');
  });

  it('keeps account identity separate from employer profile data', () => {
    const user = component.session.user();

    expect(user?.email).toBe('account@example.test');
    expect(profile.contactEmail).toBe('employer@example.test');
    expect(user?.userId).toBe('user-1');
  });
});
