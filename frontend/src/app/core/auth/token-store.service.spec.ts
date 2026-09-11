import { TestBed } from '@angular/core/testing';

import { TokenStore } from './token-store.service';

describe('TokenStore', () => {
  let store: TokenStore;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    store = TestBed.inject(TokenStore);
    store.clear();
  });

  afterEach(() => store.clear());

  it('stores and retrieves an access token through its abstraction', () => {
    store.set('access-token');
    expect(store.get()).toBe('access-token');
    expect(store.hasToken()).toBeTrue();
  });

  it('clears the access token', () => {
    store.set('access-token');
    store.clear();
    expect(store.get()).toBeNull();
    expect(store.hasToken()).toBeFalse();
  });
});
