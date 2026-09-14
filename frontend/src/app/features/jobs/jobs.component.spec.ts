/// <reference types="jasmine" />

import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { SessionService } from '../../core/auth/session.service';
import { SeekerApiService } from '../seeker/data/seeker-api.service';
import { JobsComponent } from './jobs.component';

describe('JobsComponent', () => {
  let api: jasmine.SpyObj<SeekerApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<SeekerApiService>('SeekerApiService', ['getVacancies']);
    api.getVacancies.and.returnValue(of([]));
    const user = signal(null);

    await TestBed.configureTestingModule({
      imports: [JobsComponent],
      providers: [
        provideRouter([]),
        { provide: SeekerApiService, useValue: api },
        { provide: SessionService, useValue: { user: user.asReadonly() } },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({ q: 'Angular' }) } }
        }
      ]
    }).compileComponents();
  });

  it('uses server-side vacancy discovery with route query filters', () => {
    const fixture = TestBed.createComponent(JobsComponent);
    fixture.detectChanges();

    expect(api.getVacancies).toHaveBeenCalled();
    const [filters, bestMatch] = api.getVacancies.calls.mostRecent().args;
    expect(filters?.q).toBe('Angular');
    expect(bestMatch).toBeFalse();
  });

  it('does not call the Candidate-only Best Match endpoint for anonymous users', () => {
    const fixture = TestBed.createComponent(JobsComponent);
    fixture.detectChanges();
    api.getVacancies.calls.reset();

    fixture.componentInstance.selectPreset('best');

    expect(api.getVacancies).not.toHaveBeenCalled();
    expect(fixture.componentInstance.noticeMessage).toContain('Sign in');
  });
});