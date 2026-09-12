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

export enum RequirementFamily {
  Skill = 1,
  Experience = 2,
  Education = 3,
  LocationWorkMode = 4,
  Certification = 5,
  LicenceRegistration = 6,
  Language = 7,
  Availability = 8,
  ProjectPortfolio = 9,
  StructuredQuestion = 10
}

export enum RequirementImportance {
  Low = 1,
  Medium = 2,
  High = 3
}

export enum RequirementMode {
  Mandatory = 1,
  Preferred = 2,
  Informational = 3
}

export enum AlternativeSetType {
  AnyOf = 1,
  MinSatisfied = 2
}

export interface MatchingPolicyRevisionDto {
  id: string;
  vacancyId: string;
  revisionNumber: number;
  isCurrent: boolean;
  isMateriallyLocked: boolean;
  materiallyLockedAtUtc: string | null;
}

export interface FamilyPolicyRequest {
  requirementFamily: RequirementFamily;
  familyImportance: RequirementImportance;
  isActive: boolean;
  isScored: boolean;
}

export interface FamilyPolicyDto {
  id: string;
  matchingPolicyRevisionId: string;
  requirementFamily: RequirementFamily;
  familyImportance: RequirementImportance;
  isActive: boolean;
  isScored: boolean;
}

export interface VacancyRequirementRequest {
  familyPolicyId: string;
  requirementFamily: RequirementFamily;
  mode: RequirementMode;
  importance: RequirementImportance;
  isActive: boolean;
  isScored: boolean;
  description: string;
  skillConceptId: string | null;
  canonicalTargetKey: string | null;
  requiredMonths: number | null;
  requiredValue: string | null;
  acceptedValuesJson: string | null;
  isRegulatoryGate: boolean;
  requiresVerification: boolean;
  questionText: string | null;
  expectedAnswer: string | null;
  displayOrder: number;
}

export interface VacancyRequirementPolicyDto
  extends VacancyRequirementRequest {
  id: string;
  vacancyId: string;
  matchingPolicyRevisionId: string;
  alternativeSetId: string | null;
}

export interface AlternativeSetRequest {
  familyPolicyId: string;
  setType: AlternativeSetType;
  minimumSatisfiedCount: number | null;
  mode: RequirementMode;
  importance: RequirementImportance;
  isActive: boolean;
  isScored: boolean;
  displayOrder: number;
  memberRequirementIds: string[];
}

export interface AlternativeSetDto
  extends AlternativeSetRequest {
  id: string;
  matchingPolicyRevisionId: string;
}

export interface VacancyPolicyAggregateDto {
  revision: MatchingPolicyRevisionDto;
  families: FamilyPolicyDto[];
  requirements: VacancyRequirementPolicyDto[];
  alternativeSets: AlternativeSetDto[];
}

export interface FamilyPolicyAggregateUpdate {
  clientKey: string;
  requirementFamily: RequirementFamily;
  familyImportance: RequirementImportance;
  isActive: boolean;
  isScored: boolean;
}

export interface VacancyRequirementAggregateUpdate {
  clientKey: string;
  familyClientKey: string;
  requirementFamily: RequirementFamily;
  mode: RequirementMode;
  importance: RequirementImportance;
  isActive: boolean;
  isScored: boolean;
  description: string;
  skillConceptId: string | null;
  canonicalTargetKey: string | null;
  requiredMonths: number | null;
  requiredValue: string | null;
  acceptedValuesJson: string | null;
  isRegulatoryGate: boolean;
  requiresVerification: boolean;
  questionText: string | null;
  expectedAnswer: string | null;
  displayOrder: number;
}

export interface AlternativeSetAggregateUpdate {
  clientKey: string;
  familyClientKey: string;
  setType: AlternativeSetType;
  minimumSatisfiedCount: number | null;
  mode: RequirementMode;
  importance: RequirementImportance;
  isActive: boolean;
  isScored: boolean;
  displayOrder: number;
  memberRequirementClientKeys: string[];
}

export interface MatchingPolicyAggregateUpdateRequest {
  families: FamilyPolicyAggregateUpdate[];
  requirements: VacancyRequirementAggregateUpdate[];
  alternativeSets: AlternativeSetAggregateUpdate[];
}
export interface EmployerApplyDecisionReason {
  code: string;
  message: string;
  targetCta: string;
}

export interface EmployerApplyDecision {
  canSubmit: boolean;
  requiresBaselineAcknowledgement: boolean;
  primaryCode: string;
  reasons: EmployerApplyDecisionReason[];
  evaluatedAtUtc: string;
  vacancyId: string;
  matchingPolicyRevisionId: string | null;
  matchingPolicyRevisionNumber: number | null;
  resumeVersionId: string | null;
}

export interface EmployerJobApplication {
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
  applyDecision: EmployerApplyDecision | null;
}

export interface EmployerMatchCriterionResult {
  requirementId: string | null;
  alternativeSetId: string | null;
  family: string;
  mode: string;
  importance: string;
  state: string | number;
  score: number | null;
  isRegulatoryGate: boolean;
  label: string;
}

export interface EmployerMatchFamilyResult {
  family: string;
  importance: string;
  rawScore: number | null;
  displayScore: number | null;
  criteria: EmployerMatchCriterionResult[];
}

export interface EmployerRankedApplicant {
  applicationId: string;
  candidateId: string;
  vacancyId: string;
  appliedAt: string;
  status: string;
  rawCompatibility: number | null;
  matchScore: number | null;
  assessmentStatus: string | number;
  eligibility: string | number;
  eligibilityReason: string | null;
  highTierAggregate: number | null;
  mediumTierAggregate: number | null;
  coverage: number;
  matchedSkills: string[];
  missingSkills: string[];
  missingInputs: string[];
  families: EmployerMatchFamilyResult[];
}
export interface EmployerContactRequest {
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
export interface EmployerInterview { id:string; jobApplicationId:string; status:string; notes:string; }
export interface EmployerInterviewSlot { id:string; interviewId:string; startsAtUtc:string; endsAtUtc:string; locationOrMeetingUrl:string; }
export interface EmployerScorecard { id:string; jobApplicationId:string; interviewId:string|null; overallRating:number; notes:string; }
export interface EmployerOffer { id:string; jobApplicationId:string; status:string; offeredSalary:number|null; expiresAtUtc:string|null; extendedAtUtc:string|null; notes:string; }
export interface EmployerTalentPoolEntry { id:string; jobApplicationId:string; candidateUserId:string; hasCandidateConsent:boolean; consentRecordedAtUtc:string|null; isActive:boolean; notes:string; }
export interface EmployerWorkflowSummary {
  jobApplicationId:string;
  interviews:EmployerInterview[];
  interviewSlots:EmployerInterviewSlot[];
  scorecards:EmployerScorecard[];
  offers:EmployerOffer[];
  talentPoolEntries:EmployerTalentPoolEntry[];
}
export interface EmployerNotification { id:string; message:string; isRead:boolean; createdAtUtc:string; }
