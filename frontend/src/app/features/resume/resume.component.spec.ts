/// <reference types="jasmine" />

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CvApiService } from './cv-api.service';
import { ResumeComponent } from './resume.component';

describe('ResumeComponent S07', () => {
  const resume = {
    id: 'resume-1',
    candidateProfileId: 'candidate-1',
    currentVersionId: 'version-1',
    versions: [
      {
        id: 'version-1',
        versionNumber: 1,
        originalFileName: 'maya-cv.pdf',
        storageKey: 'private-do-not-render',
        contentType: 'application/pdf',
        fileSizeBytes: 1200,
        isCurrent: true
      }
    ]
  };

  const api = jasmine.createSpyObj<CvApiService>(
    'CvApiService',
    ['getResume', 'uploadVersion', 'setCurrentVersion', 'downloadVersion']
  );

  beforeEach(async () => {
    api.getResume.and.returnValue(of(resume));

    await TestBed.configureTestingModule({
      imports: [ResumeComponent],
      providers: [provideRouter([]), { provide: CvApiService, useValue: api }]
    }).compileComponents();
  });

  it('uses the authoritative 5 MiB client convenience limit and never renders StorageKey', () => {
    const fixture = TestBed.createComponent(ResumeComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.maxFileSizeBytes).toBe(5 * 1024 * 1024);
    expect(fixture.nativeElement.textContent).toContain('maya-cv.pdf');
    expect(fixture.nativeElement.textContent).not.toContain('private-do-not-render');
    expect(fixture.nativeElement.textContent).not.toContain('Delete');
  });

  it('keeps current version state unchanged when set-current fails', () => {
    api.setCurrentVersion.and.returnValue(
      throwError(() => new HttpErrorResponse({ status: 409 }))
    );

    const fixture = TestBed.createComponent(ResumeComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    const other = {
      ...resume.versions[0],
      id: 'version-2',
      versionNumber: 2,
      isCurrent: false
    };

    component.setCurrent(other);

    expect(component.currentVersion?.id).toBe('version-1');
    expect(component.errorMessage).toContain('Unable to change');
  });
});