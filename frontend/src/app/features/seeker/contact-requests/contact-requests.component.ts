import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { CompanyMonogramComponent } from '../../../shared/avatar/company-monogram.component';
import { EmptyStateVisualComponent } from '../../../shared/states/empty-state-visual.component';
import { ErrorStateComponent } from '../../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../../shared/states/loading-state.component';
import { SeekerWorkflowApiService } from '../data/seeker-workflow-api.service';
import { ContactRequestView } from '../data/seeker-workflow.models';

type ContactFilter = 'All' | 'Pending' | 'Accepted' | 'Closed';
type ContactAction = 'Accepted' | 'Declined' | 'Revoked';

@Component({
  selector: 'app-contact-requests',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CompanyMonogramComponent,
    EmptyStateVisualComponent,
    ErrorStateComponent,
    LoadingStateComponent
  ],
  templateUrl: './contact-requests.component.html',
  styleUrl: './contact-requests.component.css'
})
export class ContactRequestsComponent implements OnInit {
  private readonly api = inject(SeekerWorkflowApiService);

  readonly filters: ContactFilter[] = ['All', 'Pending', 'Accepted', 'Closed'];

  requests: ContactRequestView[] = [];
  filter: ContactFilter = 'All';

  loading = true;
  refreshing = false;
  busyId: string | null = null;

  errorMessage = '';
  conflictMessage = '';
  successMessage = '';

  selectedRequest: ContactRequestView | null = null;
  selectedAction: ContactAction | null = null;

  ngOnInit(): void {
    this.loadRequests();
  }

  get filteredRequests(): ContactRequestView[] {
    if (this.filter === 'All') {
      return this.requests;
    }

    if (this.filter === 'Closed') {
      return this.requests.filter(request =>
        ['Declined', 'Cancelled', 'Revoked'].includes(request.status)
      );
    }

    return this.requests.filter(request => request.status === this.filter);
  }

  loadRequests(showLoader = true): void {
    if (showLoader && this.requests.length === 0) {
      this.loading = true;
    } else {
      this.refreshing = true;
    }

    this.errorMessage = '';

    this.api
      .getContactRequests()
      .pipe(finalize(() => {
        this.loading = false;
        this.refreshing = false;
      }))
      .subscribe({
        next: requests => {
          this.requests = requests;
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(
            error,
            'Unable to load contact requests.'
          );
        }
      });
  }

  setFilter(filter: ContactFilter): void {
    this.filter = filter;
  }

  askAction(request: ContactRequestView, action: ContactAction): void {
    const valid =
      (request.status === 'Pending'
        && (action === 'Accepted' || action === 'Declined'))
      || (request.status === 'Accepted' && action === 'Revoked');

    if (!valid || this.busyId) {
      return;
    }

    this.selectedRequest = request;
    this.selectedAction = action;
  }

  cancelAction(): void {
    if (!this.busyId) {
      this.selectedRequest = null;
      this.selectedAction = null;
    }
  }

  confirmAction(): void {
    const request = this.selectedRequest;
    const action = this.selectedAction;

    if (!request || !action) {
      return;
    }

    this.busyId = request.id;
    this.conflictMessage = '';
    this.successMessage = '';

    this.api
      .updateContactStatus(request.id, action)
      .pipe(finalize(() => {
        this.busyId = null;
        this.selectedRequest = null;
        this.selectedAction = null;
      }))
      .subscribe({
        next: updated => {
          this.requests = this.requests.map(item =>
            item.id === updated.id ? updated : item
          );

          this.successMessage =
            action === 'Accepted'
              ? 'Direct contact sharing is now allowed while the server relationship remains valid.'
              : action === 'Declined'
                ? 'Contact request declined.'
                : 'Direct contact sharing revoked.';
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 409) {
            this.conflictMessage =
              'This request changed before your decision. We refreshed the current server state.';
            this.loadRequests(false);
            return;
          }

          this.errorMessage = this.errorText(
            error,
            'Unable to update this contact request.'
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