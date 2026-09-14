/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminAuditSystemComponent
} from './admin-audit-system.component';

describe('AdminAuditSystemComponent', () => {
  let api: jasmine.SpyObj<AdminApiService>;

  const auditEvents = [
    {
      id: 'audit-1',
      userId: 'admin-user-id',
      action: 'CompanyVerificationReviewed',
      entityName: 'CompanyVerification',
      entityId: 'verification-1',
      details: 'Status changed to Verified.',
      occurredAtUtc: '2026-09-11T07:42:00Z'
    }
  ];

  beforeEach(async () => {
    api = jasmine.createSpyObj<AdminApiService>(
      'AdminApiService',
      [
        'getAuditEvents',
        'getSystemSettings'
      ]
    );

    api.getAuditEvents.and.returnValue(
      of(auditEvents)
    );

    api.getSystemSettings.and.returnValue(
      of([
        {
          id: 'setting-1',
          key: 'PublicSetting',
          value: 'Enabled',
          description: 'Visible setting',
          isSensitive: false
        },
        {
          id: 'setting-2',
          key: 'JwtSecret',
          value: 'should-never-render',
          description: 'Sensitive setting',
          isSensitive: true
        }
      ])
    );

    await TestBed.configureTestingModule({
      imports: [AdminAuditSystemComponent],
      providers: [
        {
          provide: AdminApiService,
          useValue: api
        }
      ]
    }).compileComponents();
  });

  it('loads the latest 50 audit events by default', () => {
    const fixture =
      TestBed.createComponent(
        AdminAuditSystemComponent
      );

    fixture.detectChanges();

    expect(api.getAuditEvents)
      .toHaveBeenCalledOnceWith(50);
    expect(
      fixture.componentInstance.auditEvents().length
    ).toBe(1);
  });

  it('renders only the Audit events and System settings release tabs', () => {
    const fixture =
      TestBed.createComponent(
        AdminAuditSystemComponent
      );

    fixture.detectChanges();

    const tabElements =
      fixture.nativeElement.querySelectorAll(
        '[role="tab"]'
      ) as NodeListOf<HTMLElement>;
    const tabs = Array.from(tabElements)
      .map(tab => tab.textContent?.trim());

    expect(tabs).toEqual([
      'Audit events',
      'System settings'
    ]);
  });

  it('filters only the currently loaded audit window', () => {
    const fixture =
      TestBed.createComponent(
        AdminAuditSystemComponent
      );

    fixture.detectChanges();

    fixture.componentInstance.auditSearchControl
      .setValue('application');

    expect(
      fixture.componentInstance.filteredAuditEvents()
    ).toEqual([]);

    fixture.componentInstance.auditSearchControl
      .setValue('verified');

    expect(
      fixture.componentInstance.filteredAuditEvents()
        .length
    ).toBe(1);
  });

  it('loads system settings only when the settings tab is opened', () => {
    const fixture =
      TestBed.createComponent(
        AdminAuditSystemComponent
      );

    fixture.detectChanges();

    expect(api.getSystemSettings)
      .not.toHaveBeenCalled();

    fixture.componentInstance
      .setTab('settings');

    expect(api.getSystemSettings)
      .toHaveBeenCalledTimes(1);
    expect(
      fixture.componentInstance.settingsLoaded()
    ).toBeTrue();
  });

  it('never renders a sensitive setting value even if a bad payload contains one', () => {
    const fixture =
      TestBed.createComponent(
        AdminAuditSystemComponent
      );

    fixture.detectChanges();
    fixture.componentInstance
      .setTab('settings');

    const sensitive =
      fixture.componentInstance.settings()[1];

    expect(
      fixture.componentInstance
        .displaySettingValue(sensitive)
    ).toBe('Redacted by server');

    expect(
      fixture.componentInstance
        .displaySettingValue(sensitive)
    ).not.toContain('should-never-render');
  });
});
