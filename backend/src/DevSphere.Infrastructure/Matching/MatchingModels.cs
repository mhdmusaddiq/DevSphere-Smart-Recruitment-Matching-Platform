using DevSphere.Domain.Entities.Candidates;
using DevSphere.Domain.Entities.Career;
using DevSphere.Domain.Entities.Vacancies;

namespace DevSphere.Infrastructure.Matching;

public class CandidateMatchingEvidence
{
    public CandidateProfile Candidate { get; init; } = null!;

    public IReadOnlyList<WorkExperience> WorkExperiences { get; init; }
        = Array.Empty<WorkExperience>();

    public IReadOnlyList<EducationRecord> EducationRecords { get; init; }
        = Array.Empty<EducationRecord>();

    public IReadOnlyList<CertificationRecord> Certifications { get; init; }
        = Array.Empty<CertificationRecord>();

    public IReadOnlyList<LicenceRegistration> Licences { get; init; }
        = Array.Empty<LicenceRegistration>();

    public IReadOnlyList<LanguageCapability> Languages { get; init; }
        = Array.Empty<LanguageCapability>();

    public IReadOnlyList<ProjectRecord> Projects { get; init; }
        = Array.Empty<ProjectRecord>();
}

public class VacancyMatchingPolicy
{
    public Vacancy Vacancy { get; init; } = null!;

    public MatchingPolicyRevision? Revision { get; init; }

    public IReadOnlyList<FamilyPolicy> Families { get; init; }
        = Array.Empty<FamilyPolicy>();

    public IReadOnlyList<VacancyRequirement> Requirements { get; init; }
        = Array.Empty<VacancyRequirement>();

    public IReadOnlyList<AlternativeSet> AlternativeSets { get; init; }
        = Array.Empty<AlternativeSet>();
}
