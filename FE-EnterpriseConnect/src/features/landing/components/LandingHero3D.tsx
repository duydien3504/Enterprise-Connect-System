import React from 'react';
import { ArrowRight, CheckCircle2 } from 'lucide-react';
import { BrandLogo } from '@/shared/components/BrandLogo';
import { EnterpriseCursor } from '@/shared/components/EnterpriseCursor';
import { EnterpriseScene3D } from './EnterpriseScene3D';

interface LandingHero3DProps {
  onOpenLogin: () => void;
}

export const LandingHero3D: React.FC<LandingHero3DProps> = ({ onOpenLogin }) => {
  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-gradient-to-b from-[#1a6edb] via-[#3fa1f5] to-[#c7e8fc] font-sans text-slate-800 select-none cursor-none">
      {/* 1. Custom Cohesive Futuristic Enterprise Cursor */}
      <EnterpriseCursor />

      {/* 2. Architectural Skyline Background & Reflective Ground */}
      <div className="fixed inset-0 pointer-events-none z-0">
        {/* Vector Architectural High-Rise Towers */}
        <div className="absolute bottom-36 left-0 right-0 h-96 opacity-45 flex items-end justify-between px-8">
          <div className="w-20 h-76 bg-gradient-to-t from-sky-400 to-sky-100/60 rounded-t-sm shadow-xl"></div>
          <div className="w-28 h-96 bg-gradient-to-t from-sky-500 to-sky-200/50 rounded-t-md"></div>
          <div className="w-24 h-72 bg-gradient-to-t from-sky-400 to-sky-200/60 rounded-t-sm"></div>
          <div className="w-36 h-[27rem] bg-gradient-to-t from-sky-600 to-sky-200/40 rounded-t-lg"></div>
          <div className="w-28 h-88 bg-gradient-to-t from-sky-500 to-sky-300/50 rounded-t-sm"></div>
          <div className="w-40 h-[30rem] bg-gradient-to-t from-sky-600 to-sky-100/40 rounded-t-xl"></div>
          <div className="w-28 h-80 bg-gradient-to-t from-sky-400 to-sky-200/50 rounded-t-sm"></div>
          <div className="w-44 h-[32rem] bg-gradient-to-t from-sky-600 to-sky-300/40 rounded-t-2xl"></div>
        </div>

        {/* Polished Architectural Glass Terrace Floor */}
        <div className="absolute bottom-0 left-0 right-0 h-44 bg-gradient-to-b from-[#bfe2fa]/90 via-[#92c5f1]/95 to-[#6faaec] border-t border-white/70 shadow-[0_-15px_35px_rgba(37,99,235,0.15)]">
          <div className="w-full h-full opacity-35 bg-[radial-gradient(#ffffff_1.2px,transparent_1.2px)] [background-size:24px_24px]"></div>
        </div>
      </div>

      {/* 3. Pure, Uncluttered Header Bar (No redundant badge pills) */}
      <header className="relative z-20 flex items-center justify-between px-10 py-7 max-w-7xl mx-auto">
        <BrandLogo size="md" />
      </header>

      {/* 4. Hero Content & 3D Centerpiece */}
      <main className="relative z-10 max-w-7xl mx-auto px-10 pt-2 pb-14 min-h-[calc(100vh-100px)] flex flex-col lg:flex-row items-center justify-between gap-8 lg:gap-14">
        
        {/* Left Column: Typography & Primary Action */}
        <div className="flex-1 max-w-xl text-left space-y-7">
          <div className="space-y-3">
            <h1 className="text-5xl sm:text-6xl font-black text-white tracking-tight leading-[1.08] drop-shadow-[0_4px_16px_rgba(0,0,0,0.18)]">
              Enterprise<br />
              <span className="bg-clip-text text-transparent bg-gradient-to-r from-white via-sky-50 to-sky-200">
                Connection
              </span>
            </h1>
            <p className="text-2xl sm:text-3xl font-semibold text-sky-100/95 tracking-wide drop-shadow-[0_2px_4px_rgba(0,0,0,0.12)]">
              Connect · Collaborate · Grow Together
            </p>
          </div>

          <p className="text-lg text-sky-50/95 font-normal leading-relaxed drop-shadow-sm max-w-lg">
            A unified communication platform for your enterprise — Chat, Call, Share, and Get Help, all in one place.
          </p>

          {/* Designer-Grade Tactile Primary CTA Button */}
          <div className="pt-3">
            <button
              onClick={onOpenLogin}
              className="group relative inline-flex items-center gap-3.5 px-9 py-4 rounded-full bg-gradient-to-r from-blue-600 via-sky-600 to-cyan-500 hover:from-blue-500 hover:via-sky-500 hover:to-cyan-400 text-white font-bold text-lg shadow-[0_12px_28px_-6px_rgba(37,99,235,0.5),inset_0_1px_1px_rgba(255,255,255,0.7)] hover:shadow-[0_16px_36px_-6px_rgba(14,165,233,0.65),inset_0_1px_1px_rgba(255,255,255,0.9)] transform hover:-translate-y-0.5 active:translate-y-0.5 transition-all duration-200 cursor-pointer border border-white/60"
            >
              <span className="tracking-wide">Đăng Nhập</span>
              <ArrowRight className="w-5 h-5 transform group-hover:translate-x-1.5 transition-transform duration-200" />
            </button>
          </div>

          {/* Micro Value Proposition Badges */}
          <div className="pt-6 flex flex-wrap items-center gap-6 text-xs text-sky-100/95 font-medium">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-300 drop-shadow" />
              <span>Bảo Mật Zero-Trust</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-300 drop-shadow" />
              <span>Real-time SignalR WebSockets</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-300 drop-shadow" />
              <span>Hợp Nhất 5 Microservices</span>
            </div>
          </div>
        </div>

        {/* Right Column: Genuine Three.js WebGL 3D Experience */}
        <div className="flex-1 w-full max-w-xl flex items-center justify-center">
          <EnterpriseScene3D onSelectAction={onOpenLogin} />
        </div>

      </main>
    </div>
  );
};
