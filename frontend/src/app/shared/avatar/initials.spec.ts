import { initials } from './initials';

describe('initials', () => {
  it('creates deterministic one or two letter identities', () => {
    expect(initials('Ada Lovelace')).toBe('AL');
    expect(initials('Angular')).toBe('AN');
    expect(initials('  Grace   Brewster Murray Hopper ')).toBe('GH');
  });

  it('uses a neutral fallback for missing factual identity', () => {
    expect(initials('')).toBe('?');
  });
});
