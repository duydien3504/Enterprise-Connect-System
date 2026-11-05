namespace EnterpriseConnect.MessagingService.Application.Common.Interfaces;

public record KnowledgeAnswerResult(string Answer, IReadOnlyList<string> Sources);
