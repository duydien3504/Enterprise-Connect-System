using EnterpriseConnect.MessagingService.Application.Features.Messages.DTOs;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public record SendMessageResult(
    bool Success,
    MessageDto? Message,
    bool IsToxicBlocked,
    string? ToxicReason,
    bool IsBotResponse,
    string? BotAnswer,
    IReadOnlyList<string>? BotSources
);
