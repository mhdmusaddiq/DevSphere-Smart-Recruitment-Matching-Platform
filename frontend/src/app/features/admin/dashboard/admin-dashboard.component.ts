import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  finalize,
  forkJoin
} from 'rxjs';

import {
  CompanyMonogramComponent
} from '../../../shared/avatar/company-monogram.component';
import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminAccountDashboard,
  AdminCompanyVerification,
  AuditEvent
} from '../data/admin.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    CompanyMonogramComponent
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminDashboardComponent
  implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly metrics =
    signal<AdminAccountDashboard | null>(null);

  readonly pending =
    signal<AdminCompanyVerification[]>([]);

  readonly auditEvents =
    signal<AuditEvent[]>([]);

  readonly loading = signal(true);
  readonly errorMessage =
    signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    if (this.loading() && this.metrics()) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      metrics: this.api.getDashboard(),
      pending:
        this.api.getPendingCompanyVerifications(),
      auditEvents: this.api.getAuditEvents(5)
    })
      .pipe(
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: result => {
          this.metrics.set(result.metrics);
          this.pending.set(result.pending);
          this.auditEvents.set(result.auditEvents);
        },
        error: () => {
          this.errorMessage.set(
            'AptLens could not load the administration overview. Try again.'
          );
        }
      });
  }
}
