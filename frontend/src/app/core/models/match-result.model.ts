export interface MatchResult {
  matchedSkills: string[];
  missingSkills: string[];
  skillsScore: number;
  experienceScore: number;
  educationScore: number;
  locationScore: number;
  totalScore: number;
}