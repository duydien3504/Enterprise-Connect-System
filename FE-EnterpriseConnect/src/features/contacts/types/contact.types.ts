export interface ContactItem {
  contactId: string;
  userId: string;
  fullName: string;
  email: string;
  avatarUrl?: string | null;
  position?: string | null;
  departmentName?: string | null;
  status: number; // 1 = Pending, 2 = Accepted, 3 = Rejected, 4 = Blocked
  isRequester: boolean;
  createdAt: string;
}

export interface UserListItem {
  id: string;
  fullName: string;
  departmentName?: string | null;
  departmentId?: string | null;
  email: string;
  position?: string | null;
  avatarUrl?: string | null;
  phone?: string | null;
}

export interface GetUsersResult {
  items: UserListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface GetUsersParams {
  name?: string;
  departmentId?: string;
  departmentName?: string;
  page?: number;
  pageSize?: number;
}

export type ContactFilterType = 'accepted' | 'pending_received' | 'pending_sent' | 'blocked';

export type RelationStatus =
  | 'self'
  | 'accepted'
  | 'pending_sent'
  | 'pending_received'
  | 'blocked'
  | 'none';

export interface SendContactResponse {
  contactId: string;
  status: number;
  message?: string;
}

export interface ActionResponse {
  success?: boolean;
  message?: string;
}
