import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import {
  CandidateProfile,
  CandidateSkill,
  EducationRecord,
  WorkExperience
} from '../../core/models/candidate-profile.model';
import { CandidateProfileApiService } from '../../core/services/candidate-profile-api.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  profile: CandidateProfile = this.emptyProfile();
  profileExists = false;

  skills: CandidateSkill[] = [];
  workExperiences: WorkExperience[] = [];
  educationRecords: EducationRecord[] = [];

  newSkill = '';

  workForm = this.emptyWork();
  educationForm = this.emptyEducation();

  editingWorkId: string | null = null;
  editingEducationId: string | null = null;

  loading = true;
  savingProfile = false;
  message = '';
  error = '';

  constructor(private readonly api: CandidateProfileApiService) {}

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.loading = true;
    this.error = '';

    this.api.getProfile().subscribe({
      next: profile => {
        if (profile) {
          this.profile = profile;
          this.profileExists = true;
        }

        this.loadRelatedData();
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 404) {
          this.profileExists = false;
          this.loading = false;
          return;
        }

        this.showError(err, 'Unable to load candidate profile.');
        this.loading = false;
      }
    });
  }

  private loadRelatedData(): void {
    this.api.getSkills().subscribe({
      next: data => this.skills = data,
      error: err => this.showError(err, 'Unable to load skills.')
    });

    this.api.getWorkExperiences().subscribe({
      next: data => this.workExperiences = data,
      error: err => this.showError(err, 'Unable to load work experience.')
    });

    this.api.getEducation().subscribe({
      next: data => this.educationRecords = data,
      error: err => this.showError(err, 'Unable to load education.')
    });

    this.loading = false;
  }

  saveProfile(): void {
    this.message = '';
    this.error = '';

    if (!this.profile.fullName.trim()) {
      this.error = 'Full name is required.';
      return;
    }

    this.savingProfile = true;

    const request = this.profileExists
      ? this.api.updateProfile(this.profile)
      : this.api.createProfile(this.profile);

    request.subscribe({
      next: result => {
        this.profile = result;
        this.profileExists = true;
        this.savingProfile = false;
        this.message = 'Profile saved successfully.';
        this.loadRelatedData();
      },
      error: err => {
        this.savingProfile = false;
        this.showError(err, 'Unable to save profile.');
      }
    });
  }

  addSkill(): void {
    const name = this.newSkill.trim();

    if (!name) {
      return;
    }

    this.api.addSkill(name).subscribe({
      next: skill => {
        this.skills = [...this.skills, skill]
          .sort((a, b) => a.name.localeCompare(b.name));
        this.newSkill = '';
        this.message = 'Skill added.';
        this.error = '';
      },
      error: err => this.showError(err, 'Unable to add skill.')
    });
  }

  removeSkill(id: string): void {
    this.api.deleteSkill(id).subscribe({
      next: () => {
        this.skills = this.skills.filter(x => x.id !== id);
        this.message = 'Skill removed.';
      },
      error: err => this.showError(err, 'Unable to remove skill.')
    });
  }

  saveWork(): void {
    if (!this.workForm.jobTitle?.trim() ||
        !this.workForm.companyName?.trim() ||
        !this.workForm.startDate) {
      this.error = 'Job title, company and start date are required.';
      return;
    }

    const request = this.editingWorkId
      ? this.api.updateWorkExperience(this.editingWorkId, this.workForm)
      : this.api.addWorkExperience(this.workForm);

    request.subscribe({
      next: () => {
        this.cancelWorkEdit();
        this.loadWorkExperiences();
        this.message = 'Work experience saved.';
        this.error = '';
      },
      error: err => this.showError(err, 'Unable to save work experience.')
    });
  }

  editWork(item: WorkExperience): void {
    this.editingWorkId = item.id;
    this.workForm = {
      jobTitle: item.jobTitle,
      companyName: item.companyName,
      startDate: item.startDate,
      endDate: item.endDate,
      description: item.description
    };
  }

  cancelWorkEdit(): void {
    this.editingWorkId = null;
    this.workForm = this.emptyWork();
  }

  deleteWork(id: string): void {
    this.api.deleteWorkExperience(id).subscribe({
      next: () => {
        this.workExperiences =
          this.workExperiences.filter(x => x.id !== id);
        this.message = 'Work experience removed.';
      },
      error: err => this.showError(err, 'Unable to remove work experience.')
    });
  }

  saveEducation(): void {
    if (!this.educationForm.institution?.trim() ||
        !this.educationForm.qualification?.trim() ||
        !this.educationForm.startDate) {
      this.error = 'Institution, qualification and start date are required.';
      return;
    }

    const request = this.editingEducationId
      ? this.api.updateEducation(
          this.editingEducationId,
          this.educationForm
        )
      : this.api.addEducation(this.educationForm);

    request.subscribe({
      next: () => {
        this.cancelEducationEdit();
        this.loadEducation();
        this.message = 'Education saved.';
        this.error = '';
      },
      error: err => this.showError(err, 'Unable to save education.')
    });
  }

  editEducation(item: EducationRecord): void {
    this.editingEducationId = item.id;
    this.educationForm = {
      institution: item.institution,
      qualification: item.qualification,
      fieldOfStudy: item.fieldOfStudy,
      startDate: item.startDate,
      endDate: item.endDate
    };
  }

  cancelEducationEdit(): void {
    this.editingEducationId = null;
    this.educationForm = this.emptyEducation();
  }

  deleteEducation(id: string): void {
    this.api.deleteEducation(id).subscribe({
      next: () => {
        this.educationRecords =
          this.educationRecords.filter(x => x.id !== id);
        this.message = 'Education removed.';
      },
      error: err => this.showError(err, 'Unable to remove education.')
    });
  }

  private loadWorkExperiences(): void {
    this.api.getWorkExperiences().subscribe({
      next: data => this.workExperiences = data,
      error: err => this.showError(err, 'Unable to refresh work experience.')
    });
  }

  private loadEducation(): void {
    this.api.getEducation().subscribe({
      next: data => this.educationRecords = data,
      error: err => this.showError(err, 'Unable to refresh education.')
    });
  }

  private showError(error: HttpErrorResponse, fallback: string): void {
    this.message = '';

    if (typeof error.error === 'string' && error.error.trim()) {
      this.error = error.error;
      return;
    }

    this.error = fallback;
  }

  private emptyProfile(): CandidateProfile {
    return {
      fullName: '',
      location: '',
      experienceMonths: 0,
      education: ''
    };
  }

  private emptyWork(): Partial<WorkExperience> {
    return {
      jobTitle: '',
      companyName: '',
      startDate: '',
      endDate: null,
      description: ''
    };
  }

  private emptyEducation(): Partial<EducationRecord> {
    return {
      institution: '',
      qualification: '',
      fieldOfStudy: '',
      startDate: '',
      endDate: null
    };
  }
}
