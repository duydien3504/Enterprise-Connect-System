namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IMessagingUnitOfWork
{
    IConversationRepository Conversations { get; }
    IMessageRepository Messages { get; }
    IAiSummaryRepository AiSummaries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
