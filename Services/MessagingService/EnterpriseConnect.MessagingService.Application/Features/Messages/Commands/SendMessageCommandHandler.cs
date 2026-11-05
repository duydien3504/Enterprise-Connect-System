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

    public SendMessageCommandHandler(
        IMessagingUnitOfWork uow,
        IToxicDetectionService toxicService,
        IKnowledgeAssistant knowledgeAssistant)
    {
        _uow = uow;
        _toxicService = toxicService;
        _knowledgeAssistant = knowledgeAssistant;
    }

    public async Task<SendMessageResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _uow.Conversations.GetByIdWithMembersAsync(request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy cuộc trò chuyện.");

        var isMember = conversation.Members.Any(m => m.UserId == request.SenderId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Bạn không phải thành viên của cuộc trò chuyện này.");
        }

        // 1. Kiểm tra Toxic Detection
        var toxicCheck = await _toxicService.AnalyzeContentAsync(request.Content, cancellationToken);
        if (toxicCheck.IsToxic && (toxicCheck.Severity == "Medium" || toxicCheck.Severity == "High"))
        {
            return new SendMessageResult(false, null, true, toxicCheck.Reason, false, null, null);
        }

        // 2. Kiểm tra lệnh hỏi đáp AI Bot (/a)
        if (request.Content.StartsWith("/a ", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleAiBotMessageAsync(conversation, request, cancellationToken);
        }

        // 3. Tin nhắn bình thường
        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Content = request.Content,
            Type = request.Type,
            Status = MessageStatus.Sent,
            ReplyToId = request.ReplyToId
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
        var botResult = await _knowledgeAssistant.AnswerQuestionAsync(question, null, null, cancellationToken);

        var botMessage = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = Guid.Empty,
            Content = $"🤖 **Trợ lý AI:** {botResult.Answer}\n\n*Nguồn tài liệu:* {string.Join(", ", botResult.Sources)}",
            Type = MessageType.Text,
            Status = MessageStatus.Sent
        };

        await _uow.Messages.AddAsync(botMessage, cancellationToken);
        conversation.LastMessageId = botMessage.Id;
        conversation.LastMessageAt = DateTimeOffset.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        var botDto = new MessageDto(
            botMessage.Id, botMessage.ConversationId, botMessage.SenderId, botMessage.Content,
            botMessage.Type, botMessage.Status, botMessage.CreatedAt, false, false,
            null, Array.Empty<string>()
        );

        return new SendMessageResult(true, botDto, false, null, true, botResult.Answer, botResult.Sources);
    }
}
