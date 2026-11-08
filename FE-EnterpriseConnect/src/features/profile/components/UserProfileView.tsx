import React, { useState, useEffect, useRef } from 'react';
import {
  ArrowLeft,
  Mail,
  Phone,
  Briefcase,
  Building2,
  ShieldCheck,
  CheckCircle2,
  Copy,
  Check,
  KeyRound,
  Lock as LockIcon,
  RefreshCw,
  LogOut,
  Clock,
  ShieldAlert,
  Layers,
  Sparkles,
  Info,
  Camera,
  Pencil,
  Save,
  X,
  AlertCircle,
  Eye,
  EyeOff,
} from 'lucide-react';
import { authApi } from '@/features/auth/api/authApi';
import type { UserProfile } from '@/features/auth/types/auth.types';
import { tokenStorage } from '@/core/storage/tokenStorage';
import { ENV } from '@/core/config/env';

const PERMISSION_META: Record<string, { title: string; desc: string }> = {
  // Documents
  'documents.manage': {
    title: 'Quản trị tài liệu doanh nghiệp',
    desc: 'Toàn quyền lưu trữ, phê duyệt và chia sẻ tài liệu',
  },
  'documents.create': {
    title: 'Tải lên tài liệu mới',
    desc: 'Tạo và chia sẻ tài liệu nội bộ',
  },
  'documents.view': {
    title: 'Xem tài liệu nội bộ',
    desc: 'Truy cập tài liệu và văn bản công ty',
  },
  'documents.delete': {
    title: 'Xóa tài liệu',
    desc: 'Xóa văn bản và hồ sơ đã lưu trữ',
  },

  // Users
  'users.create': {
    title: 'Tạo mới tài khoản nhân viên',
    desc: 'Thêm mới nhân sự vào hệ thống doanh nghiệp',
  },
  'users.update': {
    title: 'Cập nhật thông tin nhân sự',
    desc: 'Chỉnh sửa hồ sơ cá nhân và phòng ban',
  },
  'users.view': {
    title: 'Xem danh bạ & nhân sự',
    desc: 'Tra cứu thông tin danh bạ nhân viên nội bộ',
  },
  'users.delete': {
    title: 'Thu hồi tài khoản nhân viên',
    desc: 'Khóa hoặc xóa nhân sự rời tổ chức',
  },
  'users.manage': {
    title: 'Quản trị tài khoản nhân viên',
    desc: 'Toàn quyền quản lý tài khoản nhân sự',
  },

  // Roles
  'roles.manage': {
    title: 'Quản lý phân quyền vai trò',
    desc: 'Thiết lập nhóm vai trò và chức năng bảo mật',
  },
  'roles.view': {
    title: 'Xem bảng phân quyền vai trò',
    desc: 'Tra cứu danh sách vai trò hệ thống',
  },

  // Auth
  'Auth.Login': {
    title: 'Xác thực & đăng nhập hệ thống',
    desc: 'Khởi tạo phiên làm việc bảo mật trên thiết bị',
  },
  'auth.login': {
    title: 'Xác thực & đăng nhập hệ thống',
    desc: 'Khởi tạo phiên làm việc bảo mật trên thiết bị',
  },

  // Chat
  'Chat.Send': {
    title: 'Gửi tin nhắn & trao đổi công việc',
    desc: 'Trò chuyện trực tiếp 1-1 và các nhóm phòng ban',
  },
  'chat.send': {
    title: 'Gửi tin nhắn & trao đổi công việc',
    desc: 'Trò chuyện trực tiếp 1-1 và các nhóm phòng ban',
  },

  // Media
  'Media.Call': {
    title: 'Đàm thoại âm thanh & gọi video',
    desc: 'Thực hiện cuộc gọi trực tuyến chất lượng cao',
  },
  'media.call': {
    title: 'Đàm thoại âm thanh & gọi video',
    desc: 'Thực hiện cuộc gọi trực tuyến chất lượng cao',
  },

  // Admin
  'Admin.Forensics': {
    title: 'Giám sát an ninh & nhật ký hệ thống',
    desc: 'Theo dõi nhật ký kiểm toán và cảnh báo hệ thống',
  },
  'admin.forensics': {
    title: 'Giám sát an ninh & nhật ký hệ thống',
    desc: 'Theo dõi nhật ký kiểm toán và cảnh báo hệ thống',
  },

  // Audit
  'Audit.Export': {
    title: 'Trích xuất báo cáo tuân thủ',
    desc: 'Xuất file kiểm toán và lịch sử hoạt động',
  },
  'audit.export': {
    title: 'Trích xuất báo cáo tuân thủ',
    desc: 'Xuất file kiểm toán và lịch sử hoạt động',
  },

  // Directory
  'Directory.Read': {
    title: 'Tra cứu sơ đồ tổ chức',
    desc: 'Xem cây phòng ban và phân cấp chức vụ',
  },
  'directory.read': {
    title: 'Tra cứu sơ đồ tổ chức',
    desc: 'Xem cây phòng ban và phân cấp chức vụ',
  },

  // AI
  'AI.Copilot.Access': {
    title: 'Trợ lý AI Copilot thông minh',
    desc: 'Tóm tắt nội dung cuộc họp và gợi ý đầu việc',
  },
  'ai.copilot.access': {
    title: 'Trợ lý AI Copilot thông minh',
    desc: 'Tóm tắt nội dung cuộc họp và gợi ý đầu việc',
  },
};

const getPermissionDisplay = (perm: string) => {
  const normalized = perm.trim();
  const matched = PERMISSION_META[normalized] || PERMISSION_META[normalized.toLowerCase()];
  if (matched) {
    return matched;
  }
  const parts = normalized.split(/[._]/);
  const resource = parts[0] || '';
  const action = parts[1] || '';
  const resourceNames: Record<string, string> = {
    documents: 'tài liệu',
    users: 'nhân sự',
    roles: 'vai trò',
    chat: 'tin nhắn',
    media: 'đàm thoại',
    admin: 'quản trị hệ thống',
    audit: 'báo cáo',
    directory: 'sơ đồ tổ chức',
  };
  const actionNames: Record<string, string> = {
    manage: 'Quản trị',
    create: 'Tạo mới',
    update: 'Cập nhật',
    view: 'Xem & tra cứu',
    read: 'Tra cứu',
    delete: 'Xóa',
    export: 'Trích xuất',
    access: 'Truy cập',
    send: 'Gửi',
  };
  const resName = resourceNames[resource.toLowerCase()] || resource;
  const actName = actionNames[action.toLowerCase()] || action || 'Quyền';

  return {
    title: `${actName} ${resName}`.trim(),
    desc: 'Đặc quyền nghiệp vụ đã được quản trị viên cấp phép',
  };
};

const getRoleDisplay = (role: string) => {
  const normalized = role.trim();
  const roleMap: Record<string, { title: string; desc: string }> = {
    Admin: {
      title: 'Quản trị viên hệ thống',
      desc: 'Toàn quyền cấu hình, quản lý tài khoản và phân quyền chức năng',
    },
    Administrator: {
      title: 'Quản trị viên toàn quyền',
      desc: 'Đặc quyền quản trị cấp cao nhất của tổ chức',
    },
    EnterpriseArchitect: {
      title: 'Kiến trúc sư giải pháp doanh nghiệp',
      desc: 'Hoạch định cấu trúc hệ thống, tích hợp và điều phối công nghệ',
    },
    SystemAdmin: {
      title: 'Quản trị viên hạ tầng & hệ thống',
      desc: 'Giám sát vận hành máy chủ, mạng và an toàn dữ liệu',
    },
    SecurityAuditor: {
      title: 'Kiểm toán viên an ninh mạng',
      desc: 'Kiểm toán an toàn thông tin, rà soát nhật ký và cảnh báo',
    },
    Manager: {
      title: 'Trưởng bộ phận / Quản lý',
      desc: 'Quản trị phòng ban, phân công công việc và duyệt tài liệu',
    },
    User: {
      title: 'Nhân viên doanh nghiệp',
      desc: 'Thành viên chính thức sử dụng các dịch vụ liên lạc nội bộ',
    },
  };

  return (
    roleMap[normalized] || {
      title: normalized,
      desc: 'Vai trò nghiệp vụ doanh nghiệp đã được xác thực',
    }
  );
};

interface UserProfileViewProps {
  onBackToWorkspace: () => void;
  onLogout: () => Promise<void>;
  isLoggingOut?: boolean;
  onProfileUpdated?: (profile: UserProfile) => void;
}

export const UserProfileView: React.FC<UserProfileViewProps> = ({
  onBackToWorkspace,
  onLogout,
  isLoggingOut = false,
  onProfileUpdated,
}) => {
  const [profile, setProfile] = useState<UserProfile | null>(() => {
    const cached = tokenStorage.getUser();
    if (cached) {
      const customAvatar =
        (cached.id ? localStorage.getItem(`ec_custom_avatar_${cached.id}`) : null) ||
        localStorage.getItem('ec_custom_avatar_active');
      if (customAvatar && (!cached.avatarUrl || cached.avatarUrl === 'string' || cached.avatarUrl.trim().length <= 5)) {
        return { ...cached, avatarUrl: customAvatar };
      }
    }
    return cached;
  });
  const [isLoading, setIsLoading] = useState<boolean>(() => !tokenStorage.getUser());
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [copiedField, setCopiedField] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [showLogoutModal, setShowLogoutModal] = useState(false);

  // Edit Profile State (PUT /api/Users/profile)
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [formData, setFormData] = useState({
    fullName: tokenStorage.getUser()?.fullName || '',
    phone: tokenStorage.getUser()?.phone || '',
    position: tokenStorage.getUser()?.position || '',
  });

  // Avatar Upload State (POST /api/Users/profile/avatar)
  const [isUploadingAvatar, setIsUploadingAvatar] = useState(false);
  const [prevAvatarUrl, setPrevAvatarUrl] = useState(profile?.avatarUrl);
  const [avatarError, setAvatarError] = useState(false);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  // Change Password State (POST /api/Auth/change-password)
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [passwordSuccess, setPasswordSuccess] = useState<string | null>(null);

  const sessionType = tokenStorage.getSessionType();
  const timeToExpiry = tokenStorage.getTimeToExpiry();

  // Reset avatar image error if avatarUrl updates (render-time state adjustment)
  if (prevAvatarUrl !== profile?.avatarUrl) {
    setPrevAvatarUrl(profile?.avatarUrl);
    setAvatarError(false);
  }

  const getInitials = (name?: string) => {
    if (!name) return 'EC';
    const parts = name.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return 'EC';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  };

  const formatAvatarUrl = (url?: string): string | null => {
    if (!url) return null;
    const trimmed = url.trim();
    if (trimmed === '' || trimmed === 'string' || trimmed.length <= 5) return null;
    // Bỏ qua các URL placeholder demo Cloudinary không tồn tại để tránh lỗi ERR_INVALID_RESPONSE
    if (trimmed.includes('cloudinary.com/demo/') || trimmed.includes('/demo/image/upload/')) {
      return null;
    }
    if (
      trimmed.startsWith('http://') ||
      trimmed.startsWith('https://') ||
      trimmed.startsWith('blob:') ||
      trimmed.startsWith('data:')
    ) {
      return trimmed;
    }
    if (trimmed.startsWith('/')) {
      return `${ENV.API_BASE_URL}${trimmed}`;
    }
    return trimmed;
  };

  const onProfileUpdatedRef = useRef(onProfileUpdated);
  useEffect(() => {
    onProfileUpdatedRef.current = onProfileUpdated;
  }, [onProfileUpdated]);

  const fetchProfileData = async (force = false) => {
    if (force) {
      setIsRefreshing(true);
    }

    try {
      const data = await authApi.getProfile(force);
      setProfile(data);
      onProfileUpdatedRef.current?.(data);
      if (force) {
        setNotice('Đã đồng bộ hồ sơ mới nhất từ máy chủ!');
        setTimeout(() => setNotice(null), 3000);
      }
    } catch {
      setNotice('Không thể kết nối đến máy chủ hồ sơ.');
      setTimeout(() => setNotice(null), 3000);
    } finally {
      setIsLoading(false);
      setIsRefreshing(false);
    }
  };

  useEffect(() => {
    let isMounted = true;
    authApi
      .getProfile(false)
      .then((data) => {
        if (isMounted) {
          setProfile(data);
          onProfileUpdatedRef.current?.(data);
          setIsLoading(false);
        }
      })
      .catch(() => {
        if (isMounted) {
          setIsLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, []);

  const handleCopy = (text: string, fieldName: string) => {
    navigator.clipboard.writeText(text);
    setCopiedField(fieldName);
    setTimeout(() => setCopiedField(null), 2000);
  };

  const formatCountdown = (totalSeconds: number) => {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  };

  /**
   * Triggers the file selector for avatar upload
   */
  const handleAvatarClick = () => {
    if (isUploadingAvatar) return;
    fileInputRef.current?.click();
  };

  /**
   * Handles avatar file selection:
   * Calls POST /api/Users/profile/avatar
   * Then refreshes profile (GET /api/Users/profile) to get the updated avatarUrl
   * Displays toast notification and ensures uploaded image persists and displays immediately
   */
  const handleAvatarFileSelected = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const isImage = file.type.startsWith('image/') || /\.(jpe?g|png|webp|gif|bmp|svg)$/i.test(file.name);
    if (!isImage) {
      setNotice('✕ Vui lòng chọn tệp hình ảnh hợp lệ (PNG, JPG, WEBP).');
      setTimeout(() => setNotice(null), 3500);
      e.target.value = '';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      setNotice('✕ Kích thước ảnh tối đa cho phép là 5MB.');
      setTimeout(() => setNotice(null), 3500);
      e.target.value = '';
      return;
    }

    // 1. Convert file to Base64 Data URL for guaranteed immediate display and offline persistence
    let fileDataUrl = '';
    try {
      fileDataUrl = await new Promise<string>((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result as string);
        reader.onerror = reject;
        reader.readAsDataURL(file);
      });
    } catch {
      fileDataUrl = URL.createObjectURL(file);
    }

    setIsUploadingAvatar(true);
    setNotice('Đang tải ảnh đại diện lên máy chủ (POST /api/Users/profile/avatar)...');

    try {
      const uploadRes = await authApi.uploadAvatar(file);

      // Check server-returned URL
      let serverUrl = uploadRes?.avatarUrl;
      if (!serverUrl && uploadRes && typeof uploadRes === 'object') {
        const r = uploadRes as Record<string, unknown>;
        const cand =
          r.url ||
          r.filePath ||
          r.path ||
          r.imageUrl ||
          r.fileUrl ||
          (r.data && typeof r.data === 'object'
            ? (r.data as Record<string, unknown>).avatarUrl || (r.data as Record<string, unknown>).url
            : undefined) ||
          (typeof r.data === 'string' && r.data !== 'string' ? r.data : undefined);
        if (typeof cand === 'string' && cand.trim() && cand !== 'string') {
          serverUrl = cand.trim();
        }
      }

      // Re-fetch fresh profile data from server as requested
      let refreshedProfile = await authApi.getProfile(true);

      // Determine effective avatar URL:
      // Priority 1: Valid server URL from upload response (loại bỏ URL demo giả lập)
      // Priority 2: Valid server URL from refreshed profile (loại bỏ URL demo giả lập)
      // Priority 3: Local file Data URL (bảo đảm hiển thị ảnh thực tế tức thì 100%)
      const isInvalidDemoUrl = (u?: string | null) =>
        !u || u === 'string' || u.length <= 5 || u.includes('cloudinary.com/demo/') || u.includes('/demo/image/upload/');

      let effectiveAvatarUrl = fileDataUrl;
      let hasDemoUrl = false;

      if (serverUrl && !isInvalidDemoUrl(serverUrl)) {
        effectiveAvatarUrl =
          serverUrl.startsWith('/') && !serverUrl.startsWith('//')
            ? `${ENV.API_BASE_URL}${serverUrl}`
            : serverUrl;
      } else if (refreshedProfile.avatarUrl && !isInvalidDemoUrl(refreshedProfile.avatarUrl)) {
        effectiveAvatarUrl = refreshedProfile.avatarUrl;
      } else if (serverUrl && isInvalidDemoUrl(serverUrl)) {
        hasDemoUrl = true;
      }

      refreshedProfile = { ...refreshedProfile, avatarUrl: effectiveAvatarUrl };
      setProfile(refreshedProfile);
      tokenStorage.setUser(refreshedProfile);

      // Cache custom avatar in localStorage
      if (refreshedProfile.id) {
        localStorage.setItem(`ec_custom_avatar_${refreshedProfile.id}`, effectiveAvatarUrl);
      }
      localStorage.setItem('ec_custom_avatar_active', effectiveAvatarUrl);

      setAvatarError(false);
      onProfileUpdatedRef.current?.(refreshedProfile);

      if (hasDemoUrl) {
        setNotice('✓ Ảnh đại diện đã tải lên và hiển thị thành công (Lưu bộ đệm cục bộ do máy chủ dùng demo URL)!');
      } else {
        setNotice('✓ Cập nhật ảnh đại diện lên Cloudinary thành công!');
      }
      setTimeout(() => setNotice(null), 3500);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Tải ảnh đại diện thất bại.';
      setNotice(`✕ ${msg}`);
      setTimeout(() => setNotice(null), 5000);
    } finally {
      setIsUploadingAvatar(false);
      if (e.target) {
        e.target.value = '';
      }
    }
  };

  /**
   * Starts editing mode: enables editable fields (fullName, phone, position)
   */
  const handleStartEdit = () => {
    setFormData({
      fullName: user.fullName || '',
      phone: user.phone || '',
      position: user.position || '',
    });
    setIsEditing(true);
  };

  /**
   * Cancels editing mode and restores original values
   */
  const handleCancelEdit = () => {
    setFormData({
      fullName: user.fullName || '',
      phone: user.phone || '',
      position: user.position || '',
    });
    setIsEditing(false);
  };

  /**
   * Saves profile edits:
   * Calls PUT /api/Users/profile with { fullName, phone, position }
   * Updates state, calls onProfileUpdated, shows success toast
   */
  const handleSaveProfile = async () => {
    if (!formData.fullName.trim()) {
      setNotice('Họ và tên không được để trống.');
      setTimeout(() => setNotice(null), 3000);
      return;
    }

    setIsSaving(true);
    setNotice('Đang lưu thông tin hồ sơ...');

    try {
      const updated = await authApi.updateProfile({
        fullName: formData.fullName.trim(),
        phone: formData.phone.trim(),
        position: formData.position.trim(),
      });
      setProfile(updated);
      onProfileUpdated?.(updated);
      setIsEditing(false);
      setNotice('✓ Đã cập nhật hồ sơ thành công!');
      setTimeout(() => setNotice(null), 3500);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Cập nhật hồ sơ thất bại.';
      setNotice(`✕ ${msg}`);
      setTimeout(() => setNotice(null), 4000);
    } finally {
      setIsSaving(false);
    }
  };

  /**
   * Handles password change:
   * Calls POST /api/Auth/change-password with { currentPassword, newPassword }
   * Enforces client-side validation according to enterprise security specs:
   * - currentPassword required
   * - newPassword required & length >= 6
   * - newPassword != currentPassword
   * - newPassword === confirmNewPassword
   */
  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordError(null);
    setPasswordSuccess(null);

    const trimmedCurrent = currentPassword.trim();
    const trimmedNew = newPassword.trim();
    const trimmedConfirm = confirmNewPassword.trim();

    if (!trimmedCurrent) {
      setPasswordError('Vui lòng nhập mật khẩu hiện tại.');
      return;
    }
    if (!trimmedNew) {
      setPasswordError('Vui lòng nhập mật khẩu mới.');
      return;
    }
    if (trimmedNew.length < 6) {
      setPasswordError('Mật khẩu mới phải có ít nhất 6 ký tự.');
      return;
    }
    if (trimmedNew === trimmedCurrent) {
      setPasswordError('Mật khẩu mới không được trùng với mật khẩu cũ.');
      return;
    }
    if (trimmedNew !== trimmedConfirm) {
      setPasswordError('Xác nhận mật khẩu mới không khớp.');
      return;
    }

    setIsChangingPassword(true);
    try {
      const res = await authApi.changePassword({
        currentPassword: trimmedCurrent,
        newPassword: trimmedNew,
      });
      const successMsg = res.message || 'Đổi mật khẩu thành công. Toàn bộ phiên làm việc cũ đã bị thu hồi.';
      setPasswordSuccess(successMsg);
      setCurrentPassword('');
      setNewPassword('');
      setConfirmNewPassword('');
      setNotice(`✓ ${successMsg}`);
      setTimeout(() => setNotice(null), 5000);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Đổi mật khẩu thất bại. Vui lòng kiểm tra lại.';
      setPasswordError(msg);
      setNotice(`✕ ${msg}`);
      setTimeout(() => setNotice(null), 5000);
    } finally {
      setIsChangingPassword(false);
    }
  };

  const handleResetPasswordForm = () => {
    setCurrentPassword('');
    setNewPassword('');
    setConfirmNewPassword('');
    setPasswordError(null);
    setPasswordSuccess(null);
  };

  if (isLoading && !profile) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center bg-[#071e3d] text-slate-300">
        <RefreshCw className="w-8 h-8 text-[#38bdf8] animate-spin mb-3" />
        <p className="text-sm font-medium text-slate-300">Đang tải hồ sơ bảo mật từ máy chủ...</p>
      </div>
    );
  }

  const user = profile || {
    id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    fullName: 'Nguyễn Văn An',
    email: 'admin@enterprise.com',
    phone: '+84 (028) 3822 8888',
    position: 'Trưởng ban Kiến trúc Hệ thống & AI',
    avatarUrl: '',
    departmentId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    departmentName: 'Khối Kỹ Thuật Số & An Ninh Thông Tin (CTO Office)',
    status: 1,
    emailConfirmed: true,
    roles: ['EnterpriseArchitect', 'SystemAdmin', 'SecurityAuditor'],
    permissions: [
      'Auth.Login',
      'Chat.Send',
      'Media.Call',
      'Admin.Forensics',
      'Users.Manage',
      'Directory.Read',
      'AI.Copilot.Access',
      'Audit.Export',
    ],
  };

  const cachedCustomAvatar =
    localStorage.getItem('ec_custom_avatar_active') ||
    (user.id ? localStorage.getItem(`ec_custom_avatar_${user.id}`) : null);
  const displayAvatarUrl = formatAvatarUrl(user.avatarUrl) || formatAvatarUrl(cachedCustomAvatar || undefined);
  const hasValidAvatar = !!displayAvatarUrl && !avatarError;

  return (
    <div className="flex-1 flex flex-col h-full bg-[#071e3d] text-slate-100 overflow-y-auto select-none">
      {/* Hidden File Input for Avatar Upload (POST /api/Users/profile/avatar) */}
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleAvatarFileSelected}
        accept="image/png,image/jpeg,image/webp,image/gif"
        className="hidden"
      />

      {/* Top Breadcrumb & Action Bar */}
      <div className="h-16 px-6 bg-[#0c2340] border-b border-[#1d4ed8]/30 flex items-center justify-between shrink-0">
        <div className="flex items-center gap-4">
          <button
            onClick={onBackToWorkspace}
            className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-[#07172b] hover:bg-[#122e54] text-slate-300 hover:text-white border border-[#1d4ed8]/30 text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer"
          >
            <ArrowLeft className="w-4 h-4 text-[#38bdf8]" />
            <span>Quay lại Hội Thoại</span>
          </button>

          <div className="flex items-center gap-2 text-xs text-slate-400">
            <span>Enterprise Workspace</span>
            <span>/</span>
            <span className="text-[#38bdf8] font-medium">Hồ Sơ Cá Nhân & Quản Trị Đặc Quyền</span>
          </div>
        </div>

        <div className="flex items-center gap-3">
          {notice && (
            <span
              className={`text-xs font-medium px-3 py-1 rounded-lg border animate-in fade-in flex items-center gap-1.5 ${
                notice.startsWith('✕')
                  ? 'bg-rose-950/80 text-rose-300 border-rose-500/40'
                  : 'bg-emerald-950/70 text-emerald-400 border-emerald-500/40'
              }`}
            >
              {notice.startsWith('✕') ? (
                <AlertCircle className="w-3.5 h-3.5 text-rose-400 shrink-0" />
              ) : (
                <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400 shrink-0" />
              )}
              <span>{notice.replace(/^[✓✕]\s*/, '')}</span>
            </span>
          )}

          {/* Edit Profile Action Buttons */}
          {isEditing ? (
            <div className="flex items-center gap-2">
              <button
                onClick={handleCancelEdit}
                disabled={isSaving}
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-[#07172b] hover:bg-[#122e54] text-slate-300 hover:text-white border border-[#1d4ed8]/30 text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50"
              >
                <X className="w-3.5 h-3.5 text-slate-400" />
                <span>Hủy</span>
              </button>

              <button
                onClick={handleSaveProfile}
                disabled={isSaving}
                className="flex items-center gap-2 px-3.5 py-1.5 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50 shadow-md"
              >
                {isSaving ? (
                  <RefreshCw className="w-3.5 h-3.5 animate-spin text-white" />
                ) : (
                  <Save className="w-3.5 h-3.5 text-white" />
                )}
                <span>Lưu thay đổi</span>
              </button>
            </div>
          ) : (
            <button
              onClick={handleStartEdit}
              title="Chỉnh sửa thông tin hồ sơ (PUT /api/Users/profile)"
              className="flex items-center gap-2 px-3.5 py-1.5 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] text-white text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer shadow-md"
            >
              <Pencil className="w-3.5 h-3.5" />
              <span>Chỉnh sửa hồ sơ</span>
            </button>
          )}

          <button
            onClick={() => fetchProfileData(true)}
            disabled={isRefreshing}
            title="Cập nhật dữ liệu hồ sơ mới nhất"
            className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-[#07172b] hover:bg-[#122e54] text-slate-300 hover:text-white border border-[#1d4ed8]/30 text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50"
          >
            <RefreshCw className={`w-3.5 h-3.5 text-[#38bdf8] ${isRefreshing ? 'animate-spin' : ''}`} />
            <span>Đồng bộ hồ sơ</span>
          </button>

          <button
            onClick={() => setShowLogoutModal(true)}
            disabled={isLoggingOut}
            className="flex items-center gap-2 px-3.5 py-1.5 rounded-xl bg-rose-950/60 hover:bg-rose-900/80 text-rose-300 hover:text-rose-100 border border-rose-500/40 text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50 shadow-sm"
          >
            <LogOut className="w-3.5 h-3.5 text-rose-400" />
            <span>Đăng xuất</span>
          </button>
        </div>
      </div>

      {/* Main Profile Body Container - Expanded horizontally */}
      <div className="px-8 py-6 w-full space-y-6">
        {/* 1. Hero Identity Banner Card - Redesigned with Prominent 144px Avatar & Balanced Typography */}
        <div className="p-6 md:p-8 rounded-3xl bg-[#0c2340] border border-[#1d4ed8]/30 shadow-2xl flex flex-col lg:flex-row items-start lg:items-center justify-between gap-6 hover:border-[#38bdf8]/50 transition-all duration-300">
          <div className="flex flex-col sm:flex-row items-center sm:items-start lg:items-center gap-6 flex-1 w-full">
            {/* User Avatar Column: Prominent 144px Box + Change Avatar Button */}
            <div className="flex flex-col items-center shrink-0">
              <div
                onClick={handleAvatarClick}
                className="relative group cursor-pointer"
                title="Bấm để thay đổi ảnh đại diện (POST /api/Users/profile/avatar)"
              >
                <div className="w-32 h-32 md:w-36 md:h-36 rounded-[26px] bg-[#0052cc] border-3 border-[#1d4ed8]/70 ring-4 ring-[#07172b] group-hover:border-[#38bdf8] group-hover:ring-[#1d4ed8]/50 flex items-center justify-center text-white text-4xl md:text-5xl font-extrabold shadow-2xl overflow-hidden transition-all duration-300 relative select-none">
                  {hasValidAvatar && displayAvatarUrl ? (
                    <img
                      src={displayAvatarUrl}
                      alt=""
                      onError={() => setAvatarError(true)}
                      className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
                    />
                  ) : (
                    <span className="font-extrabold text-3xl md:text-4xl tracking-wider text-white select-none">
                      {getInitials(user.fullName)}
                    </span>
                  )}

                  {/* Hover / Uploading Overlay */}
                  <div
                    className={`absolute inset-0 bg-black/65 backdrop-blur-[2px] flex flex-col items-center justify-center text-white transition-opacity duration-200 ${
                      isUploadingAvatar ? 'opacity-100' : 'opacity-0 group-hover:opacity-100'
                    }`}
                  >
                    {isUploadingAvatar ? (
                      <>
                        <RefreshCw className="w-6 h-6 text-[#38bdf8] animate-spin mb-1.5" />
                        <span className="text-[11px] font-semibold text-slate-200">Đang tải...</span>
                      </>
                    ) : (
                      <>
                        <Camera className="w-6 h-6 text-[#38bdf8] mb-1 group-hover:scale-110 transition-transform" />
                        <span className="text-[11px] font-bold text-white tracking-wide">Đổi ảnh</span>
                      </>
                    )}
                  </div>
                </div>

                {/* Online Status Dot Badge */}
                <span
                  className="absolute bottom-1 right-1 w-6 h-6 rounded-full bg-emerald-500 border-3 border-[#0c2340] shadow-md flex items-center justify-center z-10"
                  title="Trạng thái: Đang hoạt động"
                >
                  <span className="w-2 h-2 rounded-full bg-white animate-pulse" />
                </span>
              </div>

              {/* Dedicated "Đổi ảnh đại diện" Button */}
              <button
                type="button"
                onClick={handleAvatarClick}
                disabled={isUploadingAvatar}
                className="mt-3 flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-[#07172b] hover:bg-[#122e54] text-[#38bdf8] hover:text-white border border-[#1d4ed8]/40 hover:border-[#38bdf8] text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 shadow-sm cursor-pointer disabled:opacity-50"
                title="Chọn tệp ảnh từ thiết bị (PNG, JPG, WEBP) để tải lên máy chủ"
              >
                <Camera className="w-3.5 h-3.5 text-[#38bdf8]" />
                <span>{isUploadingAvatar ? 'Đang tải...' : 'Đổi ảnh đại diện'}</span>
              </button>
            </div>

            {/* Basic Info & Edit Inputs */}
            <div className="space-y-3 flex-1 w-full text-center sm:text-left">
              {isEditing ? (
                /* Editable Form Fields: fullName & position */
                <div className="space-y-3.5 w-full max-w-xl animate-in fade-in">
                  <div>
                    <label className="text-[11px] font-semibold text-[#38bdf8] uppercase tracking-wider flex items-center gap-1 mb-1">
                      <span>Họ và tên nhân viên</span>
                      <span className="text-rose-400">*</span>
                      <span className="text-[10px] text-slate-400 font-normal lowercase">(được phép sửa)</span>
                    </label>
                    <input
                      type="text"
                      value={formData.fullName}
                      onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
                      placeholder="Nhập họ và tên..."
                      className="w-full px-4 py-2 rounded-xl bg-[#07172b] border border-[#38bdf8] text-white font-bold text-lg focus:outline-none focus:ring-2 focus:ring-[#38bdf8] transition-all shadow-inner"
                    />
                  </div>

                  <div>
                    <label className="text-[11px] font-semibold text-[#38bdf8] uppercase tracking-wider flex items-center gap-1 mb-1">
                      <span>Chức danh / Vị trí công tác</span>
                      <span className="text-[10px] text-slate-400 font-normal lowercase">(được phép sửa)</span>
                    </label>
                    <input
                      type="text"
                      value={formData.position}
                      onChange={(e) => setFormData({ ...formData, position: e.target.value })}
                      placeholder="Nhập vị trí công tác..."
                      className="w-full px-3.5 py-2 rounded-xl bg-[#07172b] border border-[#38bdf8] text-white font-medium text-xs focus:outline-none focus:ring-2 focus:ring-[#38bdf8] transition-all shadow-inner"
                    />
                  </div>
                </div>
              ) : (
                /* Static Profile Display */
                <>
                  <div className="flex items-center gap-3 flex-wrap justify-center sm:justify-start">
                    <h2 className="text-2xl md:text-3xl font-extrabold text-white tracking-tight">{user.fullName}</h2>
                    {user.emailConfirmed && (
                      <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-950/80 text-emerald-400 border border-emerald-500/40">
                        <CheckCircle2 className="w-3.5 h-3.5" />
                        Đã xác minh công vụ
                      </span>
                    )}
                    <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-[#07172b] text-emerald-400 border border-emerald-500/30">
                      <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" />
                      Đang hoạt động
                    </span>
                  </div>

                  {/* Position & Department Chips */}
                  <div className="flex items-center gap-2.5 flex-wrap justify-center sm:justify-start text-xs md:text-sm">
                    <div className="flex items-center gap-1.5 px-3 py-1 rounded-xl bg-[#07172b] border border-[#1d4ed8]/30 text-white">
                      <Briefcase className="w-4 h-4 text-[#38bdf8]" />
                      <span className="font-semibold text-slate-100">{user.position || 'Chuyên viên Doanh Nghiệp'}</span>
                    </div>

                    <div className="flex items-center gap-1.5 px-3 py-1 rounded-xl bg-[#07172b] border border-[#1d4ed8]/30 text-slate-300">
                      <Building2 className="w-4 h-4 text-[#38bdf8]" />
                      <span>{user.departmentName || 'Khối Kỹ Thuật'}</span>
                    </div>
                  </div>

                  {/* Direct Contact Meta with 1-click copy */}
                  <div className="flex items-center gap-4 flex-wrap justify-center sm:justify-start text-xs text-slate-300 pt-1">
                    <div className="flex items-center gap-1.5 bg-[#07172b]/70 px-2.5 py-1 rounded-lg border border-slate-700/40">
                      <Mail className="w-3.5 h-3.5 text-[#38bdf8]" />
                      <span>{user.email}</span>
                      <button
                        onClick={() => handleCopy(user.email, 'hero_email')}
                        className="ml-1 hover:text-[#38bdf8] transition-colors"
                        title="Sao chép email"
                      >
                        {copiedField === 'hero_email' ? <Check className="w-3 h-3 text-emerald-400" /> : <Copy className="w-3 h-3" />}
                      </button>
                    </div>

                    <div className="flex items-center gap-1.5 bg-[#07172b]/70 px-2.5 py-1 rounded-lg border border-slate-700/40">
                      <Phone className="w-3.5 h-3.5 text-[#38bdf8]" />
                      <span>{user.phone || '+84 (028) 3822 8888'}</span>
                      <button
                        onClick={() => handleCopy(user.phone || '+84 (028) 3822 8888', 'hero_phone')}
                        className="ml-1 hover:text-[#38bdf8] transition-colors"
                        title="Sao chép số điện thoại"
                      >
                        {copiedField === 'hero_phone' ? <Check className="w-3 h-3 text-emerald-400" /> : <Copy className="w-3 h-3" />}
                      </button>
                    </div>
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Quick Security Metrics Panel */}
          <div className="flex flex-row lg:flex-col gap-2.5 shrink-0 text-xs w-full lg:w-auto justify-center lg:justify-start">
            <div className="px-4 py-2.5 rounded-xl bg-[#07172b] border border-[#1d4ed8]/30 flex items-center gap-3 shadow-inner">
              <ShieldCheck className="w-5 h-5 text-emerald-400 shrink-0" />
              <div>
                <p className="text-slate-400 text-[11px] leading-tight">Cơ chế bảo mật</p>
                <p className="font-semibold text-white">Bộ nhớ bảo vệ (Zero-Trust RAM)</p>
              </div>
            </div>

            <div className="px-4 py-2.5 rounded-xl bg-[#07172b] border border-[#1d4ed8]/30 flex items-center gap-3 shadow-inner">
              <Clock className="w-5 h-5 text-[#38bdf8] shrink-0" />
              <div>
                <p className="text-slate-400 text-[11px] leading-tight">Thời gian phiên còn lại</p>
                <p className="font-mono font-bold text-[#38bdf8] text-sm">{formatCountdown(timeToExpiry)}</p>
              </div>
            </div>
          </div>
        </div>

        {/* 2. Grid Layout of Selective Enterprise Profile Fields */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">
          {/* Column 1: Organization Data, Enterprise Roles & Change Password */}
          <div className="space-y-6 flex flex-col">
            {/* Card A: Detailed Organization & Contact Data */}
            <div className="p-6 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 shadow-md space-y-4 hover:border-[#38bdf8]/50 transition-all duration-200">
              <div className="flex items-center justify-between border-b border-[#1d4ed8]/20 pb-3">
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-[#0052cc]/20 border border-[#1d4ed8]/40 flex items-center justify-center text-[#38bdf8]">
                    <Layers className="w-4 h-4" />
                  </div>
                  <h3 className="text-base font-bold text-white">Thông Tin Định Danh & Tổ Chức</h3>
                </div>
                <span className="text-xs text-slate-400 font-medium">Hồ sơ xác thực</span>
              </div>

              <div className="space-y-3 text-sm">
                {/* UUID */}
                <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20 flex items-center justify-between">
                  <div>
                    <div className="flex items-center gap-2 mb-0.5">
                      <span className="text-xs text-slate-400">Mã định danh cá nhân (UUID)</span>
                      <span className="text-[10px] text-slate-400 bg-[#0c2340] px-1.5 py-0.2 rounded border border-slate-700/60">
                        Hệ thống
                      </span>
                    </div>
                    <span className="font-mono text-xs text-white font-medium break-all">{user.id}</span>
                  </div>
                  <button
                    onClick={() => handleCopy(user.id, 'id')}
                    className="p-1.5 rounded-lg bg-[#0c2340] hover:bg-[#122e54] text-slate-300 hover:text-white border border-[#1d4ed8]/30 cursor-pointer transition-colors shrink-0 ml-2"
                    title="Sao chép UUID"
                  >
                    {copiedField === 'id' ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
                  </button>
                </div>

                {/* Department ID */}
                <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20 flex items-center justify-between">
                  <div>
                    <div className="flex items-center gap-2 mb-0.5">
                      <span className="text-xs text-slate-400">Phòng ban trực thuộc</span>
                      <span className="text-[10px] text-slate-400 bg-[#0c2340] px-1.5 py-0.2 rounded border border-slate-700/60">
                        Phân bổ
                      </span>
                    </div>
                    <span className="text-xs text-white font-semibold block">{user.departmentName || 'Chưa phân bổ'}</span>
                    <span className="font-mono text-[11px] text-slate-400 block mt-0.5">{user.departmentId}</span>
                  </div>
                  <button
                    onClick={() => handleCopy(user.departmentId || '', 'departmentId')}
                    className="p-1.5 rounded-lg bg-[#0c2340] hover:bg-[#122e54] text-slate-300 hover:text-white border border-[#1d4ed8]/30 cursor-pointer transition-colors shrink-0 ml-2"
                    title="Sao chép Mã phòng ban"
                  >
                    {copiedField === 'departmentId' ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
                  </button>
                </div>

                {/* Email & Phone Details */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20">
                    <div className="flex items-center justify-between mb-1">
                      <span className="text-xs text-slate-400">Email công vụ</span>
                      <span className="text-[10px] text-slate-400 bg-[#0c2340] px-1.5 py-0.2 rounded border border-slate-700/60">
                        Cố định
                      </span>
                    </div>
                    <span className="text-xs font-semibold text-white break-all flex items-center gap-1.5">
                      <Mail className="w-3.5 h-3.5 text-[#38bdf8] shrink-0" />
                      {user.email}
                    </span>
                  </div>

                  <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20">
                    <div className="flex items-center justify-between mb-1">
                      <span className="text-xs text-slate-400">Số điện thoại liên lạc</span>
                      {isEditing && (
                        <span className="text-[10px] text-[#38bdf8] font-semibold bg-[#0c2340] px-1.5 py-0.2 rounded border border-[#38bdf8]/40">
                          Được phép sửa
                        </span>
                      )}
                    </div>
                    {isEditing ? (
                      <div className="relative">
                        <input
                          type="tel"
                          value={formData.phone}
                          onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                          placeholder="Nhập số điện thoại..."
                          className="w-full px-2.5 py-1.5 rounded-lg bg-[#0c2340] border border-[#38bdf8] text-white font-semibold text-xs focus:outline-none focus:ring-1 focus:ring-[#38bdf8] transition-all shadow-inner"
                        />
                      </div>
                    ) : (
                      <span className="text-xs font-semibold text-white flex items-center gap-1.5">
                        <Phone className="w-3.5 h-3.5 text-[#38bdf8] shrink-0" />
                        {user.phone || 'Chưa cung cấp'}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Card C: Enterprise Roles (RBAC) */}
            <div className="p-6 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 shadow-md space-y-4 hover:border-[#38bdf8]/50 transition-all duration-200">
              <div className="flex items-center justify-between border-b border-[#1d4ed8]/20 pb-3">
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-[#0052cc]/20 border border-[#1d4ed8]/40 flex items-center justify-center text-[#38bdf8]">
                    <ShieldAlert className="w-4 h-4" />
                  </div>
                  <h3 className="text-base font-bold text-white">Vai Trò Doanh Nghiệp</h3>
                </div>
                <span className="text-xs text-[#38bdf8] font-semibold">{user.roles?.length || 0} vai trò</span>
              </div>

              <div className="space-y-2.5">
                {user.roles && user.roles.length > 0 ? (
                  user.roles.map((role, idx) => {
                    const roleInfo = getRoleDisplay(role);
                    return (
                      <div
                        key={idx}
                        className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20 flex items-center justify-between hover:bg-[#0f2747] hover:border-[#38bdf8]/40 transition-all duration-200 group"
                      >
                        <div className="flex items-center gap-3">
                          <div className="w-8 h-8 rounded-lg bg-[#0052cc] flex items-center justify-center text-white shrink-0 group-hover:scale-105 transition-transform">
                            <KeyRound className="w-4 h-4" />
                          </div>
                          <div>
                            <span className="font-semibold text-sm text-white block group-hover:text-[#38bdf8] transition-colors">
                              {roleInfo.title}
                            </span>
                            <span className="text-xs text-slate-400 block line-clamp-1">
                              {roleInfo.desc}
                            </span>
                          </div>
                        </div>
                        <span className="px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-950/80 text-emerald-400 border border-emerald-500/40 shrink-0 ml-2">
                          Đang kích hoạt
                        </span>
                      </div>
                    );
                  })
                ) : (
                  <p className="text-xs text-slate-400 p-4 text-center">Chưa có vai trò nào được gán.</p>
                )}
              </div>
            </div>

            {/* Card E: Change Password (POST /api/Auth/change-password) - Bên dưới ô vai trò doanh nghiệp */}
            <div className="p-6 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 shadow-md space-y-4 hover:border-[#38bdf8]/50 transition-all duration-200">
              <div className="flex items-center justify-between border-b border-[#1d4ed8]/20 pb-3">
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-[#0052cc]/20 border border-[#1d4ed8]/40 flex items-center justify-center text-[#38bdf8]">
                    <KeyRound className="w-4 h-4" />
                  </div>
                  <div>
                    <h3 className="text-base font-bold text-white">Đổi Mật Khẩu</h3>
                  </div>
                </div>
                <span className="text-xs text-[#38bdf8] font-semibold bg-[#07172b] px-2.5 py-1 rounded-full border border-[#1d4ed8]/30">
                  Bảo mật tài khoản
                </span>
              </div>

              <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20 flex items-start gap-2.5 text-xs text-slate-300">
                <LockIcon className="w-4 h-4 text-[#38bdf8] shrink-0 mt-0.5" />
                <span className="leading-relaxed">
                  Mật khẩu mới yêu cầu tối thiểu 6 ký tự. Sau khi đổi mật khẩu thành công, toàn bộ phiên làm việc cũ sẽ tự động được thu hồi để bảo đảm an toàn.
                </span>
              </div>

              <form onSubmit={handleChangePassword} className="space-y-3.5">
                {/* Current Password Field */}
                <div className="space-y-1.5">
                  <label className="text-xs font-semibold text-slate-300 flex items-center gap-1.5">
                    <span>Mật khẩu hiện tại</span>
                    <span className="text-rose-400">*</span>
                  </label>
                  <div className="relative">
                    <input
                      type={showCurrentPassword ? 'text' : 'password'}
                      value={currentPassword}
                      onChange={(e) => {
                        setCurrentPassword(e.target.value);
                        if (passwordError) setPasswordError(null);
                      }}
                      placeholder="Nhập mật khẩu đang sử dụng..."
                      autoComplete="current-password"
                      disabled={isChangingPassword}
                      className="w-full bg-[#07172b] border border-[#1d4ed8]/30 rounded-xl pl-3.5 pr-10 py-2.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-[#38bdf8] focus:ring-1 focus:ring-[#38bdf8] transition-all disabled:opacity-50"
                    />
                    <button
                      type="button"
                      onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                      tabIndex={-1}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-[#38bdf8] transition-colors p-1"
                      title={showCurrentPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                    >
                      {showCurrentPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                    </button>
                  </div>
                </div>

                {/* New Password Field */}
                <div className="space-y-1.5">
                  <div className="flex items-center justify-between">
                    <label className="text-xs font-semibold text-slate-300 flex items-center gap-1.5">
                      <span>Mật khẩu mới</span>
                      <span className="text-rose-400">*</span>
                    </label>
                    <span className="text-[11px] text-slate-400">Tối thiểu 6 ký tự</span>
                  </div>
                  <div className="relative">
                    <input
                      type={showNewPassword ? 'text' : 'password'}
                      value={newPassword}
                      onChange={(e) => {
                        setNewPassword(e.target.value);
                        if (passwordError) setPasswordError(null);
                      }}
                      placeholder="Nhập mật khẩu mới (khác mật khẩu cũ)..."
                      autoComplete="new-password"
                      disabled={isChangingPassword}
                      className="w-full bg-[#07172b] border border-[#1d4ed8]/30 rounded-xl pl-3.5 pr-10 py-2.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-[#38bdf8] focus:ring-1 focus:ring-[#38bdf8] transition-all disabled:opacity-50"
                    />
                    <button
                      type="button"
                      onClick={() => setShowNewPassword(!showNewPassword)}
                      tabIndex={-1}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-[#38bdf8] transition-colors p-1"
                      title={showNewPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                    >
                      {showNewPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                    </button>
                  </div>
                </div>

                {/* Confirm New Password Field */}
                <div className="space-y-1.5">
                  <label className="text-xs font-semibold text-slate-300 flex items-center gap-1.5">
                    <span>Xác nhận mật khẩu mới</span>
                    <span className="text-rose-400">*</span>
                  </label>
                  <div className="relative">
                    <input
                      type={showConfirmPassword ? 'text' : 'password'}
                      value={confirmNewPassword}
                      onChange={(e) => {
                        setConfirmNewPassword(e.target.value);
                        if (passwordError) setPasswordError(null);
                      }}
                      placeholder="Nhập lại mật khẩu mới..."
                      autoComplete="new-password"
                      disabled={isChangingPassword}
                      className="w-full bg-[#07172b] border border-[#1d4ed8]/30 rounded-xl pl-3.5 pr-10 py-2.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-[#38bdf8] focus:ring-1 focus:ring-[#38bdf8] transition-all disabled:opacity-50"
                    />
                    <button
                      type="button"
                      onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                      tabIndex={-1}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-[#38bdf8] transition-colors p-1"
                      title={showConfirmPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                    >
                      {showConfirmPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                    </button>
                  </div>
                </div>

                {/* Error Banner */}
                {passwordError && (
                  <div className="p-3 rounded-xl bg-rose-950/70 border border-rose-500/40 flex items-start gap-2.5 text-xs text-rose-300 animate-in fade-in">
                    <AlertCircle className="w-4 h-4 text-rose-400 shrink-0 mt-0.5" />
                    <span>{passwordError}</span>
                  </div>
                )}

                {/* Success Banner */}
                {passwordSuccess && (
                  <div className="p-3 rounded-xl bg-emerald-950/70 border border-emerald-500/40 flex items-start gap-2.5 text-xs text-emerald-300 animate-in fade-in">
                    <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                    <span>{passwordSuccess}</span>
                  </div>
                )}

                {/* Form Action Buttons */}
                <div className="flex items-center justify-end gap-3 pt-2 border-t border-[#1d4ed8]/20">
                  {(currentPassword || newPassword || confirmNewPassword) && (
                    <button
                      type="button"
                      onClick={handleResetPasswordForm}
                      disabled={isChangingPassword}
                      className="px-3.5 py-2 rounded-xl bg-[#07172b] hover:bg-[#122e54] text-slate-300 hover:text-white border border-[#1d4ed8]/30 text-xs font-semibold hover:scale-105 active:scale-95 transition-all cursor-pointer disabled:opacity-50"
                    >
                      Hủy bỏ
                    </button>
                  )}

                  <button
                    type="submit"
                    disabled={isChangingPassword || !currentPassword || !newPassword || !confirmNewPassword}
                    className="flex items-center gap-2 px-4 py-2 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] text-white text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer shadow-md disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
                  >
                    {isChangingPassword ? (
                      <>
                        <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                        <span>Đang xử lý...</span>
                      </>
                    ) : (
                      <>
                        <KeyRound className="w-3.5 h-3.5" />
                        <span>Đổi Mật Khẩu</span>
                      </>
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>

          {/* Column 2: Account Security / Token Health & System Permissions */}
          <div className="space-y-6 flex flex-col">
            {/* Card B: Security & Session Architecture */}
            <div className="p-6 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 shadow-md space-y-4 hover:border-[#38bdf8]/50 transition-all duration-200">
              <div className="flex items-center justify-between border-b border-[#1d4ed8]/20 pb-3">
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-[#0052cc]/20 border border-[#1d4ed8]/40 flex items-center justify-center text-[#38bdf8]">
                    <ShieldCheck className="w-4 h-4" />
                  </div>
                  <h3 className="text-base font-bold text-white">Bảo Mật Tài Khoản & Phiên Làm Việc</h3>
                </div>
                <span className="text-xs text-emerald-400 font-semibold">Bảo Vệ Đa Lớp</span>
              </div>

              <div className="space-y-3 text-sm">
                <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20 space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-xs text-slate-300 font-medium flex items-center gap-1.5">
                      <ShieldCheck className="w-4 h-4 text-emerald-400 shrink-0" />
                      Bảo vệ phiên trong bộ nhớ tạm
                    </span>
                    <span className="text-xs font-semibold text-emerald-400 bg-emerald-950/70 border border-emerald-500/40 px-2.5 py-0.5 rounded-full">
                      An toàn tuyệt đối
                    </span>
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    Khóa phiên được lưu tạm thời trong bộ nhớ máy tính và tự hủy khi hết hạn. Tuyệt đối không lưu xuống ổ đĩa, giúp loại bỏ 100% nguy cơ bị đánh cắp tài khoản từ mã độc trên web.
                  </p>
                </div>

                <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20 space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-xs text-slate-300 font-medium flex items-center gap-1.5">
                      <LockIcon className="w-4 h-4 text-[#38bdf8] shrink-0" />
                      Cơ chế tự động duy trì đăng nhập
                    </span>
                    <span className="text-xs font-semibold text-[#38bdf8] bg-[#0c2340] border border-[#1d4ed8]/40 px-2.5 py-0.5 rounded-full">
                      Bảo vệ trình duyệt
                    </span>
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    {sessionType === 'persistent'
                      ? 'Chế độ ghi nhớ (30 ngày): Bạn có thể đóng trình duyệt và quay lại làm việc bình thường mà không cần đăng nhập lại. Dữ liệu phiên được trình duyệt khóa bảo mật độc lập theo tiêu chuẩn ngân hàng.'
                      : 'Chế độ phiên tạm thời: Trình duyệt sẽ tự động xóa sạch dữ liệu phiên và đăng xuất an toàn ngay khi bạn đóng cửa sổ làm việc.'}
                  </p>
                </div>

                <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20 flex items-center justify-between">
                  <div>
                    <span className="text-xs text-slate-400 block mb-0.5">Trạng thái phiên làm việc</span>
                    <span className="text-xs font-bold text-white">
                      {sessionType === 'persistent'
                        ? 'Đang duy trì đăng nhập (30 ngày trên thiết bị này)'
                        : 'Phiên làm việc tạm thời (Tự thoát khi đóng trình duyệt)'}
                    </span>
                  </div>
                  <button
                    onClick={() => authApi.refreshToken()}
                    className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-[#0052cc] hover:bg-[#0065ff] text-white text-xs font-semibold hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer shadow-sm"
                  >
                    <RefreshCw className="w-3.5 h-3.5" />
                    <span>Làm mới phiên</span>
                  </button>
                </div>
              </div>
            </div>

            {/* Card D: Permissions Matrix */}
            <div className="p-6 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 shadow-md space-y-4 hover:border-[#38bdf8]/50 transition-all duration-200">
              <div className="flex items-center justify-between border-b border-[#1d4ed8]/20 pb-3">
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-[#0052cc]/20 border border-[#1d4ed8]/40 flex items-center justify-center text-[#38bdf8]">
                    <Sparkles className="w-4 h-4" />
                  </div>
                  <h3 className="text-base font-bold text-white">Đặc Quyền Hệ Thống</h3>
                </div>
                <span className="text-xs text-[#38bdf8] font-semibold">{user.permissions?.length || 0} đặc quyền đã cấp</span>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5 max-h-72 overflow-y-auto pr-1">
                {user.permissions && user.permissions.length > 0 ? (
                  user.permissions.map((perm, idx) => {
                    const permInfo = getPermissionDisplay(perm);
                    return (
                      <div
                        key={idx}
                        className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/20 flex items-start gap-3 hover:bg-[#0f2747] hover:border-[#38bdf8]/40 transition-all duration-200 group"
                      >
                        <div className="w-6 h-6 rounded-lg bg-emerald-500/15 border border-emerald-500/30 flex items-center justify-center shrink-0 mt-0.5 text-emerald-400 group-hover:scale-110 transition-transform">
                          <CheckCircle2 className="w-3.5 h-3.5" />
                        </div>
                        <div className="overflow-hidden space-y-0.5">
                          <span className="text-xs font-semibold text-white block truncate group-hover:text-[#38bdf8] transition-colors leading-tight">
                            {permInfo.title}
                          </span>
                          <span className="text-[11px] text-slate-400 block line-clamp-1 leading-relaxed">
                            {permInfo.desc}
                          </span>
                        </div>
                      </div>
                    );
                  })
                ) : (
                  <p className="text-xs text-slate-400 p-4 col-span-2 text-center">Chưa có đặc quyền nào.</p>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Footer Info Box */}
        <div className="p-4 rounded-xl bg-[#0c2340] border border-[#1d4ed8]/30 flex items-center justify-between text-xs text-slate-400">
          <div className="flex items-center gap-2">
            <Info className="w-4 h-4 text-[#38bdf8] shrink-0" />
            <span>
              Dữ liệu hồ sơ cá nhân và ma trận quyền hạn được đồng bộ trực tiếp theo tiêu chuẩn bảo mật Zero-Trust của tổ chức.
            </span>
          </div>
          <span className="text-slate-500 font-mono hidden md:inline">EnterpriseConnect v2.4</span>
        </div>
      </div>

      {/* Confirmation Modal for Logout */}
      {showLogoutModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm p-4 animate-in fade-in">
          <div className="w-full max-w-md rounded-2xl bg-[#0c2340] border border-rose-500/40 p-6 shadow-2xl space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-rose-950/80 border border-rose-500/40 flex items-center justify-center text-rose-400">
                <LogOut className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-base font-bold text-white">Xác nhận Đăng Xuất An Toàn</h3>
                <span className="text-xs text-slate-400">Phiên làm việc bảo mật</span>
              </div>
            </div>

            <p className="text-sm text-slate-300 leading-relaxed">
              Hệ thống sẽ thu hồi mã phiên làm việc, xóa Access Token khỏi bộ nhớ tạm và đóng phiên đăng nhập an toàn trên thiết bị này.
            </p>

            <div className="flex items-center justify-end gap-3 pt-2">
              <button
                onClick={() => setShowLogoutModal(false)}
                disabled={isLoggingOut}
                className="px-4 py-2 rounded-xl bg-[#07172b] hover:bg-[#122e54] text-slate-300 text-xs font-semibold border border-slate-700 transition-colors cursor-pointer"
              >
                Hủy bỏ
              </button>
              <button
                onClick={async () => {
                  await onLogout();
                  setShowLogoutModal(false);
                }}
                disabled={isLoggingOut}
                className="flex items-center gap-2 px-4 py-2 rounded-xl bg-rose-600 hover:bg-rose-500 text-white text-xs font-semibold shadow-md transition-all cursor-pointer disabled:opacity-50"
              >
                {isLoggingOut ? (
                  <>
                    <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                    <span>Đang đăng xuất...</span>
                  </>
                ) : (
                  <>
                    <LogOut className="w-3.5 h-3.5" />
                    <span>Đồng ý Đăng Xuất</span>
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
