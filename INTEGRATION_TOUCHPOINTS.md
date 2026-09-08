# INTEGRATION TOUCHPOINTS — Jeni + Thikal merged notes

This file intentionally preserves both Second-Brain integration scopes after cherry-picking
Jeni candidate/auth-admin safeguards and Thikal employer/vacancy/P1 workflow.

## Jeni scope already integrated
- candidate readiness
- candidate career evidence, including licence/registration
- canonical skill taxonomy foundation
- resume hardening
- core Auth/Admin safeguards
- Second-Brain security contract tests

## Thikal scope being integrated
- company trust / membership / verification
- vacancy lifecycle and matching policy revisions
- FamilyPolicy / AlternativeSet / VacancyRequirement policy model
- required-skill persistence and mapping
- interview / interview slot
- scorecard
- offer
- talent pool
- employer workflow tests

## Shared-file rule
The following shared files were manually union-merged and must be revalidated:
- `backend/src/DevSphere.Infrastructure/Configurations/DomainEntityConfigurations.cs`
- `backend/src/DevSphere.Infrastructure/Services/Profile/VacancyService.cs`
- `backend/src/DevSphere.Infrastructure/Data/DevSphereDbContext.cs`
- `backend/src/DevSphere.Api/Extensions/ServiceCollectionExtensions.cs`

## Final integration gate
After the Thikal implementation + test commits are integrated:
1. `dotnet build`
2. `dotnet test`
3. `git diff --check`
4. inspect EF migrations/model registration
5. confirm no duplicate DI/entity configuration
6. do not start RM-2.1 final engine or ApplicationSnapshot closure until this base is green
