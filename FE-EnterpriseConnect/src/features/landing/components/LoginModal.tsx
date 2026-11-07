import React, { useState } from 'react';
import { X, Lock, Mail, ShieldCheck, ArrowRight } from 'lucide-react';
import { Button } from '@/shared/components/Button';
import { Input } from '@/shared/components/Input';
import { authApi } from '@/features/auth/api/authApi';

interface LoginModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const LoginModal: React.FC<LoginModalProps> = ({ isOpen, onClose, onSuccess }) => {
  const [email, setEmail] = useState('admin@enterprise.com');
  const [password, setPassword] = useState('Password123!');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      await authApi.login({ email, password });
      onSuccess();
      onClose();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axErr = err as { response?: { status?: number } };
        setError(`Đăng nhập thất bại (Mã lỗi ${axErr.response?.status || '500'}). Hãy kiểm tra lại thông tin.`);
      } else {
        setError('Không thể kết nối với API Gateway https://api.hugonef.id.vn');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#071e3d]/85 backdrop-blur-md animate-in fade-in duration-150 cursor-default">
      <div className="relative w-full max-w-md rounded-2xl bg-white border border-[#0052cc] p-8 shadow-2xl shadow-black/60 transform transition-all cursor-auto">
        {/* Close button */}
        <button
          onClick={onClose}
          className="absolute top-5 right-5 w-8 h-8 rounded-full bg-slate-100 hover:bg-slate-200 flex items-center justify-center text-slate-500 hover:text-slate-900 transition-colors cursor-pointer"
        >
          <X className="w-4 h-4" />
        </button>

        {/* Modal Header */}
        <div className="flex items-center gap-3.5 mb-6">
          <div className="w-11 h-11 rounded-xl bg-[#0052cc] flex items-center justify-center text-white shadow-md">
            <Lock className="w-5 h-5" />
          </div>
          <div>
            <h2 className="text-xl font-bold text-slate-900">Đăng Nhập Hệ Thống</h2>
            <p className="text-xs text-slate-500 flex items-center gap-1 mt-0.5">
              <ShieldCheck className="w-3.5 h-3.5 text-[#0052cc]" />
              <span>Xác thực an toàn qua EnterpriseConnect Gateway</span>
            </p>
          </div>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            id="email"
            type="email"
            label="Email Doanh Nghiệp"
            placeholder="admin@enterprise.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            icon={<Mail className="w-4 h-4" />}
            required
            className="bg-slate-50 border-slate-200 text-slate-900 placeholder:text-slate-400 focus:border-[#0052cc]"
          />

          <Input
            id="password"
            type="password"
            label="Mật Khẩu"
            placeholder="••••••••"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            icon={<Lock className="w-4 h-4" />}
            required
            className="bg-slate-50 border-slate-200 text-slate-900 placeholder:text-slate-400 focus:border-[#0052cc]"
          />

          {error && (
            <div className="p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium">
              {error}
            </div>
          )}

          <div className="flex items-center justify-between text-xs pt-1">
            <label className="flex items-center gap-2 cursor-pointer text-slate-600 select-none">
              <input type="checkbox" defaultChecked className="rounded border-slate-300 text-[#0052cc] focus:ring-[#0052cc]" />
              <span>Ghi nhớ phiên</span>
            </label>
            <a href="#forgot" className="text-[#0052cc] hover:underline font-medium">
              Quên mật khẩu?
            </a>
          </div>

          {/* Solid Blue Button - NO GRADIENT */}
          <Button
            type="submit"
            size="lg"
            className="w-full mt-2 rounded-xl bg-[#0052cc] hover:bg-[#0065ff] active:bg-[#0747a6] text-white font-bold shadow-md border-0"
            isLoading={isLoading}
          >
            <span>Vào Hệ Thống</span>
            <ArrowRight className="w-4 h-4" />
          </Button>
        </form>
      </div>
    </div>
  );
};
