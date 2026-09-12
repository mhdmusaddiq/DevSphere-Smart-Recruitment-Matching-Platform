export interface AdminAccountDashboard {
  totalUsers: number;
  activeUsers: number;
  disabledUsers: number;
  adminUsers: number;
  employerUsers: number;
  jobSeekerUsers: number;
}

export interface AdminUser {
  userId: string;
  email: string;
  displayName: string;
  role: 'JobSeeker' | 'Employer' | 'Admin' | string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface AdminUserPage {
  page: number;
  pageSize: number;
  totalCount: number;
  items: AdminUser[];
}

export interface AdminUserQuery {
  q?: string;
  role?: 'JobSeeker' | 'Employer' | 'Admin';
  isActive?: boolean;
  page: number;
  pageSize: number;
}

export type CompanyVerificationStatus =
  | 'Draft'
  | 'PendingReview'
  | 'Verified'
  | 'NeedsMoreInformation'
  | 'Rejected'
  | 'Suspended';

export interface AdminCompanyVerification {
  id: string;
  companyId: string | null;
  companyName: string;
  status: CompanyVerificationStatus | string;
  evidenceStorageKey: string;
  notes: string;
  submittedAtUtc: string;
  reviewedAtUtc: string | null;
  reviewedByUserId: string | null;
}

export interface AdminSkillAlias {
  id: string;
  skillConceptId: string;
  skillConceptName: string;
  alias: string;
  isActive: boolean;
  skillConceptIsActive: boolean;
}

export interface AdminSkillConcept {
  id: string;
  name: string;
  isActive: boolean;
  aliases: AdminSkillAlias[];
}

export interface AdminOccupationConcept {
  id: string;
  name: string;
  isActive: boolean;
}

export interface AuditEvent {
  id: string;
  userId: string | null;
  action: string;
  entityName: string;
  entityId: string;
  details: string;
  occurredAtUtc: string;
}

export interface ApiProblem {
  message?: string;
}
