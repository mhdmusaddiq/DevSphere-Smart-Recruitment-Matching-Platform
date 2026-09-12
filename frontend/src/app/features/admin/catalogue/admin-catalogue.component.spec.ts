/// <reference types="jasmine" />

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import {
  of,
  throwError
} from 'rxjs';

import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminCatalogueComponent
} from './admin-catalogue.component';

describe('AdminCatalogueComponent', () => {
  let api: jasmine.SpyObj<AdminApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<AdminApiService>(
      'AdminApiService',
      [
        'getSkillConcepts',
        'getSkillAliases',
        'getOccupationConcepts',
        'setSkillConceptStatus',
        'setSkillAliasStatus',
        'setOccupationConceptStatus'
      ]
    );

    api.getSkillConcepts.and.returnValue(
      of([
        {
          id: 'skill-1',
          name: 'Angular',
          isActive: true,
          aliases: []
        }
      ])
    );

    api.getSkillAliases.and.returnValue(
      of([
        {
          id: 'alias-1',
          skillConceptId: 'skill-1',
          skillConceptName: 'Angular',
          alias: 'AngularJS',
          isActive: true,
          skillConceptIsActive: false
        }
      ])
    );

    api.getOccupationConcepts.and.returnValue(
      of([])
    );

    api.setSkillConceptStatus.and.returnValue(
      of({
        id: 'skill-1',
        name: 'Angular',
        isActive: false,
        aliases: []
      })
    );

    await TestBed.configureTestingModule({
      imports: [AdminCatalogueComponent],
      providers: [
        {
          provide: AdminApiService,
          useValue: api
        }
      ]
    }).compileComponents();
  });

  it('loads all catalogue types including inactive records', () => {
    const fixture =
      TestBed.createComponent(AdminCatalogueComponent);

    fixture.detectChanges();

    expect(api.getSkillConcepts)
      .toHaveBeenCalledOnceWith(true);
    expect(api.getSkillAliases)
      .toHaveBeenCalledOnceWith(true);
    expect(api.getOccupationConcepts)
      .toHaveBeenCalledOnceWith(true);
  });

  it('computes alias availability only for display from alias and parent activity', () => {
    const fixture =
      TestBed.createComponent(AdminCatalogueComponent);

    fixture.detectChanges();

    expect(
      fixture.componentInstance.aliasAvailable(
        fixture.componentInstance.aliases()[0]
      )
    ).toBeFalse();
  });

  it('updates current skill availability using only isActive', () => {
    const fixture =
      TestBed.createComponent(AdminCatalogueComponent);

    fixture.detectChanges();

    const skill =
      fixture.componentInstance.skills()[0];

    fixture.componentInstance
      .requestSkillChange(skill);
    fixture.componentInstance
      .confirmChange();

    expect(api.setSkillConceptStatus)
      .toHaveBeenCalledOnceWith(
        'skill-1',
        false
      );
    expect(
      fixture.componentInstance.skills()[0]
        .isActive
    ).toBeFalse();
  });

  it('handles a stale catalogue target without inventing a local replacement', () => {
    api.setSkillConceptStatus.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 404
          })
      )
    );

    const fixture =
      TestBed.createComponent(AdminCatalogueComponent);

    fixture.detectChanges();

    fixture.componentInstance
      .requestSkillChange(
        fixture.componentInstance.skills()[0]
      );
    fixture.componentInstance
      .confirmChange();

    expect(
      fixture.componentInstance.actionError()
    ).toContain('no longer exists');
  });
});
