import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';

const TOKEN_KEY = 'aptlens_access_token';

@Injectable({ providedIn: 'root' })
export class TokenStore {
  private readonly document = inject(DOCUMENT);

  get(): string | null {
    return this.storage?.getItem(TOKEN_KEY) ?? null;
  }

  set(token: string): void {
    this.storage?.setItem(TOKEN_KEY, token);
  }

  clear(): void {
    this.storage?.removeItem(TOKEN_KEY);
  }

  hasToken(): boolean {
    return Boolean(this.get());
  }

  private get storage(): Storage | null {
    return this.document.defaultView?.sessionStorage ?? null;
  }
}
