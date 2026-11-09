using System.Security.Claims;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;
using EnterpriseConnect.MessagingService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseConnect.MessagingService.Api.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IMediator _mediator;
    private readonly IMessagingUnitOfWork _uow;

    public ChatHub(IMediator mediator, IMessagingUnitOfWork uow)
    {
        _mediator = mediator;
        _uow = uow;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId != Guid.Empty)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            try
            {
                var conversations = await _uow.Conversations.GetUserConversationsAsync(userId, Context.ConnectionAborted);
                foreach (var conv in conversations)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, conv.Id.ToString());
                }
            }
            catch
            {
                // Non-fatal if initial fetch fails
            }
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task SendTyping(Guid conversationId, bool isTyping)
    {
        var userId = GetCurrentUserId();
        await Clients.OthersInGroup(conversationId.ToString()).UserTyping(conversationId, userId, isTyping);
    }

    public async Task SendMessage(Guid conversationId, string content, Guid? replyToId = null)
    {
        var senderId = GetCurrentUserId();
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

    private Guid GetCurrentUserId()
    {
        var sub = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? Context.User?.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
