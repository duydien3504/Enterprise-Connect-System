import React, { useState } from 'react';
import {
  X,
  Sparkles,
  Users,
  FileText,
  Download,
  CheckCircle2,
  Clock,
  AlertCircle,
  FileDown,
  ShieldCheck,
  CheckSquare,
  Square,
  ArrowLeft,
} from 'lucide-react';
import type { Conversation, AISummaryReport, ActionItem } from '../types/chat.types';

interface ConversationDetailDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  conversation: Conversation;
  aiSummary?: AISummaryReport;
  activeMode: 'info' | 'ai-summary';
  onSwitchMode: (mode: 'info' | 'ai-summary') => void;
}

export const ConversationDetailDrawer: React.FC<ConversationDetailDrawerProps> = ({
  isOpen,
  onClose,
  conversation,
  aiSummary,
  activeMode,
  onSwitchMode,
}) => {
  const [actionItems, setActionItems] = useState<ActionItem[]>(aiSummary?.actionItems || []);

  if (!isOpen) return null;

  const toggleActionItem = (id: string) => {
    setActionItems((prev) =>
      prev.map((item) => (item.id === id ? { ...item, completed: !item.completed } : item))
    );
  };

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
                <span>Chi Tiết Nhóm</span>
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
          Thành Viên & File
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
                {conversation.isGroup ? (
                  <Users className="w-7 h-7 text-white" />
                ) : (
                  conversation.title.slice(0, 2)
                )}
              </div>
              <h3 className="text-sm font-bold text-white mb-1">{conversation.title}</h3>
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

            {/* Participants Section */}
            <div>
              <div className="flex items-center justify-between mb-3">
                <h4 className="text-xs font-bold uppercase tracking-wider text-slate-300">
                  Thành Viên ({conversation.participants.length})
                </h4>
              </div>

              <div className="space-y-2">
                {conversation.participants.map((p) => (
                  <div
                    key={p.id}
                    className="flex items-center justify-between p-2.5 rounded-xl bg-[#07172b]/60 border border-slate-800/80 hover:border-slate-700 hover:bg-[#0e2744] hover:translate-x-0.5 transition-all duration-200"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <div className="relative">
                        <div className="w-9 h-9 rounded-full bg-[#0052cc] text-white flex items-center justify-center text-xs font-bold uppercase shadow-sm">
                          {p.fullName.slice(0, 2)}
                        </div>
                        <span
                          className={`absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full border border-[#0c2340] ${
                            p.status === 'online'
                              ? 'bg-emerald-500'
                              : p.status === 'busy'
                              ? 'bg-rose-500'
                              : p.status === 'away'
                              ? 'bg-amber-500'
                              : 'bg-slate-500'
                          }`}
                        />
                      </div>
                      <div className="min-w-0">
                        <div className="text-sm font-semibold text-white truncate">{p.fullName}</div>
                        <div className="text-xs text-slate-400 truncate">{p.email}</div>
                      </div>
                    </div>

                    <span
                      className={`text-xs px-2.5 py-0.5 rounded-full font-medium shrink-0 ml-2 ${
                        p.role === 'owner'
                          ? 'bg-[#0052cc] text-white'
                          : p.role === 'admin'
                          ? 'bg-[#1d4ed8]/40 text-[#38bdf8] border border-[#1d4ed8]/60'
                          : 'bg-slate-800 text-slate-300'
                      }`}
                    >
                      {p.role === 'owner' ? 'Trưởng nhóm' : p.role === 'admin' ? 'Quản trị' : 'Thành viên'}
                    </span>
                  </div>
                ))}
              </div>
            </div>

            {/* Shared Files & Media */}
            <div>
              <h4 className="text-xs font-bold uppercase tracking-wider text-slate-300 mb-3">
                Tài Liệu & Tệp Đính Kèm
              </h4>

              <div className="space-y-2">
                {[
                  { name: 'ZeroTrust_Token_Policy_v2.pdf', size: '2.4 MB', date: 'Hôm nay' },
                  { name: 'Network_Topology_Architecture.png', size: '1.8 MB', date: 'Hôm nay' },
                  { name: 'Bien_Ban_Hop_Chien_Luoc_Q4.docx', size: '540 KB', date: 'Thứ 6' },
                ].map((f, i) => (
                  <div
                    key={i}
                    className="flex items-center justify-between p-3 rounded-xl bg-[#07172b]/60 border border-slate-800 hover:border-slate-700 hover:bg-[#0e2744] hover:translate-x-0.5 transition-all duration-200"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <div className="w-9 h-9 rounded-xl bg-[#0c2340] border border-[#1d4ed8]/40 flex items-center justify-center text-[#38bdf8] shrink-0">
                        <FileText className="w-4 h-4" />
                      </div>
                      <div className="min-w-0">
                        <div className="text-xs font-semibold text-slate-200 truncate">{f.name}</div>
                        <div className="text-xs text-slate-400">
                          {f.size} • {f.date}
                        </div>
                      </div>
                    </div>

                    <button
                      title="Tải về tệp"
                      className="p-2 rounded-xl text-slate-400 hover:text-white hover:bg-[#0052cc] transition-all duration-150 hover:scale-105 active:scale-95 cursor-pointer"
                    >
                      <Download className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>
            </div>
          </>
        ) : (
          /* AI Summary Report View - 14px Readable Text & Smooth Micro-interactions */
          <>
            {aiSummary ? (
              <div className="space-y-5">
                {/* AI Executive Overview */}
                <div className="p-4 rounded-2xl bg-[#07172b] border border-[#1d4ed8]/50 shadow-md">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-xs font-mono text-[#38bdf8]">
                      Cập nhật: {aiSummary.generatedAt}
                    </span>
                    <span className="px-2.5 py-0.5 rounded-full text-xs font-bold bg-[#0052cc] text-white">
                      AI Generated
                    </span>
                  </div>
                  <h4 className="text-xs font-bold text-white uppercase tracking-wider mb-2">
                    Tóm Tắt Điều Hành
                  </h4>
                  <p className="text-sm text-slate-200 leading-relaxed">
                    {aiSummary.overview}
                  </p>
                </div>

                {/* Key Decisions */}
                <div>
                  <h4 className="text-xs font-bold uppercase tracking-wider text-slate-200 mb-2.5 flex items-center gap-1.5">
                    <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                    <span>Quyết Định Chính ({aiSummary.keyDecisions.length})</span>
                  </h4>
                  <div className="space-y-2">
                    {aiSummary.keyDecisions.map((dec, idx) => (
                      <div
                        key={idx}
                        className="p-3.5 rounded-xl bg-[#07172b]/70 border border-slate-800 text-sm text-slate-200 leading-relaxed flex items-start gap-3 hover:border-slate-700 hover:bg-[#0e2744] transition-all duration-200"
                      >
                        <span className="w-5 h-5 rounded-full bg-[#0052cc]/30 text-[#38bdf8] flex items-center justify-center font-mono font-bold text-xs shrink-0 mt-0.5">
                          {idx + 1}
                        </span>
                        <span>{dec}</span>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Action Items with Interactive Checkboxes */}
                <div>
                  <h4 className="text-xs font-bold uppercase tracking-wider text-slate-200 mb-2.5 flex items-center gap-1.5">
                    <Clock className="w-4 h-4 text-[#38bdf8]" />
                    <span>Đầu Việc Cần Làm (Action Items)</span>
                  </h4>
                  <div className="space-y-2">
                    {actionItems.map((act) => (
                      <div
                        key={act.id}
                        onClick={() => toggleActionItem(act.id)}
                        className={`p-3.5 rounded-xl border transition-all duration-200 ease-out cursor-pointer flex items-start gap-3 ${
                          act.completed
                            ? 'bg-[#07172b]/40 border-slate-800/60 opacity-60'
                            : 'bg-[#07172b] border-[#1d4ed8]/30 hover:border-[#38bdf8]/60 hover:bg-[#0e2744] hover:translate-x-1 shadow-sm'
                        }`}
                      >
                        <div className="mt-0.5 text-[#38bdf8] shrink-0 transition-transform duration-150 hover:scale-110">
                          {act.completed ? (
                            <CheckSquare className="w-4 h-4 text-emerald-400" />
                          ) : (
                            <Square className="w-4 h-4 text-slate-500" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p
                            className={`text-sm font-medium leading-snug ${
                              act.completed ? 'line-through text-slate-500' : 'text-white'
                            }`}
                          >
                            {act.title}
                          </p>
                          <div className="flex items-center gap-3 mt-2 text-xs text-slate-400">
                            <span className="text-[#38bdf8] font-semibold">{act.assignee}</span>
                            <span>Hạn: {act.dueDate}</span>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Deadlines Section */}
                <div>
                  <h4 className="text-xs font-bold uppercase tracking-wider text-slate-200 mb-2.5 flex items-center gap-1.5">
                    <AlertCircle className="w-4 h-4 text-amber-400" />
                    <span>Thời Hạn Chót (Deadlines)</span>
                  </h4>
                  <div className="space-y-2">
                    {aiSummary.deadlines.map((dl, idx) => (
                      <div
                        key={idx}
                        className="flex items-center justify-between p-3 rounded-xl bg-[#07172b] border border-slate-800 hover:border-slate-700 transition-all duration-200"
                      >
                        <span className="text-sm text-slate-200 font-medium truncate pr-2">
                          {dl.task}
                        </span>
                        <div className="flex items-center gap-2.5 shrink-0">
                          <span className="text-xs font-mono text-slate-300">{dl.date}</span>
                          <span
                            className={`px-2.5 py-0.5 rounded-full text-xs font-bold ${
                              dl.priority === 'high'
                                ? 'bg-rose-950 text-rose-300 border border-rose-500/40'
                                : 'bg-amber-950 text-amber-300 border border-amber-500/40'
                            }`}
                          >
                            {dl.priority === 'high' ? 'Gấp' : 'Trung bình'}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Export Report Action */}
                <div className="pt-2">
                  <button
                    type="button"
                    onClick={() => alert('Đã trích xuất biên bản họp AI tóm tắt thành công!')}
                    className="w-full py-3 px-4 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:scale-95 text-white font-bold text-xs shadow-md transition-all duration-200 hover:scale-[1.02] flex items-center justify-center gap-2 cursor-pointer"
                  >
                    <FileDown className="w-4 h-4" />
                    <span>Xuất Biên Bản Họp (PDF)</span>
                  </button>
                </div>
              </div>
            ) : (
              <div className="text-center py-12 text-slate-400 text-xs">
                Chưa có dữ liệu tóm tắt AI cho cuộc trò chuyện này.
              </div>
            )}
          </>
        )}
      </div>
    </aside>
  );
};
