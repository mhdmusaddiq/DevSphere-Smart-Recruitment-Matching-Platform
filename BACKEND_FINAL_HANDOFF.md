# Backend Final Handoff

- Branch: `fix/backend-final-reopen-20260911`
- Base: `develop` at `e3f6b8c323036ae7af1e16133682f91537c272c7`
- Re-frozen backend authority: this commit; resolve with `git rev-parse HEAD`
- Scope: the six accepted Global 3-Family backend ledger items only
- Push/merge status: local commit only; not pushed or merged

## Accepted ledger

- G-SEC-01 — PASS: roles always seed, while administrator bootstrap is disabled by default and requires complete runtime-only configuration when explicitly enabled. No fixed administrator email or password remains in tracked defaults/source.
- G-SEC-02 — PASS: JWT signing material is required from protected runtime configuration, validated at startup, and absent from tracked `appsettings.json`. A production-like launch without it exits non-zero with a clear configuration error.
- G-SEC-03 — PASS: bearer tokens carry the current Identity security stamp; every protected request validates the user is active and the stamp is current. Disable, logout, and successful password reset invalidate previously issued tokens.
- G-SEC-04 — PASS: server-authoritative account/IP throttling uses configuration-backed submission defaults, generic 429 responses, and `Retry-After` when available. Verification/recovery issue and challenge controls remain configuration-backed and anti-enumerating.
- G-E05-01 — PASS: owner policy full-read returns every persisted editable requirement field needed for lossless GET→PUT→GET, including owner-only `ExpectedAnswer`; public/candidate projections were not broadened.
- G-APP-01 — PASS: employer status transitions use a dedicated request DTO and preserve 400 invalid input, 403 non-owner, 404 missing application, and 409 illegal/stale transition semantics.

## Frozen G-SEC-04 defaults

- Login: 5 failed attempts/account/15 minutes; 30 requests/IP/15 minutes; successful valid login resets the account counter.
- Verification issue: 5 issues/email/60 minutes; 60-second cooldown; 20 requests/IP/60 minutes.
- Verification consume: 5 failed attempts/challenge; 10-minute lifetime; 30 requests/IP/15 minutes.
- Recovery issue: 5 issues/email/60 minutes; 60-second cooldown; 20 requests/IP/60 minutes.
- Reset consume: 5 failed attempts/challenge; 10-minute lifetime; 30 requests/IP/15 minutes.

## Verification

- Accepted-ledger targeted tests: 11 passed, 0 failed, 0 skipped.
- Security regression suite: 108 passed, 0 failed, 0 skipped.
- Full backend suite: 226 passed, 0 failed, 0 skipped.
- Build: green with 0 warnings and 0 errors (`dotnet build backend/DevSphere.sln --no-restore`).
- Database: SQL Server zero-to-head apply passed on disposable `DevSphere_BackendReopen_20260911`; current head remains `20260909213539_BackendFinalClosure`; EF reports no pending model changes.
- OpenAPI: regenerated and reviewed; OpenAPI 3.0.1 with 100 paths at `backend/artifacts/openapi/devsphere-backend-final.openapi.json`. The application-status operation now references `ApplicationStatusTransitionRequest`.
- G-SEC-04 HTTP smoke: all account and IP limit+1 boundaries passed, including valid-login reset, generic body, and `Retry-After`.
- Backend/frontend-enablement HTTP smoke: passed the complete existing journey plus application-transition 200/400/403/404/409 checks.
- Frontend files changed: no.
- Deferred scope changed: no.

## Residual risks

- Any environment that previously ran the historical bootstrap credential must rotate or disable that already-persisted account operationally before deployment; this repository no longer provisions or contains the credential.
- Rate-limit counters are intentionally process-local for this academic/local submission; multi-instance distributed enforcement remains out of scope.
- Direct `HttpContext.Connection.RemoteIpAddress` partitioning is correct for the current no-proxy deployment. A future trusted reverse proxy must configure forwarded-header processing before relying on forwarded client addresses.
- Canonical offer-state expansion and interview reschedule/conflict support remain deferred P1 scope.
