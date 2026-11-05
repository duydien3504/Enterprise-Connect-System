namespace EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;

public interface IGroqApiClient
{
    Task<string> CreateChatCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
