using EnterpriseConnect.BuildingBlocks.Application.Audit;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Messages.DTOs;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResult>
{
    private readonly IMessagingUnitOfWork _uow;
    private readonly IToxicDetectionService _toxicService;
    private readonly IKnowledgeAssistant _knowledgeAssistant;
    private readonly ICurrentUserService? _currentUserService;
    private readonly IAuditClient? _auditClient;

    public SendMessageCommandHandler(
        IMessagingUnitOfWork uow,
        IToxicDetectionService toxicService,
        IKnowledgeAssistant knowledgeAssistant,
        ICurrentUserService? currentUserService = null,
        IAuditClient? auditClient = null)
    {
        _uow = uow;
        _toxicService = toxicService;
        _knowledgeAssistant = knowledgeAssistant;
        _currentUserService = currentUserService;
        _auditClient = auditClient;
    }

    public async Task<SendMessageResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _uow.Conversations.GetByIdWithMembersAsync(request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy cuộc trò chuyện.");

        if (!conversation.Members.Any(m => m.UserId == request.SenderId))
            throw new UnauthorizedAccessException("Bạn không phải thành viên của cuộc trò chuyện này.");

        var toxicCheck = await _toxicService.AnalyzeContentAsync(request.Content, cancellationToken);
        if (toxicCheck.IsToxic && (toxicCheck.Severity == "Medium" || toxicCheck.Severity == "High"))
        {
            _ = _auditClient?.RecordEventAsync(new AuditEventDto
            {
                ServiceName = "MessagingService", ActionName = "TOXIC_MESSAGE_BLOCKED", TargetEntity = "Message",
                TargetId = request.ConversationId.ToString(), ActorId = request.SenderId,
                RiskLevel = toxicCheck.Severity == "High" ? 4 : 3,
                Details = $"Toxic content blocked: {toxicCheck.Reason}. Severity: {toxicCheck.Severity}"
            }, cancellationToken);
            return new SendMessageResult(false, null, true, toxicCheck.Reason, false, null, null);
        }

        if (request.Content.StartsWith("/a ", StringComparison.OrdinalIgnoreCase))
            return await HandleAiBotMessageAsync(conversation, request, cancellationToken);

        var message = new Message
        {
            ConversationId = request.ConversationId, SenderId = request.SenderId,
            Content = request.Content, Type = request.Type, Status = MessageStatus.Sent, ReplyToId = request.ReplyToId
        };

        await _uow.Messages.AddAsync(message, cancellationToken);
        conversation.LastMessageId = message.Id;
        conversation.LastMessageAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        var dto = new MessageDto(
            message.Id, message.ConversationId, message.SenderId, message.Content,
            message.Type, message.Status, message.CreatedAt, false, false,
            message.ReplyToId, Array.Empty<string>()
        );
        return new SendMessageResult(true, dto, false, null, false, null, null);
    }

    private async Task<SendMessageResult> HandleAiBotMessageAsync(Conversation conversation, SendMessageCommand request, CancellationToken cancellationToken)
    {
        var question = request.Content.Substring(3).Trim();
        var departmentId = _currentUserService?.DepartmentId;
        var roleIds = _currentUserService?.RoleIds;
        var botResult = await _knowledgeAssistant.AnswerQuestionAsync(question, departmentId, roleIds, cancellationToken);

        var sourceText = botResult.Sources.Count > 0 ? $"\n\n*Nguồn tài liệu:* {string.Join(", ", botResult.Sources)}" : string.Empty;
        var botMessage = new Message
        {
            ConversationId = request.ConversationId, SenderId = Guid.Empty,
            Content = $"🤖 **Trợ lý AI:** {botResult.Answer}{sourceText}",
            Type = MessageType.Text, Status = MessageStatus.Sent
        };

        await _uow.Messages.AddAsync(botMessage, cancellationToken);
        conversation.LastMessageId = botMessage.Id;
        conversation.LastMessageAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        var botDto = new MessageDto(
            botMessage.Id, botMessage.ConversationId, botMessage.SenderId, botMessage.Content,
            botMessage.Type, botMessage.Status, botMessage.CreatedAt, false, false, null, Array.Empty<string>()
        );
        return new SendMessageResult(true, botDto, false, null, true, botResult.Answer, botResult.Sources);
    }
}
