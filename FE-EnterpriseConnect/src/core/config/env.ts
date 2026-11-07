export const ENV = {
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL || 'https://api.hugonef.id.vn',
  SIGNALR: {
    CHAT_HUB: import.meta.env.VITE_SIGNALR_CHAT_HUB || 'https://api.hugonef.id.vn/hubs/chat',
    CALL_HUB: import.meta.env.VITE_SIGNALR_CALL_HUB || 'https://api.hugonef.id.vn/hubs/call',
    REMOTE_HUB: import.meta.env.VITE_SIGNALR_REMOTE_HUB || 'https://api.hugonef.id.vn/hubs/remote',
  },
} as const;
