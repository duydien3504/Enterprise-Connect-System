import React from 'react';
import {
  Check,
  X,
  Clock,
  UserCheck,
  Send,
  Building2,
  Briefcase,
  Mail,
  RefreshCw,
  Trash2,
} from 'lucide-react';
import { Avatar } from '@/shared/components/Avatar';
import type { ContactItem } from '../types/contact.types';
import type { PendingSubTab } from '../hooks/useContactsManager';

interface PendingRequestsViewProps {
  pendingSubTab: PendingSubTab;
  onSelectSubTab: (tab: PendingSubTab) => void;
  receivedList: ContactItem[];
  sentList: ContactItem[];
  isLoading: boolean;
  actionLoadingId: string | null;
  onRespondRequest: (contactId: string, accept: boolean) => void;
  onCancelSentRequest: (targetUserId: string, fullName: string) => void;
  onRefresh: () => void;
}

export const PendingRequestsView: React.FC<PendingRequestsViewProps> = ({
  pendingSubTab,
  onSelectSubTab,
  receivedList,
  sentList,
  isLoading,
  actionLoadingId,
  onRespondRequest,
  onCancelSentRequest,
  onRefresh,
}) => {
  const currentList = pendingSubTab === 'received' ? receivedList : sentList;

  return (
    <div className="flex flex-col h-full overflow-hidden bg-[#071e3d]/40">
      {/* Top Segmented Tab Switcher */}
      <div className="p-5 border-b border-slate-800/80 bg-[#07172b]/60 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 shrink-0">
        <div className="flex items-center p-1 rounded-xl bg-[#0c2340] border border-slate-700/60 w-fit">
          <button
            onClick={() => onSelectSubTab('received')}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition-all cursor-pointer ${
              pendingSubTab === 'received'
                ? 'bg-[#0052cc] text-white shadow-sm'
                : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            <UserCheck className="w-3.5 h-3.5" />
            <span>Lời mời đã nhận</span>
            {receivedList.length > 0 && (
              <span className="ml-1 px-1.5 py-0.2 rounded-full text-[10px] bg-emerald-500 text-white font-bold">
                {receivedList.length}
              </span>
            )}
          </button>

          <button
            onClick={() => onSelectSubTab('sent')}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-xs font-semibold transition-all cursor-pointer ${
              pendingSubTab === 'sent'
                ? 'bg-[#0052cc] text-white shadow-sm'
                : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            <Send className="w-3.5 h-3.5" />
            <span>Lời mời đã gửi</span>
            {sentList.length > 0 && (
              <span className="ml-1 px-1.5 py-0.2 rounded-full text-[10px] bg-[#38bdf8] text-slate-900 font-bold">
                {sentList.length}
              </span>
            )}
          </button>
        </div>

        <button
          onClick={onRefresh}
          disabled={isLoading}
          title="Tải lại danh sách"
          className="p-2 rounded-xl bg-[#0c2340] border border-slate-700/70 text-slate-300 hover:text-white hover:border-[#38bdf8] transition-all cursor-pointer self-end sm:self-auto disabled:opacity-50"
        >
          <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin' : ''}`} />
        </button>
      </div>

      {/* Requests List */}
      <div className="flex-1 overflow-y-auto p-5">
        {currentList.length === 0 ? (
          <div className="h-full min-h-[300px] flex flex-col items-center justify-center text-center p-8">
            <div className="w-16 h-16 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 flex items-center justify-center text-[#38bdf8] mb-4 shadow-lg">
              {pendingSubTab === 'received' ? (
                <UserCheck className="w-8 h-8" />
              ) : (
                <Send className="w-8 h-8" />
              )}
            </div>
            <h3 className="text-base font-bold text-white mb-2">
              {pendingSubTab === 'received'
                ? 'Không có lời mời kết bạn nào đang chờ'
                : 'Bạn chưa gửi lời mời kết bạn nào'}
            </h3>
            <p className="text-xs text-slate-400 max-w-sm leading-relaxed">
              {pendingSubTab === 'received'
                ? 'Khi có đồng nghiệp gửi lời mời kết bạn, thông báo và danh sách sẽ hiển thị tại đây để bạn xử lý.'
                : 'Chuyển sang mục "Tra cứu toàn công ty" để tìm kiếm và gửi lời mời kết nối tới các đồng nghiệp.'}
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {currentList.map((item) => {
              const isActionLoading =
                actionLoadingId === item.contactId || actionLoadingId === item.userId;

              return (
                <div
                  key={item.contactId}
                  className="p-4 rounded-2xl bg-[#0c2340]/90 border border-slate-800 hover:border-[#1d4ed8]/60 shadow-md transition-all duration-200 flex flex-col justify-between"
                >
                  {/* Top user details */}
                  <div className="flex items-start gap-3.5 mb-4">
                    <Avatar
                      name={item.fullName}
                      src={item.avatarUrl || undefined}
                      size="md"
                      status="online"
                    />
                    <div className="flex-1 min-w-0">
                      <h4 className="text-sm font-bold text-white truncate">{item.fullName}</h4>
                      {item.position && (
                        <p className="text-xs text-slate-300 flex items-center gap-1.5 mt-0.5 truncate">
                          <Briefcase className="w-3 h-3 text-slate-400 shrink-0" />
                          <span className="truncate">{item.position}</span>
                        </p>
                      )}
                      {item.departmentName && (
                        <p className="text-xs text-slate-400 flex items-center gap-1.5 mt-0.5 truncate">
                          <Building2 className="w-3 h-3 text-[#38bdf8]/70 shrink-0" />
                          <span className="truncate">{item.departmentName}</span>
                        </p>
                      )}
                      <p className="text-[11px] text-slate-400 flex items-center gap-1.5 mt-1 truncate">
                        <Mail className="w-3 h-3 text-slate-500 shrink-0" />
                        <span className="truncate">{item.email}</span>
                      </p>
                    </div>
                  </div>

                  {/* Actions according to sub-tab */}
                  <div className="pt-3 border-t border-slate-800/80 flex items-center gap-2">
                    {pendingSubTab === 'received' ? (
                      <>
                        <button
                          onClick={() => onRespondRequest(item.contactId, true)}
                          disabled={isActionLoading}
                          className="flex-1 py-2 px-3 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold shadow-sm transition-all hover:scale-[1.02] active:scale-95 cursor-pointer flex items-center justify-center gap-1.5 disabled:opacity-50"
                        >
                          {isActionLoading ? (
                            <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                          ) : (
                            <Check className="w-3.5 h-3.5" />
                          )}
                          <span>Đồng ý</span>
                        </button>
                        <button
                          onClick={() => onRespondRequest(item.contactId, false)}
                          disabled={isActionLoading}
                          className="py-2 px-3 rounded-xl bg-[#07172b] hover:bg-rose-950/40 border border-slate-700 hover:border-rose-500/40 text-slate-300 hover:text-rose-300 text-xs font-semibold transition-all hover:scale-[1.02] active:scale-95 cursor-pointer flex items-center justify-center gap-1.5 disabled:opacity-50"
                        >
                          <X className="w-3.5 h-3.5" />
                          <span>Từ chối</span>
                        </button>
                      </>
                    ) : (
                      <div className="w-full flex items-center justify-between gap-2">
                        <span className="text-[11px] text-amber-400/90 flex items-center gap-1 font-medium">
                          <Clock className="w-3 h-3 animate-pulse" />
                          <span>Đang chờ duyệt</span>
                        </span>
                        <button
                          onClick={() => onCancelSentRequest(item.userId, item.fullName)}
                          disabled={isActionLoading}
                          className="py-1.5 px-3 rounded-xl bg-[#07172b] hover:bg-rose-950/40 border border-slate-700 hover:border-rose-500/40 text-slate-300 hover:text-rose-300 text-xs font-semibold transition-all hover:scale-105 active:scale-95 cursor-pointer flex items-center gap-1.5 disabled:opacity-50"
                        >
                          {isActionLoading ? (
                            <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                          ) : (
                            <Trash2 className="w-3.5 h-3.5" />
                          )}
                          <span>Thu hồi</span>
                        </button>
                      </div>
                    )}
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
