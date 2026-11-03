using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;
using EnterpriseConnect.MessagingService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseConnect.MessagingService.Api.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);
    Task MessageStatusChanged(Guid messageId, MessageStatus status);
    Task UserTyping(Guid conversationId, Guid userId, bool isTyping);
    Task MessageReacted(Guid messageId, Guid userId, string emoji, bool isAdded);
    Task MessageEdited(Guid messageId, string newContent, DateTimeOffset editedAt);
    Task MessageDeleted(Guid messageId);
    Task MessagePinned(Guid messageId, bool isPinned, Guid? pinnedBy);
    Task ToxicWarning(string warningMessage);
}

public class ChatHub : Hub<IChatClient>
{
    private readonly MediatR.IMediator _mediator;

    public ChatHub(MediatR.IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task SendTyping(Guid conversationId, Guid userId, bool isTyping)
    {
        await Clients.OthersInGroup(conversationId.ToString()).UserTyping(conversationId, userId, isTyping);
    }

    public async Task SendMessage(Guid conversationId, Guid senderId, string content, Guid? replyToId = null)
    {
        var command = new SendMessageCommand(conversationId, senderId, content, MessageType.Text, replyToId);
        var result = await _mediator.Send(command);

        if (result.IsToxicBlocked)
        {
            await Clients.Caller.ToxicWarning($"Tin nhắn của bạn không thể gửi đi do vi phạm chính sách kiểm duyệt: {result.ToxicReason}");
            return;
        }

        if (result.Message != null)
        {
            await Clients.Group(conversationId.ToString()).ReceiveMessage(result.Message);
        }
    }
}
