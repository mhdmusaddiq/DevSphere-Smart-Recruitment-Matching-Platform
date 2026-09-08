using DevSphere.Application.DTOs.Employers;

namespace DevSphere.Application.Interfaces;

public interface IEmployerWorkflowService
{
    Task<InterviewDto> CreateInterviewAsync(
        string employerUserId,
        CreateInterviewRequest request);

    Task<InterviewSlotDto> AddInterviewSlotAsync(
        string employerUserId,
        Guid interviewId,
        CreateInterviewSlotRequest request);

    Task<ScorecardDto> CreateScorecardAsync(
        string employerUserId,
        CreateScorecardRequest request);

    Task<OfferDto> CreateOfferAsync(
        string employerUserId,
        CreateOfferRequest request);

    Task<OfferDto> UpdateOfferStatusAsync(
        string employerUserId,
        Guid offerId,
        UpdateOfferStatusRequest request);

    Task<TalentPoolEntryDto> AddTalentPoolEntryAsync(
        string employerUserId,
        CreateTalentPoolEntryRequest request);

    Task<InterviewDto> UpdateInterviewStatusAsync(
        string employerUserId,
        Guid interviewId,
        UpdateInterviewStatusRequest request);
}

