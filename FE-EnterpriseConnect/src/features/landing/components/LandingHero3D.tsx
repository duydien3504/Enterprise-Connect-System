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
    <div className="relative min-h-screen w-full overflow-hidden bg-[#1871e0] font-['Plus_Jakarta_Sans',sans-serif] select-none cursor-none">
      {/* 1. Custom Magnetic Precision Designer Cursor */}
      <EnterpriseCursor />

      {/* 2. Full-bleed Master Photorealistic 3D Environment Background */}
      <div className="absolute inset-0 z-0 pointer-events-none">
        <img
          src="/hero-reference.jpg"
          alt="Enterprise Connection Master Environment"
          className="w-full h-full object-cover object-center filter brightness-[1.01] contrast-[1.02]"
        />
        {/* Seamless Soft Atmospheric Backdrop Mask */}
        <div className="absolute inset-0 bg-gradient-to-r from-[#176edb] via-[#176edb] via-35% lg:via-42% to-transparent opacity-95" />
      </div>

      {/* 3. Pure Top Header Bar - KHÔNG CÓ BẤT KỲ BADGE NÀO */}
      <header className="relative z-20 flex items-center justify-between px-8 sm:px-14 py-8 max-w-7xl mx-auto">
        <BrandLogo size="md" />
      </header>

      {/* 4. Main Hero Section */}
      <main className="relative z-10 max-w-7xl mx-auto px-8 sm:px-14 pt-2 pb-12 min-h-[calc(100vh-120px)] flex flex-col lg:flex-row items-center justify-between gap-8">
        
        {/* Left Column: Authentic Senior-Designer Typography & Single Primary Action */}
        <div className="flex-1 max-w-xl text-left space-y-6">
          <div className="space-y-4">
            <h1 className="text-5xl sm:text-6xl lg:text-7xl font-extrabold text-white tracking-[-0.03em] leading-[1.06] drop-shadow-[0_2px_10px_rgba(0,0,0,0.18)]">
              Enterprise<br />
              <span className="text-white drop-shadow-[0_2px_12px_rgba(0,0,0,0.15)]">
                Connection
              </span>
            </h1>
            <p className="text-2xl sm:text-3xl font-semibold text-white/95 tracking-[-0.015em] drop-shadow-[0_1px_4px_rgba(0,0,0,0.15)]">
              Connect · Collaborate · Grow Together
            </p>
          </div>

          <p className="text-base sm:text-lg text-white/90 font-normal leading-relaxed drop-shadow-[0_1px_4px_rgba(0,0,0,0.15)] max-w-lg">
            A unified communication platform for your enterprise — Chat, Call, Share, and Get Help, all in one place.
          </p>

          {/* NÚT ĐĂNG NHẬP DUY NHẤT: CHỈ KHI CLICK ĐÚNG VÀO NÚT NÀY MỚI HIỆN POPUP FORM ĐĂNG NHẬP */}
          <div className="pt-3">
            <button
              onClick={onOpenLogin}
              className="group relative inline-flex items-center gap-3.5 px-9 py-4 rounded-full bg-gradient-to-r from-[#1d64d8] via-[#2563eb] to-[#0284c7] hover:from-[#2563eb] hover:via-[#3b82f6] hover:to-[#0ea5e9] text-white font-bold text-lg shadow-[0_14px_30px_-6px_rgba(29,78,216,0.55),inset_0_1px_1.5px_rgba(255,255,255,0.8)] hover:shadow-[0_18px_38px_-6px_rgba(14,165,233,0.7),inset_0_1px_1.5px_rgba(255,255,255,1)] transform hover:-translate-y-0.5 active:translate-y-0.5 transition-all duration-200 cursor-pointer border border-white/70"
            >
              <span className="tracking-wide">Đăng Nhập</span>
              <ArrowRight className="w-5 h-5 transform group-hover:translate-x-1.5 transition-transform duration-200" />
            </button>
          </div>

          {/* BỎ TOÀN BỘ CÁC BADGE DƯỚI NÚT ĐĂNG NHẬP THEO ĐÚNG YÊU CẦU */}
        </div>

        {/* Right Column: Dynamic Real-time 3D WebGL Canvas with Real Human & 3D Glass Crystals */}
        <div className="flex-1 w-full max-w-xl h-[560px] relative flex items-center justify-center">
          <EnterpriseScene3D />
        </div>

      </main>
    </div>
  );
};
