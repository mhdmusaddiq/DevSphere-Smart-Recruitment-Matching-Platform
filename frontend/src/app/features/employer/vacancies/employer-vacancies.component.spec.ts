import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { EmployerApiService } from '../data-access/employer-api.service';
import { EmployerVacancy } from '../data-access/employer.models';
import { EmployerVacanciesComponent } from './employer-vacancies.component';

describe('EmployerVacanciesComponent', () => {
  let fixture: ComponentFixture<EmployerVacanciesComponent>;
  let component: EmployerVacanciesComponent;
  let employerApi: jasmine.SpyObj<EmployerApiService>;

  const makeVacancy = (
    overrides: Partial<EmployerVacancy> = {}
  ): EmployerVacancy => ({
    id: 'vacancy-1',
    companyId: 'company-1',
    companyName: 'AptLens',
    companyVerificationStatus: 'Verified',
    title: 'Software Engineer',
    description: 'Build product features.',
    location: 'Colombo',
    workMode: 'Hybrid',
    employmentType: 'FullTime',
    requiredExperienceMonths: 24,
    minExperienceMonths: 12,
    maxExperienceMonths: 36,
    requiredEducation: 'Bachelor degree',
    salaryMin: 100000,
    salaryMax: 180000,
    closingDateUtc: '2026-10-10T00:00:00Z',
    publishedAtUtc: null,
    requiredSkills: [
      {
        name: 'Angular',
        weight: 1
      }
    ],
    lifecycleStatus: 'Draft',
    isOpen: false,
    assessmentStatus: 'NotAssessed',
    eligibility: 'Unknown',
    rawCompatibility: null,
    displayCompatibility: null,
    highTierAggregate: null,
    mediumTierAggregate: null,
    coverage: null,
    ...overrides
  });

  beforeEach(async () => {
    employerApi = jasmine.createSpyObj<EmployerApiService>(
      'EmployerApiService',
      [
        'getVacancies',
        'publishVacancy',
        'closeVacancy'
      ]
    );

    employerApi.getVacancies.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [EmployerVacanciesComponent],
      providers: [
        provideRouter([]),
        {
          provide: EmployerApiService,
          useValue: employerApi
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(
      EmployerVacanciesComponent
    );
    component = fixture.componentInstance;
  });

  it('loads employer vacancies on init', () => {
    const vacancies = [makeVacancy()];
    employerApi.getVacancies.and.returnValue(of(vacancies));

    fixture.detectChanges();

    expect(employerApi.getVacancies).toHaveBeenCalledTimes(1);
    expect(component.vacancies).toEqual(vacancies);
    expect(component.loading).toBeFalse();
  });

  it('filters vacancies by lifecycle locally', () => {
    const draft = makeVacancy({
      id: 'draft-1',
      lifecycleStatus: 'Draft'
    });
    const published = makeVacancy({
      id: 'published-1',
      lifecycleStatus: 'Published'
    });

    component.vacancies = [draft, published];

    component.setFilter('Published');

    expect(component.filteredVacancies).toEqual([published]);

    component.setFilter('All');

    expect(component.filteredVacancies).toEqual([
      draft,
      published
    ]);
  });

  it('publishes only a Draft vacancy', () => {
    const draft = makeVacancy();
    const published = makeVacancy({
      lifecycleStatus: 'Published',
      isOpen: true,
      publishedAtUtc: '2026-09-12T12:00:00Z'
    });

    component.vacancies = [draft];
    employerApi.publishVacancy.and.returnValue(of(published));

    component.publishVacancy(draft);

    expect(employerApi.publishVacancy)
      .toHaveBeenCalledOnceWith(draft.id);
    expect(component.vacancies[0]).toEqual(published);
    expect(component.actionMessage)
      .toBe('Vacancy published successfully.');
  });

  it('does not publish a non-Draft vacancy', () => {
    const published = makeVacancy({
      lifecycleStatus: 'Published',
      isOpen: true
    });

    component.publishVacancy(published);

    expect(employerApi.publishVacancy)
      .not.toHaveBeenCalled();
  });

  it('closes only a Published vacancy', () => {
    const published = makeVacancy({
      lifecycleStatus: 'Published',
      isOpen: true
    });
    const closed = makeVacancy({
      lifecycleStatus: 'Closed',
      isOpen: false
    });

    component.vacancies = [published];
    employerApi.closeVacancy.and.returnValue(of(closed));

    component.closeVacancy(published);

    expect(employerApi.closeVacancy)
      .toHaveBeenCalledOnceWith(published.id);
    expect(component.vacancies[0]).toEqual(closed);
    expect(component.actionMessage)
      .toBe('Vacancy closed successfully.');
  });

  it('does not close a non-Published vacancy', () => {
    const draft = makeVacancy({
      lifecycleStatus: 'Draft'
    });

    component.closeVacancy(draft);

    expect(employerApi.closeVacancy)
      .not.toHaveBeenCalled();
  });

  it('reloads server state after publish conflict', () => {
    const draft = makeVacancy();

    employerApi.publishVacancy.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: {
              message: 'Vacancy state changed.'
            }
          })
      )
    );

    employerApi.getVacancies.and.returnValue(of([draft]));

    component.publishVacancy(draft);

    expect(employerApi.getVacancies).toHaveBeenCalledTimes(1);
    expect(component.errorMessage)
      .toBe('Vacancy state changed.');
  });

  it('prevents a second lifecycle action while one is running', () => {
    const draft = makeVacancy({
      id: 'draft-1'
    });
    const secondDraft = makeVacancy({
      id: 'draft-2'
    });
    const pending =
      new Subject<EmployerVacancy>();

    employerApi.publishVacancy.and.returnValue(
      pending.asObservable()
    );

    component.publishVacancy(draft);
    component.publishVacancy(secondDraft);

    expect(employerApi.publishVacancy)
      .toHaveBeenCalledTimes(1);
    expect(component.actionVacancyId)
      .toBe('draft-1');

    pending.complete();
  });

  it('renders complete mobile cards with lifecycle-specific actions', () => {
    employerApi.getVacancies.and.returnValue(of([
      makeVacancy({
        id: 'draft-1',
        lifecycleStatus: 'Draft'
      }),
      makeVacancy({
        id: 'published-1',
        lifecycleStatus: 'Published'
      }),
      makeVacancy({
        id: 'closed-1',
        lifecycleStatus: 'Closed'
      })
    ]));

    fixture.detectChanges();

    const cards = Array.from(
      fixture.nativeElement.querySelectorAll('.vacancy-card')
    ) as HTMLElement[];
    const cardText = cards.map(card => card.textContent ?? '');

    expect(cards.length).toBe(3);
    expect(cardText[0]).toContain('Software Engineer');
    expect(cardText[0]).toContain('Colombo');
    expect(cardText[0]).toContain('Angular');
    expect(cardText[0]).toContain('Hybrid');
    expect(cardText[0]).toContain('FullTime');
    expect(cardText[0]).toContain('Draft');
    expect(cardText[0]).toContain('Edit');
    expect(cardText[0]).toContain('Publish');
    expect(cardText[1]).toContain('Manage');
    expect(cardText[1]).toContain('Close');
    expect(cardText[2]).toContain('View');
  });
});
