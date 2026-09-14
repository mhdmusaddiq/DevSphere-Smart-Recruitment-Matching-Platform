/// <reference types="jasmine" />

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  convertToParamMap
} from '@angular/router';
import {
  of,
  Subject,
  throwError
} from 'rxjs';

import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminUsersComponent
} from './admin-users.component';

describe('AdminUsersComponent', () => {
  let api: jasmine.SpyObj<AdminApiService>;
  let queryParamMap: ReturnType<typeof convertToParamMap>;

  const page = {
    page: 1,
    pageSize: 25,
    totalCount: 1,
    items: [
      {
        userId: 'admin-1',
        email: 'admin@example.test',
        displayName: 'Admin One',
        role: 'Admin',
        isActive: true,
        createdAtUtc: '2026-09-12T00:00:00Z'
      }
    ]
  };

  beforeEach(async () => {
    queryParamMap = convertToParamMap({});
    api = jasmine.createSpyObj<AdminApiService>(
      'AdminApiService',
      [
        'getUsers',
        'setUserStatus'
      ]
    );

    api.getUsers.and.returnValue(of(page));
    api.setUserStatus.and.returnValue(
      of({
        ...page.items[0],
        isActive: false
      })
    );

    await TestBed.configureTestingModule({
      imports: [AdminUsersComponent],
      providers: [
        {
          provide: AdminApiService,
          useValue: api
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              get queryParamMap() {
                return queryParamMap;
              }
            }
          }
        }
      ]
    }).compileComponents();
  });

  it('hydrates status=disabled before the first API load', () => {
    queryParamMap = convertToParamMap({ status: 'disabled' });

    const fixture =
      TestBed.createComponent(AdminUsersComponent);

    fixture.detectChanges();

    expect(api.getUsers).toHaveBeenCalledOnceWith({
      q: '',
      role: undefined,
      isActive: false,
      page: 1,
      pageSize: 25
    });
    expect(fixture.componentInstance.statusControl.value)
      .toBe('disabled');
  });

  it('loads users with server-side pagination', () => {
    const fixture =
      TestBed.createComponent(AdminUsersComponent);

    fixture.detectChanges();

    expect(api.getUsers).toHaveBeenCalledWith({
      q: '',
      role: undefined,
      isActive: undefined,
      page: 1,
      pageSize: 25
    });
    expect(
      fixture.componentInstance.result()?.totalCount
    ).toBe(1);
  });

  it('opens confirmation without disabling when Disable is clicked', () => {
    const fixture =
      TestBed.createComponent(AdminUsersComponent);

    fixture.detectChanges();

    const disableButton = fixture.nativeElement.querySelector(
      '.desktop-table .status-button'
    ) as HTMLButtonElement;

    disableButton.click();
    fixture.detectChanges();

    expect(api.setUserStatus).not.toHaveBeenCalled();
    expect(fixture.componentInstance.disableCandidate())
      .toEqual(page.items[0]);
    expect(fixture.nativeElement.querySelector('[role="dialog"]'))
      .not.toBeNull();
  });

  it('cancels disable confirmation without calling the API', () => {
    const fixture =
      TestBed.createComponent(AdminUsersComponent);

    fixture.detectChanges();

    fixture.componentInstance.toggleStatus(page.items[0]);
    fixture.componentInstance.cancelDisable();

    expect(api.setUserStatus).not.toHaveBeenCalled();
    expect(fixture.componentInstance.disableCandidate()).toBeNull();
  });

  it('confirms disable exactly once while the request is pending', () => {
    const pending = new Subject<typeof page.items[0]>();
    api.setUserStatus.and.returnValue(pending.asObservable());

    const fixture =
      TestBed.createComponent(AdminUsersComponent);

    fixture.detectChanges();

    fixture.componentInstance.toggleStatus(page.items[0]);
    fixture.componentInstance.confirmDisable();
    fixture.componentInstance.confirmDisable();

    expect(api.setUserStatus)
      .toHaveBeenCalledOnceWith(
        'admin-1',
        false
      );
  });

  it('replaces the returned user after a confirmed status update', () => {
    const fixture =
      TestBed.createComponent(AdminUsersComponent);

    fixture.detectChanges();

    fixture.componentInstance.toggleStatus(page.items[0]);
    fixture.componentInstance.confirmDisable();

    expect(
      fixture.componentInstance.result()?.items[0]
        .isActive
    ).toBeFalse();
  });

  it('renders the exact backend 409 safeguard message', () => {
    api.setUserStatus.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: {
              message:
                'Administrators cannot disable their own account.'
            }
          })
      )
    );

    const fixture =
      TestBed.createComponent(AdminUsersComponent);

    fixture.detectChanges();

    fixture.componentInstance.toggleStatus(
      page.items[0]
    );
    fixture.componentInstance.confirmDisable();

    expect(
      fixture.componentInstance.actionError()
    ).toBe(
      'Administrators cannot disable their own account.'
    );
  });
});
