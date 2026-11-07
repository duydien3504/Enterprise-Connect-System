import React from 'react';
import { ArrowRight } from 'lucide-react';
import { BrandLogo } from '@/shared/components/BrandLogo';
import { EnterpriseCursor } from '@/shared/components/EnterpriseCursor';
import { EnterpriseScene3D } from './EnterpriseScene3D';

interface LandingHero3DProps {
  onOpenLogin: () => void;
}

export const LandingHero3D: React.FC<LandingHero3DProps> = ({ onOpenLogin }) => {
  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-[#071e3d] font-['Plus_Jakarta_Sans',sans-serif] text-white select-none cursor-default flex flex-col justify-between">
      {/* 1. Custom Magnetic Precision Designer Halo (0ms lag, direct GPU transform) */}
      <EnterpriseCursor />

      {/* 2. Full-Page 3D WebGL Enterprise Node Mesh Canvas */}
      <EnterpriseScene3D />

      {/* 3. Top Clean Header Bar - Pure Brand Logo, NO Badges */}
      <header className="relative z-10 w-full px-8 sm:px-16 py-8 flex items-center justify-between max-w-7xl mx-auto">
        <BrandLogo size="md" />
      </header>

      {/* 4. Centered Hero Presentation (Bố cục Trung tâm Sang trọng) */}
      <main className="relative z-10 w-full max-w-4xl mx-auto px-6 text-center my-auto py-12 flex flex-col items-center justify-center">
        
        {/* Main Headline - Solid White, No Gradient, No AI Blur */}
        <h1 className="text-5xl sm:text-7xl lg:text-8xl font-black text-white tracking-[-0.04em] leading-[1.04] drop-shadow-[0_4px_16px_rgba(0,0,0,0.5)] mb-6">
          Enterprise<br />
          Connection
        </h1>

        {/* Subtitle - Solid Cyan-Blue */}
        <p className="text-xl sm:text-2xl lg:text-3xl font-semibold text-[#38bdf8] tracking-[-0.02em] mb-5 drop-shadow-[0_2px_8px_rgba(0,0,0,0.4)]">
          Hệ Thống Kết Nối Doanh Nghiệp Tập Trung
        </p>

        {/* Narrative Description - Crisp, Clean */}
        <p className="text-base sm:text-lg text-slate-200 max-w-2xl font-normal leading-relaxed mb-10 drop-shadow-[0_2px_6px_rgba(0,0,0,0.4)]">
          Hợp nhất liên lạc đa kênh, tương tác thời gian thực và quản trị an toàn trên một không gian số đồng bộ duy nhất.
        </p>

        {/* DUY NHẤT 1 BUTTON [ĐĂNG NHẬP] - TONE MÀU XANH DƯƠNG THUẦN (SOLID), KHÔNG DÙNG GRADIENT */}
        <div className="pt-2">
          <button
            onClick={onOpenLogin}
            className="group relative inline-flex items-center gap-3.5 px-12 py-5 rounded-full bg-[#0052cc] hover:bg-[#0065ff] active:bg-[#0747a6] text-white font-bold text-lg shadow-[0_12px_28px_rgba(0,82,204,0.45)] transform hover:-translate-y-0.5 active:translate-y-0.5 transition-all duration-150 cursor-pointer border border-[#38bdf8]"
          >
            <span className="tracking-wide">Đăng Nhập</span>
            <ArrowRight className="w-5 h-5 transform group-hover:translate-x-1.5 transition-transform duration-150 text-[#38bdf8] group-hover:text-white" />
          </button>
        </div>

        {/* TUYỆT ĐỐI KHÔNG CÓ BẤT KỲ BADGE HAY NÚT BẤM NÀO KHÁC */}
      </main>

      {/* 5. Minimalist Footer */}
      <footer className="relative z-10 w-full px-8 py-6 text-center text-xs text-slate-400">
        © 2026 Enterprise Connection. All rights reserved.
      </footer>
    </div>
  );
};
