import { safeInternalReturnUrl } from './safe-return-url';

describe('safeInternalReturnUrl', () => {
  it('accepts an internal absolute route', () => {
    expect(safeInternalReturnUrl('/seeker/applications?view=open'))
      .toBe('/seeker/applications?view=open');
  });

  it('rejects external, protocol-relative and backslash routes', () => {
    expect(safeInternalReturnUrl('https://evil.example')).toBe('/jobs');
    expect(safeInternalReturnUrl('//evil.example')).toBe('/jobs');
    expect(safeInternalReturnUrl('/\\evil.example')).toBe('/jobs');
  });
});
