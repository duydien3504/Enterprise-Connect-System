import React, { useState, useMemo } from 'react';
import {
  Search,
  MessageSquare,
  MoreVertical,
  UserMinus,
  Ban,
  Building2,
  Mail,
  Briefcase,
  Users,
  RefreshCw,
} from 'lucide-react';
import { Avatar } from '@/shared/components/Avatar';
import type { ContactItem } from '../types/contact.types';

interface AcceptedContactsViewProps {
  contacts: ContactItem[];
  isLoading: boolean;
  actionLoadingId: string | null;
  onStartChat: (userId: string) => void;
  onRemoveContact: (userId: string, fullName: string) => void;
  onBlockUser: (userId: string, fullName: string) => void;
  onGoToDirectory: () => void;
  onRefresh: () => void;
}

export const AcceptedContactsView: React.FC<AcceptedContactsViewProps> = ({
  contacts,
  isLoading,
  actionLoadingId,
  onStartChat,
  onRemoveContact,
  onBlockUser,
  onGoToDirectory,
  onRefresh,
}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedDept, setSelectedDept] = useState<string>('all');
  const [activeMenuId, setActiveMenuId] = useState<string | null>(null);

  // Extract unique departments
  const departments = useMemo(() => {
    const set = new Set<string>();
    contacts.forEach((c) => {
      if (c.departmentName) set.add(c.departmentName);
    });
    return Array.from(set);
  }, [contacts]);

  // Filtered contacts
  const filteredContacts = useMemo(() => {
    return contacts.filter((c) => {
      const matchQuery =
        !searchQuery.trim() ||
        c.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        c.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
        (c.position && c.position.toLowerCase().includes(searchQuery.toLowerCase()));

      const matchDept = selectedDept === 'all' || c.departmentName === selectedDept;

      return matchQuery && matchDept;
    });
  }, [contacts, searchQuery, selectedDept]);

  return (
    <div className="flex flex-col h-full overflow-hidden bg-[#071e3d]/40">
      {/* Top Filter Bar */}
      <div className="p-5 border-b border-slate-800/80 bg-[#07172b]/60 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 shrink-0">
        <div className="flex items-center gap-3 flex-1 max-w-md">
          <div className="relative flex-1">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Tìm kiếm theo tên, chức vụ, email..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-4 py-2 bg-[#0c2340] border border-slate-700/70 rounded-xl text-xs text-white placeholder-slate-400 focus:outline-none focus:border-[#38bdf8] focus:ring-1 focus:ring-[#38bdf8] transition-all"
            />
          </div>

          {departments.length > 0 && (
            <select
              value={selectedDept}
              onChange={(e) => setSelectedDept(e.target.value)}
              className="px-3 py-2 bg-[#0c2340] border border-slate-700/70 rounded-xl text-xs text-slate-200 focus:outline-none focus:border-[#38bdf8] cursor-pointer"
            >
              <option value="all">Tất cả phòng ban</option>
              {departments.map((d) => (
                <option key={d} value={d}>
                  {d}
                </option>
              ))}
            </select>
          )}
        </div>

        <div className="flex items-center gap-3">
          <span className="text-xs text-slate-400 font-medium">
            Tổng số: <strong className="text-white">{filteredContacts.length}</strong> bạn bè
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

      {/* Main Content List / Grid */}
      <div className="flex-1 overflow-y-auto p-5">
        {filteredContacts.length === 0 ? (
          <div className="h-full min-h-[300px] flex flex-col items-center justify-center text-center p-8">
            <div className="w-16 h-16 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/30 flex items-center justify-center text-[#38bdf8] mb-4 shadow-lg">
              <Users className="w-8 h-8" />
            </div>
            <h3 className="text-base font-bold text-white mb-2">
              {searchQuery ? 'Không tìm thấy liên hệ phù hợp' : 'Chưa có bạn bè trong danh bạ'}
            </h3>
            <p className="text-xs text-slate-400 max-w-sm mb-5 leading-relaxed">
              {searchQuery
                ? 'Thử thay đổi từ khóa tìm kiếm hoặc bỏ chọn lọc phòng ban.'
                : 'Khám phá cây nhân sự và gửi lời mời kết nối tới các đồng nghiệp trong doanh nghiệp của bạn ngay bây giờ.'}
            </p>
            {!searchQuery && (
              <button
                onClick={onGoToDirectory}
                className="px-4 py-2 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] text-white text-xs font-semibold shadow-md transition-all hover:scale-105 active:scale-95 cursor-pointer flex items-center gap-2"
              >
                <Users className="w-4 h-4" />
                <span>Tra cứu đồng nghiệp toàn công ty</span>
              </button>
            )}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {filteredContacts.map((contact) => {
              const isActionLoading = actionLoadingId === contact.userId;
              const isMenuOpen = activeMenuId === contact.contactId;

              return (
                <div
                  key={contact.contactId}
                  className="relative p-4 rounded-2xl bg-[#0c2340]/90 border border-slate-800 hover:border-[#1d4ed8]/60 shadow-md hover:shadow-lg transition-all duration-200 flex flex-col justify-between group"
                >
                  {/* Top card info */}
                  <div className="flex items-start gap-3.5 mb-4">
                    <Avatar
                      name={contact.fullName}
                      src={contact.avatarUrl || undefined}
                      size="md"
                      status="online"
                    />
                    <div className="flex-1 min-w-0">
                      <h4 className="text-sm font-bold text-white truncate group-hover:text-[#38bdf8] transition-colors">
                        {contact.fullName}
                      </h4>
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

                    {/* Context menu toggle */}
                    <div className="relative">
                      <button
                        onClick={() => setActiveMenuId(isMenuOpen ? null : contact.contactId)}
                        className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-[#122e54] transition-all cursor-pointer"
                        title="Tùy chọn khác"
                      >
                        <MoreVertical className="w-4 h-4" />
                      </button>

                      {isMenuOpen && (
                        <>
                          <div
                            className="fixed inset-0 z-20"
                            onClick={() => setActiveMenuId(null)}
                          />
                          <div className="absolute right-0 top-8 w-44 rounded-xl bg-[#07172b] border border-slate-700 shadow-2xl py-1.5 z-30 animate-in fade-in zoom-in-95">
                            <button
                              onClick={() => {
                                setActiveMenuId(null);
                                onRemoveContact(contact.userId, contact.fullName);
                              }}
                              className="w-full px-3 py-2 text-left text-xs text-rose-300 hover:bg-rose-950/40 flex items-center gap-2 transition-colors cursor-pointer"
                            >
                              <UserMinus className="w-3.5 h-3.5" />
                              <span>Hủy kết bạn</span>
                            </button>
                            <button
                              onClick={() => {
                                setActiveMenuId(null);
                                onBlockUser(contact.userId, contact.fullName);
                              }}
                              className="w-full px-3 py-2 text-left text-xs text-slate-400 hover:text-rose-400 hover:bg-rose-950/20 flex items-center gap-2 transition-colors cursor-pointer"
                            >
                              <Ban className="w-3.5 h-3.5" />
                              <span>Chặn liên hệ</span>
                            </button>
                          </div>
                        </>
                      )}
                    </div>
                  </div>

                  {/* Bottom Action Button */}
                  <div className="pt-3 border-t border-slate-800/80 flex items-center gap-2">
                    <button
                      onClick={() => onStartChat(contact.userId)}
                      disabled={isActionLoading}
                      className="flex-1 py-2 px-3 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] text-white text-xs font-semibold shadow-sm transition-all hover:scale-[1.02] active:scale-95 cursor-pointer flex items-center justify-center gap-2 disabled:opacity-50"
                    >
                      <MessageSquare className="w-3.5 h-3.5" />
                      <span>Nhắn tin</span>
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
