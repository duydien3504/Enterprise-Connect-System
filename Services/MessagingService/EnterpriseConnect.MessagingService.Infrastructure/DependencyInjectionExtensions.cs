using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Infrastructure.Persistence.Repositories;
using EnterpriseConnect.MessagingService.Infrastructure.Security;
using EnterpriseConnect.MessagingService.Infrastructure.Services.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseConnect.MessagingService.Infrastructure;

public static partial class DependencyInjectionExtensions
{
    public static IServiceCollection AddMessagingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Repositories & Unit of Work
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IAiSummaryRepository, AiSummaryRepository>();
        services.AddScoped<IMessagingUnitOfWork, MessagingUnitOfWork>();

        // AI Services & Groq Client
        services.AddHttpClient<IGroqApiClient, GroqApiClient>();
        services.AddScoped<IToxicDetectionService, ToxicDetectionService>();
        services.AddScoped<IConversationSummarizer, ConversationSummarizer>();
        services.AddScoped<IKnowledgeAssistant, KnowledgeAssistant>();

        return services;
    }
}
