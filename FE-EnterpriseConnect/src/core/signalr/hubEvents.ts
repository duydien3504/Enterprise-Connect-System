export const HUB_EVENTS = {
  CHAT: {
    RECEIVE_MESSAGE: 'ReceiveMessage',
    MESSAGE_READ: 'MessageRead',
    USER_TYPING: 'UserTyping',
    MESSAGE_PINNED: 'MessagePinned',
    MESSAGE_RECALLED: 'MessageRecalled',
  },
  PRESENCE: {
    USER_ONLINE: 'UserOnline',
    USER_OFFLINE: 'UserOffline',
    STATUS_CHANGED: 'UserStatusChanged',
  },
  CALL: {
    INCOMING_CALL: 'IncomingCall',
    CALL_ACCEPTED: 'CallAccepted',
    CALL_REJECTED: 'CallRejected',
    CALL_ENDED: 'CallEnded',
    ICE_CANDIDATE: 'IceCandidate',
    SDP_OFFER: 'SdpOffer',
    SDP_ANSWER: 'SdpAnswer',
  },
  REMOTE: {
    SESSION_INVITATION: 'RemoteSessionInvitation',
    SESSION_CONNECTED: 'RemoteSessionConnected',
    COMMAND_RECEIVED: 'RemoteCommandReceived',
  },
} as const;
