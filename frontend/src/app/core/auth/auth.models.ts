export type PublicRole = 'JobSeeker' | 'Employer' | 'Admin';
export type AccountState =
  | 'Active'
  | 'Disabled'
  | 'PendingEmailVerification';

export interface AuthenticatedUser {
  userId: string;
  email: string;
  displayName: string;
  role: PublicRole;
  accountState: AccountState;
  emailVerified: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  displayName: string;
}
