import React from 'react';
import { ShieldAlert, ShieldCheck, RefreshCw, X } from 'lucide-react';

interface BlockConfirmModalProps {
  isOpen: boolean;
  type: 'block' | 'unblock';
  targetUser: { id: string; fullName: string } | null;
  isLoading: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export const BlockConfirmModal: React.FC<BlockConfirmModalProps> = ({
  isOpen,
  type,
  targetUser,
  isLoading,
  onClose,
  onConfirm,
}) => {
  if (!isOpen || !targetUser) return null;

  const isBlock = type === 'block';

  return (
    <div className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4 animate-in fade-in duration-200">
      {/* Click outside backdrop */}
      <div className="fixed inset-0" onClick={isLoading ? undefined : onClose} />

      {/* Modal Dialog */}
      <div className="relative w-full max-w-md rounded-2xl bg-[#0c2340] border border-slate-700/80 shadow-2xl p-6 text-slate-200 z-10 animate-in zoom-in-95 duration-200">
        {/* Close Button */}
        <button
          onClick={onClose}
          disabled={isLoading}
          className="absolute top-4 right-4 p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-[#122e54] transition-colors cursor-pointer disabled:opacity-50"
        >
          <X className="w-4 h-4" />
        </button>

        {/* Icon & Title */}
        <div className="flex flex-col items-center text-center">
          <div
            className={`w-14 h-14 rounded-2xl flex items-center justify-center mb-4 shadow-lg ${
              isBlock
                ? 'bg-rose-950/60 border border-rose-500/40 text-rose-400'
                : 'bg-emerald-950/60 border border-emerald-500/40 text-emerald-400'
            }`}
          >
            {isBlock ? (
              <ShieldAlert className="w-7 h-7" />
            ) : (
              <ShieldCheck className="w-7 h-7" />
            )}
          </div>

          <h3 className="text-base font-bold text-white mb-2">
            {isBlock ? 'Xác Nhận Chặn Người Dùng' : 'Xác Nhận Bỏ Chặn Người Dùng'}
          </h3>

          <p className="text-xs text-slate-300 leading-relaxed mb-4">
            {isBlock ? (
              <>
                Bạn có chắc chắn muốn chặn{' '}
                <strong className="text-white font-semibold">"{targetUser.fullName}"</strong>?
              </>
            ) : (
              <>
                Bạn có chắc chắn muốn bỏ chặn cho{' '}
                <strong className="text-white font-semibold">"{targetUser.fullName}"</strong>?
              </>
            )}
          </p>
        </div>

        {/* Warning bullets */}
        <div
          className={`p-3.5 rounded-xl border text-xs mb-6 space-y-1.5 ${
            isBlock
              ? 'bg-rose-950/20 border-rose-500/20 text-rose-200/90'
              : 'bg-emerald-950/20 border-emerald-500/20 text-emerald-200/90'
          }`}
        >
          {isBlock ? (
            <>
              <p className="flex items-start gap-2">
                <span className="text-rose-400 font-bold">•</span>
                <span>Người này sẽ bị gỡ khỏi danh bạ liên hệ của bạn.</span>
              </p>
              <p className="flex items-start gap-2">
                <span className="text-rose-400 font-bold">•</span>
                <span>Họ sẽ không thể gửi tin nhắn hoặc gọi điện cho bạn.</span>
              </p>
              <p className="flex items-start gap-2">
                <span className="text-rose-400 font-bold">•</span>
                <span>Bạn có thể bỏ chặn bất kỳ lúc nào trong mục "Danh sách đã chặn".</span>
              </p>
            </>
          ) : (
            <>
              <p className="flex items-start gap-2">
                <span className="text-emerald-400 font-bold">•</span>
                <span>Người này sẽ có thể tìm thấy và gửi lời mời kết bạn tới bạn.</span>
              </p>
              <p className="flex items-start gap-2">
                <span className="text-emerald-400 font-bold">•</span>
                <span>Hai bạn sẽ có thể kết nối trò chuyện lại sau khi kết bạn.</span>
              </p>
            </>
          )}
        </div>

        {/* Action Buttons */}
        <div className="flex items-center justify-end gap-3">
          <button
            onClick={onClose}
            disabled={isLoading}
            className="px-4 py-2.5 rounded-xl bg-[#07172b] hover:bg-[#122e54] border border-slate-700 text-xs font-semibold text-slate-300 hover:text-white transition-all cursor-pointer disabled:opacity-50"
          >
            Hủy bỏ
          </button>

          <button
            onClick={onConfirm}
            disabled={isLoading}
            className={`px-5 py-2.5 rounded-xl text-xs font-semibold text-white shadow-md transition-all hover:scale-105 active:scale-95 cursor-pointer flex items-center gap-2 disabled:opacity-50 ${
              isBlock
                ? 'bg-rose-600 hover:bg-rose-500 shadow-rose-900/40'
                : 'bg-emerald-600 hover:bg-emerald-500 shadow-emerald-900/40'
            }`}
          >
            {isLoading && <RefreshCw className="w-3.5 h-3.5 animate-spin" />}
            <span>{isBlock ? 'Xác nhận chặn' : 'Xác nhận bỏ chặn'}</span>
          </button>
        </div>
      </div>
    </div>
  );
};
