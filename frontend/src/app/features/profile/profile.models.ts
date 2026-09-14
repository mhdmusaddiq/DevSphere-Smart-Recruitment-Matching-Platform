export interface CandidateProfilePayload {
  fullName: string;
  location: string;
  experienceMonths: number;
  education: string;
  preferredWorkMode: string;
  preferredLocation: string;
  willingToRelocate: boolean;
  preferredEmploymentType: string;
  availabilityStatus: string;
  availableFrom: string | null;
  noticePeriodDays: number | null;
}

export interface CandidateSkillRecord {
  id: string;
  name: string;
}

export interface WorkExperienceRecord {
  id: string;
  candidateProfileId: string;
  jobTitle: string;
  companyName: string;
  startDate: string;
  endDate: string | null;
  description: string;
}

export interface EducationRecord {
  id: string;
  candidateProfileId: string;
  institution: string;
  qualification: string;
  fieldOfStudy: string;
  startDate: string;
  endDate: string | null;
}

export interface CertificationRecord {
  id: string;
  candidateProfileId: string;
  name: string;
  issuer: string;
  issuedOn: string;
  expiresOn: string | null;
  credentialUrl: string;
}

export interface ProjectRecord {
  id: string;
  candidateProfileId: string;
  name: string;
  description: string;
  projectUrl: string;
}

export interface LanguageRecord {
  id: string;
  candidateProfileId: string;
  language: string;
  proficiency: string;
}

export interface LicenceRecord {
  id: string;
  candidateProfileId: string;
  type: string;
  class: string;
  issuer: string;
  identifier: string;
  issuedOn: string;
  expiresOn: string | null;
  status: string;
  verificationStatus: string;
}

export interface ProfileReadiness {
  isReady: boolean;
  missingItems: string[];
}

export interface ApplicationReadiness {
  isReady: boolean;
  profileReady: boolean;
  resumeReady: boolean;
  currentResumeVersionId: string | null;
  missingItems: string[];
}