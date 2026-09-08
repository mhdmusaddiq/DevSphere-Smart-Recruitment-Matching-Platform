using DevSphere.Application.DTOs.Employers;

namespace DevSphere.Application.Interfaces;

public interface ICompanyTrustService
{
    Task<CompanyProfileDto> CreateCompanyAsync(
        string employerUserId,
        CreateCompanyProfileRequest request);

    Task<IEnumerable<CompanyProfileDto>> GetMineAsync(
        string employerUserId);

    Task<CompanyVerificationResultDto> SubmitVerificationAsync(
        string employerUserId,
        Guid companyId,
        SubmitCompanyVerificationRequest request);
}
