import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AppIconComponent } from '../../../shared/icons/app-icon.component';

import { EmployerApiService } from '../data-access/employer-api.service';
import {
  AlternativeSetAggregateUpdate,
  AlternativeSetType,
  CompanyProfile,
  EmployerVacancy,
  EmployerVacancyUpsertRequest,
  FamilyPolicyAggregateUpdate,
  RequirementFamily,
  RequirementImportance,
  RequirementMode,
  MatchingPolicyAggregateUpdateRequest,
  VacancyPolicyAggregateDto,
  VacancyRequirementAggregateUpdate
} from '../data-access/employer.models';

@Component({
  selector: 'app-employer-vacancy-editor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AppIconComponent
  ],
  templateUrl: './employer-vacancy-editor.component.html',
  styleUrl: './employer-vacancy-editor.component.css'
})
export class EmployerVacancyEditorComponent implements OnInit {
  @ViewChild('stageError') private stageError?: ElementRef<HTMLElement>;

  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private readonly employerApi = inject(EmployerApiService);
  private readonly router = inject(Router);

  readonly stages = [
    'Basics',
    'Location & Work',
    'Experience & Education',
    'Compensation & Closing',
    'Required Skills',
    'Matching Policy',
    'Review'
  ];

  readonly workModeOptions = ['Remote', 'On-site', 'Hybrid'];
  readonly employmentTypeOptions = ['Full-time', 'Part-time', 'Contract', 'Internship'];

  readonly policyFamilyOptions = [
    { value: RequirementFamily.Skill, label: 'Skill' },
    { value: RequirementFamily.Experience, label: 'Experience' },
    { value: RequirementFamily.Education, label: 'Education' },
    {
      value: RequirementFamily.LocationWorkMode,
      label: 'Location & Work Mode'
    },
    {
      value: RequirementFamily.Certification,
      label: 'Certification'
    },
    {
      value: RequirementFamily.LicenceRegistration,
      label: 'Licence / Registration'
    },
    { value: RequirementFamily.Language, label: 'Language' },
    {
      value: RequirementFamily.Availability,
      label: 'Availability'
    },
    {
      value: RequirementFamily.ProjectPortfolio,
      label: 'Project / Portfolio'
    },
    {
      value: RequirementFamily.StructuredQuestion,
      label: 'Structured Question'
    }
  ];

  readonly importanceOptions = [
    {
      value: RequirementImportance.Low,
      label: 'Low'
    },
    {
      value: RequirementImportance.Medium,
      label: 'Medium'
    },
    {
      value: RequirementImportance.High,
      label: 'High'
    }
  ];

  readonly requirementModeOptions = [
    {
      value: RequirementMode.Mandatory,
      label: 'Mandatory'
    },
    {
      value: RequirementMode.Preferred,
      label: 'Preferred'
    },
    {
      value: RequirementMode.Informational,
      label: 'Informational'
    }
  ];

  readonly alternativeSetTypeOptions = [
    {
      value: AlternativeSetType.AnyOf,
      label: 'Any of'
    },
    {
      value: AlternativeSetType.MinSatisfied,
      label: 'Minimum satisfied'
    }
  ];

  readonly vacancyId =
    this.route.snapshot.paramMap.get('vacancyId');

  readonly isEditMode = !!this.vacancyId;

  readonly form = this.formBuilder.group({
    companyId: this.formBuilder.control<string | null>(null, Validators.required),

    title: this.formBuilder.nonNullable.control(
      '',
      Validators.required
    ),

    description: this.formBuilder.nonNullable.control('', Validators.required),

    location: this.formBuilder.nonNullable.control('', Validators.required),

    workMode: this.formBuilder.nonNullable.control('', Validators.required),

    employmentType: this.formBuilder.nonNullable.control('', Validators.required),

    minExperienceMonths: this.formBuilder.nonNullable.control(
      0,
      [Validators.required, Validators.min(0)]
    ),

    maxExperienceMonths:
      this.formBuilder.control<number | null>(null),

    requiredEducation:
      this.formBuilder.nonNullable.control(''),

    salaryMin:
      this.formBuilder.control<number | null>(null),

    salaryMax:
      this.formBuilder.control<number | null>(null),

    closingDateUtc:
      this.formBuilder.control<string | null>(null, Validators.required),

    requiredSkills: this.formBuilder.array([
      this.createSkillGroup()
    ])
  });

  activeStage = 0;
  highestAccessibleStage = 0;
  stageErrorMessage = '';
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';
  loadedVacancy: EmployerVacancy | null = null;

  companies: CompanyProfile[] = [];
  companiesLoading = false;
  companiesErrorMessage = '';

  policy: VacancyPolicyAggregateDto | null = null;
  policyFamilies: FamilyPolicyAggregateUpdate[] = [];
  policyRequirements: VacancyRequirementAggregateUpdate[] = [];
  policyAlternativeSets: AlternativeSetAggregateUpdate[] = [];
  policyLoading = false;
  policySaving = false;
  policyErrorMessage = '';
  policySuccessMessage = '';

  publishing = false;
  publishErrorMessage = '';
  publishSuccessMessage = '';

  get pageTitle(): string {
    return this.isEditMode
      ? 'Edit vacancy'
      : 'Create vacancy';
  }

  get requiredSkills(): FormArray {
    return this.form.controls.requiredSkills;
  }

  get isReadOnly(): boolean {
    return this.loadedVacancy?.lifecycleStatus === 'Closed';
  }

  get isPolicyReadOnly(): boolean {
    return (
      this.isReadOnly ||
      this.policy?.revision.isMateriallyLocked === true
    );
  }

  get canPublish(): boolean {
    return (
      !!this.vacancyId &&
      this.loadedVacancy?.lifecycleStatus === 'Draft' &&
      !this.publishing &&
      this.publishReadinessIssues.length === 0
    );
  }

  get publishReadinessIssues(): string[] {
    if (!this.loadedVacancy) {
      return ['Save the vacancy facts before publishing.'];
    }

    const vacancy = this.loadedVacancy;
    const issues: string[] = [];
    if (!vacancy.companyId) issues.push('Select a company.');
    if (!vacancy.title.trim()) issues.push('Add a vacancy title.');
    if (!vacancy.description.trim()) issues.push('Add a role description.');
    if (!vacancy.location.trim()) issues.push('Add a location.');
    if (!vacancy.workMode.trim()) issues.push('Select a work mode.');
    if (!vacancy.employmentType.trim()) issues.push('Select an employment type.');
    if (!vacancy.closingDateUtc || new Date(vacancy.closingDateUtc).getTime() <= Date.now()) {
      issues.push('Set a future closing date.');
    }
    if (vacancy.requiredSkills.length === 0) issues.push('Add at least one required skill.');
    return issues;
  }

  ngOnInit(): void {
    this.loadCompanies();

    if (this.vacancyId) {
      this.loadVacancy(this.vacancyId);
    }
  }

  private loadCompanies(): void {
    this.companiesLoading = true;
    this.companiesErrorMessage = '';

    this.employerApi.getCompanies().subscribe({
      next: companies => {
        this.companies = companies;
        this.companiesLoading = false;
      },
      error: error => {
        this.companiesErrorMessage =
          this.readServerMessage(error) ||
          'Unable to load your companies.';
        this.companiesLoading = false;
      }
    });
  }
  goToStage(index: number): void {
    if (
      index < 0 ||
      index >= this.stages.length ||
      index > this.highestAccessibleStage
    ) {
      return;
    }

    this.stageErrorMessage = '';
    this.activeStage = index;
  }

  previousStage(): void {
    this.goToStage(this.activeStage - 1);
  }

  nextStage(): void {
    const validationMessage = this.validateStage(this.activeStage);
    if (validationMessage) {
      this.stageErrorMessage = validationMessage;
      this.markStageTouched(this.activeStage);
      setTimeout(() => this.stageError?.nativeElement.focus());
      return;
    }

    this.stageErrorMessage = '';
    this.highestAccessibleStage = Math.max(
      this.highestAccessibleStage,
      Math.min(this.activeStage + 1, this.stages.length - 1)
    );
    this.activeStage = Math.min(this.activeStage + 1, this.stages.length - 1);
  }

  private validateStage(index: number): string {
    const value = this.form.getRawValue();

    if (index === 0) {
      if (!value.companyId) return 'Select the company responsible for this vacancy.';
      if (!value.title.trim()) return 'Enter a vacancy title before continuing.';
      if (!value.description.trim()) return 'Add a role description before continuing.';
    }

    if (index === 1) {
      if (!value.location.trim()) return 'Enter the role location before continuing.';
      if (!value.workMode.trim()) return 'Select a work mode before continuing.';
      if (!value.employmentType.trim()) return 'Select an employment type before continuing.';
    }

    if (index === 2) {
      if (value.minExperienceMonths < 0) return 'Minimum experience cannot be negative.';
      if (value.maxExperienceMonths !== null && value.maxExperienceMonths < value.minExperienceMonths) {
        return 'Maximum experience must be greater than or equal to minimum experience.';
      }
    }

    if (index === 3) {
      if (value.salaryMin !== null && value.salaryMin < 0) return 'Minimum salary cannot be negative.';
      if (value.salaryMax !== null && value.salaryMax < 0) return 'Maximum salary cannot be negative.';
      if (value.salaryMin !== null && value.salaryMax !== null && value.salaryMax < value.salaryMin) {
        return 'Maximum salary must be greater than or equal to minimum salary.';
      }
      if (!value.closingDateUtc || new Date(value.closingDateUtc).getTime() <= Date.now()) {
        return 'Set a valid future closing date before continuing.';
      }
    }

    if (index === 4) {
      const skillNames = value.requiredSkills.map(skill => skill.name.trim());
      if (skillNames.length === 0 || skillNames.some(name => !name)) {
        return 'Add at least one named required skill before continuing.';
      }
      if (new Set(skillNames.map(name => name.toLocaleLowerCase())).size !== skillNames.length) {
        return 'Remove duplicate required skills before continuing.';
      }
      if (value.requiredSkills.some(skill => skill.weight <= 0)) {
        return 'Every required skill must have a weight greater than zero.';
      }
    }

    if (index === 5 && !this.vacancyId) {
      return 'Save the Draft before configuring its matching policy.';
    }

    return '';
  }

  private markStageTouched(index: number): void {
    const controlsByStage = [
      ['companyId', 'title', 'description'],
      ['location', 'workMode', 'employmentType'],
      ['minExperienceMonths', 'maxExperienceMonths', 'requiredEducation'],
      ['salaryMin', 'salaryMax', 'closingDateUtc']
    ];

    if (index === 4) {
      this.requiredSkills.markAllAsTouched();
      return;
    }

    for (const name of controlsByStage[index] ?? []) {
      this.form.get(name)?.markAsTouched();
    }
  }

  private updateAccessibleStages(): void {
    let highest = 0;
    for (let index = 0; index < 5; index += 1) {
      if (this.validateStage(index)) break;
      highest = index + 1;
    }
    if (highest >= 5 && this.vacancyId) highest = 6;
    this.highestAccessibleStage = Math.min(highest, this.stages.length - 1);
  }

  publishVacancy(): void {
    if (
      !this.vacancyId ||
      this.loadedVacancy?.lifecycleStatus !== 'Draft' ||
      this.publishing
    ) {
      return;
    }

    this.publishErrorMessage = '';
    this.publishSuccessMessage = '';
    this.publishing = true;

    this.employerApi
      .publishVacancy(this.vacancyId)
      .subscribe({
        next: vacancy => {
          this.loadedVacancy = vacancy;
          this.populateForm(vacancy);

          this.employerApi
            .getVacancy(this.vacancyId!)
            .subscribe({
              next: latest => {
                if (
                  latest.lifecycleStatus !==
                  vacancy.lifecycleStatus
                ) {
                  return;
                }

                this.loadedVacancy = latest;
                this.populateForm(latest);
              }
            });

          this.publishSuccessMessage =
            'Vacancy published successfully.';
          this.publishing = false;
        },
        error: error => {
          this.handlePublishError(error);
        }
      });
  }

  private handlePublishError(
    error: unknown
  ): void {
    const status = this.readStatus(error);
    const serverMessage =
      this.readServerMessage(error);

    if (status === 409) {
      const conflictMessage =
        serverMessage ||
        'Publishing is currently blocked. The latest vacancy status has been reloaded.';

      this.reloadAfterPublishConflict(
        conflictMessage
      );
      return;
    }

    if (status === 400) {
      this.publishErrorMessage =
        serverMessage ||
        'The vacancy cannot be published with its current data.';
    } else if (status === 403) {
      this.publishErrorMessage =
        serverMessage ||
        'You are not allowed to publish this vacancy.';
    } else if (status === 404) {
      this.publishErrorMessage =
        serverMessage ||
        'The vacancy could not be found.';
    } else {
      this.publishErrorMessage =
        serverMessage ||
        'Unable to publish the vacancy.';
    }

    this.publishing = false;
  }

  private reloadAfterPublishConflict(
    conflictMessage: string
  ): void {
    if (!this.vacancyId) {
      this.publishErrorMessage =
        conflictMessage;
      this.publishing = false;
      return;
    }

    this.employerApi
      .getVacancy(this.vacancyId)
      .subscribe({
        next: vacancy => {
          this.loadedVacancy = vacancy;
          this.populateForm(vacancy);

          if (
            vacancy.lifecycleStatus === 'Closed'
          ) {
            this.form.disable();
          }

          this.employerApi
            .getFullMatchingPolicy(
              this.vacancyId!
            )
            .subscribe({
              next: policy => {
                this.policy = policy;
                this.populatePolicyFamilies(policy);
                this.populatePolicyRequirements(policy);
                this.populatePolicyAlternativeSets(policy);

                this.publishErrorMessage =
                  conflictMessage;
                this.publishing = false;
              },
              error: () => {
                this.publishErrorMessage =
                  conflictMessage;
                this.publishing = false;
              }
            });
        },
        error: () => {
          this.publishErrorMessage =
            conflictMessage;
          this.publishing = false;
        }
      });
  }
  saveVacancy(): void {
    if (this.isReadOnly || this.saving) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';

    const validationMessage = this.validateVacancyFacts();

    if (validationMessage) {
      this.errorMessage = validationMessage;
      return;
    }

    const request = this.buildVacancyRequest();

    this.saving = true;

    if (this.vacancyId) {
      this.employerApi
        .updateVacancy(this.vacancyId, request)
        .subscribe({
          next: vacancy => {
            this.loadedVacancy = vacancy;
            this.populateForm(vacancy);

            this.employerApi
              .getVacancy(this.vacancyId!)
              .subscribe({
                next: latest => {
                  this.loadedVacancy = latest;
                  this.populateForm(latest);
                }
              });

            this.successMessage =
              'Vacancy changes saved.';
            this.saving = false;
          },
          error: error => {
            this.handleSaveError(error);
          }
        });

      return;
    }

    this.employerApi.createVacancy(request).subscribe({
      next: vacancy => {
        this.loadedVacancy = vacancy;
        this.successMessage =
          'Draft vacancy created.';
        this.saving = false;

        void this.router.navigate([
          '/employer/vacancies',
          vacancy.id,
          'edit'
        ]);
      },
      error: error => {
        this.handleSaveError(error);
      }
    });
  }

  private validateVacancyFacts(): string {
    const value = this.form.getRawValue();
    const title = value.title.trim();

    if (!title) {
      return 'Vacancy title is required.';
    }

    if (value.minExperienceMonths < 0) {
      return 'Minimum experience cannot be negative.';
    }

    if (
      value.maxExperienceMonths !== null &&
      value.maxExperienceMonths <
        value.minExperienceMonths
    ) {
      return 'Maximum experience must be greater than or equal to minimum experience.';
    }

    if (
      value.salaryMin !== null &&
      value.salaryMin < 0
    ) {
      return 'Minimum salary cannot be negative.';
    }

    if (
      value.salaryMax !== null &&
      value.salaryMax < 0
    ) {
      return 'Maximum salary cannot be negative.';
    }

    if (
      value.salaryMin !== null &&
      value.salaryMax !== null &&
      value.salaryMax < value.salaryMin
    ) {
      return 'Maximum salary must be greater than or equal to minimum salary.';
    }

    if (value.closingDateUtc) {
      const closingDate =
        new Date(value.closingDateUtc);

      if (
        Number.isNaN(closingDate.getTime()) ||
        closingDate.getTime() <= Date.now()
      ) {
        return 'Closing date must be in the future.';
      }
    }

    if (value.requiredSkills.length === 0) {
      return 'At least one required skill is required.';
    }

    const skillNames = value.requiredSkills.map(
      skill => skill.name.trim()
    );

    if (skillNames.some(name => !name)) {
      return 'Required skill name cannot be blank.';
    }

    const normalizedNames = skillNames.map(
      name => name.toLocaleLowerCase()
    );

    if (
      new Set(normalizedNames).size !==
      normalizedNames.length
    ) {
      return 'Duplicate required skills are not allowed.';
    }

    if (
      value.requiredSkills.some(
        skill => skill.weight <= 0
      )
    ) {
      return 'Required skill weight must be greater than zero.';
    }

    return '';
  }

  private buildVacancyRequest(): EmployerVacancyUpsertRequest {
    const value = this.form.getRawValue();

    return {
      companyId: value.companyId,
      title: value.title.trim(),
      description: value.description,
      location: value.location,
      workMode: value.workMode,
      employmentType: value.employmentType,
      minExperienceMonths:
        value.minExperienceMonths,
      maxExperienceMonths:
        value.maxExperienceMonths,
      requiredEducation:
        value.requiredEducation,
      salaryMin: value.salaryMin,
      salaryMax: value.salaryMax,
      closingDateUtc:
        this.toUtcValue(value.closingDateUtc),
      requiredSkills: value.requiredSkills.map(
        skill => ({
          name: skill.name.trim(),
          weight: skill.weight
        })
      )
    };
  }

  private toUtcValue(
    value: string | null
  ): string | null {
    if (!value) {
      return null;
    }

    const date = new Date(value);

    return Number.isNaN(date.getTime())
      ? null
      : date.toISOString();
  }

  private handleSaveError(error: unknown): void {
    const status =
      typeof error === 'object' &&
      error !== null &&
      'status' in error
        ? Number(
            (error as { status?: unknown }).status
          )
        : 0;

    const serverMessage =
      this.readServerMessage(error);

    if (status === 409) {
      this.errorMessage =
        serverMessage ||
        'The vacancy changed elsewhere. Reload the latest status before continuing.';

      if (this.vacancyId) {
        this.reloadAfterConflict(this.vacancyId);
        return;
      }
    } else if (status === 403) {
      this.errorMessage =
        serverMessage ||
        'You are not allowed to update this vacancy.';
    } else if (status === 404) {
      this.errorMessage =
        serverMessage ||
        'The vacancy could not be found.';
    } else if (status === 400) {
      this.errorMessage =
        serverMessage ||
        'The vacancy details are invalid.';
    } else {
      this.errorMessage =
        serverMessage ||
        'Unable to save the vacancy.';
    }

    this.saving = false;
  }

  private reloadAfterConflict(
    vacancyId: string
  ): void {
    const conflictMessage = this.errorMessage;

    this.employerApi.getVacancy(vacancyId).subscribe({
      next: vacancy => {
        this.loadedVacancy = vacancy;
        this.populateForm(vacancy);

        if (vacancy.lifecycleStatus === 'Closed') {
          this.form.disable();
        }

        this.errorMessage = conflictMessage;
        this.saving = false;
      },
      error: () => {
        this.errorMessage = conflictMessage;
        this.saving = false;
      }
    });
  }
  addSkill(): void {
    if (this.isReadOnly) {
      return;
    }

    this.requiredSkills.push(this.createSkillGroup());
  }

  removeSkill(index: number): void {
    if (
      this.isReadOnly ||
      this.requiredSkills.length <= 1
    ) {
      return;
    }

    this.requiredSkills.removeAt(index);
  }

  private createSkillGroup(
    name = '',
    weight = 1
  ) {
    return this.formBuilder.group({
      name: this.formBuilder.nonNullable.control(
        name,
        Validators.required
      ),
      weight: this.formBuilder.nonNullable.control(
        weight,
        [Validators.required, Validators.min(1)]
      )
    });
  }

  private loadVacancy(vacancyId: string): void {
    this.loading = true;
    this.errorMessage = '';

    this.employerApi.getVacancy(vacancyId).subscribe({
      next: vacancy => {
        this.loadedVacancy = vacancy;
        this.populateForm(vacancy);

        if (vacancy.lifecycleStatus === 'Closed') {
          this.form.disable();
        }

        this.loadPolicy(vacancy.id);
        this.loading = false;
      },
      error: error => {
        this.errorMessage =
          this.readServerMessage(error) ||
          'Unable to load this vacancy.';
        this.loading = false;
      }
    });
  }

  private loadPolicy(vacancyId: string): void {
    this.policyLoading = true;
    this.policyErrorMessage = '';
    this.policySuccessMessage = '';

    this.employerApi
      .getFullMatchingPolicy(vacancyId)
      .subscribe({
        next: policy => {
          this.policy = policy;
          this.populatePolicyFamilies(policy);
          this.populatePolicyRequirements(policy);
          this.populatePolicyAlternativeSets(policy);
          this.policyLoading = false;
        },
        error: error => {
          const status = this.readStatus(error);
          const serverMessage =
            this.readServerMessage(error);

          if (status === 403) {
            this.policyErrorMessage =
              serverMessage ||
              'You are not allowed to manage this matching policy.';
          } else if (status === 404) {
            this.policyErrorMessage =
              serverMessage ||
              'The matching policy could not be found.';
          } else {
            this.policyErrorMessage =
              serverMessage ||
              'Unable to load the matching policy.';
          }

          this.policyLoading = false;
        }
      });
  }
  savePolicy(): void {
    if (
      !this.vacancyId ||
      this.isPolicyReadOnly ||
      this.policySaving
    ) {
      return;
    }

    this.policyErrorMessage = '';
    this.policySuccessMessage = '';

    const validationMessage =
      this.validatePolicy();

    if (validationMessage) {
      this.policyErrorMessage =
        validationMessage;
      return;
    }

    const request:
      MatchingPolicyAggregateUpdateRequest = {
        families: this.policyFamilies,
        requirements: this.policyRequirements,
        alternativeSets:
          this.policyAlternativeSets
      };

    this.policySaving = true;

    this.employerApi
      .updateMatchingPolicy(
        this.vacancyId,
        request
      )
      .subscribe({
        next: policy => {
          this.policy = policy;
          this.populatePolicyFamilies(policy);
          this.populatePolicyRequirements(policy);
          this.populatePolicyAlternativeSets(policy);

          this.employerApi
            .getFullMatchingPolicy(this.vacancyId!)
            .subscribe({
              next: latest => {
                this.policy = latest;
                this.populatePolicyFamilies(latest);
                this.populatePolicyRequirements(latest);
                this.populatePolicyAlternativeSets(latest);
              }
            });

          this.policySuccessMessage =
            'Matching policy saved.';
          this.policySaving = false;
        },
        error: error => {
          this.handlePolicySaveError(error);
        }
      });
  }

  private validatePolicy(): string {
    const canonicalTargets =
      new Set<string>();

    for (
      const requirement of
        this.policyRequirements
    ) {
      if (
        requirement.requiredMonths !== null &&
        requirement.requiredMonths < 0
      ) {
        return 'Required months cannot be negative.';
      }

      if (
        requirement.mode ===
        RequirementMode.Informational &&
        requirement.isRegulatoryGate
      ) {
        return 'Informational requirements cannot be regulatory gates.';
      }

      if (
        requirement.requirementFamily ===
        RequirementFamily.StructuredQuestion
      ) {
        if (
          !requirement.questionText?.trim()
        ) {
          return 'Structured questions require question text.';
        }

        if (
          requirement.mode !==
            RequirementMode.Informational &&
          requirement.isScored &&
          !requirement.expectedAnswer?.trim()
        ) {
          return 'Scored structured questions require an expected answer.';
        }
      }

      if (
        requirement.canonicalTargetKey?.trim()
      ) {
        const canonicalKey =
          `${requirement.requirementFamily}:` +
          requirement.canonicalTargetKey
            .trim()
            .toLocaleLowerCase();

        if (
          canonicalTargets.has(canonicalKey)
        ) {
          return 'Duplicate canonical requirement targets are not allowed within the same family.';
        }

        canonicalTargets.add(canonicalKey);
      }
    }

    const assignedRequirements =
      new Set<string>();

    for (
      const set of
        this.policyAlternativeSets
    ) {
      const members =
        set.memberRequirementClientKeys;

      if (members.length === 0) {
        return 'Alternative set must contain at least one requirement.';
      }

      if (
        new Set(members).size !==
        members.length
      ) {
        return 'Alternative sets cannot contain duplicate members.';
      }

      if (
        set.setType ===
        AlternativeSetType.AnyOf &&
        set.minimumSatisfiedCount !== null
      ) {
        return 'Any-of sets cannot define a minimum satisfied count.';
      }

      if (
        set.setType ===
        AlternativeSetType.MinSatisfied &&
        (
          set.minimumSatisfiedCount === null ||
          set.minimumSatisfiedCount < 1 ||
          set.minimumSatisfiedCount >
            members.length
        )
      ) {
        return 'Minimum satisfied count must be between 1 and the number of set members.';
      }

      for (const memberKey of members) {
        if (
          assignedRequirements.has(
            memberKey
          )
        ) {
          return 'A requirement can belong to only one alternative set.';
        }

        assignedRequirements.add(
          memberKey
        );

        const requirement =
          this.policyRequirements.find(
            item =>
              item.clientKey === memberKey
          );

        if (
          !requirement ||
          requirement.familyClientKey !==
            set.familyClientKey
        ) {
          return 'Alternative-set members must belong to the referenced family.';
        }

        const requirementScored =
          requirement.mode ===
          RequirementMode.Informational
            ? false
            : requirement.isScored;

        const setScored =
          set.mode ===
          RequirementMode.Informational
            ? false
            : set.isScored;

        if (
          requirement.mode !== set.mode ||
          requirement.importance !==
            set.importance ||
          requirementScored !== setScored
        ) {
          return 'Alternative-set members must use the set-level mode, importance and scored state.';
        }
      }
    }

    return '';
  }

  private handlePolicySaveError(
    error: unknown
  ): void {
    const status = this.readStatus(error);
    const serverMessage =
      this.readServerMessage(error);

    if (status === 409) {
      const conflictMessage =
        serverMessage ||
        'The matching policy changed or is materially locked. The latest server policy has been reloaded.';

      this.reloadPolicyAfterConflict(
        conflictMessage
      );
      return;
    }

    if (status === 400) {
      this.policyErrorMessage =
        serverMessage ||
        'The matching policy is invalid.';
    } else if (status === 403) {
      this.policyErrorMessage =
        serverMessage ||
        'You are not allowed to update this matching policy.';
    } else if (status === 404) {
      this.policyErrorMessage =
        serverMessage ||
        'The matching policy could not be found.';
    } else {
      this.policyErrorMessage =
        serverMessage ||
        'Unable to save the matching policy.';
    }

    this.policySaving = false;
  }

  private reloadPolicyAfterConflict(
    conflictMessage: string
  ): void {
    if (!this.vacancyId) {
      this.policyErrorMessage =
        conflictMessage;
      this.policySaving = false;
      return;
    }

    this.employerApi
      .getFullMatchingPolicy(
        this.vacancyId
      )
      .subscribe({
        next: policy => {
          this.policy = policy;
          this.populatePolicyFamilies(policy);
          this.populatePolicyRequirements(policy);
          this.populatePolicyAlternativeSets(policy);

          this.policyErrorMessage =
            conflictMessage;
          this.policySaving = false;
        },
        error: () => {
          this.policyErrorMessage =
            conflictMessage;
          this.policySaving = false;
        }
      });
  }
  addPolicyFamily(): void {
    if (this.isPolicyReadOnly) {
      return;
    }

    const usedFamilies = new Set(
      this.policyFamilies.map(
        family => family.requirementFamily
      )
    );

    const availableFamily =
      this.policyFamilyOptions.find(
        option => !usedFamilies.has(option.value)
      );

    if (!availableFamily) {
      this.policyErrorMessage =
        'All requirement families are already configured.';
      return;
    }

    this.policyErrorMessage = '';

    this.policyFamilies = [
      ...this.policyFamilies,
      {
        clientKey:
          `family-${availableFamily.value}`,
        requirementFamily:
          availableFamily.value,
        familyImportance:
          RequirementImportance.Medium,
        isActive: true,
        isScored: true
      }
    ];
  }

  removePolicyFamily(index: number): void {
    if (
      this.isPolicyReadOnly ||
      index < 0 ||
      index >= this.policyFamilies.length
    ) {
      return;
    }

    const familyClientKey =
      this.policyFamilies[index].clientKey;

    this.policyFamilies = this.policyFamilies.filter(
      (_, familyIndex) => familyIndex !== index
    );

    this.policyRequirements =
      this.policyRequirements.filter(
        requirement =>
          requirement.familyClientKey !==
          familyClientKey
      );

    this.policyAlternativeSets =
      this.policyAlternativeSets.filter(
        set =>
          set.familyClientKey !== familyClientKey
      );
  }

  updatePolicyFamily(
    index: number,
    changes: Partial<FamilyPolicyAggregateUpdate>
  ): void {
    if (
      this.isPolicyReadOnly ||
      index < 0 ||
      index >= this.policyFamilies.length
    ) {
      return;
    }

    const updated = [...this.policyFamilies];

    updated[index] = {
      ...updated[index],
      ...changes
    };

    this.policyFamilies = updated;
  }

  familyLabel(
    family: RequirementFamily
  ): string {
    return (
      this.policyFamilyOptions.find(
        option => option.value === family
      )?.label ?? 'Unknown'
    );
  }

  private populatePolicyFamilies(
    policy: VacancyPolicyAggregateDto
  ): void {
    this.policyFamilies = policy.families.map(
      family => ({
        clientKey: `family-${family.requirementFamily}`,
        requirementFamily:
          family.requirementFamily,
        familyImportance:
          family.familyImportance,
        isActive: family.isActive,
        isScored: family.isScored
      })
    );
  }
  addPolicyRequirement(
    family: FamilyPolicyAggregateUpdate
  ): void {
    if (this.isPolicyReadOnly) {
      return;
    }

    const familyRequirements =
      this.policyRequirements.filter(
        requirement =>
          requirement.familyClientKey ===
          family.clientKey
      );

    const nextOrder =
      familyRequirements.length === 0
        ? 1
        : Math.max(
            ...familyRequirements.map(
              requirement =>
                requirement.displayOrder
            )
          ) + 1;

    const clientKey =
      `${family.clientKey}-requirement-${Date.now()}`;

    this.policyRequirements = [
      ...this.policyRequirements,
      {
        clientKey,
        familyClientKey: family.clientKey,
        requirementFamily:
          family.requirementFamily,
        mode: RequirementMode.Preferred,
        importance:
          family.familyImportance,
        isActive: true,
        isScored: family.isScored,
        description: '',
        skillConceptId: null,
        canonicalTargetKey: null,
        requiredMonths: null,
        requiredValue: null,
        acceptedValuesJson: null,
        isRegulatoryGate: false,
        requiresVerification: false,
        questionText: null,
        expectedAnswer: null,
        displayOrder: nextOrder
      }
    ];
  }

  removePolicyRequirement(
    clientKey: string
  ): void {
    if (this.isPolicyReadOnly) {
      return;
    }

    this.policyRequirements =
      this.policyRequirements.filter(
        requirement =>
          requirement.clientKey !== clientKey
      );

    this.policyAlternativeSets =
      this.policyAlternativeSets
        .map(set => ({
          ...set,
          memberRequirementClientKeys:
            set.memberRequirementClientKeys.filter(
              memberKey =>
                memberKey !== clientKey
            )
        }))
        .filter(
          set =>
            set.memberRequirementClientKeys.length > 0
        );
  }

  updatePolicyRequirement(
    clientKey: string,
    changes: Partial<VacancyRequirementAggregateUpdate>
  ): void {
    if (this.isPolicyReadOnly) {
      return;
    }

    this.policyRequirements =
      this.policyRequirements.map(requirement => {
        if (requirement.clientKey !== clientKey) {
          return requirement;
        }

        const updated = {
          ...requirement,
          ...changes
        };

        if (
          updated.mode ===
          RequirementMode.Informational
        ) {
          updated.isScored = false;
          updated.isRegulatoryGate = false;
        }

        return updated;
      });
  }

  isStructuredQuestion(
    requirement: VacancyRequirementAggregateUpdate
  ): boolean {
    return (
      requirement.requirementFamily ===
      RequirementFamily.StructuredQuestion
    );
  }
  requirementsForFamily(
    familyClientKey: string
  ): VacancyRequirementAggregateUpdate[] {
    return this.policyRequirements
      .filter(
        requirement =>
          requirement.familyClientKey ===
          familyClientKey
      )
      .sort(
        (left, right) =>
          left.displayOrder - right.displayOrder
      );
  }

  private populatePolicyRequirements(
    policy: VacancyPolicyAggregateDto
  ): void {
    const familyClientKeys = new Map(
      policy.families.map(family => [
        family.id,
        `family-${family.requirementFamily}`
      ])
    );

    this.policyRequirements =
      policy.requirements.map(requirement => ({
        clientKey: `requirement-${requirement.id}`,
        familyClientKey:
          familyClientKeys.get(
            requirement.familyPolicyId
          ) ?? '',
        requirementFamily:
          requirement.requirementFamily,
        mode: requirement.mode,
        importance: requirement.importance,
        isActive: requirement.isActive,
        isScored:
          requirement.mode ===
          RequirementMode.Informational
            ? false
            : requirement.isScored,
        description: requirement.description,
        skillConceptId:
          requirement.skillConceptId,
        canonicalTargetKey:
          requirement.canonicalTargetKey,
        requiredMonths:
          requirement.requiredMonths,
        requiredValue:
          requirement.requiredValue,
        acceptedValuesJson:
          requirement.acceptedValuesJson,
        isRegulatoryGate:
          requirement.mode ===
          RequirementMode.Informational
            ? false
            : requirement.isRegulatoryGate,
        requiresVerification:
          requirement.requiresVerification,
        questionText: requirement.questionText,
        expectedAnswer:
          requirement.expectedAnswer,
        displayOrder: requirement.displayOrder
      }));
  }
  addAlternativeSet(
    family: FamilyPolicyAggregateUpdate
  ): void {
    if (this.isPolicyReadOnly) {
      return;
    }

    const requirements =
      this.availableRequirementsForAlternativeSet(
        family.clientKey
      );

    if (requirements.length === 0) {
      this.policyErrorMessage =
        'Add an unassigned requirement to this family first.';
      return;
    }

    const nextOrder =
      this.alternativeSetsForFamily(
        family.clientKey
      ).length + 1;

    this.policyErrorMessage = '';

    this.policyAlternativeSets = [
      ...this.policyAlternativeSets,
      {
        clientKey:
          `${family.clientKey}-set-${Date.now()}`,
        familyClientKey: family.clientKey,
        setType: AlternativeSetType.AnyOf,
        minimumSatisfiedCount: null,
        mode: requirements[0].mode,
        importance: requirements[0].importance,
        isActive: true,
        isScored:
          requirements[0].mode ===
          RequirementMode.Informational
            ? false
            : requirements[0].isScored,
        displayOrder: nextOrder,
        memberRequirementClientKeys: []
      }
    ];
  }

  removeAlternativeSet(
    clientKey: string
  ): void {
    if (this.isPolicyReadOnly) {
      return;
    }

    this.policyAlternativeSets =
      this.policyAlternativeSets.filter(
        set => set.clientKey !== clientKey
      );
  }

  updateAlternativeSet(
    clientKey: string,
    changes: Partial<AlternativeSetAggregateUpdate>
  ): void {
    if (this.isPolicyReadOnly) {
      return;
    }

    this.policyAlternativeSets =
      this.policyAlternativeSets.map(set => {
        if (set.clientKey !== clientKey) {
          return set;
        }

        const updated = {
          ...set,
          ...changes
        };

        if (
          updated.setType ===
          AlternativeSetType.AnyOf
        ) {
          updated.minimumSatisfiedCount = null;
        }

        if (
          updated.mode ===
          RequirementMode.Informational
        ) {
          updated.isScored = false;
        }

        return updated;
      });
  }

  toggleAlternativeSetMember(
    setClientKey: string,
    requirement:
      VacancyRequirementAggregateUpdate,
    checked: boolean
  ): void {
    if (this.isPolicyReadOnly) {
      return;
    }

    this.policyAlternativeSets =
      this.policyAlternativeSets.map(set => {
        if (set.clientKey !== setClientKey) {
          return set;
        }

        const members = new Set(
          set.memberRequirementClientKeys
        );

        if (checked) {
          members.add(requirement.clientKey);
        } else {
          members.delete(requirement.clientKey);
        }

        const memberKeys = [...members];

        return {
          ...set,
          mode:
            memberKeys.length === 1
              ? requirement.mode
              : set.mode,
          importance:
            memberKeys.length === 1
              ? requirement.importance
              : set.importance,
          isScored:
            memberKeys.length === 1
              ? requirement.mode ===
                RequirementMode.Informational
                ? false
                : requirement.isScored
              : set.isScored,
          memberRequirementClientKeys:
            memberKeys
        };
      });
  }

  availableRequirementsForAlternativeSet(
    familyClientKey: string,
    currentSetClientKey?: string
  ): VacancyRequirementAggregateUpdate[] {
    const assigned = new Set(
      this.policyAlternativeSets
        .filter(
          set =>
            set.clientKey !== currentSetClientKey
        )
        .flatMap(
          set =>
            set.memberRequirementClientKeys
        )
    );

    const currentSet =
      currentSetClientKey
        ? this.policyAlternativeSets.find(
            set =>
              set.clientKey ===
              currentSetClientKey
          )
        : undefined;

    return this.requirementsForFamily(
      familyClientKey
    ).filter(requirement => {
      if (assigned.has(requirement.clientKey)) {
        return false;
      }

      if (
        !currentSet ||
        currentSet.memberRequirementClientKeys
          .length === 0 ||
        currentSet.memberRequirementClientKeys
          .includes(requirement.clientKey)
      ) {
        return true;
      }

      const requirementIsScored =
        requirement.mode ===
        RequirementMode.Informational
          ? false
          : requirement.isScored;

      const setIsScored =
        currentSet.mode ===
        RequirementMode.Informational
          ? false
          : currentSet.isScored;

      return (
        requirement.mode === currentSet.mode &&
        requirement.importance ===
          currentSet.importance &&
        requirementIsScored === setIsScored
      );
    });
  }

  isAlternativeSetMember(
    set: AlternativeSetAggregateUpdate,
    requirementClientKey: string
  ): boolean {
    return set.memberRequirementClientKeys.includes(
      requirementClientKey
    );
  }
  alternativeSetsForFamily(
    familyClientKey: string
  ): AlternativeSetAggregateUpdate[] {
    return this.policyAlternativeSets
      .filter(
        set =>
          set.familyClientKey === familyClientKey
      )
      .sort(
        (left, right) =>
          left.displayOrder - right.displayOrder
      );
  }

  private populatePolicyAlternativeSets(
    policy: VacancyPolicyAggregateDto
  ): void {
    const familyClientKeys = new Map(
      policy.families.map(family => [
        family.id,
        `family-${family.requirementFamily}`
      ])
    );

    const requirementClientKeys = new Map(
      policy.requirements.map(requirement => [
        requirement.id,
        `requirement-${requirement.id}`
      ])
    );

    this.policyAlternativeSets =
      policy.alternativeSets.map(set => ({
        clientKey: `alternative-set-${set.id}`,
        familyClientKey:
          familyClientKeys.get(set.familyPolicyId) ?? '',
        setType: set.setType,
        minimumSatisfiedCount:
          set.minimumSatisfiedCount,
        mode: set.mode,
        importance: set.importance,
        isActive: set.isActive,
        isScored:
          set.mode === RequirementMode.Informational
            ? false
            : set.isScored,
        displayOrder: set.displayOrder,
        memberRequirementClientKeys:
          set.memberRequirementIds
            .map(id => requirementClientKeys.get(id))
            .filter(
              (key): key is string => !!key
            )
      }));
  }
  private populateForm(vacancy: EmployerVacancy): void {
    this.form.patchValue({
      companyId: vacancy.companyId,
      title: vacancy.title,
      description: vacancy.description,
      location: vacancy.location,
      workMode: vacancy.workMode,
      employmentType: vacancy.employmentType,
      minExperienceMonths: vacancy.minExperienceMonths,
      maxExperienceMonths: vacancy.maxExperienceMonths,
      requiredEducation: vacancy.requiredEducation,
      salaryMin: vacancy.salaryMin,
      salaryMax: vacancy.salaryMax,
      closingDateUtc:
        this.toDateTimeLocalValue(vacancy.closingDateUtc)
    });

    this.requiredSkills.clear();

    for (const skill of vacancy.requiredSkills) {
      this.requiredSkills.push(
        this.createSkillGroup(skill.name, skill.weight)
      );
    }

    if (this.requiredSkills.length === 0) {
      this.requiredSkills.push(this.createSkillGroup());
    }

    this.updateAccessibleStages();
  }

  selectNumber(event: Event): number {
    return Number((event.target as HTMLSelectElement).value);
  }

  private toDateTimeLocalValue(
    value: string | null
  ): string | null {
    if (!value) {
      return null;
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return null;
    }

    const offsetMilliseconds =
      date.getTimezoneOffset() * 60_000;

    return new Date(date.getTime() - offsetMilliseconds)
      .toISOString()
      .slice(0, 16);
  }

  private readStatus(error: unknown): number {
    if (
      typeof error === 'object' &&
      error !== null &&
      'status' in error
    ) {
      return Number(
        (error as { status?: unknown }).status
      );
    }

    return 0;
  }
  private readServerMessage(error: unknown): string {
    if (
      typeof error === 'object' &&
      error !== null &&
      'error' in error
    ) {
      const response = (
        error as { error?: { message?: unknown } }
      ).error;

      if (
        response &&
        typeof response.message === 'string'
      ) {
        return response.message;
      }
    }

    return '';
  }
}
