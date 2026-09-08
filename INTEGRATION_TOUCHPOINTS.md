# Integration Touchpoints - Jeni Second Brain Backend

## Scope

This branch extends candidate professional evidence, readiness,
resume version handling, canonical skill taxonomy and the assigned
Auth/Admin rescue scope.

It does not introduce a second matching engine or redesign the frontend.

## Authentication

- Public registration accepts JobSeeker and Employer only.
- JobSeeker maps to the existing internal Candidate role.
- Public role validation occurs before Identity user creation.
- Disabled accounts cannot log in.
- `/api/auth/me` returns stable identity, normalized role and account state.

## Administration

- Adds paged account listing and account dashboard counts.
- Admins can enable and disable accounts.
- Self-disable is blocked.
- The last active administrator cannot be disabled.

## Candidate Profile and Readiness

- Structured work preferences are persisted.
- Structured availability is persisted.
- Profile readiness reports missing requirements without a score.
- Application readiness additionally requires a valid current resume version.

## Canonical Taxonomy

- Candidate skills resolve through SkillConcept and SkillAlias.
- Skill aliases normalize to one active canonical concept.
- Candidate Skill retains its display Name and stores nullable SkillConceptId.
- OccupationConcept is foundation-only.
- Existing matching logic is intentionally not replaced.

## Resume

- Replacement uploads create new ResumeVersion rows.
- Existing versions are not rewritten in place.
- Current-version selection is owner scoped.
- Resume.CurrentVersionId and ResumeVersion.IsCurrent are synchronized.
- Existing private resume authorization remains in place.

## Career Evidence

- Certification, project and language CRUD are owner scoped.
- Licence/registration CRUD is owner scoped.
- Licence/registration includes issuer, identifier, dates, status and verification state.

## Database

This branch includes a focused manual Second Brain migration for:

- Candidate work-preference columns.
- Candidate availability columns.
- SkillConcepts.
- SkillAliases.
- OccupationConcepts.
- LicenceRegistrations.
- nullable Skills.SkillConceptId and its FK/index.

The shared HEAD EF model snapshot predates several already-existing
Resume, Career, Matching and Application models present in the current
codebase. A normal EF migration therefore attempted to recreate unrelated
existing tables.

To avoid introducing destructive or duplicate shared-schema operations,
the Second Brain migration is intentionally focused and the stale shared
snapshot is not silently rewritten on this feature branch.

The coordinator should reconcile the shared EF snapshot against the
integrated migration history before generating future broad migrations.

## Integration Cautions

- Preserve internal Candidate role for existing authorization attributes.
- Public terminology should use JobSeeker.
- Do not replace the existing matching engine.
- Do not expose resume storage keys or bypass ownership checks.
- Do not regenerate this migration from the stale feature-branch snapshot.

## Deferred

- Frontend redesign/expansion.
- Matching formula changes.
- Vacancy taxonomy migration.
- Admin role promotion/demotion.
- Readiness or hiring scores.
- Shared historical EF snapshot reconciliation.
