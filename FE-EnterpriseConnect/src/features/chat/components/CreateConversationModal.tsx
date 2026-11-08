import React, { useState } from 'react';
import { X, Users, MessageSquare, Check, Shield } from 'lucide-react';
import { MOCK_DIRECTORY_USERS } from '../mock/chatMockData';
import type { CreateConversationPayload, UserParticipant } from '../types/chat.types';

interface CreateConversationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCreate: (payload: CreateConversationPayload) => void;
}

export const CreateConversationModal: React.FC<CreateConversationModalProps> = ({
  isOpen,
  onClose,
  onCreate,
}) => {
  const [isGroup, setIsGroup] = useState(true);
  const [title, setTitle] = useState('');
  const [department, setDepartment] = useState('Phòng Kỹ Thuật');
  const [selectedUserIds, setSelectedUserIds] = useState<string[]>([]);
  const [searchQuery, setSearchQuery] = useState('');

  if (!isOpen) return null;

  const toggleUser = (userId: string) => {
    setSelectedUserIds((prev) =>
      prev.includes(userId) ? prev.filter((id) => id !== userId) : [...prev, userId]
    );
  };

  const filteredUsers = MOCK_DIRECTORY_USERS.filter(
    (u) =>
      u.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      u.department?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (isGroup && !title.trim()) return;
    if (selectedUserIds.length === 0) return;

    onCreate({
      title: isGroup ? title.trim() : (MOCK_DIRECTORY_USERS.find((u) => u.id === selectedUserIds[0])?.fullName || 'Hội thoại 1-1'),
      isGroup,
      department: isGroup ? department : undefined,
      participantIds: selectedUserIds,
    });

    onClose();
    setTitle('');
    setSelectedUserIds([]);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#071e3d]/80 backdrop-blur-md animate-in fade-in duration-150">
      <div className="relative w-full max-w-[500px] rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/50 p-6 sm:p-7 shadow-[0_24px_60px_rgba(0,0,0,0.7)] text-white">
        {/* Close Button */}
        <button
          onClick={onClose}
          className="absolute top-5 right-5 w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
        >
          <X className="w-4 h-4" />
        </button>

        {/* Header */}
        <div className="mb-5">
          <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-[#38bdf8] mb-1.5">
            <Shield className="w-4 h-4" />
            <span>Enterprise Chat • Khởi Tạo Hội Thoại Mới</span>
          </div>
          <h3 className="text-xl font-bold text-white tracking-tight">
            Tạo Kênh Liên Lạc Bảo Mật
          </h3>
        </div>

        {/* Type Toggle: 1-1 vs Nhóm Phòng Ban */}
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
            <span>Nhóm Phòng Ban</span>
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
            <span>Hội Thoại 1-1 (Direct)</span>
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Group Specific Fields */}
          {isGroup && (
            <>
              <div>
                <label className="block text-xs font-semibold text-slate-300 mb-1.5">
                  Tên Nhóm / Dự Án
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

              <div>
                <label className="block text-xs font-semibold text-slate-300 mb-1.5">
                  Phòng Ban Trực Thuộc
                </label>
                <select
                  value={department}
                  onChange={(e) => setDepartment(e.target.value)}
                  className="w-full px-3.5 py-2.5 rounded-lg bg-[#07172b] border border-slate-700/80 text-white text-sm focus:outline-none focus:border-[#38bdf8] transition-colors"
                >
                  <option value="Phòng An Ninh Mạng">Phòng An Ninh Mạng</option>
                  <option value="Phòng Kỹ Thuật">Phòng Kỹ Thuật & Core Engine</option>
                  <option value="DevSecOps & Hạ Tầng">DevSecOps & Hạ Tầng</option>
                  <option value="Ban Giám Đốc">Ban Giám Đốc / Ban Lãnh Đạo</option>
                  <option value="Kiểm Thử & Đảm Bảo Chất Lượng">Kiểm Thử & Đảm Bảo Chất Lượng (QA)</option>
                </select>
              </div>
            </>
          )}

          {/* Member Selection from Directory */}
          <div>
            <div className="flex items-center justify-between mb-1.5">
              <label className="text-xs font-semibold text-slate-300">
                {isGroup ? 'Chọn thành viên tham gia' : 'Chọn nhân sự cần nhắn tin 1-1'}
              </label>
              <span className="text-[11px] font-mono text-[#38bdf8]">
                Đã chọn: {selectedUserIds.length}
              </span>
            </div>

            {/* Quick search input */}
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Tìm theo tên hoặc phòng ban..."
              className="w-full px-3 py-2 rounded-lg bg-[#07172b] border border-slate-700/80 text-xs text-white placeholder-slate-500 mb-2 focus:outline-none focus:border-[#38bdf8]"
            />

            {/* User List */}
            <div className="max-h-48 overflow-y-auto space-y-1.5 pr-1 border border-slate-800 rounded-lg p-2 bg-[#071324]/50">
              {filteredUsers.map((u: UserParticipant) => {
                const isSelected = selectedUserIds.includes(u.id);
                return (
                  <div
                    key={u.id}
                    onClick={() => {
                      if (!isGroup) {
                        setSelectedUserIds([u.id]);
                      } else {
                        toggleUser(u.id);
                      }
                    }}
                    className={`flex items-center justify-between p-2 rounded-lg cursor-pointer transition-colors ${
                      isSelected
                        ? 'bg-[#0052cc]/30 border border-[#0052cc]'
                        : 'bg-[#0c2340] border border-slate-800/80 hover:border-slate-700'
                    }`}
                  >
                    <div className="flex items-center gap-2.5">
                      <div className="w-8 h-8 rounded-full bg-[#0052cc] flex items-center justify-center text-xs font-bold text-white uppercase">
                        {u.fullName.slice(0, 2)}
                      </div>
                      <div>
                        <div className="text-xs font-semibold text-white">{u.fullName}</div>
                        <div className="text-[10px] text-slate-400">{u.department}</div>
                      </div>
                    </div>

                    <div
                      className={`w-5 h-5 rounded flex items-center justify-center border transition-colors ${
                        isSelected
                          ? 'bg-[#0052cc] border-[#0052cc] text-white'
                          : 'border-slate-600 bg-[#07172b]'
                      }`}
                    >
                      {isSelected && <Check className="w-3.5 h-3.5" />}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Action buttons */}
          <div className="pt-2 flex items-center justify-end gap-2.5">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2.5 rounded-lg border border-slate-700 text-xs font-medium text-slate-300 hover:bg-white/10 transition-colors cursor-pointer"
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={selectedUserIds.length === 0 || (isGroup && !title.trim())}
              className="px-5 py-2.5 rounded-lg bg-[#0052cc] hover:bg-[#0065ff] disabled:opacity-50 text-white font-bold text-xs shadow-md transition-colors cursor-pointer"
            >
              Tạo Hội Thoại
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
