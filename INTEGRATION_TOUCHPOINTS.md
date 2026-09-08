# Integration Touchpoints

## Thikalnila Second-Brain Fast Track

Shared backend persistence files were intentionally modified because the
company-trust and professional vacancy-policy model requires EF Core persistence.

Shared files changed:
- backend/src/DevSphere.Infrastructure/Data/DevSphereDbContext.cs
- backend/src/DevSphere.Api/Extensions/ServiceCollectionExtensions.cs
- backend/src/DevSphere.Infrastructure/Configurations/DomainEntityConfigurations.cs

Persistence concepts added:
- CompanyProfile
- CompanyMembership
- MatchingPolicyRevision
- FamilyPolicy
- AlternativeSet

Existing concepts extended:
- CompanyVerification
- VacancyRequirement
- Vacancy lifecycle state

Constraints:
- CompanyMembership: unique (CompanyId, EmployerUserId)
- MatchingPolicyRevision: unique (VacancyId, RevisionNumber)
- FamilyPolicy: unique (MatchingPolicyRevisionId, RequirementFamily)

Compatibility:
- CompanyVerification.EmployerProfileId retained.
- VacancyRequirement.VacancyId, Description and IsMandatory retained.
- Vacancy.IsOpen retained as lifecycle compatibility projection.
- Existing MatchEngine was not replaced or duplicated.

Migration note:
- No migration or DevSphereDbContextModelSnapshot update is created on this feature branch.
- Integration lead should reconcile the final migration/model snapshot after merge.


## P1 employer workflow persistence

Added professional employer workflow persistence concepts:

- Interview
- InterviewSlot
- Scorecard
- Offer
- TalentPoolEntry

Ownership is derived through JobApplication -> Vacancy -> authenticated employer.
Scorecard remains separate from compatibility/matching score.
Offer lifecycle remains separate from ApplicationStatus.
Talent-pool persistence requires an existing application relationship and candidate consent.

No feature-branch migration/model snapshot was generated.
Integration lead should reconcile EF migrations/model snapshot after branch merge.
