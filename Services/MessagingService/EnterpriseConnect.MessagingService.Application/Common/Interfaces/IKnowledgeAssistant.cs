namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public interface IKnowledgeAssistant
{
    Task<KnowledgeAnswerResult> AnswerQuestionAsync(
        string question, 
        Guid? departmentId, 
        IEnumerable<Guid>? roleIds, 
        CancellationToken cancellationToken = default);
}
