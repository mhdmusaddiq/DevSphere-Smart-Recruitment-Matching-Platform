/// <reference types="jasmine" />

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  convertToParamMap,
  provideRouter
} from '@angular/router';
import {
  of,
  throwError
} from 'rxjs';

import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminVerificationDetailComponent
} from './admin-verification-detail.component';

describe('AdminVerificationDetailComponent', () => {
  let api: jasmine.SpyObj<AdminApiService>;

  const pending = {
    id: 'verification-1',
    companyId: 'company-1',
    companyName: 'Lanka Analytics',
    status: 'PendingReview',
    evidenceStorageKey: 'private/path',
    notes: 'Owner note',
    submittedAtUtc: '2026-09-12T00:00:00Z',
    reviewedAtUtc: null,
    reviewedByUserId: null
  };

  beforeEach(async () => {
    api = jasmine.createSpyObj<AdminApiService>(
      'AdminApiService',
      [
        'getCompanyVerification',
        'downloadCompanyVerificationEvidence',
        'reviewCompanyVerification'
      ]
    );

    api.getCompanyVerification.and.returnValue(
      of(pending)
    );

    api.reviewCompanyVerification.and.returnValue(
      of({
        ...pending,
        status: 'Verified',
        reviewedAtUtc: '2026-09-12T01:00:00Z',
        reviewedByUserId: 'admin-1'
      })
    );

    await TestBed.configureTestingModule({
      imports: [AdminVerificationDetailComponent],
      providers: [
        provideRouter([]),
        {
          provide: AdminApiService,
          useValue: api
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({
                verificationId: 'verification-1'
              })
            }
          }
        }
      ]
    }).compileComponents();
  });

  it('loads factual detail by verification id', () => {
    const fixture =
      TestBed.createComponent(
        AdminVerificationDetailComponent
      );

    fixture.detectChanges();

    expect(
      api.getCompanyVerification
    ).toHaveBeenCalledOnceWith(
      'verification-1'
    );
    expect(
      fixture.componentInstance.verification()?.companyName
    ).toBe('Lanka Analytics');
  });

  it('records only the server-backed Verified decision', () => {
    const fixture =
      TestBed.createComponent(
        AdminVerificationDetailComponent
      );

    fixture.detectChanges();
    fixture.componentInstance.beginVerify();
    fixture.componentInstance.verify();

    expect(
      api.reviewCompanyVerification
    ).toHaveBeenCalledOnceWith(
      'verification-1',
      'Verified'
    );
    expect(
      fixture.componentInstance.verification()?.status
    ).toBe('Verified');
  });

  it('renders only the server-backed Verify company action while pending', () => {
    const fixture =
      TestBed.createComponent(
        AdminVerificationDetailComponent
      );

    fixture.detectChanges();

    const actionElements =
      fixture.nativeElement.querySelectorAll(
        '.decision-actions button'
      ) as NodeListOf<HTMLButtonElement>;
    const actions = Array.from(actionElements)
      .map(button => button.textContent?.trim());

    expect(actions).toEqual(['Verify company']);
  });

  it('treats a 409 as stale server state', () => {
    api.reviewCompanyVerification.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: {
              message:
                'Company verification is no longer pending review.'
            }
          })
      )
    );

    const fixture =
      TestBed.createComponent(
        AdminVerificationDetailComponent
      );

    fixture.detectChanges();
    fixture.componentInstance.beginVerify();
    fixture.componentInstance.verify();

    expect(
      fixture.componentInstance.reviewError()
    ).toBe(
      'Company verification is no longer pending review.'
    );
  });
});
