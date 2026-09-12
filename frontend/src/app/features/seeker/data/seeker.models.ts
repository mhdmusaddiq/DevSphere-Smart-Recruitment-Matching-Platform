export interface CandidateDashboardSummary {
  candidateId: string;
  applicationCount: number;
  unreadNotificationCount: number;
  pendingContactRequestCount: number;
}

export interface ApplicationReadiness {
  isReady: boolean;
  profileReady: boolean;
  resumeReady: boolean;
  currentResumeVersionId: string | null;
  missingItems: string[];
}

export interface CandidateProfileView {
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

export interface VacancyRequiredSkillView {
  name: string;
  weight: number;
}

export interface VacancyView {
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
  requiredSkills: VacancyRequiredSkillView[];
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

export type MatchAssessmentStatusValue =
  | 'Calculated'
  | 'Provisional'
  | 'NotCalculated'
  | 'CalculationFailure'
  | 1 | 2 | 3 | 4;

export type MatchEligibilityValue =
  | 'MeetsBaseline'
  | 'PendingVerification'
  | 'IncompleteAssessment'
  | 'DoesNotMeetBaseline'
  | 1 | 2 | 3 | 4;

export type MatchCriterionStateValue =
  | 'Met'
  | 'NotMet'
  | 'NotDemonstrated'
  | 'Incomplete'
  | 'PendingVerification'
  | 'NotApplicable'
  | 1 | 2 | 3 | 4 | 5 | 6;

export interface MatchCriterionView {
  requirementId: string | null;
  alternativeSetId: string | null;
  family: string;
  mode: string;
  importance: string;
  state: MatchCriterionStateValue;
  score: number | null;
  isRegulatoryGate: boolean;
  label: string;
}

export interface MatchFamilyView {
  family: string;
  importance: string;
  rawScore: number | null;
  displayScore: number | null;
  criteria: MatchCriterionView[];
}

export interface MatchResultView {
  assessmentStatus: MatchAssessmentStatusValue;
  eligibility: MatchEligibilityValue;
  eligibilityReason: string | null;
  rawCompatibility: number | null;
  displayCompatibility: number | null;
  highTierAggregate: number | null;
  mediumTierAggregate: number | null;
  coverage: number;
  matchedSkills: string[];
  missingSkills: string[];
  missingInputs: string[];
  families: MatchFamilyView[];
}

export interface ApplyDecisionReasonView {
  code: string;
  message: string;
  targetCta: string;
}

export interface ApplyDecisionView {
  canSubmit: boolean;
  requiresBaselineAcknowledgement: boolean;
  primaryCode: string;
  reasons: ApplyDecisionReasonView[];
  evaluatedAtUtc: string;
  vacancyId: string;
  matchingPolicyRevisionId: string | null;
  matchingPolicyRevisionNumber: number | null;
  resumeVersionId: string | null;
}

export interface JobApplicationView {
  id: string;
  candidateId: string;
  vacancyId: string;
  vacancyTitle: string;
  vacancyLocation: string;
  workMode: string;
  companyId: string | null;
  companyName: string;
  submittedAtUtc: string;
  status: string;
  frozenAssessmentStatus: string;
  displayCompatibility: number | null;
  eligibility: string;
  resumeVersionId: string | null;
  capturedAtUtc: string | null;
  applyDecision: ApplyDecisionView | null;
}

export interface ContactRequestView {
  id: string;
  jobApplicationId: string;
  employerId: string;
  candidateId: string;
  status: string;
  vacancyId: string;
  vacancyTitle: string;
  companyId: string | null;
  companyName: string;
  candidateDisplayName: string;
  employerDisplayName: string;
  canDiscloseContact: boolean;
  candidateEmail: string | null;
}

export interface NotificationView {
  id: string;
  message: string;
  isRead: boolean;
  createdAtUtc: string;
}

export interface ResumeVersionView {
  id: string;
  versionNumber: number;
  originalFileName: string;
  storageKey: string;
  contentType: string;
  fileSizeBytes: number;
  isCurrent: boolean;
}

export interface ResumeView {
  id: string;
  candidateProfileId: string;
  currentVersionId: string | null;
  versions: ResumeVersionView[];
}

export interface VacancyFilters {
  q?: string;
  location?: string;
  skill?: string;
  workMode?: string;
  employmentType?: string;
  page?: number;
  pageSize?: number;
}