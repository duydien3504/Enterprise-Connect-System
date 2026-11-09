import React from 'react';
import {
  Search,
  UserPlus,
  MessageSquare,
  Check,
  X,
  Clock,
  ShieldBan,
  Building2,
  Briefcase,
  Mail,
  Phone,
  RefreshCw,
  ChevronLeft,
  ChevronRight,
  Sparkles,
} from 'lucide-react';
import { Avatar } from '@/shared/components/Avatar';
import type { UserListItem, RelationStatus } from '../types/contact.types';

interface UserDirectoryViewProps {
  users: UserListItem[];
  totalCount: number;
  currentPage: number;
  totalPages: number;
  searchQuery: string;
  departmentFilter: string;
  isLoading: boolean;
  actionLoadingId: string | null;
  onSearchChange: (query: string) => void;
  onDepartmentChange: (dept: string) => void;
  onPageChange: (page: number) => void;
  getRelationStatus: (userId: string) => { status: RelationStatus; contactId?: string };
  onSendRequest: (receiverId: string) => void;
  onRespondRequest: (contactId: string, accept: boolean) => void;
  onCancelSentRequest: (targetUserId: string, fullName: string) => void;
  onUnblockUser: (targetUserId: string, fullName: string) => void;
  onStartChat: (userId: string) => void;
  onRefresh: () => void;
}

export const UserDirectoryView: React.FC<UserDirectoryViewProps> = ({
  users,
  totalCount,
  currentPage,
  totalPages,
  searchQuery,
  departmentFilter,
  isLoading,
  actionLoadingId,
  onSearchChange,
  onDepartmentChange,
  onPageChange,
  getRelationStatus,
  onSendRequest,
  onRespondRequest,
  onCancelSentRequest,
  onUnblockUser,
  onStartChat,
  onRefresh,
}) => {
  return (
    <div className="flex flex-col h-full overflow-hidden bg-[#071e3d]/40">
      {/* Top Filter Bar */}
      <div className="p-5 border-b border-slate-800/80 bg-[#07172b]/60 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 shrink-0">
        <div className="flex flex-col sm:flex-row items-center gap-3 flex-1 max-w-xl">
          {/* Name Search */}
          <div className="relative flex-1 w-full">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Tìm theo họ tên nhân viên..."
              value={searchQuery}
              onChange={(e) => onSearchChange(e.target.value)}
              className="w-full pl-9 pr-4 py-2 bg-[#0c2340] border border-slate-700/70 rounded-xl text-xs text-white placeholder-slate-400 focus:outline-none focus:border-[#38bdf8] focus:ring-1 focus:ring-[#38bdf8] transition-all"
            />
          </div>

          {/* Department Filter Input */}
          <div className="relative w-full sm:w-48">
            <Building2 className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Lọc phòng ban..."
              value={departmentFilter}
              onChange={(e) => onDepartmentChange(e.target.value)}
              className="w-full pl-9 pr-4 py-2 bg-[#0c2340] border border-slate-700/70 rounded-xl text-xs text-white placeholder-slate-400 focus:outline-none focus:border-[#38bdf8] transition-all"
            />
          </div>
        </div>

        <div className="flex items-center gap-3 self-end sm:self-auto">
          <span className="text-xs text-slate-400 font-medium">
            Tìm thấy: <strong className="text-white">{totalCount}</strong> nhân sự
          </span>
          <button
            onClick={onRefresh}
            disabled={isLoading}
            title="Làm mới kết quả tìm kiếm"
            className="p-2 rounded-xl bg-[#0c2340] border border-slate-700/70 text-slate-300 hover:text-white hover:border-[#38bdf8] transition-all cursor-pointer disabled:opacity-50"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      {/* Directory Grid */}
      <div className="flex-1 overflow-y-auto p-5">
        {users.length === 0 ? (
          <div className="h-full min-h-[300px] flex flex-col items-center justify-center text-center p-8">
            <div className="w-16 h-16 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 flex items-center justify-center text-[#38bdf8] mb-4 shadow-lg">
              <Sparkles className="w-8 h-8" />
            </div>
            <h3 className="text-base font-bold text-white mb-2">
              {isLoading ? 'Đang tra cứu dữ liệu nhân sự...' : 'Không tìm thấy người dùng phù hợp'}
            </h3>
            <p className="text-xs text-slate-400 max-w-sm leading-relaxed">
              {isLoading
                ? 'Hệ thống đang truy vấn danh sách người dùng từ Identity Service qua API Gateway.'
                : 'Thử kiểm tra lại từ khóa tìm kiếm họ tên hoặc phòng ban của bạn.'}
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {users.map((user) => {
              const relation = getRelationStatus(user.id);
              const isActionLoading = Boolean(
                actionLoadingId === user.id ||
                (relation.contactId && actionLoadingId === relation.contactId)
              );

              return (
                <div
                  key={user.id}
                  className="p-4 rounded-2xl bg-[#0c2340]/90 border border-slate-800 hover:border-[#1d4ed8]/60 shadow-md transition-all duration-200 flex flex-col justify-between group"
                >
                  {/* Top user details */}
                  <div className="flex items-start gap-3.5 mb-4">
                    <Avatar
                      name={user.fullName}
                      src={user.avatarUrl || undefined}
                      size="md"
                      status="online"
                    />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <h4 className="text-sm font-bold text-white truncate group-hover:text-[#38bdf8] transition-colors">
                          {user.fullName}
                        </h4>
                        {relation.status === 'self' && (
                          <span className="px-2 py-0.5 rounded-md text-[10px] bg-slate-700/80 text-slate-300 font-semibold shrink-0">
                            Bạn
                          </span>
                        )}
                        {relation.status === 'accepted' && (
                          <span className="px-2 py-0.5 rounded-md text-[10px] bg-emerald-500/20 text-emerald-400 border border-emerald-500/30 font-semibold shrink-0">
                            Bạn bè
                          </span>
                        )}
                      </div>

                      {user.position && (
                        <p className="text-xs text-slate-300 flex items-center gap-1.5 mt-0.5 truncate">
                          <Briefcase className="w-3 h-3 text-slate-400 shrink-0" />
                          <span className="truncate">{user.position}</span>
                        </p>
                      )}
                      {user.departmentName && (
                        <p className="text-xs text-slate-400 flex items-center gap-1.5 mt-0.5 truncate">
                          <Building2 className="w-3 h-3 text-[#38bdf8]/70 shrink-0" />
                          <span className="truncate">{user.departmentName}</span>
                        </p>
                      )}
                      <p className="text-[11px] text-slate-400 flex items-center gap-1.5 mt-1 truncate">
                        <Mail className="w-3 h-3 text-slate-500 shrink-0" />
                        <span className="truncate">{user.email}</span>
                      </p>
                      {user.phone && (
                        <p className="text-[11px] text-slate-400 flex items-center gap-1.5 mt-0.5 truncate">
                          <Phone className="w-3 h-3 text-slate-500 shrink-0" />
                          <span className="truncate">{user.phone}</span>
                        </p>
                      )}
                    </div>
                  </div>

                  {/* Actions adapted to relation status */}
                  <div className="pt-3 border-t border-slate-800/80 flex items-center gap-2">
                    {relation.status === 'self' ? (
                      <div className="w-full py-1.5 px-3 rounded-xl bg-slate-800/50 text-slate-400 text-xs text-center font-medium select-none">
                        Tài khoản của bạn
                      </div>
                    ) : relation.status === 'accepted' ? (
                      <button
                        onClick={() => onStartChat(user.id)}
                        className="w-full py-2 px-3 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] text-white text-xs font-semibold shadow-sm transition-all hover:scale-[1.02] active:scale-95 cursor-pointer flex items-center justify-center gap-2"
                      >
                        <MessageSquare className="w-3.5 h-3.5" />
                        <span>Nhắn tin</span>
                      </button>
                    ) : relation.status === 'pending_received' && relation.contactId ? (
                      <div className="w-full flex items-center gap-2">
                        <button
                          onClick={() => onRespondRequest(relation.contactId!, true)}
                          disabled={isActionLoading}
                          className="flex-1 py-1.5 px-3 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold shadow-sm transition-all hover:scale-105 active:scale-95 cursor-pointer flex items-center justify-center gap-1.5 disabled:opacity-50"
                        >
                          <Check className="w-3.5 h-3.5" />
                          <span>Đồng ý</span>
                        </button>
                        <button
                          onClick={() => onRespondRequest(relation.contactId!, false)}
                          disabled={isActionLoading}
                          className="py-1.5 px-3 rounded-xl bg-[#07172b] hover:bg-rose-950/40 border border-slate-700 text-slate-300 text-xs font-semibold transition-all hover:scale-105 active:scale-95 cursor-pointer flex items-center justify-center gap-1.5 disabled:opacity-50"
                        >
                          <X className="w-3.5 h-3.5" />
                          <span>Từ chối</span>
                        </button>
                      </div>
                    ) : relation.status === 'pending_sent' ? (
                      <div className="w-full flex items-center justify-between gap-2 px-1">
                        <span className="text-xs text-amber-400 font-medium flex items-center gap-1.5">
                          <Clock className="w-3.5 h-3.5 animate-pulse" />
                          <span>Đã gửi lời mời</span>
                        </span>
                        <button
                          onClick={() => onCancelSentRequest(user.id, user.fullName)}
                          disabled={isActionLoading}
                          className="text-[11px] text-slate-400 hover:text-rose-400 underline transition-colors cursor-pointer disabled:opacity-50"
                        >
                          Hủy yêu cầu
                        </button>
                      </div>
                    ) : relation.status === 'blocked' ? (
                      <div className="w-full flex items-center justify-between gap-2 px-1">
                        <span className="text-xs text-rose-400 font-medium flex items-center gap-1.5">
                          <ShieldBan className="w-3.5 h-3.5" />
                          <span>Đã chặn</span>
                        </span>
                        <button
                          onClick={() => onUnblockUser(user.id, user.fullName)}
                          disabled={isActionLoading}
                          className="text-xs text-[#38bdf8] hover:underline font-semibold cursor-pointer disabled:opacity-50"
                        >
                          Bỏ chặn
                        </button>
                      </div>
                    ) : (
                      <button
                        onClick={() => onSendRequest(user.id)}
                        disabled={isActionLoading}
                        className="w-full py-2 px-3 rounded-xl bg-[#0c2340] hover:bg-[#0052cc] border border-[#1d4ed8]/50 hover:border-[#0052cc] text-[#38bdf8] hover:text-white text-xs font-semibold shadow-sm transition-all hover:scale-[1.02] active:scale-95 cursor-pointer flex items-center justify-center gap-2 disabled:opacity-50"
                      >
                        {isActionLoading ? (
                          <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                        ) : (
                          <UserPlus className="w-3.5 h-3.5" />
                        )}
                        <span>Kết bạn</span>
                      </button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Pagination Footer */}
      {totalPages > 1 && (
        <div className="p-4 border-t border-slate-800/80 bg-[#07172b]/80 flex items-center justify-between shrink-0">
          <span className="text-xs text-slate-400">
            Trang <strong className="text-white">{currentPage}</strong> /{' '}
            <strong className="text-white">{totalPages}</strong>
          </span>

          <div className="flex items-center gap-2">
            <button
              onClick={() => onPageChange(currentPage - 1)}
              disabled={currentPage <= 1 || isLoading}
              className="px-3 py-1.5 rounded-lg bg-[#0c2340] border border-slate-700 text-xs font-semibold text-slate-300 hover:text-white hover:border-[#38bdf8] disabled:opacity-40 disabled:cursor-not-allowed transition-all flex items-center gap-1 cursor-pointer"
            >
              <ChevronLeft className="w-3.5 h-3.5" />
              <span>Trước</span>
            </button>

            <button
              onClick={() => onPageChange(currentPage + 1)}
              disabled={currentPage >= totalPages || isLoading}
              className="px-3 py-1.5 rounded-lg bg-[#0c2340] border border-slate-700 text-xs font-semibold text-slate-300 hover:text-white hover:border-[#38bdf8] disabled:opacity-40 disabled:cursor-not-allowed transition-all flex items-center gap-1 cursor-pointer"
            >
              <span>Sau</span>
              <ChevronRight className="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
