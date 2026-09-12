import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { DatePipe } from '@angular/common';
import {
  FormControl,
  ReactiveFormsModule
} from '@angular/forms';
import { finalize } from 'rxjs';

import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AuditEvent,
  SystemSetting
} from '../data/admin.models';

type AdminOperationsTab =
  | 'audit'
  | 'settings'
  | 'trust';

@Component({
  selector: 'app-admin-audit-system',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule
  ],
  templateUrl: './admin-audit-system.component.html',
  styleUrl: './admin-audit-system.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminAuditSystemComponent
  implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly activeTab =
    signal<AdminOperationsTab>('audit');

  readonly auditSearchControl =
    new FormControl('', { nonNullable: true });

  readonly auditTakeControl =
    new FormControl(50, { nonNullable: true });

  readonly auditEvents =
    signal<AuditEvent[]>([]);

  readonly auditLoading = signal(true);
  readonly auditError =
    signal<string | null>(null);

  readonly selectedEvent =
    signal<AuditEvent | null>(null);

  readonly settings =
    signal<SystemSetting[]>([]);

  readonly settingsLoaded = signal(false);
  readonly settingsLoading = signal(false);
  readonly settingsError =
    signal<string | null>(null);

  ngOnInit(): void {
    this.loadAudit();
  }

  setTab(tab: AdminOperationsTab): void {
    this.activeTab.set(tab);

    if (
      tab === 'settings' &&
      !this.settingsLoaded() &&
      !this.settingsLoading()
    ) {
      this.loadSettings();
    }
  }

  loadAudit(): void {
    const take = this.auditTakeControl.value;

    this.auditLoading.set(true);
    this.auditError.set(null);
    this.selectedEvent.set(null);

    this.api
      .getAuditEvents(take)
      .pipe(
        finalize(() => {
          this.auditLoading.set(false);
        })
      )
      .subscribe({
        next: events => {
          this.auditEvents.set(events);
        },
        error: () => {
          this.auditEvents.set([]);
          this.auditError.set(
            'AptLens could not load recent audit events. Try again.'
          );
        }
      });
  }

  loadSettings(): void {
    this.settingsLoading.set(true);
    this.settingsError.set(null);

    this.api
      .getSystemSettings()
      .pipe(
        finalize(() => {
          this.settingsLoading.set(false);
        })
      )
      .subscribe({
        next: settings => {
          this.settings.set(settings);
          this.settingsLoaded.set(true);
        },
        error: () => {
          this.settings.set([]);
          this.settingsError.set(
            'AptLens could not load system settings. No local defaults are being substituted.'
          );
        }
      });
  }

  filteredAuditEvents(): AuditEvent[] {
    const q =
      this.auditSearchControl.value
        .trim()
        .toLowerCase();

    const events = this.auditEvents();

    if (!q) {
      return events;
    }

    return events.filter(event => {
      const haystack = [
        event.action,
        event.entityName,
        event.entityId,
        event.details,
        event.userId ?? ''
      ]
        .join(' ')
        .toLowerCase();

      return haystack.includes(q);
    });
  }

  openDetail(event: AuditEvent): void {
    this.selectedEvent.set(event);
  }

  closeDetail(): void {
    this.selectedEvent.set(null);
  }

  displaySettingValue(
    setting: SystemSetting
  ): string {
    if (setting.isSensitive) {
      return 'Redacted by server';
    }

    return setting.value || 'Not set';
  }

  settingValueClass(
    setting: SystemSetting
  ): string {
    return setting.isSensitive
      ? 'setting-value setting-value--redacted'
      : 'setting-value';
  }
}
