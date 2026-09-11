import { apiUrl } from './api-base-url';

describe('apiUrl', () => {
  it('joins base and endpoint paths without duplicate separators', () => {
    expect(apiUrl('https://example.test/api/', '/auth/me'))
      .toBe('https://example.test/api/auth/me');
    expect(apiUrl('https://example.test/api', 'vacancies'))
      .toBe('https://example.test/api/vacancies');
  });
});
