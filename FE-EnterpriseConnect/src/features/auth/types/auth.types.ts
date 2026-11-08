export type SessionType = 'persistent' | 'session_only';

export const DeviceType = {
  Web: 1,
  Desktop: 2,
  Mobile: 3,
} as const;

export type DeviceType = (typeof DeviceType)[keyof typeof DeviceType];

export interface LoginRequest {
  email: string;
  password: string;
  deviceType?: number; // 1 = Web, 2 = Desktop, 3 = Mobile
  deviceInfo?: string;
  ipAddress?: string;
  rememberMe?: boolean; // Client session preference: true = Option A (Persistent 30d), false = Option B (Session-only 8h)
}

export interface RefreshTokenRequest {
  refreshToken?: string;
  ipAddress?: string;
  deviceInfo?: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
  department?: string;
}

export interface LogoutRequest {
  refreshToken?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ChangePasswordResponse {
  message?: string;
}

export interface UpdateProfileRequest {
  fullName: string;
  phone: string;
  position: string;
}

export interface UploadAvatarResponse {
  avatarUrl?: string;
  message?: string;
}

export interface UserSummaryDto {
  id: string;
  email: string;
  fullName: string;
  phone?: string;
  position?: string;
  avatarUrl?: string;
  departmentId?: string | null;
  departmentName?: string;
  roles: string[];
  permissions?: string[];
  status?: number | 'online' | 'offline' | 'busy' | 'away';
  emailConfirmed?: boolean;
}

export interface UserProfileDto {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  position: string;
  avatarUrl: string;
  departmentId: string;
  departmentName: string;
  status: number; // 1: Active
  emailConfirmed: boolean;
  roles: string[];
  permissions: string[];
}

export type UserProfile = UserSummaryDto;

export interface AuthResponse {
  accessToken: string;
  refreshToken?: string;
  expiresIn: number; // Duration in seconds (e.g. 3600 backend or 900 in RAM)
  tokenType: string; // 'Bearer'
  sessionType?: SessionType;
  user: UserSummaryDto;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken?: string;
  expiresIn: number;
  tokenType?: string;
  user?: UserSummaryDto;
}

export interface AuthState {
  user: UserSummaryDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  sessionType: SessionType;
  tokenExpiresAt: number;
  error: string | null;
}
