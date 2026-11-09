import React, { useState } from 'react';
import {
  X,
  Sparkles,
  Users,
  CheckCircle2,
  Clock,
  ShieldCheck,
  CheckSquare,
  Square,
  ArrowLeft,
  RefreshCw,
  Crown,
  UserCheck,
} from 'lucide-react';
import { Avatar } from '@/shared/components/Avatar';
import type {
  Conversation,
  AISummaryReport,
  ConversationMemberDto,
  ConversationSummaryResult,
} from '../types/chat.types';

interface ConversationDetailDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  conversation: Conversation;
  members?: ConversationMemberDto[];
  isLoadingMembers?: boolean;
  summary?: ConversationSummaryResult | null;
  isSummarizing?: boolean;
  onTriggerSummarize?: (count: number) => Promise<any>;
  aiSummary?: AISummaryReport;
  activeMode: 'info' | 'ai-summary';
  onSwitchMode: (mode: 'info' | 'ai-summary') => void;
}

export const ConversationDetailDrawer: React.FC<ConversationDetailDrawerProps> = ({
  isOpen,
  onClose,
  conversation,
  members = [],
  isLoadingMembers = false,
  summary,
  isSummarizing = false,
  onTriggerSummarize,
  aiSummary,
  activeMode,
  onSwitchMode,
}) => {
  const [selectedCount, setSelectedCount] = useState<number>(50);
  const [localCheckedMap, setLocalCheckedMap] = useState<Record<number, boolean>>({});

  if (!isOpen) return null;

  const title = conversation.title || (conversation as any).name || (conversation.type === 1 ? 'Cuộc trò chuyện 1-1' : 'Nhóm chat');
  const isGroup = conversation.isGroup ?? (conversation.type === 2);

  const toggleCheck = (idx: number) => {
    setLocalCheckedMap((prev) => ({ ...prev, [idx]: !prev[idx] }));
  };

  const hasSummary = Boolean(summary || aiSummary);

  return (
    <aside className="w-[340px] shrink-0 flex flex-col bg-[#0c2340] border-l border-[#1d4ed8]/20 h-full overflow-hidden select-none animate-in slide-in-from-right duration-200">
      {/* Top Header */}
      <div className="h-16 px-5 border-b border-[#1d4ed8]/20 flex items-center justify-between bg-[#081d36]/50">
        <div className="flex items-center gap-2">
          {activeMode === 'ai-summary' ? (
            <button
              onClick={() => onSwitchMode('info')}
              className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer mr-1 hover:scale-105 active:scale-95"
              title="Quay lại thông tin nhóm"
            >
              <ArrowLeft className="w-4 h-4" />
            </button>
          ) : null}
          <div className="flex items-center gap-2 font-bold text-base text-white">
            {activeMode === 'ai-summary' ? (
              <>
                <Sparkles className="w-4 h-4 text-[#38bdf8]" />
                <span>Trợ Lý Tri Thức AI</span>
              </>
            ) : (
              <>
                <Users className="w-4 h-4 text-[#38bdf8]" />
                <span>Chi Tiết Hội Thoại</span>
              </>
            )}
          </div>
        </div>

        <button
          onClick={onClose}
          className="w-8 h-8 rounded-xl flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/10 transition-all duration-150 hover:scale-105 active:scale-95 cursor-pointer"
        >
          <X className="w-4 h-4" />
        </button>
      </div>

      {/* Mode Navigation Tabs */}
      <div className="grid grid-cols-2 p-1.5 m-3.5 rounded-xl bg-[#07172b] border border-slate-700/80 text-xs font-semibold">
        <button
          onClick={() => onSwitchMode('info')}
          className={`py-2 rounded-lg text-center transition-all duration-150 cursor-pointer ${
            activeMode === 'info'
              ? 'bg-[#0052cc] text-white shadow-sm'
              : 'text-slate-400 hover:text-white hover:bg-white/5'
          }`}
        >
          Thành Viên & Info
        </button>
        <button
          onClick={() => onSwitchMode('ai-summary')}
          className={`py-2 rounded-lg text-center transition-all duration-150 cursor-pointer flex items-center justify-center gap-1.5 ${
            activeMode === 'ai-summary'
              ? 'bg-[#0052cc] text-white shadow-sm'
              : 'text-[#38bdf8] hover:text-white hover:bg-white/5'
          }`}
        >
          <Sparkles className="w-3.5 h-3.5" />
          <span>Tóm Tắt AI</span>
        </button>
      </div>

      {/* Drawer Content Body */}
      <div className="flex-1 overflow-y-auto p-4 space-y-6">
        {activeMode === 'info' ? (
          <>
            {/* Conversation Overview Card */}
            <div className="p-4 rounded-2xl bg-[#07172b] border border-slate-800 text-center shadow-md">
              <div className="w-14 h-14 rounded-2xl bg-[#0052cc] text-white font-bold text-lg mx-auto flex items-center justify-center mb-3 shadow-md">
                {isGroup ? (
                  <Users className="w-7 h-7 text-white" />
                ) : (
                  title.slice(0, 2)
                )}
              </div>
              <h3 className="text-sm font-bold text-white mb-1">{title}</h3>
              {conversation.department && (
                <span className="inline-block text-xs text-[#38bdf8] font-medium bg-[#0c2340] px-3 py-1 rounded-full border border-[#1d4ed8]/30 mb-2">
                  {conversation.department}
                </span>
              )}
              <div className="flex items-center justify-center gap-1.5 text-xs text-slate-400">
                <ShieldCheck className="w-4 h-4 text-emerald-400" />
                <span>Mã hóa bảo mật Zero-Trust E2E</span>
              </div>
            </div>

            {/* Real Members Section */}
            <div>
              <div className="flex items-center justify-between mb-3">
                <h4 className="text-xs font-bold uppercase tracking-wider text-slate-300">
                  Thành Viên ({members.length > 0 ? members.length : conversation.participants?.length || 0})
                </h4>
                {isLoadingMembers && (
                  <RefreshCw className="w-3 h-3 text-[#38bdf8] animate-spin" />
                )}
              </div>

              <div className="space-y-2">
                {members.length > 0 ? (
                  members.map((m) => {
                    const isOwner = m.memberRole === 1;
                    const displayName = m.nickname || m.fullName || `User ${m.userId.slice(0, 8)}`;

                    return (
                      <div
                        key={m.userId}
                        className="flex items-center justify-between p-2.5 rounded-xl bg-[#07172b]/60 border border-slate-800/80 hover:border-slate-700 hover:bg-[#0e2744] transition-all duration-200"
                      >
                        <div className="flex items-center gap-3 min-w-0">
                          <Avatar
                            name={displayName}
                            src={m.avatarUrl || undefined}
                            size="sm"
                          />
                          <div className="min-w-0">
                            <p className="text-xs font-semibold text-white truncate flex items-center gap-1">
                              <span>{displayName}</span>
                              {isOwner && (
                                <span title="Trưởng nhóm">
                                  <Crown className="w-3 h-3 text-amber-400 shrink-0" />
                                </span>
                              )}
                            </p>
                            <p className="text-[10px] text-slate-400 truncate">
                              Tham gia: {new Date(m.joinedAt).toLocaleDateString()}
                            </p>
                          </div>
                        </div>

                        <span
                          className={`px-2 py-0.5 rounded-md text-[10px] font-semibold ${
                            isOwner
                              ? 'bg-amber-950/60 text-amber-400 border border-amber-500/30'
                              : 'bg-slate-800 text-slate-300'
                          }`}
                        >
                          {isOwner ? 'Quản trị' : 'Thành viên'}
                        </span>
                      </div>
                    );
                  })
                ) : conversation.participants && conversation.participants.length > 0 ? (
                  conversation.participants.map((p) => (
                    <div
                      key={p.id}
                      className="flex items-center justify-between p-2.5 rounded-xl bg-[#07172b]/60 border border-slate-800/80 hover:border-slate-700 hover:bg-[#0e2744] transition-all duration-200"
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <Avatar name={p.fullName} src={p.avatarUrl} size="sm" />
                        <div className="min-w-0">
                          <p className="text-xs font-semibold text-white truncate">{p.fullName}</p>
                          <p className="text-[10px] text-slate-400 truncate">{p.department || p.email}</p>
                        </div>
                      </div>
                      <span className="px-2 py-0.5 rounded-md text-[10px] bg-slate-800 text-slate-300">
                        {p.role}
                      </span>
                    </div>
                  ))
                ) : (
                  <div className="text-center py-6 text-xs text-slate-400">
                    Đang nạp danh sách thành viên...
                  </div>
                )}
              </div>
            </div>
          </>
        ) : (
          /* AI Copilot Knowledge Summarizer */
          <div className="space-y-4">
            {/* AI Control Trigger Box */}
            <div className="p-4 rounded-2xl bg-gradient-to-br from-[#0c2340] to-[#07172b] border border-[#1d4ed8]/40 shadow-lg">
              <div className="flex items-center gap-2 text-xs font-bold text-[#38bdf8] mb-2">
                <Sparkles className="w-4 h-4 text-[#38bdf8]" />
                <span>Trợ Lý Tri Thức Doanh Nghiệp</span>
              </div>
              <p className="text-xs text-slate-300 mb-3 leading-relaxed">
                Quét toàn bộ chuỗi tin nhắn để tự động trích xuất các ý chính, quyết định và danh sách công việc.
              </p>

              {/* Message Count Selector */}
              <div className="flex items-center gap-1.5 mb-3">
                <span className="text-[11px] text-slate-400 mr-1">Số tin nhắn:</span>
                {[30, 50, 100].map((count) => (
                  <button
                    key={count}
                    type="button"
                    onClick={() => setSelectedCount(count)}
                    disabled={isSummarizing}
                    className={`px-2.5 py-1 rounded-lg text-xs font-semibold transition-all cursor-pointer ${
                      selectedCount === count
                        ? 'bg-[#0052cc] text-white shadow-sm'
                        : 'bg-[#07172b] text-slate-400 hover:text-white border border-slate-700/60'
                    }`}
                  >
                    {count} tin
                  </button>
                ))}
              </div>

              {/* Action Button */}
              <button
                type="button"
                onClick={() => onTriggerSummarize && onTriggerSummarize(selectedCount)}
                disabled={isSummarizing}
                className="w-full py-2.5 px-4 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:scale-95 text-white font-bold text-xs shadow-md transition-all hover:scale-[1.02] flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50"
              >
                {isSummarizing ? (
                  <>
                    <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                    <span>AI Copilot đang phân tích...</span>
                  </>
                ) : (
                  <>
                    <Sparkles className="w-3.5 h-3.5" />
                    <span>{hasSummary ? 'Cập Nhật Tóm Tắt AI' : 'Tạo Tóm Tắt Với AI'}</span>
                  </>
                )}
              </button>
            </div>

            {/* AI Result Blocks */}
            {summary ? (
              <div className="space-y-4 animate-in fade-in">
                {/* 1. Overview Block */}
                <div className="p-4 rounded-2xl bg-[#07172b] border border-[#1d4ed8]/50 shadow-md">
                  <h4 className="text-xs font-bold text-white uppercase tracking-wider mb-2 flex items-center gap-1.5">
                    <UserCheck className="w-4 h-4 text-[#38bdf8]" />
                    <span>Tóm Tắt Tổng Quan</span>
                  </h4>
                  <p className="text-xs text-slate-200 leading-relaxed">{summary.summary}</p>
                </div>

                {/* 2. Key Points */}
                {summary.keyPoints && summary.keyPoints.length > 0 && (
                  <div>
                    <h4 className="text-xs font-bold uppercase tracking-wider text-slate-200 mb-2 flex items-center gap-1.5">
                      <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400" />
                      <span>Điểm Cốt Lõi ({summary.keyPoints.length})</span>
                    </h4>
                    <div className="space-y-1.5">
                      {summary.keyPoints.map((point, idx) => (
                        <div
                          key={idx}
                          className="p-2.5 rounded-xl bg-[#07172b]/70 border border-slate-800 text-xs text-slate-200 leading-relaxed flex items-start gap-2.5"
                        >
                          <span className="w-4 h-4 rounded-full bg-[#0052cc]/30 text-[#38bdf8] flex items-center justify-center font-mono font-bold text-[10px] shrink-0 mt-0.5">
                            {idx + 1}
                          </span>
                          <span>{point}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* 3. Decisions */}
                {summary.decisions && summary.decisions.length > 0 && (
                  <div>
                    <h4 className="text-xs font-bold uppercase tracking-wider text-slate-200 mb-2 flex items-center gap-1.5">
                      <ShieldCheck className="w-3.5 h-3.5 text-amber-400" />
                      <span>Quyết Định Đã Chốt ({summary.decisions.length})</span>
                    </h4>
                    <div className="space-y-1.5">
                      {summary.decisions.map((dec, idx) => (
                        <div
                          key={idx}
                          className="p-2.5 rounded-xl bg-[#07172b]/70 border border-slate-800 text-xs text-slate-200 leading-relaxed flex items-start gap-2.5"
                        >
                          <span className="text-amber-400 font-bold">•</span>
                          <span>{dec}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* 4. Action Items */}
                {summary.actionItems && summary.actionItems.length > 0 && (
                  <div>
                    <h4 className="text-xs font-bold uppercase tracking-wider text-slate-200 mb-2 flex items-center gap-1.5">
                      <Clock className="w-3.5 h-3.5 text-[#38bdf8]" />
                      <span>Đầu Việc Cần Làm (Action Items)</span>
                    </h4>
                    <div className="space-y-1.5">
                      {summary.actionItems.map((act, idx) => {
                        const isChecked = Boolean(localCheckedMap[idx] ?? act.isCompleted);

                        return (
                          <div
                            key={idx}
                            onClick={() => toggleCheck(idx)}
                            className={`p-2.5 rounded-xl border transition-all duration-150 cursor-pointer flex items-start gap-2.5 ${
                              isChecked
                                ? 'bg-[#07172b]/40 border-slate-800 opacity-60'
                                : 'bg-[#07172b] border-[#1d4ed8]/30 hover:bg-[#0e2744]'
                            }`}
                          >
                            <div className="mt-0.5 text-[#38bdf8] shrink-0">
                              {isChecked ? (
                                <CheckSquare className="w-4 h-4 text-emerald-400" />
                              ) : (
                                <Square className="w-4 h-4 text-slate-500" />
                              )}
                            </div>
                            <div className="flex-1 min-w-0">
                              <p
                                className={`text-xs font-medium ${
                                  isChecked ? 'line-through text-slate-500' : 'text-white'
                                }`}
                              >
                                {act.description}
                              </p>
                              {(act.assignee || act.dueDate) && (
                                <div className="flex items-center gap-2 mt-1 text-[10px] text-slate-400">
                                  {act.assignee && (
                                    <span className="text-[#38bdf8]">{act.assignee}</span>
                                  )}
                                  {act.dueDate && <span>Hạn: {act.dueDate}</span>}
                                </div>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                )}
              </div>
            ) : aiSummary ? (
              /* Fallback to aiSummary mock if present */
              <div className="space-y-4">
                <div className="p-4 rounded-2xl bg-[#07172b] border border-[#1d4ed8]/50 shadow-md">
                  <h4 className="text-xs font-bold text-white uppercase tracking-wider mb-2">
                    Tóm Tắt Tổng Quan
                  </h4>
                  <p className="text-xs text-slate-200 leading-relaxed">{aiSummary.overview}</p>
                </div>
              </div>
            ) : (
              <div className="text-center py-10 text-slate-400 text-xs">
                Chưa có dữ liệu tóm tắt AI. Bấm nút phía trên để AI quét chuỗi tin nhắn.
              </div>
            )}
          </div>
        )}
      </div>
    </aside>
  );
};
