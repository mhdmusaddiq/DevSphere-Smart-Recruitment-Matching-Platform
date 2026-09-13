import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import {
  FormControl,
  ReactiveFormsModule
} from '@angular/forms';
import {
  takeUntilDestroyed
} from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import {
  debounceTime,
  distinctUntilChanged,
  finalize
} from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import {
  IdentityAvatarComponent
} from '../../../shared/avatar/identity-avatar.component';
import {
  AdminApiService
} from '../data/admin-api.service';
import {
  AdminUser,
  AdminUserPage,
  ApiProblem
} from '../data/admin.models';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe,
    IdentityAvatarComponent
  ],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminUsersComponent
  implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute, { optional: true });

  readonly searchControl =
    new FormControl('', { nonNullable: true });

  readonly roleControl =
    new FormControl('', { nonNullable: true });

  readonly statusControl =
    new FormControl('', { nonNullable: true });

  readonly page = signal(1);
  readonly pageSize = 25;

  readonly result =
    signal<AdminUserPage | null>(null);

  readonly loading = signal(true);
  readonly pendingUserId =
    signal<string | null>(null);

  readonly errorMessage =
    signal<string | null>(null);

  readonly actionMessage =
    signal<string | null>(null);

  readonly actionError =
    signal<string | null>(null);

  readonly totalPages = computed(() => {
    const total =
      this.result()?.totalCount ?? 0;

    return Math.max(
      1,
      Math.ceil(total / this.pageSize)
    );
  });

  readonly visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.page();

    if (total <= 5) {
      return Array.from(
        { length: total },
        (_, index) => index + 1
      );
    }

    const start = Math.max(
      1,
      Math.min(current - 2, total - 4)
    );

    return Array.from(
      { length: 5 },
      (_, index) => start + index
    );
  });

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.page.set(1);
        this.load();
      });
  }

  ngOnInit(): void {
    const requestedStatus =
      this.route?.snapshot.queryParamMap.get('status');

    if (
      requestedStatus === 'active' ||
      requestedStatus === 'disabled'
    ) {
      this.statusControl.setValue(requestedStatus, {
        emitEvent: false
      });
    }

    this.load();
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  clearFilters(): void {
    this.searchControl.setValue('', {
      emitEvent: false
    });
    this.roleControl.setValue('', {
      emitEvent: false
    });
    this.statusControl.setValue('', {
      emitEvent: false
    });
    this.page.set(1);
    this.load();
  }

  goToPage(page: number): void {
    const next = Math.min(
      Math.max(page, 1),
      this.totalPages()
    );

    if (next === this.page()) {
      return;
    }

    this.page.set(next);
    this.load();
  }

  toggleStatus(user: AdminUser): void {
    if (this.pendingUserId()) {
      return;
    }

    this.pendingUserId.set(user.userId);
    this.actionMessage.set(null);
    this.actionError.set(null);

    const nextStatus = !user.isActive;

    this.api
      .setUserStatus(
        user.userId,
        nextStatus
      )
      .pipe(
        finalize(() => {
          this.pendingUserId.set(null);
        })
      )
      .subscribe({
        next: updated => {
          this.replaceUser(updated);
          this.actionMessage.set(
            `${updated.displayName || updated.email} is now ${
              updated.isActive ? 'active' : 'disabled'
            }.`
          );
        },
        error: (error: HttpErrorResponse) => {
          this.actionError.set(
            this.statusErrorMessage(error)
          );
        }
      });
  }

  load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.api
      .getUsers({
        q: this.searchControl.value,
        role: this.roleValue(),
        isActive: this.activeValue(),
        page: this.page(),
        pageSize: this.pageSize
      })
      .pipe(
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: result => {
          this.result.set(result);

          if (
            result.page > 1 &&
            result.items.length === 0 &&
            result.totalCount > 0
          ) {
            this.page.set(
              Math.max(1, result.page - 1)
            );
            this.load();
          }
        },
        error: () => {
          this.errorMessage.set(
            'AptLens could not load user accounts. Try again.'
          );
        }
      });
  }

  private roleValue():
    | 'JobSeeker'
    | 'Employer'
    | 'Admin'
    | undefined {
    const value = this.roleControl.value;

    if (
      value === 'JobSeeker' ||
      value === 'Employer' ||
      value === 'Admin'
    ) {
      return value;
    }

    return undefined;
  }

  private activeValue():
    boolean | undefined {
    const value = this.statusControl.value;

    if (value === 'active') {
      return true;
    }

    if (value === 'disabled') {
      return false;
    }

    return undefined;
  }

  private replaceUser(updated: AdminUser): void {
    this.result.update(current => {
      if (!current) {
        return current;
      }

      return {
        ...current,
        items: current.items.map(item =>
          item.userId === updated.userId
            ? updated
            : item
        )
      };
    });
  }

  private statusErrorMessage(
    error: HttpErrorResponse
  ): string {
    const problem = error.error as ApiProblem | null;
    const backendMessage =
      typeof problem?.message === 'string'
        ? problem.message.trim()
        : '';

    if (error.status === 409 && backendMessage) {
      return backendMessage;
    }

    if (error.status === 404) {
      return 'The selected account no longer exists.';
    }

    if (error.status === 429) {
      return 'Too many requests. Please wait and try again.';
    }

    return 'AptLens could not change this account status. Try again.';
  }
}
