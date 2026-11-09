export const API_ENDPOINTS = {
  // Identity Service
  AUTH: {
    LOGIN: '/api/Auth/login',
    REGISTER: '/api/Auth/register',
    REFRESH_TOKEN: '/api/Auth/refresh-token',
    LOGOUT: '/api/Auth/logout',
    CHANGE_PASSWORD: '/api/Auth/change-password',
    FORGOT_PASSWORD: '/api/Auth/forgot-password',
    RESET_PASSWORD: '/api/Auth/reset-password',
    SESSIONS: '/api/Auth/sessions',
  },
  USERS: {
    LIST: '/api/Users',
    PROFILE: '/api/Users/profile',
    UPDATE: '/api/Users/profile',
    AVATAR: '/api/Users/profile/avatar',
    BY_ID: (id: string) => `/api/Users/${id}`,
  },
  CONTACTS: {
    BASE: '/api/Contacts',
    REQUEST: (id: string) => `/api/Contacts/request/${id}`,
    RESPOND: (id: string) => `/api/Contacts/respond/${id}`,
    BLOCK: (id: string) => `/api/Contacts/block/${id}`,
    UNBLOCK: (id: string) => `/api/Contacts/unblock/${id}`,
    REMOVE: (id: string) => `/api/Contacts/${id}`,
  },
  DIRECTORY: {
    TREE: '/api/Directory/tree',
  },
  PRESENCE: {
    STATUS: '/api/Presence/status',
    HEARTBEAT: '/api/Presence/heartbeat',
  },

  // Messaging Service
  CONVERSATIONS: {
    BASE: '/api/Conversations',
    DIRECT: '/api/Conversations/direct',
    GROUP: '/api/Conversations/group',
    DETAIL: (id: string) => `/api/Conversations/${id}`,
    MEMBERS: (id: string) => `/api/Conversations/${id}/members`,
    SUMMARIZE: (id: string) => `/api/Conversations/${id}/summarize`,
  },
  MESSAGES: {
    BASE: '/api/Messages',
    BY_CONVERSATION: (conversationId: string) => `/api/Messages/conversation/${conversationId}`,
    PINNED: (conversationId: string) => `/api/Messages/conversation/${conversationId}/pinned`,
    PIN: (id: string) => `/api/Messages/${id}/pin`,
    RECALL: (id: string) => `/api/Messages/${id}/recall`,
    REACTION: (id: string) => `/api/Messages/${id}/reaction`,
    CHECK_TOXIC: '/api/Messages/check-toxic',
  },

  // Media Service
  CALLS: {
    INITIATE: '/api/Calls/initiate',
    RESPOND: (id: string) => `/api/Calls/${id}/respond`,
    END: (id: string) => `/api/Calls/${id}/end`,
    ICE_SERVERS: '/api/Calls/ice-servers',
  },

  // Remote Assistance Service
  REMOTE: {
    SESSIONS: '/api/RemoteSessions',
    CONNECT: (id: string) => `/api/RemoteSessions/${id}/connect`,
    DISCONNECT: (id: string) => `/api/RemoteSessions/${id}/disconnect`,
  },

  // Admin Service
  ADMIN: {
    AUDIT_LOGS: '/api/AuditLogs',
    DOCUMENTS: '/api/Documents',
    MODERATION: '/api/Moderation/violations',
    SYSTEM_DASHBOARD: '/api/System/dashboard',
  },
} as const;
