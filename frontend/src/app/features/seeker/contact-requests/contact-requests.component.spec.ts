/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { SeekerWorkflowApiService } from '../data/seeker-workflow-api.service';
import { ContactRequestView } from '../data/seeker-workflow.models';
import { ContactRequestsComponent } from './contact-requests.component';

const REQUESTS: ContactRequestView[] = [
  {
    id: '1', jobApplicationId: 'a1', employerId: 'e1', candidateId: 'candidate',
    status: 'Pending', vacancyId: 'v1', vacancyTitle: 'Frontend Engineer',
    companyId: 'c1', companyName: 'Ceylon Digital Labs',
    candidateDisplayName: 'Jeni', employerDisplayName: 'Recruiter',
    canDiscloseContact: false, candidateEmail: null
  },
  {
    id: '2', jobApplicationId: 'a2', employerId: 'e2', candidateId: 'candidate',
    status: 'Revoked', vacancyId: 'v2', vacancyTitle: 'QA Engineer',
    companyId: 'c2', companyName: 'Quality Labs',
    candidateDisplayName: 'Jeni', employerDisplayName: 'Hiring team',
    canDiscloseContact: false, candidateEmail: null
  }
];

describe('ContactRequestsComponent S08', () => {
  let api: jasmine.SpyObj<SeekerWorkflowApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<SeekerWorkflowApiService>(
      'SeekerWorkflowApiService',
      ['getContactRequests', 'updateContactStatus']
    );
    api.getContactRequests.and.returnValue(of(REQUESTS));

    await TestBed.configureTestingModule({
      imports: [ContactRequestsComponent],
      providers: [
        provideRouter([]),
        { provide: SeekerWorkflowApiService, useValue: api }
      ]
    }).compileComponents();
  });

  it('groups terminal policy states under Closed without fake counts', () => {
    const fixture = TestBed.createComponent(ContactRequestsComponent);
    fixture.detectChanges();

    fixture.componentInstance.setFilter('Closed');
    expect(fixture.componentInstance.filteredRequests.map(item => item.id))
      .toEqual(['2']);
  });

  it('offers only server-supported candidate transitions', () => {
    const fixture = TestBed.createComponent(ContactRequestsComponent);
    fixture.detectChanges();

    fixture.componentInstance.askAction(REQUESTS[0], 'Accepted');
    expect(fixture.componentInstance.selectedAction).toBe('Accepted');

    fixture.componentInstance.cancelAction();
    fixture.componentInstance.askAction(REQUESTS[1], 'Accepted');
    expect(fixture.componentInstance.selectedAction).toBeNull();
  });
});