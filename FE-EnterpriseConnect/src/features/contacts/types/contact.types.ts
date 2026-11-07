export interface Contact {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  avatarUrl?: string;
  department?: string;
  status: 'online' | 'offline' | 'busy' | 'away';
  isBlocked?: boolean;
}

export interface ContactRequest {
  targetUserId: string;
  message?: string;
}
