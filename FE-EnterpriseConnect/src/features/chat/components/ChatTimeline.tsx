import React, { useState, useRef, useEffect } from 'react';
import {
  Phone,
  Video,
  Pin,
  Sparkles,
  Paperclip,
  Smile,
  Send,
  CornerDownRight,
  ShieldAlert,
  FileText,
  FileImage,
  Download,
  Info,
  X,
} from 'lucide-react';
import { Avatar } from '@/shared/components/Avatar';
import type { Conversation, Message, MessageAttachment } from '../types/chat.types';

interface ChatTimelineProps {
  conversation: Conversation;
  messages: Message[];
  isDrawerOpen: boolean;
  onToggleDrawer: () => void;
  onOpenAISummary: () => void;
  onSendMessage: (content: string, replyToId?: string, attachments?: MessageAttachment[]) => void;
  onAddReaction: (messageId: string, emoji: string) => void;
  onPinMessage: (messageId: string) => void;
}

const QUICK_EMOJIS = ['👍', '❤️', '😂', '😮', '🎉'];

export const ChatTimeline: React.FC<ChatTimelineProps> = ({
  conversation,
  messages,
  isDrawerOpen,
  onToggleDrawer,
  onOpenAISummary,
  onSendMessage,
  onAddReaction,
  onPinMessage,
}) => {
  const [inputText, setInputText] = useState('');
  const [replyingTo, setReplyingTo] = useState<Message | null>(null);
  const [showEmojiBar, setShowEmojiBar] = useState(false);
  const [pendingAttachments, setPendingAttachments] = useState<MessageAttachment[]>([]);
  const [showPinnedBanner, setShowPinnedBanner] = useState(true);
  const [isTypingSimulated, setIsTypingSimulated] = useState(false);

  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Auto scroll to latest message
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isTypingSimulated]);

  // Simulate typing indicator occasionally for realistic enterprise demo
  useEffect(() => {
    const timer = setTimeout(() => {
      setIsTypingSimulated(true);
      setTimeout(() => setIsTypingSimulated(false), 4000);
    }, 8000);
    return () => clearTimeout(timer);
  }, [conversation.id]);

  const pinnedMessage = messages.find((m) => m.isPinned);

  const handleSend = (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!inputText.trim() && pendingAttachments.length === 0) return;

    onSendMessage(
      inputText.trim(),
      replyingTo?.id,
      pendingAttachments.length > 0 ? pendingAttachments : undefined
    );

    setInputText('');
    setReplyingTo(null);
    setPendingAttachments([]);
    setShowEmojiBar(false);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleFileAttach = () => {
    const mockFiles: MessageAttachment[] = [
      {
        id: `att-${Date.now()}`,
        name: 'Enterprise_Security_Report_v2.pdf',
        size: '1.2 MB',
        type: 'pdf',
      },
    ];
    setPendingAttachments((prev) => [...prev, ...mockFiles]);
  };

  return (
    <div className="flex-1 flex flex-col bg-[#071e3d] h-full overflow-hidden select-none">
      {/* 1. Header Khung Chat */}
      <header className="h-16 px-6 bg-[#0c2340] border-b border-[#1d4ed8]/20 flex items-center justify-between shrink-0 shadow-sm">
        <div className="flex items-center gap-3.5 min-w-0">
          <Avatar
            name={conversation.title}
            src={conversation.avatarUrl}
            size="md"
            status="online"
          />
          <div className="min-w-0">
            <div className="flex items-center gap-2">
              <h3 className="text-base font-bold text-white truncate">{conversation.title}</h3>
              {conversation.department && (
                <span className="hidden sm:inline-block text-xs font-semibold text-[#38bdf8] bg-[#07172b] px-2.5 py-0.5 rounded-full border border-slate-700/80">
                  {conversation.department}
                </span>
              )}
            </div>
            <div className="flex items-center gap-2 text-xs text-slate-300">
              <span className="flex items-center gap-1.5 text-emerald-400 font-medium">
                <span className="w-2 h-2 rounded-full bg-emerald-400 inline-block animate-pulse" />
                {conversation.isGroup
                  ? `${conversation.memberCount || conversation.participants?.length || 2} Thành viên`
                  : 'Đang hoạt động'}
              </span>
              <span className="text-slate-500">•</span>
              <span className="text-slate-400">Mã hóa Zero-Trust</span>
            </div>
          </div>
        </div>

        {/* Action Buttons in Header with Hover Micro-interactions */}
        <div className="flex items-center gap-2.5">
          {/* AI Copilot Summarize Button */}
          <button
            onClick={onOpenAISummary}
            title="Kích hoạt Trợ lý Tri thức AI tóm tắt cuộc thảo luận"
            className="flex items-center gap-2 px-3.5 py-2 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:scale-95 text-white text-xs font-bold shadow-md transition-all duration-200 hover:scale-105 cursor-pointer"
          >
            <Sparkles className="w-4 h-4 text-[#38bdf8]" />
            <span className="hidden md:inline">Tóm Tắt AI</span>
          </button>

          {/* Voice Call Button */}
          <button
            onClick={() => alert('Đang kích hoạt WebRTC Audio Call với phòng ban...')}
            title="Bắt đầu cuộc gọi thoại WebRTC"
            className="w-10 h-10 rounded-xl bg-[#07172b] hover:bg-[#0f2747] text-slate-300 hover:text-white border border-slate-700/80 flex items-center justify-center transition-all duration-200 hover:scale-105 active:scale-95 cursor-pointer shadow-sm"
          >
            <Phone className="w-4 h-4 text-emerald-400" />
          </button>

          {/* Video Call Button */}
          <button
            onClick={() => alert('Đang khởi tạo WebRTC Video Conference mã hóa E2E...')}
            title="Bắt đầu cuộc gọi video"
            className="w-10 h-10 rounded-xl bg-[#07172b] hover:bg-[#0f2747] text-slate-300 hover:text-white border border-slate-700/80 flex items-center justify-center transition-all duration-200 hover:scale-105 active:scale-95 cursor-pointer shadow-sm"
          >
            <Video className="w-4 h-4 text-sky-400" />
          </button>

          {/* Drawer Toggle Button */}
          <button
            onClick={onToggleDrawer}
            title={isDrawerOpen ? 'Đóng bảng chi tiết' : 'Mở chi tiết nhóm & file'}
            className={`w-10 h-10 rounded-xl border flex items-center justify-center transition-all duration-200 hover:scale-105 active:scale-95 cursor-pointer shadow-sm ${
              isDrawerOpen
                ? 'bg-[#0052cc] text-white border-[#0052cc]'
                : 'bg-[#07172b] text-slate-300 hover:text-white border-slate-700/80 hover:bg-[#0f2747]'
            }`}
          >
            <Info className="w-4 h-4" />
          </button>
        </div>
      </header>

      {/* 2. Dải Thông Báo Tin Nhắn Ghim */}
      {pinnedMessage && showPinnedBanner && (
        <div className="px-6 py-2.5 bg-[#0c2340] border-b border-[#38bdf8]/30 flex items-center justify-between text-xs text-slate-200 shadow-sm animate-in fade-in duration-150">
          <div className="flex items-center gap-2.5 min-w-0">
            <Pin className="w-4 h-4 text-[#38bdf8] fill-[#38bdf8] shrink-0" />
            <div className="flex items-center gap-1.5 min-w-0">
              <span className="font-bold text-[#38bdf8] shrink-0">Tin ghim:</span>
              <span className="font-semibold text-white shrink-0">{pinnedMessage.senderName}:</span>
              <span className="truncate text-slate-300">{pinnedMessage.content}</span>
            </div>
          </div>
          <button
            onClick={() => setShowPinnedBanner(false)}
            className="text-slate-400 hover:text-white transition-colors p-1 hover:bg-white/10 rounded-lg ml-2"
          >
            <X className="w-3.5 h-3.5" />
          </button>
        </div>
      )}

      {/* 3. Khu Vực Dòng Thời Gian Tin Nhắn (Timeline) */}
      <div className="flex-1 overflow-y-auto p-5 sm:p-6 space-y-5">
        {messages.map((msg) => {
          const isSelf = msg.isSelf;

          // Toxic Message Warning Block
          if (msg.isToxic) {
            return (
              <div key={msg.id} className="flex justify-center my-3 animate-in fade-in zoom-in-95 duration-150">
                <div className="max-w-xl w-full p-4 rounded-2xl bg-[#3d1217] border border-rose-500/60 shadow-lg text-rose-200 text-xs">
                  <div className="flex items-start gap-3">
                    <ShieldAlert className="w-5 h-5 text-rose-400 shrink-0 mt-0.5" />
                    <div>
                      <div className="font-bold text-rose-300 text-sm mb-1">
                        Cảnh báo an ninh mạng (Toxic Content Filter)
                      </div>
                      <p className="text-xs leading-relaxed text-rose-200">
                        {msg.toxicWarning || 'Nội dung tin nhắn đã bị chặn do vi phạm tiêu chuẩn bảo mật nội bộ.'}
                      </p>
                      <div className="mt-2 text-xs text-rose-400 font-mono">
                        Người gửi: {msg.senderName} • {msg.createdAt}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            );
          }

          return (
            <div
              key={msg.id}
              className={`group relative flex flex-col ${isSelf ? 'items-end' : 'items-start'}`}
            >
              {/* Meta Sender Name & Time */}
              <div className="flex items-baseline gap-2 mb-1 px-1">
                <span className="text-xs font-semibold text-slate-200">{msg.senderName}</span>
                <span className="text-xs text-slate-400 font-medium">{msg.createdAt}</span>
                {msg.isPinned && (
                  <span className="flex items-center gap-1 text-xs text-[#38bdf8] font-semibold">
                    <Pin className="w-3 h-3 fill-[#38bdf8]" />
                    Đã ghim
                  </span>
                )}
              </div>

              {/* Message Bubble - 14px Text (text-sm), Dịu lại, bo góc mượt mà */}
              <div className="relative max-w-[75%] sm:max-w-[70%]">
                <div
                  className={`rounded-2xl p-4 text-sm leading-relaxed shadow-md transition-all duration-200 hover:shadow-lg ${
                    isSelf
                      ? 'bg-[#0052cc] text-white rounded-br-none'
                      : 'bg-[#0c2340] border border-[#1d4ed8]/30 hover:border-[#1d4ed8]/60 text-slate-100 rounded-bl-none'
                  }`}
                >
                  {/* Reply Quote Banner */}
                  {msg.replyTo && (
                    <div
                      className={`mb-2.5 p-2.5 rounded-xl border-l-3 text-xs ${
                        isSelf
                          ? 'bg-[#003da1] border-white text-slate-200'
                          : 'bg-[#07172b] border-[#38bdf8] text-slate-300'
                      }`}
                    >
                      <div className="font-bold text-[#38bdf8] flex items-center gap-1 mb-0.5">
                        <CornerDownRight className="w-3.5 h-3.5" />
                        <span>Trích dẫn {msg.replyTo.senderName}:</span>
                      </div>
                      <p className="truncate opacity-90">{msg.replyTo.content}</p>
                    </div>
                  )}

                  {/* Main Text Content */}
                  <div className="whitespace-pre-wrap">{msg.content}</div>

                  {/* Attachment Cards */}
                  {msg.attachments && msg.attachments.length > 0 && (
                    <div className="mt-3 space-y-2">
                      {msg.attachments.map((att) => (
                        <div
                          key={att.id}
                          className={`flex items-center justify-between p-2.5 rounded-xl transition-all duration-200 ${
                            isSelf
                              ? 'bg-[#0041a8] border border-blue-400/30 hover:bg-[#003994]'
                              : 'bg-[#07172b] border border-slate-700/80 hover:border-slate-600'
                          }`}
                        >
                          <div className="flex items-center gap-2.5 min-w-0">
                            {att.type === 'pdf' ? (
                              <FileText className="w-5 h-5 text-rose-400 shrink-0" />
                            ) : (
                              <FileImage className="w-5 h-5 text-emerald-400 shrink-0" />
                            )}
                            <div className="min-w-0">
                              <div className="text-xs font-semibold truncate text-white">{att.name}</div>
                              <div className="text-xs text-slate-400">{att.size}</div>
                            </div>
                          </div>
                          <button
                            title="Tải về"
                            className="p-1.5 rounded-lg hover:bg-white/10 transition-colors ml-2 hover:scale-110 active:scale-95"
                          >
                            <Download className="w-4 h-4 text-[#38bdf8]" />
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                {/* Floating Hover Action Bar - Smooth Fade & Scale Micro-interaction */}
                <div
                  className={`absolute -top-4 opacity-0 group-hover:opacity-100 transition-all duration-200 ease-out transform group-hover:scale-100 translate-y-1 group-hover:translate-y-0 flex items-center gap-1 p-1 rounded-full bg-[#0c2340] border border-[#1d4ed8]/50 shadow-xl z-10 ${
                    isSelf ? 'right-2' : 'left-2'
                  }`}
                >
                  {QUICK_EMOJIS.map((emoji) => (
                    <button
                      key={emoji}
                      onClick={() => onAddReaction(msg.id, emoji)}
                      className="w-7 h-7 flex items-center justify-center rounded-full hover:bg-white/20 text-sm transition-transform duration-150 hover:scale-125 active:scale-90 cursor-pointer"
                    >
                      {emoji}
                    </button>
                  ))}
                  <div className="w-[1px] h-3.5 bg-slate-700 mx-0.5" />
                  <button
                    onClick={() => setReplyingTo(msg)}
                    title="Trả lời trích dẫn"
                    className="w-7 h-7 flex items-center justify-center rounded-full hover:bg-white/20 text-slate-300 hover:text-white transition-all duration-150 hover:scale-110 active:scale-90 cursor-pointer"
                  >
                    <CornerDownRight className="w-3.5 h-3.5" />
                  </button>
                  <button
                    onClick={() => onPinMessage(msg.id)}
                    title={msg.isPinned ? 'Bỏ ghim' : 'Ghim tin nhắn'}
                    className="w-7 h-7 flex items-center justify-center rounded-full hover:bg-white/20 text-slate-300 hover:text-[#38bdf8] transition-all duration-150 hover:scale-110 active:scale-90 cursor-pointer"
                  >
                    <Pin className={`w-3.5 h-3.5 ${msg.isPinned ? 'fill-[#38bdf8] text-[#38bdf8]' : ''}`} />
                  </button>
                </div>

                {/* Reactions Pill List */}
                {msg.reactions && msg.reactions.length > 0 && (
                  <div
                    className={`flex flex-wrap gap-1.5 mt-2 ${
                      isSelf ? 'justify-end' : 'justify-start'
                    }`}
                  >
                    {msg.reactions.map((react, i) => (
                      <button
                        key={i}
                        onClick={() => onAddReaction(msg.id, react.emoji)}
                        title={`Phản hồi bởi: ${react.users.join(', ')}`}
                        className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs bg-[#0c2340] border border-[#1d4ed8]/40 hover:border-[#38bdf8] text-slate-200 transition-all duration-150 hover:scale-105 active:scale-95 cursor-pointer shadow-sm"
                      >
                        <span className="text-sm">{react.emoji}</span>
                        <span className="font-mono font-bold text-xs">{react.count}</span>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </div>
          );
        })}

        {/* 4. Typing Indicator */}
        {isTypingSimulated && (
          <div className="flex items-center gap-2 text-xs text-slate-300 animate-pulse py-1">
            <span className="w-2 h-2 rounded-full bg-[#38bdf8]" />
            <span>Lê Thị Thảo (DevSecOps) đang soạn tin...</span>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* 5. Khung Nhập Tin Nhắn Chân Trang (Bo tròn 16px, Focus mượt mà) */}
      <div className="p-4 bg-[#071e3d] border-t border-[#1d4ed8]/20 shrink-0">
        <div className="max-w-4xl mx-auto rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/40 focus-within:border-[#38bdf8] focus-within:shadow-[0_0_20px_rgba(56,189,248,0.15)] p-3 shadow-xl transition-all duration-200">
          {/* Active Reply Banner */}
          {replyingTo && (
            <div className="mb-2.5 p-2.5 rounded-xl bg-[#07172b] border border-slate-700/80 flex items-center justify-between text-xs animate-in fade-in duration-150">
              <div className="flex items-center gap-2 min-w-0 text-slate-300">
                <CornerDownRight className="w-4 h-4 text-[#38bdf8] shrink-0" />
                <span className="font-bold text-white shrink-0">
                  Trả lời {replyingTo.senderName}:
                </span>
                <span className="truncate">{replyingTo.content}</span>
              </div>
              <button
                onClick={() => setReplyingTo(null)}
                className="text-slate-400 hover:text-white p-1 hover:bg-white/10 rounded-lg transition-colors"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          )}

          {/* Pending Attachments preview */}
          {pendingAttachments.length > 0 && (
            <div className="mb-2.5 flex flex-wrap gap-2">
              {pendingAttachments.map((att) => (
                <div
                  key={att.id}
                  className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-[#07172b] border border-slate-700 text-xs text-white"
                >
                  <FileText className="w-4 h-4 text-[#38bdf8]" />
                  <span className="truncate max-w-[180px] font-medium">{att.name}</span>
                  <button
                    onClick={() =>
                      setPendingAttachments((prev) => prev.filter((a) => a.id !== att.id))
                    }
                    className="text-slate-400 hover:text-rose-400 transition-colors p-0.5"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                </div>
              ))}
            </div>
          )}

          {/* Text Input Row */}
          <div className="flex items-center gap-2.5">
            {/* Attach button */}
            <button
              type="button"
              onClick={handleFileAttach}
              title="Đính kèm tài liệu / hình ảnh"
              className="p-2.5 rounded-xl text-slate-400 hover:text-white hover:bg-white/10 transition-all duration-150 hover:scale-105 active:scale-95 cursor-pointer"
            >
              <Paperclip className="w-4 h-4" />
            </button>

            {/* Emoji picker toggle button */}
            <div className="relative">
              <button
                type="button"
                onClick={() => setShowEmojiBar(!showEmojiBar)}
                title="Chọn biểu tượng cảm xúc"
                className="p-2.5 rounded-xl text-slate-400 hover:text-white hover:bg-white/10 transition-all duration-150 hover:scale-105 active:scale-95 cursor-pointer"
              >
                <Smile className="w-4 h-4" />
              </button>

              {/* Emoji quick popup */}
              {showEmojiBar && (
                <div className="absolute bottom-12 left-0 p-2 rounded-2xl bg-[#07172b] border border-slate-700 shadow-2xl flex items-center gap-1.5 z-20 animate-in fade-in zoom-in-95 duration-150">
                  {['👍', '❤️', '😂', '😮', '🎉', '🚀', '🔥', '👏'].map((em) => (
                    <button
                      key={em}
                      type="button"
                      onClick={() => {
                        setInputText((prev) => prev + em);
                        setShowEmojiBar(false);
                      }}
                      className="w-8 h-8 flex items-center justify-center rounded-xl hover:bg-white/20 text-base transition-transform duration-150 hover:scale-125 active:scale-90 cursor-pointer"
                    >
                      {em}
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Auto-expanding Input Area - Comfortable 14px text */}
            <textarea
              rows={1}
              value={inputText}
              onChange={(e) => setInputText(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="Nhập tin nhắn nội bộ bảo mật... (Enter gửi, Shift+Enter xuống dòng)"
              className="flex-1 bg-transparent border-0 text-white placeholder-slate-400 text-sm focus:outline-none resize-none max-h-32 py-2 px-1 leading-relaxed"
            />

            {/* Send Button */}
            <button
              type="button"
              onClick={() => handleSend()}
              disabled={!inputText.trim() && pendingAttachments.length === 0}
              className="w-10 h-10 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:scale-95 disabled:opacity-40 text-white flex items-center justify-center shadow-md transition-all duration-200 hover:scale-105 cursor-pointer shrink-0"
              title="Gửi tin nhắn (Enter)"
            >
              <Send className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
