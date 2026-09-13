const ASSESSMENT_LABELS: Record<string, string> = {
  '1': 'Assessed',
  Calculated: 'Assessed',
  '2': 'Assessment in progress',
  Provisional: 'Assessment in progress',
  '3': 'Not assessed',
  NotCalculated: 'Not assessed',
  '4': 'Assessment unavailable',
  CalculationFailure: 'Assessment unavailable'
};

const ELIGIBILITY_LABELS: Record<string, string> = {
  '1': 'Meets requirements',
  MeetsBaseline: 'Meets requirements',
  '2': 'Verification pending',
  PendingVerification: 'Verification pending',
  '3': 'More information needed',
  IncompleteAssessment: 'More information needed',
  '4': 'Does not meet requirements',
  DoesNotMeetBaseline: 'Does not meet requirements'
};

const CRITERION_LABELS: Record<string, string> = {
  '1': 'Met',
  Met: 'Met',
  '2': 'Not met',
  NotMet: 'Not met',
  '3': 'Not demonstrated',
  NotDemonstrated: 'Not demonstrated',
  '4': 'More information needed',
  Incomplete: 'More information needed',
  '5': 'Verification pending',
  PendingVerification: 'Verification pending',
  '6': 'Not applicable',
  NotApplicable: 'Not applicable'
};

const REASON_LABELS: Record<string, string> = {
  AllMandatoryRequirementsSatisfied: 'All essential requirements are met.',
  MandatoryRequirementNotMet: 'One or more essential requirements are not met.',
  MandatoryVerificationPending: 'Verification is still pending for an essential requirement.',
  IncompleteMandatoryAssessment: 'More information is needed to assess essential requirements.',
  NoActiveScoredRequirements: 'This role does not have an active fit assessment.',
  BlockedByReadiness: 'Complete the requested profile items before applying.',
  CompleteProfile: 'Complete the requested profile items before applying.'
};

const ACTION_LABELS: Record<string, string> = {
  Retry: 'Try again',
  ContactSupport: 'Contact support',
  ViewJobs: 'Browse roles',
  ReviewRequirements: 'Review requirements',
  CompleteProfile: 'Complete your profile',
  AcknowledgeAndApply: 'Acknowledge and apply',
  Apply: 'Apply'
};

export function assessmentStatusLabel(value: string | number | null | undefined): string {
  if (value === null || value === undefined || value === '') return 'Unavailable';
  return ASSESSMENT_LABELS[String(value)] ?? humanize(value);
}

export function eligibilityStatusLabel(value: string | number | null | undefined): string {
  if (value === null || value === undefined || value === '') return 'Unavailable';
  return ELIGIBILITY_LABELS[String(value)] ?? humanize(value);
}

export function criterionStatusLabel(value: string | number): string {
  return CRITERION_LABELS[String(value)] ?? humanize(value);
}

export function matchReasonLabel(value: string | null | undefined): string {
  if (!value) return '';
  return REASON_LABELS[value] ?? humanize(value);
}

export function matchActionLabel(value: string | null | undefined): string {
  if (!value) return '';
  return ACTION_LABELS[value] ?? humanize(value);
}

export function displayLabel(value: string | number | null | undefined): string {
  if (value === null || value === undefined || value === '') return 'Unavailable';
  return humanize(value);
}

export function isCalculatedAssessment(value: string | number | null | undefined): boolean {
  return value === 1 || String(value).toLowerCase() === 'calculated';
}

function humanize(value: string | number): string {
  return String(value)
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .trim();
}
