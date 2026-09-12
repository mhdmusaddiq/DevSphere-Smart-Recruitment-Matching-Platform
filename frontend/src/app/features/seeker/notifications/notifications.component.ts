import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { EmptyStateVisualComponent } from '../../../shared/states/empty-state-visual.component';
import { ErrorStateComponent } from '../../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../../shared/states/loading-state.component';
import { SeekerWorkflowApiService } from '../data/seeker-workflow-api.service';
import { NotificationView } from '../data/seeker-workflow.models';

type NotificationFilter = 'All' | 'Unread';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    EmptyStateVisualComponent,
    ErrorStateComponent,
    LoadingStateComponent
  ],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css'
})
export class NotificationsComponent implements OnInit {
  private readonly api = inject(SeekerWorkflowApiService);

  notifications: NotificationView[] = [];
  filter: NotificationFilter = 'All';

  loading = true;
  refreshing = false;
  markingId: string | null = null;

  errorMessage = '';
  rowError = '';

  ngOnInit(): void {
    this.loadNotifications();
  }

  get filteredNotifications(): NotificationView[] {
    return this.filter === 'Unread'
      ? this.notifications.filter(notification => !notification.isRead)
      : this.notifications;
  }

  loadNotifications(showLoader = true): void {
    if (showLoader && this.notifications.length === 0) {
      this.loading = true;
    } else {
      this.refreshing = true;
    }

    this.errorMessage = '';

    this.api
      .getNotifications()
      .pipe(finalize(() => {
        this.loading = false;
        this.refreshing = false;
      }))
      .subscribe({
        next: notifications => {
          this.notifications = notifications;
          this.rowError = '';
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(
            error,
            'Unable to load notifications.'
          );
        }
      });
  }

  setFilter(filter: NotificationFilter): void {
    this.filter = filter;
  }

  markRead(notification: NotificationView): void {
    if (notification.isRead || this.markingId) {
      return;
    }

    this.markingId = notification.id;
    this.rowError = '';

    this.api
      .markNotificationRead(notification.id)
      .pipe(finalize(() => (this.markingId = null)))
      .subscribe({
        next: () => {
          this.notifications = this.notifications.map(item =>
            item.id === notification.id
              ? { ...item, isRead: true }
              : item
          );
        },
        error: (error: HttpErrorResponse) => {
          this.rowError = this.errorText(
            error,
            'Unable to mark this notification as read. It remains unread.'
          );
        }
      });
  }

  private errorText(error: HttpErrorResponse, fallback: string): string {
    return typeof error.error?.message === 'string'
      ? error.error.message
      : fallback;
  }
}