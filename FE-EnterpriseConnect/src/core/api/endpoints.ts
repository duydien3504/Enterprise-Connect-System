export const API_ENDPOINTS = {
  // Identity Service
  AUTH: {
    LOGIN: '/api/Auth/login',
    REGISTER: '/api/Auth/register',
    REFRESH_TOKEN: '/api/Auth/refresh-token',
    LOGOUT: '/api/Auth/logout',
    CHANGE_PASSWORD: '/api/Auth/change-password',
    SESSIONS: '/api/Auth/sessions',
  },
  USERS: {
    PROFILE: '/api/Users/profile',
    UPDATE: '/api/Users/profile',
  },
  CONTACTS: {
    BASE: '/api/Contacts',
    REQUEST: (id: string) => `/api/Contacts/request/${id}`,
    RESPOND: (id: string) => `/api/Contacts/respond/${id}`,
    BLOCK: (id: string) => `/api/Contacts/block/${id}`,
    UNBLOCK: (id: string) => `/api/Contacts/unblock/${id}`,
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
    DETAIL: (id: string) => `/api/Conversations/${id}`,
    MEMBERS: (id: string) => `/api/Conversations/${id}/members`,
  },
  MESSAGES: {
    BASE: '/api/Messages',
    PIN: (id: string) => `/api/Messages/${id}/pin`,
    RECALL: (id: string) => `/api/Messages/${id}/recall`,
    REACTION: (id: string) => `/api/Messages/${id}/reaction`,
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
