export interface Resume {
  id: string;
  candidateProfileId: string;
  currentVersionId: string | null;
  versions: ResumeVersion[];
}

export interface ResumeVersion {
  id: string;
  versionNumber: number;
  originalFileName: string;
  storageKey: string;
  contentType: string;
  fileSizeBytes: number;
  isCurrent: boolean;
}
