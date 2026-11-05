using EnterpriseConnect.MessagingService.Domain.Enums;

namespace EnterpriseConnect.MessagingService.Api.Models;

public record SendMessageRequest(
    Guid ConversationId, 
    string Content, 
    MessageType Type = MessageType.Text, 
    Guid? ReplyToId = null
);
