import React, { useState } from 'react';
import { Search, Plus, Users, Pin } from 'lucide-react';
import type { Conversation, ConversationFilter } from '../types/chat.types';

interface ConversationSidebarProps {
  conversations: Conversation[];
  activeConversationId: string;
  isLoading?: boolean;
  onSelectConversation: (id: string) => void;
  onOpenCreateModal: () => void;
}

export const ConversationSidebar: React.FC<ConversationSidebarProps> = ({
  conversations,
  activeConversationId,
  isLoading = false,
  onSelectConversation,
  onOpenCreateModal,
}) => {
  const [filter, setFilter] = useState<ConversationFilter>('all');
  const [search, setSearch] = useState('');

  const filteredConversations = conversations.filter((c) => {
    const title = c.title || c.name || '';
    const lastMsg = c.lastMessage || '';
    const dept = c.department || '';
    const isGrp = c.isGroup ?? (c.type === 2);

    const matchSearch =
      !search.trim() ||
      title.toLowerCase().includes(search.toLowerCase()) ||
      dept.toLowerCase().includes(search.toLowerCase()) ||
      lastMsg.toLowerCase().includes(search.toLowerCase());

    if (!matchSearch) return false;

    if (filter === 'unread') return (c.unreadCount || 0) > 0;
    if (filter === 'groups') return isGrp;
    if (filter === 'direct') return !isGrp;
    return true;
  });

  return (
    <aside className="w-80 shrink-0 flex flex-col bg-[#0c2340] border-r border-[#1d4ed8]/20 h-full overflow-hidden select-none">
      {/* Top Header & Search Bar */}
      <div className="p-4 border-b border-[#1d4ed8]/20 flex flex-col gap-3.5">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <h2 className="text-sm font-bold uppercase tracking-wider text-slate-200">
              Hội Thoại
            </h2>
            <span className="px-2 py-0.5 rounded-full text-xs font-mono font-bold bg-[#07172b] text-[#38bdf8] border border-slate-700/80">
              {isLoading ? '...' : conversations.length}
            </span>
          </div>

          {/* Plus button [+] to create group/chat */}
          <button
            onClick={onOpenCreateModal}
            title="Tạo hội thoại hoặc nhóm mới"
            className="w-8 h-8 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:scale-95 text-white flex items-center justify-center shadow-md transition-all duration-200 cursor-pointer hover:scale-110"
          >
            <Plus className="w-4 h-4" />
          </button>
        </div>

        {/* Search Input */}
        <div className="relative">
          <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
            <Search className="w-4 h-4" />
          </div>
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Tìm tin nhắn, đồng nghiệp, nhóm..."
            className="w-full pl-10 pr-3.5 py-2.5 rounded-xl bg-[#07172b] border border-slate-700/80 text-xs text-white placeholder-slate-400 focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-all duration-200"
          />
        </div>

        {/* Tab Filters */}
        <div className="grid grid-cols-3 gap-1.5 p-1 rounded-xl bg-[#07172b] border border-slate-800 text-xs font-medium">
          <button
            onClick={() => setFilter('all')}
            className={`py-1.5 rounded-lg text-center transition-all duration-150 cursor-pointer ${
              filter === 'all'
                ? 'bg-[#0052cc] text-white font-bold shadow-sm'
                : 'text-slate-400 hover:text-white hover:bg-white/5'
            }`}
          >
            Tất cả
          </button>
          <button
            onClick={() => setFilter('unread')}
            className={`py-1.5 rounded-lg text-center transition-all duration-150 cursor-pointer ${
              filter === 'unread'
                ? 'bg-[#0052cc] text-white font-bold shadow-sm'
                : 'text-slate-400 hover:text-white hover:bg-white/5'
            }`}
          >
            Chưa đọc
          </button>
          <button
            onClick={() => setFilter('groups')}
            className={`py-1.5 rounded-lg text-center transition-all duration-150 cursor-pointer ${
              filter === 'groups'
                ? 'bg-[#0052cc] text-white font-bold shadow-sm'
                : 'text-slate-400 hover:text-white hover:bg-white/5'
            }`}
          >
            Nhóm
          </button>
        </div>
      </div>

      {/* Conversation Scroll List */}
      <div className="flex-1 overflow-y-auto p-2 space-y-1.5">
        {isLoading ? (
          <div className="space-y-2 p-1">
            {[1, 2, 3, 4, 5].map((i) => (
              <div
                key={i}
                className="flex items-center gap-3 p-3 rounded-xl bg-[#07172b]/50 border border-slate-800/60 animate-pulse"
              >
                <div className="w-11 h-11 rounded-xl bg-slate-800/80 shrink-0" />
                <div className="flex-1 min-w-0 space-y-2">
                  <div className="flex justify-between items-center">
                    <div className="h-3.5 w-28 bg-slate-800/80 rounded" />
                    <div className="h-2.5 w-10 bg-slate-800/60 rounded" />
                  </div>
                  <div className="h-2.5 w-40 bg-slate-800/50 rounded" />
                </div>
              </div>
            ))}
          </div>
        ) : filteredConversations.length === 0 ? (
          <div className="p-8 text-center text-xs text-slate-400 flex flex-col items-center justify-center space-y-2">
            <span className="text-2xl">💬</span>
            <p className="font-semibold text-slate-300">Không tìm thấy cuộc trò chuyện</p>
            <p className="text-[11px] text-slate-500">Tạo nhóm mới hoặc kết nối đồng nghiệp từ Danh Bạ</p>
          </div>
        ) : (
          filteredConversations.map((conv) => {
            const isActive = conv.id === activeConversationId;
            const isOnline = conv.participants?.some((p) => p.status === 'online') || false;
            const title = conv.title || conv.name || (conv.type === 1 ? 'Hội thoại 1-1' : 'Nhóm chat');
            const isGroup = conv.isGroup ?? (conv.type === 2);
            const timeDisplay = conv.lastMessageAt
              ? (conv.lastMessageAt.includes('T')
                  ? new Date(conv.lastMessageAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
                  : conv.lastMessageAt)
              : '';

            return (
              <div
                key={conv.id}
                onClick={() => onSelectConversation(conv.id)}
                className={`group flex items-start gap-3 p-3 rounded-xl cursor-pointer transition-all duration-200 ease-out border ${
                  isActive
                    ? 'bg-[#0e2744] border-[#1d4ed8]/60 shadow-md translate-x-1'
                    : 'bg-[#0a1b32]/40 border-transparent hover:bg-[#0f2747] hover:border-slate-700/60 hover:translate-x-1'
                }`}
              >
                {/* Avatar with Online indicator */}
                <div className="relative shrink-0 mt-0.5">
                  <div
                    className={`w-11 h-11 rounded-xl flex items-center justify-center font-bold text-xs uppercase shadow-sm transition-transform duration-200 group-hover:scale-105 ${
                      isGroup
                        ? 'bg-[#081d36] border border-[#1d4ed8]/50 text-[#38bdf8]'
                        : 'bg-[#0052cc] text-white'
                    }`}
                  >
                    {isGroup ? (
                      <Users className="w-5 h-5 text-[#38bdf8]" />
                    ) : (
                      title.slice(0, 2)
                    )}
                  </div>
                  {isOnline && (
                    <span className="absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full bg-emerald-500 border-2 border-[#0c2340]" />
                  )}
                </div>

                {/* Conversation Meta */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between mb-1">
                    <div className="flex items-center gap-1.5 min-w-0">
                      {conv.isPinned && (
                        <Pin className="w-3.5 h-3.5 text-[#38bdf8] shrink-0 fill-[#38bdf8]" />
                      )}
                      <h4
                        className={`text-sm truncate font-bold ${
                          isActive ? 'text-white' : 'text-slate-100 group-hover:text-white'
                        }`}
                      >
                        {title}
                      </h4>
                    </div>
                    {timeDisplay && (
                      <span className="text-[11px] text-slate-400 font-medium shrink-0 ml-1">
                        {timeDisplay}
                      </span>
                    )}
                  </div>

                  {conv.department && (
                    <span className="inline-block text-xs text-[#38bdf8] font-medium mb-1 truncate max-w-full">
                      {conv.department}
                    </span>
                  )}

                  <div className="flex items-center justify-between">
                    <p className="text-xs text-slate-300 truncate pr-2 leading-relaxed">
                      {conv.lastMessage || 'Bắt đầu cuộc trò chuyện...'}
                    </p>

                    {(conv.unreadCount || 0) > 0 && (
                      <span className="shrink-0 flex items-center justify-center min-w-5 h-5 px-1.5 rounded-full bg-[#0052cc] text-white text-xs font-bold shadow-sm">
                        {conv.unreadCount}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            );
          })
        )}
      </div>
    </aside>
  );
};
