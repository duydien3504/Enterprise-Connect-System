import React, { useState, useEffect } from 'react';
import { X, Users, MessageSquare, Check, Shield, Search, RefreshCw } from 'lucide-react';
import { Avatar } from '@/shared/components/Avatar';
import { contactsApi } from '@/features/contacts/api/contactsApi';
import type { ContactItem } from '@/features/contacts/types/contact.types';

interface CreateConversationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCreateDirect: (targetUserId: string) => Promise<any>;
  onCreateGroup: (groupName: string, memberIds: string[]) => Promise<any>;
}

export const CreateConversationModal: React.FC<CreateConversationModalProps> = ({
  isOpen,
  onClose,
  onCreateDirect,
  onCreateGroup,
}) => {
  const [isGroup, setIsGroup] = useState(true);
  const [title, setTitle] = useState('');
  const [selectedUserIds, setSelectedUserIds] = useState<string[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [contacts, setContacts] = useState<ContactItem[]>([]);
  const [isLoadingContacts, setIsLoadingContacts] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Load real contacts when modal opens
  useEffect(() => {
    if (!isOpen) return;

    let isMounted = true;
    setIsLoadingContacts(true);
    contactsApi
      .getContacts('accepted')
      .then((data) => {
        if (isMounted) setContacts(data || []);
      })
      .catch(() => {})
      .finally(() => {
        if (isMounted) setIsLoadingContacts(false);
      });

    return () => {
      isMounted = false;
    };
  }, [isOpen]);

  if (!isOpen) return null;

  const toggleUser = (userId: string) => {
    if (!isGroup) {
      setSelectedUserIds([userId]);
    } else {
      setSelectedUserIds((prev) =>
        prev.includes(userId) ? prev.filter((id) => id !== userId) : [...prev, userId]
      );
    }
  };

  const filteredContacts = contacts.filter((c) => {
    if (!searchQuery.trim()) return true;
    const q = searchQuery.toLowerCase();
    return (
      c.fullName.toLowerCase().includes(q) ||
      c.email.toLowerCase().includes(q) ||
      (c.departmentName && c.departmentName.toLowerCase().includes(q))
    );
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isGroup && !title.trim()) return;
    if (selectedUserIds.length === 0) return;

    try {
      setIsSubmitting(true);
      if (!isGroup) {
        await onCreateDirect(selectedUserIds[0]);
      } else {
        await onCreateGroup(title.trim(), selectedUserIds);
      }
      onClose();
      setTitle('');
      setSelectedUserIds([]);
    } catch {
      // Error handled by hook feedback
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#071e3d]/80 backdrop-blur-md animate-in fade-in duration-150">
      <div className="relative w-full max-w-[500px] rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/50 p-6 sm:p-7 shadow-[0_24px_60px_rgba(0,0,0,0.7)] text-white">
        {/* Close Button */}
        <button
          onClick={onClose}
          disabled={isSubmitting}
          className="absolute top-5 right-5 w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer disabled:opacity-50"
        >
          <X className="w-4 h-4" />
        </button>

        {/* Header */}
        <div className="mb-5">
          <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-[#38bdf8] mb-1.5">
            <Shield className="w-4 h-4" />
            <span>Messaging Service • Khởi Tạo Hội Thoại</span>
          </div>
          <h3 className="text-xl font-bold text-white tracking-tight">
            Tạo Kênh Liên Lạc Bảo Mật
          </h3>
        </div>

        {/* Type Toggle: Nhóm vs 1-1 */}
        <div className="grid grid-cols-2 gap-2 p-1 rounded-xl bg-[#07172b] border border-slate-700/80 mb-5">
          <button
            type="button"
            onClick={() => {
              setIsGroup(true);
              setSelectedUserIds([]);
            }}
            className={`py-2 px-3 rounded-lg text-xs font-semibold flex items-center justify-center gap-2 transition-colors cursor-pointer ${
              isGroup ? 'bg-[#0052cc] text-white shadow-sm' : 'text-slate-400 hover:text-white'
            }`}
          >
            <Users className="w-4 h-4" />
            <span>Nhóm Dự Án / Ban</span>
          </button>
          <button
            type="button"
            onClick={() => {
              setIsGroup(false);
              setSelectedUserIds([]);
            }}
            className={`py-2 px-3 rounded-lg text-xs font-semibold flex items-center justify-center gap-2 transition-colors cursor-pointer ${
              !isGroup ? 'bg-[#0052cc] text-white shadow-sm' : 'text-slate-400 hover:text-white'
            }`}
          >
            <MessageSquare className="w-4 h-4" />
            <span>Trò Chuyện 1-1 (Direct)</span>
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Group Name Field */}
          {isGroup && (
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">
                Tên Nhóm / Dự Án <span className="text-rose-400">*</span>
              </label>
              <input
                type="text"
                required
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Ví dụ: Ban Chỉ Đạo An Ninh Mạng 2026..."
                className="w-full px-3.5 py-2.5 rounded-lg bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-[#38bdf8] transition-colors"
              />
            </div>
          )}

          {/* Members Selection List */}
          <div>
            <div className="flex items-center justify-between mb-1.5">
              <label className="text-xs font-semibold text-slate-300">
                {isGroup ? 'Chọn Thành Viên Tham Gia' : 'Chọn Người Trò Chuyện'}
              </label>
              <span className="text-xs text-[#38bdf8] font-bold">
                Đã chọn: {selectedUserIds.length}
              </span>
            </div>

            {/* Search Contact Input */}
            <div className="relative mb-2.5">
              <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                placeholder="Tìm trong danh bạ đồng nghiệp..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-9 pr-3.5 py-2 rounded-lg bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-xs focus:outline-none focus:border-[#38bdf8] transition-colors"
              />
            </div>

            {/* Scrollable list */}
            <div className="max-h-[180px] overflow-y-auto space-y-1.5 pr-1 rounded-xl bg-[#07172b]/50 p-2 border border-slate-800">
              {isLoadingContacts ? (
                <div className="py-6 text-center text-xs text-slate-400 flex items-center justify-center gap-2">
                  <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                  <span>Đang tải danh bạ...</span>
                </div>
              ) : filteredContacts.length === 0 ? (
                <div className="py-6 text-center text-xs text-slate-400">
                  {contacts.length === 0
                    ? 'Bạn chưa có đồng nghiệp nào trong danh bạ bạn bè.'
                    : 'Không tìm thấy người dùng phù hợp.'}
                </div>
              ) : (
                filteredContacts.map((contact) => {
                  const isSelected = selectedUserIds.includes(contact.userId);

                  return (
                    <div
                      key={contact.userId}
                      onClick={() => toggleUser(contact.userId)}
                      className={`flex items-center justify-between p-2 rounded-lg cursor-pointer transition-colors ${
                        isSelected
                          ? 'bg-[#0052cc]/30 border border-[#0052cc]'
                          : 'hover:bg-white/5 border border-transparent'
                      }`}
                    >
                      <div className="flex items-center gap-2.5 min-w-0">
                        <Avatar
                          name={contact.fullName}
                          src={contact.avatarUrl || undefined}
                          size="sm"
                        />
                        <div className="min-w-0">
                          <p className="text-xs font-semibold text-white truncate">
                            {contact.fullName}
                          </p>
                          <p className="text-[10px] text-slate-400 truncate">
                            {contact.departmentName || contact.email}
                          </p>
                        </div>
                      </div>

                      <div
                        className={`w-5 h-5 rounded-md flex items-center justify-center transition-colors ${
                          isSelected
                            ? 'bg-[#0052cc] text-white'
                            : 'border border-slate-600'
                        }`}
                      >
                        {isSelected && <Check className="w-3.5 h-3.5" />}
                      </div>
                    </div>
                  );
                })
              )}
            </div>
          </div>

          {/* Footer Submit Buttons */}
          <div className="pt-3 border-t border-slate-700/80 flex items-center justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="px-4 py-2 rounded-lg text-xs font-semibold text-slate-300 hover:text-white hover:bg-white/5 transition-colors cursor-pointer disabled:opacity-50"
            >
              Hủy Bỏ
            </button>
            <button
              type="submit"
              disabled={
                isSubmitting ||
                selectedUserIds.length === 0 ||
                (isGroup && !title.trim())
              }
              className="px-5 py-2.5 rounded-lg bg-[#0052cc] hover:bg-[#0065ff] text-white text-xs font-bold shadow-md shadow-[#0052cc]/30 transition-all hover:scale-105 active:scale-95 cursor-pointer disabled:opacity-40 disabled:hover:scale-100 flex items-center gap-2"
            >
              {isSubmitting ? (
                <RefreshCw className="w-3.5 h-3.5 animate-spin" />
              ) : isGroup ? (
                <Users className="w-3.5 h-3.5" />
              ) : (
                <MessageSquare className="w-3.5 h-3.5" />
              )}
              <span>
                {isSubmitting
                  ? 'Đang khởi tạo...'
                  : isGroup
                  ? 'Tạo Nhóm Hội Thoại'
                  : 'Bắt Đầu Trò Chuyện'}
              </span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
