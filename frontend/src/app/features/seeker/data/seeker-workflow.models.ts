export interface SeekerApplication {
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
}

export interface ApplicationSnapshot {
  id: string;
  jobApplicationId: string;
  resumeVersionId: string | null;
  matchingPolicyRevisionId: string | null;
  compatibilityScore: number | null;
  rawCompatibilityScore: number | null;
  displayCompatibilityScore: number | null;
  highTierAggregateScore: number | null;
  mediumTierAggregateScore: number | null;
  coverage: number;
  compatibilityStatus: string;
  eligibilityStatus: string;
  eligibilityReason: string | null;
  isEligible: boolean;
  applyDecision: string;
  matchedSkillsJson: string;
  gapSkillsJson: string;
  evidenceSummaryJson: string;
  matchResultJson: string;
  candidateSnapshotJson: string;
  vacancySnapshotJson: string;
  capturedAtUtc: string;
}

export interface ApplicationStatusHistory {
  id: string;
  jobApplicationId: string;
  previousStatus: string | null;
  newStatus: string;
  changedAtUtc: string;
  notes: string;
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

export interface InterviewView {
  id: string;
  jobApplicationId: string;
  status: string;
  notes: string;
}

export interface InterviewSlotView {
  id: string;
  interviewId: string;
  startsAtUtc: string;
  endsAtUtc: string;
  locationOrMeetingUrl: string;
}

export interface OfferView {
  id: string;
  jobApplicationId: string;
  status: string;
  offeredSalary: number | null;
  expiresAtUtc: string | null;
  extendedAtUtc: string | null;
  notes: string;
}

export interface TalentPoolEntryView {
  id: string;
  jobApplicationId: string;
  candidateUserId: string;
  hasCandidateConsent: boolean;
  consentRecordedAtUtc: string | null;
  isActive: boolean;
  notes: string;
}

export interface CandidateWorkflowSummary {
  jobApplicationId: string;
  interviews: InterviewView[];
  interviewSlots: InterviewSlotView[];
  offers: OfferView[];
  talentPoolEntries: TalentPoolEntryView[];
}