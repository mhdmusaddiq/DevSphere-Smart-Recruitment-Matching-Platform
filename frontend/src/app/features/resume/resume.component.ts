import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { Resume, ResumeVersion } from '../../core/models/resume.model';
import { ResumeApiService } from '../../core/services/resume-api.service';

@Component({
  selector: 'app-resume',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './resume.component.html',
  styleUrl: './resume.component.css'
})
export class ResumeComponent implements OnInit {
  resume: Resume | null = null;
  selectedFile: File | null = null;

  loading = true;
  uploading = false;
  downloadingId: string | null = null;

  message = '';
  error = '';

  readonly maxFileSizeBytes = 10 * 1024 * 1024;

  constructor(private readonly api: ResumeApiService) {}

  ngOnInit(): void {
    this.loadResume();
  }

  loadResume(): void {
    this.loading = true;
    this.error = '';

    this.api.getResume().subscribe({
      next: resume => {
        this.resume = resume;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;

        if (err.status === 404) {
          this.resume = null;
          return;
        }

        this.showError(err, 'Unable to load your CV.');
      }
    });
  }

  onFileSelected(event: Event): void {
    this.message = '';
    this.error = '';

    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.selectedFile = null;
      return;
    }

    if (file.size > this.maxFileSizeBytes) {
      this.selectedFile = null;
      input.value = '';
      this.error = 'The selected CV exceeds the 10 MB upload limit.';
      return;
    }

    this.selectedFile = file;
  }

  upload(): void {
    if (!this.selectedFile) {
      this.error = 'Select a CV file before uploading.';
      return;
    }

    this.uploading = true;
    this.message = '';
    this.error = '';

    this.api.uploadVersion(this.selectedFile).subscribe({
      next: resume => {
        this.resume = resume;
        this.selectedFile = null;
        this.uploading = false;
        this.message = 'CV uploaded successfully.';

        const input = document.getElementById(
          'resumeFile'
        ) as HTMLInputElement | null;

        if (input) {
          input.value = '';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.uploading = false;
        this.showError(err, 'Unable to upload CV.');
      }
    });
  }

  download(version: ResumeVersion): void {
    this.downloadingId = version.id;
    this.message = '';
    this.error = '';

    this.api.downloadVersion(version.id).subscribe({
      next: response => {
        this.downloadingId = null;
        this.saveDownload(response, version.originalFileName);
      },
      error: (err: HttpErrorResponse) => {
        this.downloadingId = null;
        this.showError(err, 'Unable to download CV.');
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

  private saveDownload(
    response: HttpResponse<Blob>,
    fallbackName: string
  ): void {
    if (!response.body) {
      this.error = 'The downloaded CV was empty.';
      return;
    }

    const contentDisposition =
      response.headers.get('content-disposition');

    const fileName =
      this.extractFileName(contentDisposition) || fallbackName;

    const objectUrl = URL.createObjectURL(response.body);
    const anchor = document.createElement('a');

    anchor.href = objectUrl;
    anchor.download = fileName;
    anchor.click();

    URL.revokeObjectURL(objectUrl);
  }

  private extractFileName(
    contentDisposition: string | null
  ): string | null {
    if (!contentDisposition) {
      return null;
    }

    const utf8Match =
      /filename\*=UTF-8''([^;]+)/i.exec(contentDisposition);

    if (utf8Match?.[1]) {
      try {
        return decodeURIComponent(utf8Match[1]);
      } catch {
        return utf8Match[1];
      }
    }

    const normalMatch =
      /filename="?([^";]+)"?/i.exec(contentDisposition);

    return normalMatch?.[1] ?? null;
  }

  private showError(
    error: HttpErrorResponse,
    fallback: string
  ): void {
    this.message = '';

    if (
      error.error &&
      typeof error.error === 'object' &&
      'message' in error.error &&
      typeof error.error.message === 'string'
    ) {
      this.error = error.error.message;
      return;
    }

    if (typeof error.error === 'string' && error.error.trim()) {
      this.error = error.error;
      return;
    }

    this.error = fallback;
  }
}
