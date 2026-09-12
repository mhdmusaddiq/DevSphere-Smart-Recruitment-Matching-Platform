/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { API_BASE_URL } from '../../../core/config/api-base-url';
import { VerifyEmailComponent } from './verify-email.component';

describe('VerifyEmailComponent', () => {
  let component: VerifyEmailComponent;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VerifyEmailComponent],
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
      TestBed.createComponent(VerifyEmailComponent);

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

  it('does not submit an incomplete verification code', () => {
    component.emailControl.setValue('user@example.test');
    setCode('123');

    component.submitVerification();

    http.expectNone(
      'https://api.example.test/api/auth/verify-email'
    );
    expect(component.codeTouched()).toBeTrue();
  });

  it('submits the six-digit code without creating a session', () => {
    component.emailControl.setValue(
      ' user@example.test '
    );
    setCode('482731');

    component.submitVerification();

    const request = http.expectOne(
      'https://api.example.test/api/auth/verify-email'
    );

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'user@example.test',
      code: '482731'
    });

    request.flush({
      message: 'Email verified successfully.'
    });

    expect(component.verified()).toBeTrue();
    expect(component.statusMessage())
      .toBe('Email verified successfully.');
  });

  it('keeps verify failures generic', () => {
    component.emailControl.setValue('user@example.test');
    setCode('111111');

    component.submitVerification();

    const request = http.expectOne(
      'https://api.example.test/api/auth/verify-email'
    );

    request.flush(
      {
        message:
          'The verification challenge is invalid or expired.'
      },
      {
        status: 400,
        statusText: 'Bad Request'
      }
    );

    expect(component.statusMessage()).toBe(
      'The verification challenge is invalid or expired.'
    );
  });

  it('preserves the privacy-safe resend response', () => {
    component.emailControl.setValue(
      ' user@example.test '
    );

    component.resendVerification();

    const request = http.expectOne(
      'https://api.example.test/api/auth/resend-verification'
    );

    expect(request.request.body).toEqual({
      email: 'user@example.test'
    });

    request.flush({
      message:
        'If the account exists, a verification challenge has been processed.',
      developmentCode: '654321'
    });

    expect(component.statusMessage()).toBe(
      'If the account exists, a verification challenge has been processed.'
    );
    expect(component.developmentCode()).toBe('654321');
  });
});
