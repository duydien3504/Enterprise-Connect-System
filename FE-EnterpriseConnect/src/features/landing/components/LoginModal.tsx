import React, { useState } from 'react';
import { X, Lock, Mail, ArrowRight, ShieldCheck } from 'lucide-react';
import { authApi } from '@/features/auth/api/authApi';

interface LoginModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const LoginModal: React.FC<LoginModalProps> = ({ isOpen, onClose, onSuccess }) => {
  const [email, setEmail] = useState('admin@enterprise.com');
  const [password, setPassword] = useState('Password123!');
  const [rememberMe, setRememberMe] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      // Calls POST /api/Auth/login with exact backend schema:
      // { email, password, deviceType: 1, deviceInfo, ipAddress }
      await authApi.login({
        email,
        password,
        deviceType: 1, // Web
        deviceInfo: typeof navigator !== 'undefined' ? navigator.userAgent : 'Enterprise Web Client',
        ipAddress: '',
        rememberMe,
      });

      onSuccess();
      onClose();
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản và mật khẩu.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#071e3d]/80 backdrop-blur-md animate-in fade-in duration-150 cursor-default">
      {/* Modal Surface - Solid Deep Navy Enterprise Card (Zero Gradient) */}
      <div className="relative w-full max-w-[420px] rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/50 p-7 sm:p-8 shadow-[0_24px_60px_rgba(0,0,0,0.7)] text-white cursor-auto">
        
        {/* Minimalist Close Button */}
        <button
          onClick={onClose}
          aria-label="Đóng"
          className="absolute top-5 right-5 w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
        >
          <X className="w-4 h-4" />
        </button>

        {/* Brand Header */}
        <div className="mb-6">
          <div className="flex items-center gap-2.5 mb-3">
            <svg width="28" height="22" viewBox="0 0 100 80" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M32 14 L50 24 L50 44 L32 54 L14 44 L14 24 Z" stroke="#ffffff" strokeWidth="10" strokeLinejoin="round" strokeLinecap="round"/>
              <path d="M66 26 L84 36 L84 56 L66 66 L48 56 L48 36 Z" stroke="#38bdf8" strokeWidth="10" strokeLinejoin="round" strokeLinecap="round"/>
            </svg>
            <span className="text-xs font-bold tracking-wider text-[#38bdf8] uppercase">
              Enterprise Connection
            </span>
          </div>

          <h2 className="text-2xl font-bold text-white tracking-[-0.02em]">
            Đăng nhập hệ thống
          </h2>
          <p className="text-xs text-slate-400 mt-1">
            Xác thực tài khoản và khởi tạo phiên làm việc bảo mật
          </p>
        </div>

        {/* Error notification from Backend */}
        {error && (
          <div className="mb-5 p-3 rounded-lg bg-[#3d1217] border border-rose-500/50 text-xs text-rose-200">
            {error}
          </div>
        )}

        {/* Clean Corporate Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          
          {/* Email Field */}
          <div>
            <label className="block text-xs font-semibold text-slate-300 mb-1.5">
              Email công việc
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                <Mail className="w-4 h-4" />
              </div>
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@enterprise.com"
                className="w-full pl-10 pr-3.5 py-2.5 rounded-lg bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-colors"
              />
            </div>
          </div>

          {/* Password Field */}
          <div>
            <div className="flex items-center justify-between mb-1.5">
              <label className="text-xs font-semibold text-slate-300">
                Mật khẩu
              </label>
              <a
                href="#forgot-password"
                onClick={(e) => e.preventDefault()}
                className="text-xs text-[#38bdf8] hover:underline"
              >
                Quên mật khẩu?
              </a>
            </div>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                <Lock className="w-4 h-4" />
              </div>
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••••••"
                className="w-full pl-10 pr-3.5 py-2.5 rounded-lg bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-colors"
              />
            </div>
          </div>

          {/* Remember session checkbox */}
          <div className="pt-1">
            <label className="inline-flex items-center gap-2.5 cursor-pointer select-none">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                className="w-4 h-4 rounded bg-[#07172b] border border-slate-700 text-[#0052cc] focus:ring-0 cursor-pointer"
              />
              <span className="text-xs font-medium text-slate-300">
                Ghi nhớ phiên đăng nhập trên thiết bị này
              </span>
            </label>
          </div>

          {/* Solid Blue Submit Button (NO GRADIENTS) */}
          <div className="pt-2">
            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-3 px-5 rounded-lg bg-[#0052cc] hover:bg-[#0065ff] active:bg-[#0747a6] text-white font-bold text-sm shadow-md transition-colors flex items-center justify-center gap-2 cursor-pointer disabled:opacity-60"
            >
              <span>{isLoading ? 'Đang xác thực...' : 'Đăng Nhập'}</span>
              <ArrowRight className="w-4 h-4" />
            </button>
          </div>
        </form>

        {/* Minimal Enterprise Security Footer */}
        <div className="mt-6 pt-4 border-t border-slate-800/80 flex items-center justify-center gap-1.5 text-[11px] text-slate-400">
          <ShieldCheck className="w-3.5 h-3.5 text-[#38bdf8]" />
          <span>Bảo mật Zero-Trust kết nối qua API Gateway</span>
        </div>
      </div>
    </div>
  );
};
