/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  Router,
  provideRouter
} from '@angular/router';

import { API_BASE_URL } from '../../../core/config/api-base-url';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let http: HttpTestingController;
  let router: Router;
  let navigateSpy: jasmine.Spy;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: API_BASE_URL,
          useValue: 'https://api.example.test/api'
        }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);

    navigateSpy =
      spyOn(router, 'navigate').and.resolveTo(true);

    const fixture =
      TestBed.createComponent(RegisterComponent);

    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('creates the register page', () => {
    expect(component).toBeTruthy();
  });

  it('defaults to JobSeeker registration', () => {
    expect(component.form.controls.role.value)
      .toBe('JobSeeker');
  });

  it('does not submit invalid details', () => {
    component.submit();

    expect(component.form.touched).toBeTrue();
  });

  it('rejects passwords that do not meet requirements', () => {
    component.form.patchValue({
      password: 'short',
      confirmPassword: 'short'
    });

    expect(
      component.form.controls.password.invalid
    ).toBeTrue();
  });

  it('submits valid registration details', () => {
    component.form.setValue({
      role: 'Employer',
      displayName: ' Example Employer ',
      email: ' employer@example.test ',
      password: 'StrongPassword1!',
      confirmPassword: 'StrongPassword1!'
    });

    component.submit();

    const request = http.expectOne(
      'https://api.example.test/api/auth/register'
    );

    expect(request.request.method).toBe('POST');

    expect(request.request.body).toEqual({
      displayName: 'Example Employer',
      email: 'employer@example.test',
      password: 'StrongPassword1!',
      role: 'Employer'
    });

    request.flush({
      email: 'employer@example.test',
      displayName: 'Example Employer',
      accountState: 'PendingEmailVerification',
      emailVerificationRequired: true
    });

    expect(navigateSpy).toHaveBeenCalledWith(
      ['/verify-email'],
      {
        queryParams: {
          email: 'employer@example.test'
        }
      }
    );
  });
});
