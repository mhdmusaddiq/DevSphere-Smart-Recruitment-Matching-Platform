import {
  HttpClient,
  HttpParams
} from '@angular/common/http';
import {
  Injectable,
  inject
} from '@angular/core';
import { Observable } from 'rxjs';

import {
  API_BASE_URL,
  apiUrl
} from '../../../core/config/api-base-url';
import {
  AdminAccountDashboard,
  AdminCompanyVerification,
  AdminUser,
  AdminUserPage,
  AdminUserQuery,
  AuditEvent
} from './admin.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getDashboard(): Observable<AdminAccountDashboard> {
    return this.http.get<AdminAccountDashboard>(
      apiUrl(this.baseUrl, '/admin/dashboard')
    );
  }

  getPendingCompanyVerifications():
    Observable<AdminCompanyVerification[]> {
    const params = new HttpParams().set(
      'status',
      'PendingReview'
    );

    return this.http.get<AdminCompanyVerification[]>(
      apiUrl(
        this.baseUrl,
        '/admin/company-verifications'
      ),
      { params }
    );
  }

  getAuditEvents(
    take = 5
  ): Observable<AuditEvent[]> {
    const params = new HttpParams().set(
      'take',
      String(take)
    );

    return this.http.get<AuditEvent[]>(
      apiUrl(this.baseUrl, '/admin/audit-events'),
      { params }
    );
  }

  getUsers(
    query: AdminUserQuery
  ): Observable<AdminUserPage> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    const q = query.q?.trim();

    if (q) {
      params = params.set('q', q);
    }

    if (query.role) {
      params = params.set('role', query.role);
    }

    if (query.isActive !== undefined) {
      params = params.set(
        'isActive',
        String(query.isActive)
      );
    }

    return this.http.get<AdminUserPage>(
      apiUrl(this.baseUrl, '/admin/users'),
      { params }
    );
  }

  setUserStatus(
    userId: string,
    isActive: boolean
  ): Observable<AdminUser> {
    return this.http.put<AdminUser>(
      apiUrl(
        this.baseUrl,
        `/admin/users/${encodeURIComponent(userId)}/status`
      ),
      { isActive }
    );
  }
}
