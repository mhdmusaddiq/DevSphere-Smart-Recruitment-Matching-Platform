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
  AdminOccupationConcept,
  AdminSkillAlias,
  AdminSkillConcept,
  AdminUser,
  AdminUserPage,
  AdminUserQuery,
  AuditEvent,
  CompanyVerificationStatus
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

  getCompanyVerifications(
    status: CompanyVerificationStatus = 'PendingReview'
  ): Observable<AdminCompanyVerification[]> {
    const params = new HttpParams().set(
      'status',
      status
    );

    return this.http.get<AdminCompanyVerification[]>(
      apiUrl(
        this.baseUrl,
        '/admin/company-verifications'
      ),
      { params }
    );
  }

  getPendingCompanyVerifications():
    Observable<AdminCompanyVerification[]> {
    return this.getCompanyVerifications('PendingReview');
  }

  getCompanyVerification(
    verificationId: string
  ): Observable<AdminCompanyVerification> {
    return this.http.get<AdminCompanyVerification>(
      apiUrl(
        this.baseUrl,
        `/admin/company-verifications/${encodeURIComponent(
          verificationId
        )}`
      )
    );
  }

  downloadCompanyVerificationEvidence(
    verificationId: string
  ): Observable<Blob> {
    return this.http.get(
      apiUrl(
        this.baseUrl,
        `/admin/company-verifications/${encodeURIComponent(
          verificationId
        )}/evidence`
      ),
      { responseType: 'blob' }
    );
  }

  reviewCompanyVerification(
    verificationId: string,
    decision:
      | 'Verified'
      | 'NeedsMoreInformation'
      | 'Rejected'
  ): Observable<AdminCompanyVerification> {
    return this.http.put<AdminCompanyVerification>(
      apiUrl(
        this.baseUrl,
        `/admin/company-verifications/${encodeURIComponent(
          verificationId
        )}/review`
      ),
      { decision }
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

  getSkillConcepts(
    includeInactive = true
  ): Observable<AdminSkillConcept[]> {
    const params = new HttpParams().set(
      'includeInactive',
      String(includeInactive)
    );

    return this.http.get<AdminSkillConcept[]>(
      apiUrl(
        this.baseUrl,
        '/admin/catalogue/skills'
      ),
      { params }
    );
  }

  getSkillAliases(
    includeInactive = true
  ): Observable<AdminSkillAlias[]> {
    const params = new HttpParams().set(
      'includeInactive',
      String(includeInactive)
    );

    return this.http.get<AdminSkillAlias[]>(
      apiUrl(
        this.baseUrl,
        '/admin/catalogue/aliases'
      ),
      { params }
    );
  }

  getOccupationConcepts(
    includeInactive = true
  ): Observable<AdminOccupationConcept[]> {
    const params = new HttpParams().set(
      'includeInactive',
      String(includeInactive)
    );

    return this.http.get<AdminOccupationConcept[]>(
      apiUrl(
        this.baseUrl,
        '/admin/catalogue/occupations'
      ),
      { params }
    );
  }

  setSkillConceptStatus(
    conceptId: string,
    isActive: boolean
  ): Observable<AdminSkillConcept> {
    return this.http.put<AdminSkillConcept>(
      apiUrl(
        this.baseUrl,
        `/admin/catalogue/skills/${encodeURIComponent(
          conceptId
        )}/status`
      ),
      { isActive }
    );
  }

  setSkillAliasStatus(
    aliasId: string,
    isActive: boolean
  ): Observable<AdminSkillAlias> {
    return this.http.put<AdminSkillAlias>(
      apiUrl(
        this.baseUrl,
        `/admin/catalogue/aliases/${encodeURIComponent(
          aliasId
        )}/status`
      ),
      { isActive }
    );
  }

  setOccupationConceptStatus(
    occupationId: string,
    isActive: boolean
  ): Observable<AdminOccupationConcept> {
    return this.http.put<AdminOccupationConcept>(
      apiUrl(
        this.baseUrl,
        `/admin/catalogue/occupations/${encodeURIComponent(
          occupationId
        )}/status`
      ),
      { isActive }
    );
  }
}
