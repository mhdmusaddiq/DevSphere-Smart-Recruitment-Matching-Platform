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
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import {
  CompanyMonogramComponent
} from '../../../shared/avatar/company-monogram.component';
import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminCompanyVerification,
  CompanyVerificationStatus
} from '../data/admin.models';

@Component({
  selector: 'app-admin-verification-queue',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    CompanyMonogramComponent
  ],
  templateUrl: './admin-verification-queue.component.html',
  styleUrl: './admin-verification-queue.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminVerificationQueueComponent
  implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly searchControl =
    new FormControl('', { nonNullable: true });

  readonly statusControl =
    new FormControl<CompanyVerificationStatus>(
      'PendingReview',
      { nonNullable: true }
    );

  readonly rows =
    signal<AdminCompanyVerification[]>([]);

  readonly loading = signal(true);
  readonly errorMessage =
    signal<string | null>(null);

  readonly statuses:
    CompanyVerificationStatus[] = [
      'PendingReview',
      'Draft',
      'Verified',
      'NeedsMoreInformation',
      'Rejected',
      'Suspended'
    ];

  filteredRows(): AdminCompanyVerification[] {
    const q =
      this.searchControl.value
        .trim()
        .toLowerCase();

    const rows = this.rows();

    if (!q) {
      return rows;
    }

    return rows.filter(row =>
      row.companyName
        .toLowerCase()
        .includes(q)
    );
  }

  ngOnInit(): void {
    this.load();
  }

  apply(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.api
      .getCompanyVerifications(
        this.statusControl.value
      )
      .pipe(
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: rows => {
          this.rows.set(rows);
        },
        error: () => {
          this.rows.set([]);
          this.errorMessage.set(
            'AptLens could not load the company verification queue. Try again.'
          );
        }
      });
  }

  statusLabel(
    status: string
  ): string {
    return status
      .replace(
        /([a-z])([A-Z])/g,
        '$1 $2'
      )
      .replace(/^./, value =>
        value.toUpperCase()
      );
  }
}
