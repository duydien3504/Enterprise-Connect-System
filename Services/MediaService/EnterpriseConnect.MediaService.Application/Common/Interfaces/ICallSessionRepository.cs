using EnterpriseConnect.MediaService.Domain.Entities;

namespace EnterpriseConnect.MediaService.Application.Common.Interfaces;

public interface ICallSessionRepository
{
    Task<CallSession?> GetByIdWithParticipantsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CallSession?> GetActiveCallByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CallSession>> GetUserCallHistoryAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task AddAsync(CallSession callSession, CancellationToken cancellationToken = default);
    Task UpdateAsync(CallSession callSession, CancellationToken cancellationToken = default);
}
