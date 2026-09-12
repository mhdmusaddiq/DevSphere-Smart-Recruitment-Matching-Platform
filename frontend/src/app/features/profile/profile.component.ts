import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import {
  FormsModule,
  ReactiveFormsModule,
  UntypedFormArray,
  UntypedFormBuilder,
  UntypedFormGroup,
  Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Observable, finalize } from 'rxjs';

import { ErrorStateComponent } from '../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../shared/states/loading-state.component';
import { CareerProfileApiService } from './career-profile-api.service';
import {
  ApplicationReadiness,
  CandidateProfilePayload,
  CandidateSkillRecord,
  CertificationRecord,
  EducationRecord,
  LanguageRecord,
  LicenceRecord,
  ProfileReadiness,
  ProjectRecord,
  WorkExperienceRecord
} from './profile.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
  ReactiveFormsModule,
    RouterLink,
    LoadingStateComponent,
    ErrorStateComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  private readonly api = inject(CareerProfileApiService);

  readonly sections = [
    ['profile-basics', 'Profile basics'],
    ['skills', 'Skills'],
    ['work-experience', 'Work experience'],
    ['education', 'Education'],
    ['certifications', 'Certifications'],
    ['licences', 'Licences'],
    ['languages', 'Languages'],
    ['projects', 'Projects'],
    ['preferences', 'Preferences'],
    ['availability', 'Availability']
  ] as const;

  readonly profileForm = this.fb.group({
    fullName: ['', Validators.required],
    location: [''],
    experienceMonths: [0, [Validators.required, Validators.min(0)]],
    education: [''],
    preferredWorkMode: [''],
    preferredLocation: [''],
    willingToRelocate: [false],
    preferredEmploymentType: [''],
    availabilityStatus: [''],
    availableFrom: [''],
    noticePeriodDays: [null, Validators.min(0)]
  });

  readonly workExperiences = this.fb.array([]);
  readonly educationRecords = this.fb.array([]);
  readonly certifications = this.fb.array([]);
  readonly licences = this.fb.array([]);
  readonly languages = this.fb.array([]);
  readonly projects = this.fb.array([]);

  skills: CandidateSkillRecord[] = [];
  skillName = '';

  profileReadiness: ProfileReadiness | null = null;
  applicationReadiness: ApplicationReadiness | null = null;

  loading = true;
  profileExists = false;
  savingProfile = false;
  successMessage = '';
  errorMessage = '';

  private readonly busyKeys = new Set<string>();

  ngOnInit(): void {
    this.loadInitial();
  }

  get workArray(): UntypedFormArray {
    return this.workExperiences;
  }

  get educationArray(): UntypedFormArray {
    return this.educationRecords;
  }

  get certificationArray(): UntypedFormArray {
    return this.certifications;
  }

  get licenceArray(): UntypedFormArray {
    return this.licences;
  }

  get languageArray(): UntypedFormArray {
    return this.languages;
  }

  get projectArray(): UntypedFormArray {
    return this.projects;
  }

  get readinessMissingItems(): string[] {
    return [
      ...new Set([
        ...(this.profileReadiness?.missingItems ?? []),
        ...(this.applicationReadiness?.missingItems ?? [])
      ])
    ];
  }

  loadInitial(): void {
    this.loading = true;
    this.errorMessage = '';

    this.api.getProfile().subscribe({
      next: profile => {
        if (profile) {
          this.profileExists = true;
          this.profileForm.patchValue(profile);
          this.loadEvidence();
        } else {
          this.profileExists = false;
        }

        this.loading = false;
        this.loadReadiness();
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;

        if (error.status === 404) {
          this.profileExists = false;
          this.loadReadiness();
          return;
        }

        this.errorMessage = this.errorText(
          error,
          'Unable to load your career profile.'
        );
      }
    });
  }

  saveProfile(): void {
    this.profileForm.markAllAsTouched();

    if (this.profileForm.invalid) {
      this.errorMessage = 'Check the highlighted profile fields.';
      return;
    }

    const payload = this.profilePayload();
    const request = this.profileExists
      ? this.api.updateProfile(payload)
      : this.api.createProfile(payload);

    this.savingProfile = true;
    this.clearMessages();

    request
      .pipe(finalize(() => (this.savingProfile = false)))
      .subscribe({
        next: profile => {
          this.profileExists = true;
          this.profileForm.patchValue(profile);
          this.successMessage = 'Career profile saved.';
          this.loadEvidence();
          this.loadReadiness();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(
            error,
            'Unable to save your career profile.'
          );
        }
      });
  }

  addSkill(): void {
    const name = this.skillName.trim();

    if (!name || this.isBusy('skill:add')) {
      return;
    }

    this.setBusy('skill:add', true);
    this.clearMessages();

    this.api
      .addSkill(name)
      .pipe(finalize(() => this.setBusy('skill:add', false)))
      .subscribe({
        next: skill => {
          this.skills = [...this.skills, skill].sort((a, b) =>
            a.name.localeCompare(b.name)
          );
          this.skillName = '';
          this.successMessage = 'Skill added.';
          this.loadReadiness();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(error, 'Unable to add this skill.');
        }
      });
  }

  removeSkill(skill: CandidateSkillRecord): void {
    if (!window.confirm(`Remove ${skill.name} from your profile?`)) {
      return;
    }

    const key = `skill:${skill.id}`;
    this.setBusy(key, true);
    this.clearMessages();

    this.api
      .deleteSkill(skill.id)
      .pipe(finalize(() => this.setBusy(key, false)))
      .subscribe({
        next: () => {
          this.skills = this.skills.filter(item => item.id !== skill.id);
          this.successMessage = 'Skill removed.';
          this.loadReadiness();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(
            error,
            'Unable to remove this skill.'
          );
        }
      });
  }

  addWork(): void {
    this.workExperiences.insert(0, this.createWorkGroup());
  }

  saveWork(group: UntypedFormGroup, index: number): void {
    group.markAllAsTouched();
    if (group.invalid) {
      return;
    }

    const value = group.getRawValue();
    const payload = {
      jobTitle: value.jobTitle,
      companyName: value.companyName,
      startDate: value.startDate,
      endDate: value.endDate || null,
      description: value.description
    };

    const key = `work:${index}`;
    const request = value.id
      ? this.api.updateWorkExperience(value.id, payload)
      : this.api.addWorkExperience(payload);

    this.runRecordSave(
      request,
      key,
      record => this.workExperiences.setControl(index, this.createWorkGroup(record)),
      'Work experience saved.'
    );
  }

  deleteWork(group: UntypedFormGroup, index: number): void {
    this.deleteRecord(
      group,
      index,
      this.workExperiences,
      id => this.api.deleteWorkExperience(id),
      'Delete this work experience?',
      'Work experience removed.'
    );
  }

  addEducation(): void {
    this.educationRecords.insert(0, this.createEducationGroup());
  }

  saveEducation(group: UntypedFormGroup, index: number): void {
    group.markAllAsTouched();
    if (group.invalid) {
      return;
    }

    const value = group.getRawValue();
    const payload = {
      institution: value.institution,
      qualification: value.qualification,
      fieldOfStudy: value.fieldOfStudy,
      startDate: value.startDate,
      endDate: value.endDate || null
    };

    const key = `education:${index}`;
    const request = value.id
      ? this.api.updateEducation(value.id, payload)
      : this.api.addEducation(payload);

    this.runRecordSave(
      request,
      key,
      record => this.educationRecords.setControl(index, this.createEducationGroup(record)),
      'Education saved.'
    );
  }

  deleteEducation(group: UntypedFormGroup, index: number): void {
    this.deleteRecord(
      group,
      index,
      this.educationRecords,
      id => this.api.deleteEducation(id),
      'Delete this education record?',
      'Education removed.'
    );
  }

  addCertification(): void {
    this.certifications.insert(0, this.createCertificationGroup());
  }

  saveCertification(group: UntypedFormGroup, index: number): void {
    group.markAllAsTouched();
    if (group.invalid) {
      return;
    }

    const value = group.getRawValue();
    const payload = {
      name: value.name,
      issuer: value.issuer,
      issuedOn: value.issuedOn,
      expiresOn: value.expiresOn || null,
      credentialUrl: value.credentialUrl
    };

    const key = `certification:${index}`;
    const request = value.id
      ? this.api.updateCertification(value.id, payload)
      : this.api.addCertification(payload);

    this.runRecordSave(
      request,
      key,
      record => this.certifications.setControl(
        index,
        this.createCertificationGroup(record)
      ),
      'Certification saved.'
    );
  }

  deleteCertification(group: UntypedFormGroup, index: number): void {
    this.deleteRecord(
      group,
      index,
      this.certifications,
      id => this.api.deleteCertification(id),
      'Delete this certification?',
      'Certification removed.'
    );
  }

  addLicence(): void {
    this.licences.insert(0, this.createLicenceGroup());
  }

  saveLicence(group: UntypedFormGroup, index: number): void {
    group.markAllAsTouched();
    if (group.invalid) {
      return;
    }

    const value = group.getRawValue();
    const payload = {
      type: value.type,
      class: value.class,
      issuer: value.issuer,
      identifier: value.identifier,
      issuedOn: value.issuedOn,
      expiresOn: value.expiresOn || null,
      status: value.status,
      verificationStatus: value.verificationStatus
    };

    const key = `licence:${index}`;
    const request = value.id
      ? this.api.updateLicence(value.id, payload)
      : this.api.addLicence(payload);

    this.runRecordSave(
      request,
      key,
      record => this.licences.setControl(index, this.createLicenceGroup(record)),
      'Licence or registration saved.'
    );
  }

  deleteLicence(group: UntypedFormGroup, index: number): void {
    this.deleteRecord(
      group,
      index,
      this.licences,
      id => this.api.deleteLicence(id),
      'Delete this licence or registration?',
      'Licence or registration removed.'
    );
  }

  addLanguage(): void {
    this.languages.insert(0, this.createLanguageGroup());
  }

  saveLanguage(group: UntypedFormGroup, index: number): void {
    group.markAllAsTouched();
    if (group.invalid) {
      return;
    }

    const value = group.getRawValue();
    const payload = {
      language: value.language,
      proficiency: value.proficiency
    };

    const key = `language:${index}`;
    const request = value.id
      ? this.api.updateLanguage(value.id, payload)
      : this.api.addLanguage(payload);

    this.runRecordSave(
      request,
      key,
      record => this.languages.setControl(index, this.createLanguageGroup(record)),
      'Language saved.'
    );
  }

  deleteLanguage(group: UntypedFormGroup, index: number): void {
    this.deleteRecord(
      group,
      index,
      this.languages,
      id => this.api.deleteLanguage(id),
      'Delete this language?',
      'Language removed.'
    );
  }

  addProject(): void {
    this.projects.insert(0, this.createProjectGroup());
  }

  saveProject(group: UntypedFormGroup, index: number): void {
    group.markAllAsTouched();
    if (group.invalid) {
      return;
    }

    const value = group.getRawValue();
    const payload = {
      name: value.name,
      description: value.description,
      projectUrl: value.projectUrl
    };

    const key = `project:${index}`;
    const request = value.id
      ? this.api.updateProject(value.id, payload)
      : this.api.addProject(payload);

    this.runRecordSave(
      request,
      key,
      record => this.projects.setControl(index, this.createProjectGroup(record)),
      'Project saved.'
    );
  }

  deleteProject(group: UntypedFormGroup, index: number): void {
    this.deleteRecord(
      group,
      index,
      this.projects,
      id => this.api.deleteProject(id),
      'Delete this project?',
      'Project removed.'
    );
  }

  isBusy(key: string): boolean {
    return this.busyKeys.has(key);
  }

  private loadEvidence(): void {
    this.api.getSkills().subscribe({
      next: items => (this.skills = items),
      error: error => this.setSectionError(error, 'skills')
    });

    this.api.getWorkExperiences().subscribe({
      next: items => this.replaceArray(
        this.workExperiences,
        items.map(item => this.createWorkGroup(item))
      ),
      error: error => this.setSectionError(error, 'work experience')
    });

    this.api.getEducation().subscribe({
      next: items => this.replaceArray(
        this.educationRecords,
        items.map(item => this.createEducationGroup(item))
      ),
      error: error => this.setSectionError(error, 'education')
    });

    this.api.getCertifications().subscribe({
      next: items => this.replaceArray(
        this.certifications,
        items.map(item => this.createCertificationGroup(item))
      ),
      error: error => this.setSectionError(error, 'certifications')
    });

    this.api.getLicences().subscribe({
      next: items => this.replaceArray(
        this.licences,
        items.map(item => this.createLicenceGroup(item))
      ),
      error: error => this.setSectionError(error, 'licences')
    });

    this.api.getLanguages().subscribe({
      next: items => this.replaceArray(
        this.languages,
        items.map(item => this.createLanguageGroup(item))
      ),
      error: error => this.setSectionError(error, 'languages')
    });

    this.api.getProjects().subscribe({
      next: items => this.replaceArray(
        this.projects,
        items.map(item => this.createProjectGroup(item))
      ),
      error: error => this.setSectionError(error, 'projects')
    });
  }

  private loadReadiness(): void {
    this.api.getProfileReadiness().subscribe({
      next: readiness => (this.profileReadiness = readiness),
      error: () => (this.profileReadiness = null)
    });

    this.api.getApplicationReadiness().subscribe({
      next: readiness => (this.applicationReadiness = readiness),
      error: () => (this.applicationReadiness = null)
    });
  }

  private runRecordSave<T>(
    request: Observable<T>,
    key: string,
    onSuccess: (record: T) => void,
    message: string
  ): void {
    this.setBusy(key, true);
    this.clearMessages();

    request
      .pipe(finalize(() => this.setBusy(key, false)))
      .subscribe({
        next: record => {
          onSuccess(record);
          this.successMessage = message;
          this.loadReadiness();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(error, 'Unable to save this record.');
        }
      });
  }

  private deleteRecord(
    group: UntypedFormGroup,
    index: number,
    collection: UntypedFormArray,
    request: (id: string) => Observable<void>,
    prompt: string,
    message: string
  ): void {
    const id = String(group.get('id')?.value ?? '');

    if (!id) {
      collection.removeAt(index);
      return;
    }

    if (!window.confirm(prompt)) {
      return;
    }

    const key = `delete:${id}`;
    this.setBusy(key, true);
    this.clearMessages();

    request(id)
      .pipe(finalize(() => this.setBusy(key, false)))
      .subscribe({
        next: () => {
          collection.removeAt(index);
          this.successMessage = message;
          this.loadReadiness();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(error, 'Unable to delete this record.');
        }
      });
  }

  private profilePayload(): CandidateProfilePayload {
    const value = this.profileForm.getRawValue();

    return {
      fullName: String(value.fullName ?? '').trim(),
      location: String(value.location ?? '').trim(),
      experienceMonths: Number(value.experienceMonths ?? 0),
      education: String(value.education ?? '').trim(),
      preferredWorkMode: String(value.preferredWorkMode ?? '').trim(),
      preferredLocation: String(value.preferredLocation ?? '').trim(),
      willingToRelocate: Boolean(value.willingToRelocate),
      preferredEmploymentType: String(value.preferredEmploymentType ?? '').trim(),
      availabilityStatus: String(value.availabilityStatus ?? '').trim(),
      availableFrom: value.availableFrom || null,
      noticePeriodDays:
        value.noticePeriodDays === null || value.noticePeriodDays === ''
          ? null
          : Number(value.noticePeriodDays)
    };
  }

  private createWorkGroup(item?: Partial<WorkExperienceRecord>): UntypedFormGroup {
    return this.fb.group({
      id: [item?.id ?? ''],
      candidateProfileId: [item?.candidateProfileId ?? ''],
      jobTitle: [item?.jobTitle ?? '', Validators.required],
      companyName: [item?.companyName ?? '', Validators.required],
      startDate: [item?.startDate ?? '', Validators.required],
      endDate: [item?.endDate ?? ''],
      description: [item?.description ?? '']
    });
  }

  private createEducationGroup(item?: Partial<EducationRecord>): UntypedFormGroup {
    return this.fb.group({
      id: [item?.id ?? ''],
      candidateProfileId: [item?.candidateProfileId ?? ''],
      institution: [item?.institution ?? '', Validators.required],
      qualification: [item?.qualification ?? '', Validators.required],
      fieldOfStudy: [item?.fieldOfStudy ?? ''],
      startDate: [item?.startDate ?? '', Validators.required],
      endDate: [item?.endDate ?? '']
    });
  }

  private createCertificationGroup(
    item?: Partial<CertificationRecord>
  ): UntypedFormGroup {
    return this.fb.group({
      id: [item?.id ?? ''],
      candidateProfileId: [item?.candidateProfileId ?? ''],
      name: [item?.name ?? '', Validators.required],
      issuer: [item?.issuer ?? '', Validators.required],
      issuedOn: [item?.issuedOn ?? '', Validators.required],
      expiresOn: [item?.expiresOn ?? ''],
      credentialUrl: [item?.credentialUrl ?? '']
    });
  }

  private createLicenceGroup(item?: Partial<LicenceRecord>): UntypedFormGroup {
    return this.fb.group({
      id: [item?.id ?? ''],
      candidateProfileId: [item?.candidateProfileId ?? ''],
      type: [item?.type ?? '', Validators.required],
      class: [item?.class ?? ''],
      issuer: [item?.issuer ?? '', Validators.required],
      identifier: [item?.identifier ?? '', Validators.required],
      issuedOn: [item?.issuedOn ?? '', Validators.required],
      expiresOn: [item?.expiresOn ?? ''],
      status: [item?.status ?? 'Valid', Validators.required],
      verificationStatus: [
        item?.verificationStatus ?? 'Pending',
        Validators.required
      ]
    });
  }

  private createLanguageGroup(item?: Partial<LanguageRecord>): UntypedFormGroup {
    return this.fb.group({
      id: [item?.id ?? ''],
      candidateProfileId: [item?.candidateProfileId ?? ''],
      language: [item?.language ?? '', Validators.required],
      proficiency: [item?.proficiency ?? '', Validators.required]
    });
  }

  private createProjectGroup(item?: Partial<ProjectRecord>): UntypedFormGroup {
    return this.fb.group({
      id: [item?.id ?? ''],
      candidateProfileId: [item?.candidateProfileId ?? ''],
      name: [item?.name ?? '', Validators.required],
      description: [item?.description ?? ''],
      projectUrl: [item?.projectUrl ?? '']
    });
  }

  private replaceArray(
    array: UntypedFormArray,
    groups: UntypedFormGroup[]
  ): void {
    array.clear();
    for (const group of groups) {
      array.push(group);
    }
  }

  private setSectionError(error: HttpErrorResponse, section: string): void {
    if (!this.errorMessage) {
      this.errorMessage = this.errorText(
        error,
        `Unable to load ${section}.`
      );
    }
  }

  private setBusy(key: string, busy: boolean): void {
    if (busy) {
      this.busyKeys.add(key);
    } else {
      this.busyKeys.delete(key);
    }
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }

  private errorText(error: HttpErrorResponse, fallback: string): string {
    if (
      error.error &&
      typeof error.error === 'object' &&
      'message' in error.error &&
      typeof error.error.message === 'string' &&
      error.error.message.trim()
    ) {
      return error.error.message;
    }

    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }

    return fallback;
  }
}