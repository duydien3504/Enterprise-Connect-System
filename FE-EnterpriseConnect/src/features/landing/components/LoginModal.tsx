import React, { useState, useEffect } from 'react';
import {
  X,
  Lock,
  Mail,
  ArrowRight,
  ShieldCheck,
  KeyRound,
  Clock,
  RefreshCw,
  CheckCircle2,
  AlertCircle,
  Eye,
  EyeOff,
  ArrowLeft,
} from 'lucide-react';
import { authApi } from '@/features/auth/api/authApi';

interface LoginModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

type ModalMode = 'login' | 'forgot_password' | 'verify_otp';

export const LoginModal: React.FC<LoginModalProps> = ({ isOpen, onClose, onSuccess }) => {
  // Modal flow mode: 'login' | 'forgot_password' | 'verify_otp'
  const [mode, setMode] = useState<ModalMode>('login');

  // Login form state
  const [email, setEmail] = useState('admin@enterprise.com');
  const [password, setPassword] = useState('Password123!');
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(true);
  const [isLoading, setIsLoading] = useState(false);

  // Forgot password form state (POST /api/Auth/forgot-password)
  const [forgotEmail, setForgotEmail] = useState('');
  const [isSendingForgot, setIsSendingForgot] = useState(false);

  // Verify OTP & Reset password state (POST /api/Auth/reset-password)
  const [otp, setOtp] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [otpCountdown, setOtpCountdown] = useState<number>(300); // 5 minutes = 300s
  const [isResetting, setIsResetting] = useState(false);
  const [isResendingOtp, setIsResendingOtp] = useState(false);

  // Notifications
  const [error, setError] = useState<string | null>(null);
  const [successNotice, setSuccessNotice] = useState<string | null>(null);

  const handleClose = () => {
    setMode('login');
    setError(null);
    setSuccessNotice(null);
    setOtp('');
    setNewPassword('');
    setConfirmPassword('');
    onClose();
  };

  // 5-minute countdown ticker for OTP verification
  useEffect(() => {
    if (mode !== 'verify_otp' || otpCountdown <= 0) return;

    const interval = setInterval(() => {
      setOtpCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(interval);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [mode, otpCountdown]);

  if (!isOpen) return null;

  const formatCountdown = (totalSeconds: number) => {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  };

  /**
   * Submit Login (POST /api/Auth/login)
   */
  const handleLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    setSuccessNotice(null);

    try {
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

  /**
   * Switch to Forgot Password Form
   */
  const handleOpenForgotPassword = (e: React.MouseEvent) => {
    e.preventDefault();
    setForgotEmail(email.trim() || '');
    setError(null);
    setSuccessNotice(null);
    setMode('forgot_password');
  };

  /**
   * Submit Forgot Password (POST /api/Auth/forgot-password)
   * On 200 OK: transitions to OTP Verification Form with 5-minute timer
   */
  const handleForgotPasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!forgotEmail.trim()) {
      setError('Vui lòng nhập địa chỉ email công vụ.');
      return;
    }

    setIsSendingForgot(true);
    setError(null);
    setSuccessNotice(null);

    try {
      const res = await authApi.forgotPassword(forgotEmail.trim());
      const msg = res.message || 'Mã xác thực OTP 6 số đã được gửi tới email của bạn (hiệu lực 5 phút).';
      setSuccessNotice(msg);
      setOtp('');
      setNewPassword('');
      setConfirmPassword('');
      setOtpCountdown(300); // 5 minutes
      setMode('verify_otp');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Không thể gửi yêu cầu đặt lại mật khẩu.';
      setError(msg);
    } finally {
      setIsSendingForgot(false);
    }
  };

  /**
   * Resend OTP with the same original email body (POST /api/Auth/forgot-password)
   */
  const handleResendOtp = async () => {
    if (isResendingOtp || isResetting) return;
    setIsResendingOtp(true);
    setError(null);

    try {
      const res = await authApi.forgotPassword(forgotEmail.trim());
      setOtpCountdown(300); // Reset timer back to 5 minutes
      setSuccessNotice(res.message || `Đã gửi lại mã xác thực OTP mới tới ${forgotEmail}`);
      setTimeout(() => setSuccessNotice(null), 4000);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Không thể gửi lại mã OTP. Vui lòng thử lại sau.';
      setError(msg);
    } finally {
      setIsResendingOtp(false);
    }
  };

  /**
   * Submit Reset Password with OTP (POST /api/Auth/reset-password)
   */
  const handleResetPasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const cleanOtp = otp.trim();
    if (!cleanOtp || cleanOtp.length !== 6) {
      setError('Vui lòng nhập đầy đủ mã xác thực OTP 6 chữ số.');
      return;
    }

    if (!newPassword) {
      setError('Vui lòng nhập mật khẩu mới.');
      return;
    }

    if (newPassword.length < 6) {
      setError('Mật khẩu mới phải có ít nhất 6 ký tự.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Xác nhận mật khẩu mới không khớp.');
      return;
    }

    if (otpCountdown <= 0) {
      setError('Mã OTP đã hết hạn (quá 5 phút). Vui lòng bấm "Gửi lại mã OTP".');
      return;
    }

    setIsResetting(true);
    setError(null);
    setSuccessNotice(null);

    try {
      const res = await authApi.resetPassword({
        email: forgotEmail.trim(),
        otp: cleanOtp,
        newPassword,
      });

      const successMsg = res.message || 'Đặt lại mật khẩu thành công! Bạn có thể sử dụng mật khẩu mới để đăng nhập.';
      setMode('login');
      setEmail(forgotEmail.trim());
      setPassword('');
      setSuccessNotice(successMsg);
      setError(null);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Đặt lại mật khẩu không thành công. Vui lòng kiểm tra lại OTP.';
      setError(msg);
    } finally {
      setIsResetting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#071e3d]/80 backdrop-blur-md animate-in fade-in duration-150 cursor-default">
      {/* Modal Surface - Solid Deep Navy Enterprise Card (Zero Gradient) */}
      <div className="relative w-full max-w-[430px] rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/50 p-7 sm:p-8 shadow-[0_24px_60px_rgba(0,0,0,0.7)] text-white cursor-auto">
        
        {/* Minimalist Close Button */}
        <button
          onClick={handleClose}
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

          {mode === 'login' && (
            <>
              <h2 className="text-2xl font-bold text-white tracking-[-0.02em]">
                Đăng nhập hệ thống
              </h2>
              <p className="text-xs text-slate-400 mt-1">
                Xác thực tài khoản và khởi tạo phiên làm việc bảo mật
              </p>
            </>
          )}

          {mode === 'forgot_password' && (
            <>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => {
                    setMode('login');
                    setError(null);
                    setSuccessNotice(null);
                  }}
                  className="p-1 -ml-1 rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
                  title="Quay lại đăng nhập"
                >
                  <ArrowLeft className="w-4 h-4" />
                </button>
                <h2 className="text-2xl font-bold text-white tracking-[-0.02em]">
                  Quên mật khẩu
                </h2>
              </div>
              <p className="text-xs text-slate-400 mt-1">
                Nhập email công vụ để nhận mã xác thực OTP 6 số bảo mật
              </p>
            </>
          )}

          {mode === 'verify_otp' && (
            <>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => {
                    setMode('forgot_password');
                    setError(null);
                    setSuccessNotice(null);
                  }}
                  className="p-1 -ml-1 rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
                  title="Quay lại nhập email"
                >
                  <ArrowLeft className="w-4 h-4" />
                </button>
                <h2 className="text-2xl font-bold text-white tracking-[-0.02em]">
                  Xác thực OTP
                </h2>
              </div>
              <p className="text-xs text-slate-400 mt-1 truncate">
                Mã xác thực đã gửi tới <span className="text-white font-medium">{forgotEmail}</span>
              </p>
            </>
          )}
        </div>

        {/* Global Error Notice */}
        {error && (
          <div className="mb-5 p-3 rounded-xl bg-rose-950/70 border border-rose-500/50 text-xs text-rose-200 flex items-start gap-2.5 animate-in fade-in">
            <AlertCircle className="w-4 h-4 text-rose-400 shrink-0 mt-0.5" />
            <span className="leading-relaxed">{error}</span>
          </div>
        )}

        {/* Global Success Notice */}
        {successNotice && (
          <div className="mb-5 p-3 rounded-xl bg-emerald-950/70 border border-emerald-500/50 text-xs text-emerald-200 flex items-start gap-2.5 animate-in fade-in">
            <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
            <span className="leading-relaxed">{successNotice}</span>
          </div>
        )}

        {/* ======================================================== */}
        {/* 1. FORM ĐĂNG NHẬP (MODE: 'login')                         */}
        {/* ======================================================== */}
        {mode === 'login' && (
          <form onSubmit={handleLoginSubmit} className="space-y-4">
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
                  className="w-full pl-10 pr-3.5 py-2.5 rounded-xl bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-colors"
                />
              </div>
            </div>

            {/* Password Field with textlink [Quên mật khẩu?] */}
            <div>
              <div className="flex items-center justify-between mb-1.5">
                <label className="text-xs font-semibold text-slate-300">
                  Mật khẩu
                </label>
                <a
                  href="#forgot-password"
                  onClick={handleOpenForgotPassword}
                  className="text-xs text-[#38bdf8] hover:text-[#7dd3fc] hover:underline cursor-pointer font-medium transition-colors"
                >
                  Quên mật khẩu?
                </a>
              </div>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                  <Lock className="w-4 h-4" />
                </div>
                <input
                  type={showPassword ? 'text' : 'password'}
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••••••"
                  className="w-full pl-10 pr-10 py-2.5 rounded-xl bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-colors"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  tabIndex={-1}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-[#38bdf8] transition-colors p-1"
                  title={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                >
                  {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
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
                className="w-full py-3 px-5 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:bg-[#0747a6] text-white font-bold text-sm shadow-md transition-colors flex items-center justify-center gap-2 cursor-pointer disabled:opacity-60"
              >
                {isLoading ? (
                  <>
                    <RefreshCw className="w-4 h-4 animate-spin" />
                    <span>Đang xác thực...</span>
                  </>
                ) : (
                  <>
                    <span>Đăng Nhập</span>
                    <ArrowRight className="w-4 h-4" />
                  </>
                )}
              </button>
            </div>
          </form>
        )}

        {/* ======================================================== */}
        {/* 2. FORM QUÊN MẬT KHẨU (MODE: 'forgot_password')         */}
        {/* ======================================================== */}
        {mode === 'forgot_password' && (
          <form onSubmit={handleForgotPasswordSubmit} className="space-y-4">
            <div className="p-3.5 rounded-xl bg-[#07172b] border border-[#1d4ed8]/30 flex items-start gap-2.5 text-xs text-slate-300">
              <KeyRound className="w-4 h-4 text-[#38bdf8] shrink-0 mt-0.5" />
              <span className="leading-relaxed">
                Hệ thống sẽ tạo mã OTP 6 chữ số ngẫu nhiên và gửi tới hòm thư của bạn. Mã có hiệu lực trong vòng <strong>5 phút</strong>.
              </span>
            </div>

            {/* Email Field */}
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">
                Email công việc cần khôi phục
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                  <Mail className="w-4 h-4" />
                </div>
                <input
                  type="email"
                  required
                  autoFocus
                  value={forgotEmail}
                  onChange={(e) => setForgotEmail(e.target.value)}
                  placeholder="name@enterprise.com"
                  disabled={isSendingForgot}
                  className="w-full pl-10 pr-3.5 py-2.5 rounded-xl bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-colors disabled:opacity-50"
                />
              </div>
            </div>

            {/* Actions */}
            <div className="pt-2 space-y-2.5">
              <button
                type="submit"
                disabled={isSendingForgot || !forgotEmail.trim()}
                className="w-full py-3 px-5 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:bg-[#0747a6] text-white font-bold text-sm shadow-md transition-colors flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSendingForgot ? (
                  <>
                    <RefreshCw className="w-4 h-4 animate-spin" />
                    <span>Đang gửi mã OTP...</span>
                  </>
                ) : (
                  <>
                    <span>Gửi Mã Xác Thực (OTP)</span>
                    <ArrowRight className="w-4 h-4" />
                  </>
                )}
              </button>

              <button
                type="button"
                onClick={() => {
                  setMode('login');
                  setError(null);
                  setSuccessNotice(null);
                }}
                disabled={isSendingForgot}
                className="w-full py-2.5 px-4 rounded-xl bg-[#07172b] hover:bg-[#122e54] text-slate-300 hover:text-white border border-[#1d4ed8]/30 text-xs font-semibold transition-colors cursor-pointer"
              >
                Quay lại đăng nhập
              </button>
            </div>
          </form>
        )}

        {/* ======================================================== */}
        {/* 3. FORM VERIFY OTP & RESET PASSWORD (MODE: 'verify_otp')  */}
        {/* ======================================================== */}
        {mode === 'verify_otp' && (
          <form onSubmit={handleResetPasswordSubmit} className="space-y-3.5">
            {/* 5-minute Countdown Banner */}
            <div className="p-3 rounded-xl bg-[#07172b] border border-[#1d4ed8]/30 flex items-center justify-between text-xs">
              <div className="flex items-center gap-2">
                <Clock className="w-4 h-4 text-[#38bdf8] shrink-0" />
                <span className="text-slate-300">Thời gian hiệu lực OTP:</span>
              </div>
              <span
                className={`font-mono font-bold text-xs px-2.5 py-0.5 rounded-md border ${
                  otpCountdown > 60
                    ? 'bg-emerald-950/80 text-emerald-400 border-emerald-500/40'
                    : otpCountdown > 0
                    ? 'bg-amber-950/80 text-amber-300 border-amber-500/40 animate-pulse'
                    : 'bg-rose-950/80 text-rose-300 border-rose-500/40'
                }`}
              >
                {formatCountdown(otpCountdown)}
              </span>
            </div>

            {/* OTP 6 Digits Field */}
            <div>
              <div className="flex items-center justify-between mb-1.5">
                <label className="text-xs font-semibold text-slate-300 flex items-center gap-1">
                  <span>Mã OTP 6 chữ số</span>
                  <span className="text-rose-400">*</span>
                </label>
                {/* Textlink nhỏ gửi lại OTP */}
                <button
                  type="button"
                  onClick={handleResendOtp}
                  disabled={isResendingOtp || isResetting}
                  className="text-xs text-[#38bdf8] hover:text-[#7dd3fc] hover:underline cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-1"
                >
                  {isResendingOtp && <RefreshCw className="w-3 h-3 animate-spin" />}
                  <span>Gửi lại mã OTP</span>
                </button>
              </div>
              <input
                type="text"
                required
                autoFocus
                maxLength={6}
                value={otp}
                onChange={(e) => {
                  const val = e.target.value.replace(/\D/g, '').slice(0, 6);
                  setOtp(val);
                  if (error) setError(null);
                }}
                placeholder="123456"
                disabled={isResetting}
                className="w-full py-2.5 px-3 rounded-xl bg-[#07172b] border border-slate-700/80 text-center font-mono font-bold text-lg tracking-[0.35em] text-[#38bdf8] placeholder-slate-600 focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-colors"
              />
            </div>

            {/* New Password Field */}
            <div>
              <div className="flex items-center justify-between mb-1.5">
                <label className="text-xs font-semibold text-slate-300 flex items-center gap-1">
                  <span>Mật khẩu mới</span>
                  <span className="text-rose-400">*</span>
                </label>
                <span className="text-[11px] text-slate-400">Tối thiểu 6 ký tự</span>
              </div>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                  <Lock className="w-4 h-4" />
                </div>
                <input
                  type={showNewPassword ? 'text' : 'password'}
                  required
                  value={newPassword}
                  onChange={(e) => {
                    setNewPassword(e.target.value);
                    if (error) setError(null);
                  }}
                  placeholder="••••••••••••"
                  disabled={isResetting}
                  className="w-full pl-10 pr-10 py-2.5 rounded-xl bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-colors"
                />
                <button
                  type="button"
                  onClick={() => setShowNewPassword(!showNewPassword)}
                  tabIndex={-1}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-[#38bdf8] transition-colors p-1"
                  title={showNewPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                >
                  {showNewPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>

            {/* Confirm New Password Field */}
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">
                Xác nhận mật khẩu mới <span className="text-rose-400">*</span>
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                  <Lock className="w-4 h-4" />
                </div>
                <input
                  type={showConfirmPassword ? 'text' : 'password'}
                  required
                  value={confirmPassword}
                  onChange={(e) => {
                    setConfirmPassword(e.target.value);
                    if (error) setError(null);
                  }}
                  placeholder="••••••••••••"
                  disabled={isResetting}
                  className="w-full pl-10 pr-10 py-2.5 rounded-xl bg-[#07172b] border border-slate-700/80 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-[#38bdf8] focus:bg-[#081d36] transition-colors"
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  tabIndex={-1}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-[#38bdf8] transition-colors p-1"
                  title={showConfirmPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                >
                  {showConfirmPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>

            {/* Actions */}
            <div className="pt-2 space-y-2">
              <button
                type="submit"
                disabled={isResetting || otp.length !== 6 || !newPassword || !confirmPassword || otpCountdown <= 0}
                className="w-full py-3 px-5 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:bg-[#0747a6] text-white font-bold text-sm shadow-md transition-colors flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isResetting ? (
                  <>
                    <RefreshCw className="w-4 h-4 animate-spin" />
                    <span>Đang cập nhật mật khẩu...</span>
                  </>
                ) : (
                  <>
                    <KeyRound className="w-4 h-4" />
                    <span>Xác Nhận & Đặt Lại Mật Khẩu</span>
                  </>
                )}
              </button>

              <button
                type="button"
                onClick={() => {
                  setMode('login');
                  setError(null);
                  setSuccessNotice(null);
                }}
                disabled={isResetting}
                className="w-full py-2.5 px-4 rounded-xl bg-[#07172b] hover:bg-[#122e54] text-slate-300 hover:text-white border border-[#1d4ed8]/30 text-xs font-semibold transition-colors cursor-pointer"
              >
                Hủy & Quay lại đăng nhập
              </button>
            </div>
          </form>
        )}

        {/* Minimal Enterprise Security Footer */}
        <div className="mt-6 pt-4 border-t border-slate-800/80 flex items-center justify-center gap-1.5 text-[11px] text-slate-400">
          <ShieldCheck className="w-3.5 h-3.5 text-[#38bdf8]" />
          <span>Bảo mật Zero-Trust kết nối qua API Gateway</span>
        </div>
      </div>
    </div>
  );
};
