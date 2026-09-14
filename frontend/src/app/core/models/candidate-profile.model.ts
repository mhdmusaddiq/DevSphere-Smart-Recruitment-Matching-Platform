export interface CandidateProfile {
  fullName: string;
  location: string;
  experienceMonths: number;
  education: string;
}

export interface CandidateSkill {
  id: string;
  name: string;
}

export interface WorkExperience {
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
