/// <reference types="jasmine" />

import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { API_BASE_URL } from '../../../core/config/api-base-url';
import { ForgotPasswordComponent } from './forgot-password.component';

describe('ForgotPasswordComponent', () => {
  let component: ForgotPasswordComponent;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ForgotPasswordComponent],
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
      TestBed.createComponent(ForgotPasswordComponent);

    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('does not submit an invalid email', () => {
    component.emailControl.setValue('invalid');

    component.submit();

    http.expectNone(
      'https://api.example.test/api/auth/forgot-password'
    );

    expect(component.emailControl.touched).toBeTrue();
    expect(component.emailControl.invalid).toBeTrue();
  });

  it('preserves the exact generic privacy-safe response', () => {
    component.emailControl.setValue(
      ' user@example.test '
    );

    component.submit();

    const request = http.expectOne(
      'https://api.example.test/api/auth/forgot-password'
    );

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'user@example.test'
    });

    request.flush({
      message:
        'If the account exists, a password recovery challenge has been processed.',
      developmentCode: '123456'
    });

    expect(component.processed()).toBeTrue();
    expect(component.statusMessage()).toBe(
      'If the account exists, a password recovery challenge has been processed.'
    );
    expect(component.developmentCode()).toBe('123456');
  });
});
