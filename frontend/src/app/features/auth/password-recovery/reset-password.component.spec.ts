/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { API_BASE_URL } from '../../../core/config/api-base-url';
import { ResetPasswordComponent } from './reset-password.component';

describe('ResetPasswordComponent', () => {
  let component: ResetPasswordComponent;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResetPasswordComponent],
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

    const fixture =
      TestBed.createComponent(ResetPasswordComponent);

    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  function setCode(code: string): void {
    component.codeControls.forEach((control, index) => {
      control.setValue(code[index] ?? '');
    });
  }

  it('does not submit when the code is incomplete', () => {
    component.form.setValue({
      email: 'user@example.test',
      newPassword: 'StrongPassword1!',
      confirmPassword: 'StrongPassword1!'
    });
    setCode('123');

    component.submit();

    http.expectNone(
      'https://api.example.test/api/auth/reset-password'
    );

    expect(component.codeTouched()).toBeTrue();
    expect(component.code).toBe('123');
  });

  it('sends the new password without rewriting it', () => {
    const password = ' StrongPassword1! ';

    component.form.setValue({
      email: ' user@example.test ',
      newPassword: password,
      confirmPassword: password
    });
    setCode('123456');

    component.submit();

    const request = http.expectOne(
      'https://api.example.test/api/auth/reset-password'
    );

    expect(request.request.body).toEqual({
      email: 'user@example.test',
      code: '123456',
      newPassword: password
    });

    request.flush({
      message: 'Password reset successfully.'
    });

    expect(component.completed()).toBeTrue();
    expect(component.statusMessage())
      .toBe('Password reset successfully.');
  });

  it('preserves the password-rejected server truth', () => {
    component.form.setValue({
      email: 'user@example.test',
      newPassword: 'StrongPassword1!',
      confirmPassword: 'StrongPassword1!'
    });
    setCode('123456');

    component.submit();

    const request = http.expectOne(
      'https://api.example.test/api/auth/reset-password'
    );

    request.flush(
      {
        message:
          'The new password does not meet password requirements.',
        errors: [
          {
            code: 'PasswordTooShort',
            description: 'Password policy rejected the value.'
          }
        ]
      },
      {
        status: 400,
        statusText: 'Bad Request'
      }
    );

    expect(component.completed()).toBeFalse();
    expect(component.statusMessage()).toBe(
      'The new password does not meet password requirements.'
    );
    expect(component.serverErrors()).toEqual([
      'Password policy rejected the value.'
    ]);
  });

  it('keeps invalid challenges generic', () => {
    component.form.setValue({
      email: 'user@example.test',
      newPassword: 'StrongPassword1!',
      confirmPassword: 'StrongPassword1!'
    });
    setCode('123456');

    component.submit();

    const request = http.expectOne(
      'https://api.example.test/api/auth/reset-password'
    );

    request.flush(
      {
        message:
          'The password recovery challenge is invalid or expired.'
      },
      {
        status: 400,
        statusText: 'Bad Request'
      }
    );

    expect(component.statusMessage()).toBe(
      'The password recovery challenge is invalid or expired.'
    );
  });
});
