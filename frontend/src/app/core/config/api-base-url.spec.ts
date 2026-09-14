import { TestBed } from '@angular/core/testing';

import { API_BASE_URL, apiUrl } from './api-base-url';

describe('apiUrl', () => {
  it('uses the deployment-safe same-origin API base by default', () => {
    expect(TestBed.inject(API_BASE_URL)).toBe('/api');
  });

  it('joins base and endpoint paths without duplicate separators', () => {
    expect(apiUrl('https://example.test/api/', '/auth/me'))
      .toBe('https://example.test/api/auth/me');
    expect(apiUrl('https://example.test/api', 'vacancies'))
      .toBe('https://example.test/api/vacancies');
  });
});
