/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { SeekerWorkflowApiService } from '../data/seeker-workflow-api.service';
import { NotificationsComponent } from './notifications.component';

describe('NotificationsComponent S09', () => {
  let api: jasmine.SpyObj<SeekerWorkflowApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<SeekerWorkflowApiService>(
      'SeekerWorkflowApiService',
      ['getNotifications', 'markNotificationRead']
    );

    api.getNotifications.and.returnValue(of([
      { id: 'n1', message: 'Unread update', isRead: false, createdAtUtc: '2026-09-10T09:20:00Z' },
      { id: 'n2', message: 'Read update', isRead: true, createdAtUtc: '2026-09-08T10:42:00Z' }
    ]));
    api.markNotificationRead.and.returnValue(of({ message: 'Notification marked as read.' }));

    await TestBed.configureTestingModule({
      imports: [NotificationsComponent],
      providers: [
        provideRouter([]),
        { provide: SeekerWorkflowApiService, useValue: api }
      ]
    }).compileComponents();
  });

  it('filters unread locally over the owner notification list', () => {
    const fixture = TestBed.createComponent(NotificationsComponent);
    fixture.detectChanges();

    fixture.componentInstance.setFilter('Unread');

    expect(fixture.componentInstance.filteredNotifications.map(item => item.id))
      .toEqual(['n1']);
  });

  it('updates the local row only after the mark-read request succeeds', () => {
    const fixture = TestBed.createComponent(NotificationsComponent);
    fixture.detectChanges();

    const notification = fixture.componentInstance.notifications[0];
    expect(notification.isRead).toBeFalse();

    fixture.componentInstance.markRead(notification);

    expect(api.markNotificationRead).toHaveBeenCalledWith('n1');
    expect(fixture.componentInstance.notifications[0].isRead).toBeTrue();
  });
});