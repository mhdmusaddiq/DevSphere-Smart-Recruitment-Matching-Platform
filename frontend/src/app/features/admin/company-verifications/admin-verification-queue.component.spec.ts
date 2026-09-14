/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminVerificationQueueComponent
} from './admin-verification-queue.component';

describe('AdminVerificationQueueComponent', () => {
  let api: jasmine.SpyObj<AdminApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<AdminApiService>(
      'AdminApiService',
      ['getCompanyVerifications']
    );

    api.getCompanyVerifications.and.returnValue(
      of([
        {
          id: 'verification-1',
          companyId: 'company-1',
          companyName: 'Lanka Analytics',
          status: 'PendingReview',
          evidenceStorageKey: 'private/path',
          notes: 'Owner note',
          submittedAtUtc: '2026-09-12T00:00:00Z',
          reviewedAtUtc: null,
          reviewedByUserId: null
        }
      ])
    );

    await TestBed.configureTestingModule({
      imports: [AdminVerificationQueueComponent],
      providers: [
        provideRouter([]),
        {
          provide: AdminApiService,
          useValue: api
        }
      ]
    }).compileComponents();
  });

  it('loads the server PendingReview queue by default', () => {
    const fixture =
      TestBed.createComponent(
        AdminVerificationQueueComponent
      );

    fixture.detectChanges();

    expect(
      api.getCompanyVerifications
    ).toHaveBeenCalledOnceWith(
      'PendingReview'
    );

    expect(
      fixture.componentInstance.filteredRows().length
    ).toBe(1);
  });

  it('filters the loaded queue by company name without inventing server rows', () => {
    const fixture =
      TestBed.createComponent(
        AdminVerificationQueueComponent
      );

    fixture.detectChanges();

    fixture.componentInstance.searchControl
      .setValue('coastal');

    expect(
      fixture.componentInstance.filteredRows()
    ).toEqual([]);
  });
});
