import { Injectable, signal } from '@angular/core';

export interface TransportFailure {
  kind: 'network' | 'server' | 'throttled';
  message: string;
  retryAfterSeconds: number | null;
}

@Injectable({ providedIn: 'root' })
export class TransportErrorPresenter {
  private readonly failureState = signal<TransportFailure | null>(null);

  readonly failure = this.failureState.asReadonly();

  present(failure: TransportFailure): void {
    this.failureState.set(failure);
  }

  clear(): void {
    this.failureState.set(null);
  }
}
