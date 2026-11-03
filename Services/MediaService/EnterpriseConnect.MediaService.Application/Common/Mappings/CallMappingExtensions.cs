using EnterpriseConnect.MediaService.Application.Common.Models;
using EnterpriseConnect.MediaService.Domain.Entities;

namespace EnterpriseConnect.MediaService.Application.Common.Mappings;

public static class CallMappingExtensions
{
    public static CallSessionDto ToDto(this CallSession session, string? liveKitToken = null)
    {
        return new CallSessionDto
        {
            Id = session.Id,
            ConversationId = session.ConversationId,
            InitiatorId = session.InitiatorId,
            Type = session.Type,
            Scope = session.Scope,
            Status = session.Status,
            LiveKitRoomName = session.LiveKitRoomName,
            LiveKitToken = liveKitToken,
            StartTime = session.StartTime,
            ConnectedAt = session.ConnectedAt,
            EndTime = session.EndTime,
            DurationSeconds = session.DurationSeconds,
            EndReason = session.EndReason,
            Participants = session.Participants.Select(p => p.ToDto()).ToList()
        };
    }

    public static CallParticipantDto ToDto(this CallParticipant participant)
    {
        return new CallParticipantDto
        {
            Id = participant.Id,
            CallId = participant.CallId,
            UserId = participant.UserId,
            Status = participant.Status,
            IsMuted = participant.IsMuted,
            IsCameraOn = participant.IsCameraOn,
            IsScreenSharing = participant.IsScreenSharing,
            JoinTime = participant.JoinTime,
            LeaveTime = participant.LeaveTime
        };
    }
}
