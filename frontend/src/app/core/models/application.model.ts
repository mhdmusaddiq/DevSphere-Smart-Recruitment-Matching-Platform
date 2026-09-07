export interface JobApplication {
  id: string;
  candidateId: string;
  vacancyId: string;
  status: string;
}

export interface ApplicationStatusHistory {
  id: string;
  jobApplicationId: string;
  previousStatus: string | null;
  newStatus: string;
  changedByUserId: string;
  changedAtUtc: string;
  notes: string;
}