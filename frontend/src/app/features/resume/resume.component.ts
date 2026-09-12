import { CommonModule } from '@angular/common';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { Resume, ResumeVersion } from '../../core/models/resume.model';
import { ErrorStateComponent } from '../../shared/states/error-state.component';
import { LoadingStateComponent } from '../../shared/states/loading-state.component';
import { CvApiService } from './cv-api.service';

@Component({
  selector: 'app-resume',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    LoadingStateComponent,
    ErrorStateComponent
  ],
  templateUrl: './resume.component.html',
  styleUrl: './resume.component.css'
})
export class ResumeComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  private readonly api = inject(CvApiService);

  readonly maxFileSizeBytes = 5 * 1024 * 1024;
  readonly uploadForm = this.fb.group({
    file: [null, Validators.required]
  });

  resume: Resume | null = null;
  selectedFile: File | null = null;

  loading = true;
  uploading = false;
  dragActive = false;
  downloadingId: string | null = null;
  settingCurrentId: string | null = null;

  successMessage = '';
  errorMessage = '';
  fileError = '';

  ngOnInit(): void {
    this.loadResume();
  }

  get currentVersion(): ResumeVersion | null {
    if (!this.resume?.currentVersionId) {
      return this.resume?.versions.find(version => version.isCurrent) ?? null;
    }

    return this.resume.versions.find(
      version => version.id === this.resume?.currentVersionId
    ) ?? null;
  }

  loadResume(showLoader = true): void {
    if (showLoader) {
      this.loading = true;
    }

    this.api.getResume().subscribe({
      next: resume => {
        this.resume = resume;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;

        if (error.status === 404) {
          this.resume = null;
          return;
        }

        this.errorMessage = this.errorText(error, 'Unable to load your CV versions.');
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.acceptFile(input.files?.[0] ?? null);

    if (!this.selectedFile) {
      input.value = '';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragActive = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragActive = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragActive = false;
    this.acceptFile(event.dataTransfer?.files?.[0] ?? null);
  }

  upload(): void {
    if (!this.selectedFile) {
      this.fileError = 'Choose a PDF before uploading.';
      return;
    }

    this.uploading = true;
    this.clearMessages();

    this.api
      .uploadVersion(this.selectedFile)
      .pipe(finalize(() => (this.uploading = false)))
      .subscribe({
        next: () => {
          this.successMessage = 'CV version uploaded successfully.';
          this.clearSelectedFile();
          this.loadResume(false);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 413) {
            this.fileError = 'PDF must be 5 MB or smaller.';
            return;
          }

          this.errorMessage = this.errorText(error, 'Unable to upload this CV.');
        }
      });
  }

  setCurrent(version: ResumeVersion): void {
    if (version.isCurrent || this.settingCurrentId) {
      return;
    }

    this.settingCurrentId = version.id;
    this.clearMessages();

    this.api
      .setCurrentVersion(version.id)
      .pipe(finalize(() => (this.settingCurrentId = null)))
      .subscribe({
        next: resume => {
          this.resume = resume;
          this.successMessage = `Version ${version.versionNumber} is now current.`;
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(
            error,
            'Unable to change the current CV version.'
          );
        }
      });
  }

  download(version: ResumeVersion): void {
    this.downloadingId = version.id;
    this.clearMessages();

    this.api
      .downloadVersion(version.id)
      .pipe(finalize(() => (this.downloadingId = null)))
      .subscribe({
        next: response => this.saveDownload(response, version.originalFileName),
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.errorText(error, 'Unable to download this CV.');
        }
      });
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) {
      return `${bytes} B`;
    }

    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(1)} KB`;
    }

    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  private acceptFile(file: File | null): void {
    this.fileError = '';
    this.errorMessage = '';
    this.successMessage = '';

    if (!file) {
      this.clearSelectedFile();
      return;
    }

    const pdfType = file.type.toLowerCase() === 'application/pdf';
    const pdfName = file.name.toLowerCase().endsWith('.pdf');

    if (!pdfType && !pdfName) {
      this.clearSelectedFile();
      this.fileError = 'Only PDF files are allowed.';
      return;
    }

    if (file.size > this.maxFileSizeBytes) {
      this.clearSelectedFile();
      this.fileError = 'PDF must be 5 MB or smaller.';
      return;
    }

    this.selectedFile = file;
    this.uploadForm.patchValue({ file });
    this.uploadForm.get('file')?.updateValueAndValidity();
  }

  private clearSelectedFile(): void {
    this.selectedFile = null;
    this.uploadForm.reset();

    const input = document.getElementById('cvFile') as HTMLInputElement | null;
    if (input) {
      input.value = '';
    }
  }

  private saveDownload(
    response: HttpResponse<Blob>,
    fallbackName: string
  ): void {
    if (!response.body) {
      this.errorMessage = 'The downloaded CV was empty.';
      return;
    }

    const contentDisposition = response.headers.get('content-disposition');
    const fileName = this.extractFileName(contentDisposition) || fallbackName;
    const objectUrl = URL.createObjectURL(response.body);
    const anchor = document.createElement('a');

    anchor.href = objectUrl;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(objectUrl);
  }

  private extractFileName(contentDisposition: string | null): string | null {
    if (!contentDisposition) {
      return null;
    }

    const utf8Match = /filename\*=UTF-8''([^;]+)/i.exec(contentDisposition);

    if (utf8Match?.[1]) {
      try {
        return decodeURIComponent(utf8Match[1]);
      } catch {
        return utf8Match[1];
      }
    }

    return /filename="?([^";]+)"?/i.exec(contentDisposition)?.[1] ?? null;
  }

  private clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
    this.fileError = '';
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