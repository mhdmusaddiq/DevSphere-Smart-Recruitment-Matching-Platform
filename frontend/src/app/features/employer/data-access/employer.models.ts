export interface EmployerDashboard {
  employerId: string;
  openVacancyCount: number;
  applicationCount: number;
  pendingContactRequestCount: number;
}

export interface EmployerProfile {
  companyName: string;
  contactEmail: string;
  website: string;
  location: string;
}

export interface CompanyProfile {
  id: string;
  name: string;
  description: string | null;
  website: string | null;
  location: string | null;
  membershipStatus: string;
  verificationStatus: string;
}

export interface CompanyProfileRequest {
  name: string;
  description: string | null;
  website: string | null;
  location: string | null;
}

export interface SubmitCompanyVerificationRequest {
  evidenceStorageKey: string;
  notes: string;
}

export interface CompanyVerificationResult {
  id: string;
  companyId: string;
  status: string;
  submittedAtUtc: string;
}

export interface EmployerVacancySkill {
  name: string;
  weight: number;
}

export interface EmployerVacancy {
  id: string;
  companyId: string | null;
  companyName: string;
  companyVerificationStatus: string;
  title: string;
  description: string;
  location: string;
  workMode: string;
  employmentType: string;
  requiredExperienceMonths: number;
  minExperienceMonths: number;
  maxExperienceMonths: number | null;
  requiredEducation: string;
  salaryMin: number | null;
  salaryMax: number | null;
  closingDateUtc: string | null;
  publishedAtUtc: string | null;
  requiredSkills: EmployerVacancySkill[];
  lifecycleStatus: string;
  isOpen: boolean;
  assessmentStatus: string;
  eligibility: string;
  rawCompatibility: number | null;
  displayCompatibility: number | null;
  highTierAggregate: number | null;
  mediumTierAggregate: number | null;
  coverage: number | null;
}

export interface StoredFileDescriptor {
  storageKey: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
}
