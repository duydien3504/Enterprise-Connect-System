import React, { useState, useEffect } from 'react';
import { ArrowRight, Sparkles } from 'lucide-react';
import { BrandLogo } from '@/shared/components/BrandLogo';
import { EnterpriseCursor } from '@/shared/components/EnterpriseCursor';

interface LandingHero3DProps {
  onOpenLogin: () => void;
}

export const LandingHero3D: React.FC<LandingHero3DProps> = ({ onOpenLogin }) => {
  const [mouseOffset, setMouseOffset] = useState({ x: 0, y: 0 });
  const [activeTooltip, setActiveTooltip] = useState<string | null>(null);

  // Smooth 3D mouse parallax tracking
  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      const { innerWidth, innerHeight } = window;
      const x = (e.clientX / innerWidth - 0.5) * 2;
      const y = (e.clientY / innerHeight - 0.5) * 2;
      setMouseOffset({ x, y });
    };

    window.addEventListener('mousemove', handleMouseMove);
    return () => window.removeEventListener('mousemove', handleMouseMove);
  }, []);

  const parallaxX = mouseOffset.x * 12;
  const parallaxY = mouseOffset.y * 10;
  const tiltRotX = -mouseOffset.y * 6;
  const tiltRotY = mouseOffset.x * 8;

  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-[#1a6edb] font-['Plus_Jakarta_Sans',sans-serif] text-slate-800 select-none cursor-none">
      {/* 1. Custom Cohesive Designer Cursor */}
      <EnterpriseCursor />

      {/* 2. Full-bleed Master Photorealistic 3D Environment Background */}
      <div className="absolute inset-0 z-0 pointer-events-none">
        <img
          src="/hero-reference.jpg"
          alt="Enterprise Connection 3D Master Environment"
          className="w-full h-full object-cover object-center filter brightness-[1.01] contrast-[1.02]"
        />
        {/* Crisp Gradient Mask covering baked-in text cleanly without ghosting */}
        <div className="absolute inset-0 bg-gradient-to-r from-[#176edb] via-[#176edb] via-35% lg:via-42% to-transparent" />
      </div>

      {/* 3. Pure, Uncluttered Top Header Bar - Không có bất kỳ badge nào */}
      <header className="relative z-20 flex items-center justify-between px-8 sm:px-14 py-8 max-w-7xl mx-auto">
        <BrandLogo size="md" />
      </header>

      {/* 4. Main Hero Section */}
      <main className="relative z-10 max-w-7xl mx-auto px-8 sm:px-14 pt-2 pb-16 min-h-[calc(100vh-120px)] flex flex-col lg:flex-row items-center justify-between gap-12">
        
        {/* Left Column: Authentic Designer Typography & Single Primary Action */}
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

          {/* Designer-Grade Tactile Primary CTA Button - Nút Đăng Nhập duy nhất */}
          <div className="pt-3">
            <button
              onClick={onOpenLogin}
              className="group relative inline-flex items-center gap-3.5 px-9 py-4 rounded-full bg-gradient-to-r from-[#1d64d8] via-[#2563eb] to-[#0284c7] hover:from-[#2563eb] hover:via-[#3b82f6] hover:to-[#0ea5e9] text-white font-bold text-lg shadow-[0_14px_30px_-6px_rgba(29,78,216,0.55),inset_0_1px_1.5px_rgba(255,255,255,0.8)] hover:shadow-[0_18px_38px_-6px_rgba(14,165,233,0.7),inset_0_1px_1.5px_rgba(255,255,255,1)] transform hover:-translate-y-0.5 active:translate-y-0.5 transition-all duration-200 cursor-pointer border border-white/70"
            >
              <span className="tracking-wide">Đăng Nhập</span>
              <ArrowRight className="w-5 h-5 transform group-hover:translate-x-1.5 transition-transform duration-200" />
            </button>
          </div>

          {/* BỎ TOÀN BỘ CÁC BADGE DƯỚI BUTTON NHƯ HÌNH 2 YÊU CẦU */}
        </div>

        {/* Right Column: 100% Authentic 3D Master Podium & Orbiting Glass Cubes */}
        <div className="flex-1 w-full max-w-xl relative flex items-center justify-center">
          
          {/* Interactive Parallax 3D Depth Layer */}
          <div
            className="relative w-full h-[540px] flex items-center justify-center [perspective:1000px] transition-transform duration-100 ease-out"
            style={{
              transform: `translate3d(${parallaxX}px, ${parallaxY}px, 0) rotateX(${tiltRotX}deg) rotateY(${tiltRotY}deg)`,
            }}
          >
            {/* Interactive Hotspots for the 7 3D Orbiting Cubes & Central Crystal */}
            <div className="absolute inset-0 pointer-events-none">
              
              {/* Central Crystal Trigger */}
              <div
                onClick={onOpenLogin}
                onMouseEnter={() => setActiveTooltip('Enterprise Connection Platform')}
                onMouseLeave={() => setActiveTooltip(null)}
                className="absolute left-[54%] top-[34%] -translate-x-1/2 -translate-y-1/2 w-48 h-48 rounded-3xl cursor-pointer pointer-events-auto group"
                title="Bấm để Đăng Nhập"
              >
                <div className="w-full h-full rounded-3xl border-2 border-white/0 group-hover:border-white/80 group-hover:bg-white/20 group-hover:backdrop-blur-sm group-hover:shadow-[0_0_50px_rgba(56,189,248,0.7)] transition-all duration-300 flex items-center justify-center">
                  <span className="opacity-0 group-hover:opacity-100 px-4 py-1.5 rounded-full bg-sky-900/90 text-white text-xs font-bold shadow-lg transition-opacity flex items-center gap-1.5">
                    <Sparkles className="w-3.5 h-3.5 text-sky-400" />
                    Đăng Nhập Ngay
                  </span>
                </div>
              </div>

              {/* Orbiting Cube 1: Chat (Top) */}
              <div
                onClick={onOpenLogin}
                onMouseEnter={() => setActiveTooltip('Tin Nhắn Tức Thì (Real-time SignalR Chat)')}
                onMouseLeave={() => setActiveTooltip(null)}
                className="absolute left-[54%] top-[12%] -translate-x-1/2 -translate-y-1/2 w-20 h-20 rounded-2xl cursor-pointer pointer-events-auto group"
              >
                <div className="w-full h-full rounded-2xl border border-transparent group-hover:border-white/90 group-hover:bg-white/20 group-hover:shadow-[0_0_30px_rgba(56,189,248,0.85)] transition-all flex items-center justify-center">
                  <div className="w-2.5 h-2.5 rounded-full bg-sky-300 opacity-0 group-hover:opacity-100 animate-ping"></div>
                </div>
              </div>

              {/* Orbiting Cube 2: Video Call (Right Top) */}
              <div
                onClick={onOpenLogin}
                onMouseEnter={() => setActiveTooltip('Hội Nghị Truyền Hình (WebRTC Video Calls)')}
                onMouseLeave={() => setActiveTooltip(null)}
                className="absolute left-[82%] top-[20%] -translate-x-1/2 -translate-y-1/2 w-20 h-20 rounded-2xl cursor-pointer pointer-events-auto group"
              >
                <div className="w-full h-full rounded-2xl border border-transparent group-hover:border-white/90 group-hover:bg-white/20 group-hover:shadow-[0_0_30px_rgba(56,189,248,0.85)] transition-all flex items-center justify-center">
                  <div className="w-2.5 h-2.5 rounded-full bg-sky-300 opacity-0 group-hover:opacity-100 animate-ping"></div>
                </div>
              </div>

              {/* Orbiting Cube 3: Remote Desktop (Right Middle) */}
              <div
                onClick={onOpenLogin}
                onMouseEnter={() => setActiveTooltip('Điều Khiển Từ Xa (Remote Assistance)')}
                onMouseLeave={() => setActiveTooltip(null)}
                className="absolute left-[88%] top-[45%] -translate-x-1/2 -translate-y-1/2 w-20 h-20 rounded-2xl cursor-pointer pointer-events-auto group"
              >
                <div className="w-full h-full rounded-2xl border border-transparent group-hover:border-white/90 group-hover:bg-white/20 group-hover:shadow-[0_0_30px_rgba(56,189,248,0.85)] transition-all flex items-center justify-center">
                  <div className="w-2.5 h-2.5 rounded-full bg-sky-300 opacity-0 group-hover:opacity-100 animate-ping"></div>
                </div>
              </div>

              {/* Orbiting Cube 4: Helpdesk Support (Right Bottom) */}
              <div
                onClick={onOpenLogin}
                onMouseEnter={() => setActiveTooltip('Trợ Giúp Kỹ Thuật (Enterprise Helpdesk)')}
                onMouseLeave={() => setActiveTooltip(null)}
                className="absolute left-[82%] top-[68%] -translate-x-1/2 -translate-y-1/2 w-20 h-20 rounded-2xl cursor-pointer pointer-events-auto group"
              >
                <div className="w-full h-full rounded-2xl border border-transparent group-hover:border-white/90 group-hover:bg-white/20 group-hover:shadow-[0_0_30px_rgba(56,189,248,0.85)] transition-all flex items-center justify-center">
                  <div className="w-2.5 h-2.5 rounded-full bg-sky-300 opacity-0 group-hover:opacity-100 animate-ping"></div>
                </div>
              </div>

              {/* Orbiting Cube 5: Security Shield (Bottom Left) */}
              <div
                onClick={onOpenLogin}
                onMouseEnter={() => setActiveTooltip('Bảo Mật Zero-Trust & Tuân Thủ ISO')}
                onMouseLeave={() => setActiveTooltip(null)}
                className="absolute left-[44%] top-[72%] -translate-x-1/2 -translate-y-1/2 w-20 h-20 rounded-2xl cursor-pointer pointer-events-auto group"
              >
                <div className="w-full h-full rounded-2xl border border-transparent group-hover:border-white/90 group-hover:bg-white/20 group-hover:shadow-[0_0_30px_rgba(56,189,248,0.85)] transition-all flex items-center justify-center">
                  <div className="w-2.5 h-2.5 rounded-full bg-sky-300 opacity-0 group-hover:opacity-100 animate-ping"></div>
                </div>
              </div>

              {/* Orbiting Cube 6: Team Collaboration (Left Middle) */}
              <div
                onClick={onOpenLogin}
                onMouseEnter={() => setActiveTooltip('Danh Bạ & Phân Quyền Nhóm Doanh Nghiệp')}
                onMouseLeave={() => setActiveTooltip(null)}
                className="absolute left-[28%] top-[55%] -translate-x-1/2 -translate-y-1/2 w-20 h-20 rounded-2xl cursor-pointer pointer-events-auto group"
              >
                <div className="w-full h-full rounded-2xl border border-transparent group-hover:border-white/90 group-hover:bg-white/20 group-hover:shadow-[0_0_30px_rgba(56,189,248,0.85)] transition-all flex items-center justify-center">
                  <div className="w-2.5 h-2.5 rounded-full bg-sky-300 opacity-0 group-hover:opacity-100 animate-ping"></div>
                </div>
              </div>

              {/* Orbiting Cube 7: Voice Phone (Left Top) */}
              <div
                onClick={onOpenLogin}
                onMouseEnter={() => setActiveTooltip('Thoại Nội Bộ Doanh Nghiệp (Voice Calls)')}
                onMouseLeave={() => setActiveTooltip(null)}
                className="absolute left-[34%] top-[30%] -translate-x-1/2 -translate-y-1/2 w-20 h-20 rounded-2xl cursor-pointer pointer-events-auto group"
              >
                <div className="w-full h-full rounded-2xl border border-transparent group-hover:border-white/90 group-hover:bg-white/20 group-hover:shadow-[0_0_30px_rgba(56,189,248,0.85)] transition-all flex items-center justify-center">
                  <div className="w-2.5 h-2.5 rounded-full bg-sky-300 opacity-0 group-hover:opacity-100 animate-ping"></div>
                </div>
              </div>
            </div>

            {/* Dynamic Active Feature Tooltip Floating in 3D */}
            {activeTooltip && (
              <div className="absolute top-4 right-4 z-30 px-4 py-2 rounded-2xl bg-white/95 backdrop-blur-xl border border-white text-xs font-bold text-sky-900 shadow-2xl shadow-sky-950/30 animate-in fade-in slide-in-from-top-2 duration-150 flex items-center gap-2">
                <Sparkles className="w-3.5 h-3.5 text-sky-500" />
                <span>{activeTooltip}</span>
              </div>
            )}
          </div>
        </div>

      </main>
    </div>
  );
};
