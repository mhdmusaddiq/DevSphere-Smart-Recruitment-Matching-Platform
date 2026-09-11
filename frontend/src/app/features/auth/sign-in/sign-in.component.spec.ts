/// <reference types="jasmine" />

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  Router,
  convertToParamMap,
  provideRouter
} from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { SessionService } from '../../../core/auth/session.service';
import { SignInComponent } from './sign-in.component';

describe('SignInComponent', () => {
  let component: SignInComponent;
  let authService: jasmine.SpyObj<AuthService>;
  let session: jasmine.SpyObj<SessionService>;
  let router: Router;
  let navigateSpy: jasmine.Spy;
  let navigateByUrlSpy: jasmine.Spy;

  const activeJobSeeker = {
    userId: 'user-1',
    email: 'user@example.test',
    displayName: 'User One',
    role: 'JobSeeker' as const,
    accountState: 'Active' as const,
    emailVerified: true
  };

  beforeEach(async () => {
    authService = jasmine.createSpyObj<AuthService>(
      'AuthService',
      ['login']
    );

    session = jasmine.createSpyObj<SessionService>(
      'SessionService',
      ['clear']
    );

    await TestBed.configureTestingModule({
      imports: [SignInComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: authService
        },
        {
          provide: SessionService,
          useValue: session
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({})
            }
          }
        }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);

    navigateSpy =
      spyOn(router, 'navigate').and.resolveTo(true);

    navigateByUrlSpy =
      spyOn(router, 'navigateByUrl').and.resolveTo(true);

    const fixture =
      TestBed.createComponent(SignInComponent);

    component = fixture.componentInstance;
  });

  it('creates the sign-in page', () => {
    expect(component).toBeTruthy();
  });

  it('does not submit an invalid form', () => {
    component.submit();

    expect(component.form.touched).toBeTrue();
    expect(authService.login).not.toHaveBeenCalled();
  });

  it('signs in and routes an active job seeker to their dashboard', () => {
    authService.login.and.returnValue(
      of(activeJobSeeker)
    );

    component.form.setValue({
      email: ' user@example.test ',
      password: 'Password123!'
    });

    component.submit();

    expect(authService.login).toHaveBeenCalledWith({
      email: 'user@example.test',
      password: 'Password123!'
    });

    expect(navigateByUrlSpy)
      .toHaveBeenCalledWith('/seeker/dashboard');
  });

  it('shows a generic message for invalid credentials', () => {
    authService.login.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 401
          })
      )
    );

    component.form.setValue({
      email: 'user@example.test',
      password: 'wrong-password'
    });

    component.submit();

    expect(component.submitError()).toBe(
      'We couldn’t sign you in with those details. Check your email and password, or use account recovery.'
    );
  });

  it('toggles password visibility', () => {
    expect(component.passwordVisible()).toBeFalse();

    component.togglePasswordVisibility();
    expect(component.passwordVisible()).toBeTrue();

    component.togglePasswordVisibility();
    expect(component.passwordVisible()).toBeFalse();
  });
});
