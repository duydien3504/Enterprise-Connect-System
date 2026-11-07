import React, { useState, useEffect } from 'react';
import {
  ArrowRight,
  MessageSquare,
  Video,
  Monitor,
  Headphones,
  ShieldCheck,
  Phone,
  Users,
  CheckCircle2,
  Sparkles,
  Radio,
  UserCheck,
} from 'lucide-react';
import { ENV } from '@/core/config/env';

interface LandingHero3DProps {
  onOpenLogin: () => void;
}

export const LandingHero3D: React.FC<LandingHero3DProps> = ({ onOpenLogin }) => {
  const [mousePos, setMousePos] = useState({ x: 0, y: 0 });

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      const { innerWidth, innerHeight } = window;
      const x = (e.clientX / innerWidth - 0.5) * 2;
      const y = (e.clientY / innerHeight - 0.5) * 2;
      setMousePos({ x, y });
    };

    window.addEventListener('mousemove', handleMouseMove);
    return () => window.removeEventListener('mousemove', handleMouseMove);
  }, []);

  const rotX = -mousePos.y * 14;
  const rotY = mousePos.x * 18;

  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-gradient-to-b from-[#1864cc] via-[#3ea0f5] to-[#c8e8fc] font-sans text-slate-800 select-none">
      {/* 1. Skyline Background Silhouettes & Reflective Terrace */}
      <div className="fixed inset-0 pointer-events-none z-0">
        {/* City Skyline */}
        <div className="absolute bottom-32 left-0 right-0 h-96 opacity-50 flex items-end justify-between px-6">
          <div className="w-16 h-80 bg-gradient-to-t from-sky-400 to-sky-200/50 rounded-t-sm shadow-lg"></div>
          <div className="w-24 h-96 bg-gradient-to-t from-sky-500 to-sky-300/40 rounded-t-md"></div>
          <div className="w-20 h-72 bg-gradient-to-t from-sky-400 to-sky-200/60 rounded-t-sm"></div>
          <div className="w-32 h-[26rem] bg-gradient-to-t from-sky-600 to-sky-200/30 rounded-t-lg"></div>
          <div className="w-28 h-84 bg-gradient-to-t from-sky-400 to-sky-300/50 rounded-t-sm"></div>
          <div className="w-36 h-[28rem] bg-gradient-to-t from-sky-500 to-sky-100/40 rounded-t-md"></div>
          <div className="w-24 h-76 bg-gradient-to-t from-sky-400 to-sky-200/40 rounded-t-sm"></div>
          <div className="w-40 h-[30rem] bg-gradient-to-t from-sky-600 to-sky-300/30 rounded-t-xl"></div>
        </div>

        {/* Reflective Terrace Floor */}
        <div className="absolute bottom-0 left-0 right-0 h-44 bg-gradient-to-b from-[#bee1fa]/90 via-[#91c3f0]/95 to-[#6eaae6] border-t border-white/60 shadow-2xl">
          <div className="w-full h-full opacity-30 bg-[radial-gradient(#ffffff_1px,transparent_1px)] [background-size:24px_24px]"></div>
        </div>
      </div>

      {/* 2. Top Navigation Bar */}
      <header className="relative z-20 flex items-center justify-between px-8 py-6 max-w-7xl mx-auto">
        <div className="flex items-center gap-3.5">
          <div className="w-11 h-11 rounded-xl bg-white/95 backdrop-blur-md flex items-center justify-center shadow-lg shadow-sky-500/20 border border-white">
            <Radio className="w-6 h-6 text-sky-600" />
          </div>
          <span className="text-xl font-bold tracking-tight text-white drop-shadow-md">Enterprise Connection</span>
        </div>

        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-white/20 backdrop-blur-md border border-white/40 text-xs font-semibold text-white shadow-sm">
            <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
            <span>ec.hugonef.id.vn</span>
          </div>

          <a
            href={`${ENV.API_BASE_URL}/swagger`}
            target="_blank"
            rel="noreferrer"
            className="hidden sm:inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-white/15 hover:bg-white/25 border border-white/30 text-xs font-medium text-white transition-colors"
          >
            <span>Swagger API</span>
            <Sparkles className="w-3 h-3 text-sky-200" />
          </a>
        </div>
      </header>

      {/* 3. Hero Main Layout */}
      <main className="relative z-10 max-w-7xl mx-auto px-8 pt-4 pb-16 min-h-[calc(100vh-100px)] flex flex-col lg:flex-row items-center justify-between gap-12">
        {/* Left Column: Typography & Button */}
        <div className="flex-1 max-w-xl text-left space-y-6">
          <div className="space-y-2">
            <h1 className="text-5xl sm:text-6xl font-black text-white tracking-tight leading-tight drop-shadow-lg">
              Enterprise<br />
              <span className="bg-clip-text text-transparent bg-gradient-to-r from-white via-sky-100 to-sky-200">
                Connection
              </span>
            </h1>
            <p className="text-2xl sm:text-3xl font-semibold text-sky-100 tracking-wide drop-shadow-sm">
              Connect · Collaborate · Grow Together
            </p>
          </div>

          <p className="text-lg text-sky-50 font-normal leading-relaxed drop-shadow-sm max-w-lg">
            A unified communication platform for your enterprise — Chat, Call, Share, and Get Help, all in one place.
          </p>

          {/* The Single Primary CTA Button */}
          <div className="pt-4">
            <button
              onClick={onOpenLogin}
              className="group relative inline-flex items-center gap-3.5 px-9 py-4 rounded-full bg-gradient-to-r from-blue-600 via-sky-600 to-cyan-500 hover:from-blue-500 hover:via-sky-500 hover:to-cyan-400 text-white font-bold text-lg shadow-xl shadow-sky-600/40 hover:shadow-sky-500/60 transform hover:-translate-y-0.5 active:translate-y-0 transition-all duration-200 cursor-pointer border border-white/50"
            >
              <span>Đăng Nhập</span>
              <ArrowRight className="w-5 h-5 transform group-hover:translate-x-1.5 transition-transform duration-200" />
              <span className="absolute -inset-1 rounded-full bg-gradient-to-r from-sky-400 to-cyan-300 opacity-0 group-hover:opacity-30 blur-md transition-opacity"></span>
            </button>
          </div>

          {/* Feature Badges */}
          <div className="pt-6 flex flex-wrap items-center gap-6 text-xs text-sky-100/95 font-medium">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-300" />
              <span>Bảo Mật Zero-Trust</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-300" />
              <span>Real-time SignalR WebSockets</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-4 h-4 text-emerald-300" />
              <span>Hợp Nhất 5 Microservices</span>
            </div>
          </div>
        </div>

        {/* Right Column: Interactive 3D Holographic Stage */}
        <div className="flex-1 w-full max-w-xl h-[520px] relative [perspective:1200px] flex items-center justify-center">
          <div
            className="relative w-full h-full flex items-center justify-center [transform-style:preserve-3d] transition-transform duration-150 ease-out"
            style={{ transform: `rotateX(${rotX}deg) rotateY(${rotY}deg)` }}
          >
            {/* Multi-ring Glowing Podium Base */}
            <div
              className="absolute bottom-6 w-96 h-28 [transform-style:preserve-3d]"
              style={{ transform: 'rotateX(68deg) translateZ(-60px)' }}
            >
              <div className="absolute inset-0 rounded-full border-4 border-sky-300/80 bg-gradient-to-b from-sky-400/40 to-blue-600/70 shadow-[0_0_60px_rgba(56,189,248,0.7)] animate-pulse"></div>
              <div className="absolute inset-5 rounded-full border-2 border-white/80 bg-gradient-to-t from-sky-500/50 to-blue-800/80 shadow-[inset_0_0_30px_rgba(255,255,255,0.6)]"></div>
              <div className="absolute inset-10 rounded-full bg-gradient-to-tr from-sky-300 to-white/90 shadow-2xl flex items-center justify-center">
                <div className="w-16 h-16 rounded-full bg-blue-600/40 blur-md"></div>
              </div>
            </div>

            {/* 3D Holographic Enterprise Professionals (Male Leader) */}
            <div
              className="absolute bottom-24 -left-6 z-10 [transform-style:preserve-3d] pointer-events-none"
              style={{ transform: 'translateZ(40px)' }}
            >
              <div className="relative flex flex-col items-center">
                <div className="w-24 h-48 rounded-2xl bg-gradient-to-b from-sky-200/35 via-sky-400/40 to-blue-600/60 backdrop-blur-md border border-sky-300/80 p-2.5 flex flex-col items-center justify-between shadow-xl shadow-sky-500/30">
                  <div className="w-11 h-11 rounded-full bg-gradient-to-tr from-sky-100 to-white flex items-center justify-center shadow-inner">
                    <UserCheck className="w-6 h-6 text-sky-800" />
                  </div>
                  <div className="text-[10px] font-bold text-white uppercase tracking-wider bg-sky-900/70 px-2 py-0.5 rounded-full border border-sky-300/60">
                    Leader
                  </div>
                  <div className="w-full text-center text-[9px] text-sky-100 font-medium">Enterprise Collab</div>
                </div>
                <div className="w-16 h-4 bg-sky-400/60 blur-sm rounded-full -mt-2"></div>
              </div>
            </div>

            {/* 3D Holographic Enterprise Professionals (Female Specialist) */}
            <div
              className="absolute bottom-28 -right-4 z-10 [transform-style:preserve-3d] pointer-events-none"
              style={{ transform: 'translateZ(30px)' }}
            >
              <div className="relative flex flex-col items-center">
                <div className="w-24 h-44 rounded-2xl bg-gradient-to-b from-indigo-200/35 via-indigo-400/40 to-sky-600/60 backdrop-blur-md border border-indigo-300/80 p-2.5 flex flex-col items-center justify-between shadow-xl shadow-indigo-500/30">
                  <div className="w-11 h-11 rounded-full bg-gradient-to-tr from-indigo-100 to-white flex items-center justify-center shadow-inner">
                    <Users className="w-6 h-6 text-indigo-800" />
                  </div>
                  <div className="text-[10px] font-bold text-white uppercase tracking-wider bg-indigo-900/70 px-2 py-0.5 rounded-full border border-indigo-300/60">
                    Specialist
                  </div>
                  <div className="w-full text-center text-[9px] text-indigo-100 font-medium">Secure Remote</div>
                </div>
                <div className="w-16 h-4 bg-indigo-400/60 blur-sm rounded-full -mt-2"></div>
              </div>
            </div>

            {/* Central Floating 3D Crystal Logo Cube */}
            <div
              onClick={onOpenLogin}
              className="relative z-20 w-44 h-44 [transform-style:preserve-3d] animate-bounce [animation-duration:5s] cursor-pointer"
            >
              <div className="w-full h-full rounded-3xl bg-gradient-to-br from-white/60 via-sky-400/40 to-blue-600/50 backdrop-blur-xl border-2 border-white/80 shadow-[0_0_40px_rgba(56,189,248,0.6)] flex flex-col items-center justify-center p-4 hover:scale-105 transition-transform duration-300">
                <Radio className="w-20 h-20 text-white drop-shadow-[0_0_25px_rgba(255,255,255,0.9)]" />
                <span className="mt-2 text-xs font-black tracking-widest text-white uppercase drop-shadow-md">
                  Enterprise
                </span>
              </div>
              <div className="absolute -bottom-16 left-1/2 -translate-x-1/2 w-10 h-24 bg-gradient-to-b from-sky-400/80 to-transparent blur-md pointer-events-none"></div>
            </div>

            {/* 3D Orbiting Glass Icon Cubes */}
            <div className="absolute inset-0 flex items-center justify-center [transform-style:preserve-3d] pointer-events-none">
              <div className="w-[430px] h-[175px] rounded-full border-2 border-sky-300/50 [transform:rotateX(72deg)] shadow-[0_0_30px_rgba(56,189,248,0.5)]"></div>

              {[
                { icon: MessageSquare, label: 'Chat', angle: 0 },
                { icon: Video, label: 'Video', angle: 51 },
                { icon: Monitor, label: 'Remote', angle: 102 },
                { icon: Headphones, label: 'Support', angle: 154 },
                { icon: ShieldCheck, label: 'Security', angle: 205 },
                { icon: Users, label: 'Team', angle: 257 },
                { icon: Phone, label: 'Voice', angle: 308 },
              ].map(({ icon: Icon, label, angle }) => (
                <div
                  key={label}
                  onClick={onOpenLogin}
                  className="absolute [transform-style:preserve-3d] pointer-events-auto cursor-pointer group"
                  style={{ transform: `rotateY(${angle}deg) translateZ(200px) rotateY(-${angle}deg)` }}
                >
                  <div className="w-14 h-14 rounded-2xl bg-white/30 backdrop-blur-md border border-white/60 shadow-lg flex items-center justify-center text-white group-hover:scale-125 group-hover:bg-white/45 transition-all duration-200">
                    <Icon className="w-7 h-7 drop-shadow" />
                  </div>
                  <span className="absolute -bottom-6 left-1/2 -translate-x-1/2 text-[10px] font-bold text-white bg-sky-900/80 px-2 py-0.5 rounded-md opacity-0 group-hover:opacity-100 transition-opacity">
                    {label}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};
