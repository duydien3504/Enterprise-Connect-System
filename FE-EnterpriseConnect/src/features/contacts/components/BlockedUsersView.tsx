import React from 'react';
import { ShieldAlert, ShieldCheck, RefreshCw, Building2, Briefcase, Mail } from 'lucide-react';
import { Avatar } from '@/shared/components/Avatar';
import type { ContactItem } from '../types/contact.types';

interface BlockedUsersViewProps {
  blockedContacts: ContactItem[];
  isLoading: boolean;
  actionLoadingId: string | null;
  onUnblockUser: (targetUserId: string, fullName: string) => void;
  onRefresh: () => void;
}

export const BlockedUsersView: React.FC<BlockedUsersViewProps> = ({
  blockedContacts,
  isLoading,
  actionLoadingId,
  onUnblockUser,
  onRefresh,
}) => {
  return (
    <div className="flex flex-col h-full overflow-hidden bg-[#071e3d]/40">
      {/* Top Header */}
      <div className="p-5 border-b border-slate-800/80 bg-[#07172b]/60 flex items-center justify-between shrink-0">
        <div>
          <h3 className="text-sm font-bold text-white flex items-center gap-2">
            <ShieldAlert className="w-4 h-4 text-rose-400" />
            <span>Danh sách người dùng đã chặn</span>
          </h3>
          <p className="text-xs text-slate-400 mt-0.5">
            Những người dùng trong danh sách này sẽ không thể gửi tin nhắn hoặc gọi điện cho bạn.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-xs text-slate-400">
            Tổng số: <strong className="text-white">{blockedContacts.length}</strong>
          </span>
          <button
            onClick={onRefresh}
            disabled={isLoading}
            title="Tải lại danh sách"
            className="p-2 rounded-xl bg-[#0c2340] border border-slate-700/70 text-slate-300 hover:text-white hover:border-[#38bdf8] transition-all cursor-pointer disabled:opacity-50"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 overflow-y-auto p-5">
        {blockedContacts.length === 0 ? (
          <div className="h-full min-h-[300px] flex flex-col items-center justify-center text-center p-8">
            <div className="w-16 h-16 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 flex items-center justify-center text-emerald-400 mb-4 shadow-lg">
              <ShieldCheck className="w-8 h-8" />
            </div>
            <h3 className="text-base font-bold text-white mb-2">Không có người dùng nào bị chặn</h3>
            <p className="text-xs text-slate-400 max-w-sm leading-relaxed">
              Danh sách chặn của bạn đang trống. Bạn có thể chặn bất kỳ người dùng nào từ danh bạ bạn bè nếu muốn ngắt kết nối.
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {blockedContacts.map((contact) => {
              const isActionLoading = actionLoadingId === contact.userId;

              return (
                <div
                  key={contact.contactId}
                  className="p-4 rounded-2xl bg-[#0c2340]/90 border border-slate-800 hover:border-rose-500/30 shadow-md transition-all duration-200 flex flex-col justify-between"
                >
                  <div className="flex items-start gap-3.5 mb-4">
                    <Avatar
                      name={contact.fullName}
                      src={contact.avatarUrl || undefined}
                      size="md"
                    />
                    <div className="flex-1 min-w-0">
                      <h4 className="text-sm font-bold text-white truncate">{contact.fullName}</h4>
                      {contact.position && (
                        <p className="text-xs text-slate-300 flex items-center gap-1.5 mt-0.5 truncate">
                          <Briefcase className="w-3 h-3 text-slate-400 shrink-0" />
                          <span className="truncate">{contact.position}</span>
                        </p>
                      )}
                      {contact.departmentName && (
                        <p className="text-xs text-slate-400 flex items-center gap-1.5 mt-0.5 truncate">
                          <Building2 className="w-3 h-3 text-[#38bdf8]/70 shrink-0" />
                          <span className="truncate">{contact.departmentName}</span>
                        </p>
                      )}
                      <p className="text-[11px] text-slate-400 flex items-center gap-1.5 mt-1 truncate">
                        <Mail className="w-3 h-3 text-slate-500 shrink-0" />
                        <span className="truncate">{contact.email}</span>
                      </p>
                    </div>
                  </div>

                  <div className="pt-3 border-t border-slate-800/80 flex items-center justify-end">
                    <button
                      onClick={() => onUnblockUser(contact.userId, contact.fullName)}
                      disabled={isActionLoading}
                      className="px-4 py-2 rounded-xl bg-[#07172b] hover:bg-emerald-950/40 border border-slate-700 hover:border-emerald-500/40 text-slate-300 hover:text-emerald-300 text-xs font-semibold shadow-sm transition-all hover:scale-105 active:scale-95 cursor-pointer flex items-center gap-2 disabled:opacity-50"
                    >
                      {isActionLoading ? (
                        <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                      ) : (
                        <ShieldCheck className="w-3.5 h-3.5" />
                      )}
                      <span>Bỏ chặn</span>
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};
