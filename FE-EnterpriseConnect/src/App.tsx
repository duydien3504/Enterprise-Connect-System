import React, { useState, useEffect } from 'react';
import {
  MessageSquare,
  Users,
  PhoneCall,
  Monitor,
  ShieldAlert,
  CheckCircle2,
  Radio,
  LogOut,
  RefreshCw,
  Clock,
  ShieldCheck,
} from 'lucide-react';
import { signalRManager, type ConnectionState } from '@/core/signalr/signalRManager';
import { tokenStorage } from '@/core/storage/tokenStorage';
import { authApi } from '@/features/auth/api/authApi';
import { onSessionExpired } from '@/core/api/apiClient';
import { Avatar } from '@/shared/components/Avatar';
import { LandingHero3D } from '@/features/landing/components/LandingHero3D';
import { LoginModal } from '@/features/landing/components/LoginModal';
import { EnterpriseChatModule } from '@/features/chat/components/EnterpriseChatModule';
import { UserProfileView } from '@/features/profile/components/UserProfileView';
import type { SessionType, UserProfile } from '@/features/auth/types/auth.types';

export const App: React.FC = () => {
  // Synchronously restore session from client cookie storage on F5 page reload
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(() => tokenStorage.hasActiveSession());
  const [isLoginModalOpen, setIsLoginModalOpen] = useState(false);
  const [sessionType, setSessionType] = useState<SessionType>(() => tokenStorage.getSessionType());
  const [timeToExpiry, setTimeToExpiry] = useState<number>(() => tokenStorage.getTimeToExpiry());
  const [isRefreshingToken, setIsRefreshingToken] = useState(false);
  const [refreshNotice, setRefreshNotice] = useState<string | null>(null);
  const [currentUser, setCurrentUser] = useState<UserProfile | null>(() => tokenStorage.getUser());
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const [activeTab, setActiveTab] = useState<'chat' | 'contacts' | 'calls' | 'remote' | 'admin' | 'profile'>('chat');
  const [signalRState, setSignalRState] = useState<ConnectionState>('Disconnected');

  // Check Token & Silent Refresh on mount (Preserving Session across F5 reloads via Hybrid Coordinated Vault)
  useEffect(() => {
    const unsubscribeSignalR = signalRManager.subscribeState(setSignalRState);

    // Initial silent refresh: Check for active session in Vault (Option A: 30 days vs Option B: session-only)
    if (tokenStorage.hasActiveSession()) {
      signalRManager.connectChat().catch(() => {});

      authApi.silentRefresh().then((authenticated) => {
        if (authenticated) {
          setTimeToExpiry(tokenStorage.getTimeToExpiry());
          setCurrentUser(tokenStorage.getUser());
          authApi.getProfile().then(setCurrentUser).catch(() => {});
        } else {
          // Only log out if silentRefresh explicitly confirmed session revocation from server
          setIsLoggedIn(false);
          signalRManager.disconnectChat();
          setCurrentUser(null);
        }
      });
    }

    const unsubscribeExpired = onSessionExpired(() => {
      setIsLoggedIn(false);
      signalRManager.disconnectChat();
      setCurrentUser(null);
    });

    // Multi-tab Session Synchronization
    const unsubscribeBroadcast = tokenStorage.onAuthBroadcast((event) => {
      if (event.type === 'LOGOUT') {
        setIsLoggedIn(false);
        signalRManager.disconnectChat();
        setCurrentUser(null);
      } else if (event.type === 'LOGIN' || event.type === 'REFRESH') {
        setIsLoggedIn(true);
        setCurrentUser(tokenStorage.getUser());
        setTimeToExpiry(tokenStorage.getTimeToExpiry());
        signalRManager.connectChat().catch(() => {});
      }
    });

    return () => {
      unsubscribeSignalR();
      unsubscribeExpired();
      unsubscribeBroadcast();
    };
  }, []);

  // Live countdown ticker for the in-memory RAM Access Token
  useEffect(() => {
    if (!isLoggedIn) return;

    const interval = setInterval(() => {
      const remaining = tokenStorage.getTimeToExpiry();
      setTimeToExpiry(remaining);

      if (remaining === 0 && tokenStorage.isTokenExpired()) {
        authApi.refreshToken().catch(() => {
          if (!tokenStorage.hasActiveSession()) {
            setIsLoggedIn(false);
            setCurrentUser(null);
          }
        });
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [isLoggedIn]);

  const handleLoginSuccess = () => {
    setIsLoggedIn(true);
    setSessionType(tokenStorage.getSessionType());
    setTimeToExpiry(tokenStorage.getTimeToExpiry());
    setCurrentUser(tokenStorage.getUser());
    signalRManager.connectChat().catch(() => {});
    authApi.getProfile().then(setCurrentUser).catch(() => {});
  };

  /**
   * Calls POST /api/Auth/logout with { refreshToken: "..." }
   * Wipes RAM token, stops scheduler, revokes session cookie
   */
  const handleLogout = async () => {
    setIsLoggingOut(true);
    try {
      await authApi.logout();
    } catch (err) {
      console.warn('[App] Logout execution finished:', err);
    } finally {
      setIsLoggedIn(false);
      signalRManager.disconnectChat();
      setCurrentUser(null);
      setIsLoggingOut(false);
      setActiveTab('chat');
    }
  };

  const handleManualRefresh = async () => {
    setIsRefreshingToken(true);
    setRefreshNotice(null);
    try {
      await authApi.refreshToken();
      setTimeToExpiry(tokenStorage.getTimeToExpiry());
      setRefreshNotice('Đã làm mới RAM Token!');
      setTimeout(() => setRefreshNotice(null), 3000);
    } catch {
      setRefreshNotice('Làm mới thất bại.');
    } finally {
      setIsRefreshingToken(false);
    }
  };

  const formatCountdown = (totalSeconds: number) => {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  };

  // If NOT logged in, show the full 3D Landing Page
  if (!isLoggedIn) {
    return (
      <>
        <LandingHero3D onOpenLogin={() => setIsLoginModalOpen(true)} />
        <LoginModal
          isOpen={isLoginModalOpen}
          onClose={() => setIsLoginModalOpen(false)}
          onSuccess={handleLoginSuccess}
        />
      </>
    );
  }

  // If logged in, show the Enterprise Workspace Portal
  return (
    <div className="flex h-screen w-screen overflow-hidden bg-slate-950 text-slate-100">
      {/* 1. App Left Rail Sidebar - Ergonomic & Softened Micro-interactions */}
      <aside className="w-18 shrink-0 flex flex-col items-center justify-between border-r border-[#1d4ed8]/20 bg-[#0c2340] py-4 select-none z-20">
        <div className="flex flex-col items-center gap-6">
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-[#0052cc] text-white shadow-lg shadow-[#0052cc]/30 hover:scale-105 transition-transform duration-200">
            <Radio className="h-5 w-5 text-white" />
          </div>

          <nav className="flex flex-col gap-3">
            {[
              { id: 'chat', icon: MessageSquare, label: 'Nhắn tin & AI Copilot' },
              { id: 'contacts', icon: Users, label: 'Danh bạ doanh nghiệp' },
              { id: 'calls', icon: PhoneCall, label: 'Cuộc gọi WebRTC' },
              { id: 'remote', icon: Monitor, label: 'Hỗ trợ từ xa Remote' },
              { id: 'admin', icon: ShieldAlert, label: 'Quản trị & Giám sát' },
            ].map(({ id, icon: Icon, label }) => (
              <button
                key={id}
                onClick={() => setActiveTab(id as typeof activeTab)}
                title={label}
                className={`relative group flex h-11 w-11 items-center justify-center rounded-xl transition-all duration-200 ease-out cursor-pointer ${
                  activeTab === id
                    ? 'bg-[#0052cc] text-white shadow-md shadow-[#0052cc]/40 scale-105'
                    : 'text-slate-400 hover:bg-[#122e54] hover:text-white hover:scale-105 active:scale-95'
                }`}
              >
                <Icon className="h-5 w-5 transition-transform duration-200 group-hover:scale-110" />
                {activeTab === id && (
                  <span className="absolute left-0 w-1 h-5 rounded-r bg-[#38bdf8]" />
                )}
              </button>
            ))}
          </nav>
        </div>

        <div className="flex flex-col items-center gap-3">
          <div
            onClick={() => setActiveTab('profile')}
            title="Hồ sơ cá nhân & Đặc quyền (GET /api/Users/profile)"
            className={`relative p-0.5 rounded-full transition-all duration-200 cursor-pointer ${
              activeTab === 'profile'
                ? 'ring-2 ring-[#38bdf8] scale-105 shadow-md shadow-[#0052cc]/50'
                : 'hover:scale-110 active:scale-95'
            }`}
          >
            <Avatar name={currentUser?.fullName || "Enterprise User"} src={currentUser?.avatarUrl} size="md" status="online" />
            {activeTab === 'profile' && (
              <span className="absolute -top-1 -right-1 w-3 h-3 rounded-full bg-[#38bdf8] border-2 border-[#0c2340]" />
            )}
          </div>
        </div>
      </aside>

      {/* 2. Main Workspace Layout */}
      <div className="flex flex-1 flex-col overflow-hidden">
        {/* Header Bar - Harmonized & Clean */}
        <header className="h-16 px-6 bg-[#07172b] border-b border-slate-800 flex items-center justify-between shrink-0 z-10 select-none">
          {/* Left Title */}
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2.5">
              <div className="w-8 h-8 rounded-xl bg-[#0052cc] flex items-center justify-center text-white font-bold text-xs shadow-md">
                EC
              </div>
              <div className="flex flex-col">
                <h1 className="text-sm font-bold tracking-tight text-white leading-tight">
                  Enterprise Connection
                </h1>
                <span className="text-xs text-[#38bdf8] font-medium leading-tight">
                  {activeTab === 'chat' && 'Nhắn Tin Doanh Nghiệp Thời Gian Thực & AI Copilot'}
                  {activeTab === 'contacts' && 'Danh Bạ Doanh Nghiệp'}
                  {activeTab === 'calls' && 'Cuộc Gọi WebRTC E2E'}
                  {activeTab === 'remote' && 'Hỗ Trợ Từ Xa'}
                  {activeTab === 'admin' && 'Trung Tâm Giám Sát An Ninh'}
                  {activeTab === 'profile' && 'Hồ Sơ Cá Nhân & Quản Trị Đặc Quyền'}
                </span>
              </div>
            </div>
          </div>

          {/* Right Status Cluster - UNIFIED SignalR, Session, RAM Countdown & Actions */}
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-1.5 p-1 rounded-xl bg-[#0c2340] border border-[#1d4ed8]/40 shadow-sm text-xs">
              {/* SignalR status */}
              <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-[#07172b] border border-slate-700/60 font-medium text-slate-300">
                <Radio className="h-3.5 w-3.5 text-[#38bdf8]" />
                <span className="font-semibold text-emerald-400 flex items-center gap-1">
                  <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" />
                  {signalRState}
                </span>
              </div>

              {/* Session type */}
              <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-[#07172b] border border-slate-700/60 font-medium">
                {sessionType === 'persistent' ? (
                  <span className="text-emerald-400 flex items-center gap-1.5">
                    <CheckCircle2 className="w-3.5 h-3.5" />
                    <span>Duy trì (30 ngày)</span>
                  </span>
                ) : (
                  <span className="text-amber-400 flex items-center gap-1.5">
                    <Clock className="w-3.5 h-3.5" />
                    <span>Tạm thời (8h)</span>
                  </span>
                )}
              </div>

              {/* Token live countdown */}
              <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-[#07172b] border border-slate-700/60 font-mono font-medium text-[#38bdf8]">
                <ShieldCheck className="w-3.5 h-3.5 text-[#38bdf8]" />
                <span>Phiên: {formatCountdown(timeToExpiry)}</span>
              </div>

              {/* Manual Refresh Action */}
              <button
                onClick={handleManualRefresh}
                disabled={isRefreshingToken}
                title="Làm mới phiên làm việc ngay lập tức"
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold text-white bg-[#0052cc] hover:bg-[#0065ff] hover:scale-105 active:scale-95 transition-all duration-200 cursor-pointer disabled:opacity-50 shadow-sm"
              >
                <RefreshCw className={`w-3.5 h-3.5 ${isRefreshingToken ? 'animate-spin' : ''}`} />
                <span className="hidden sm:inline">Làm mới</span>
              </button>
            </div>

            {refreshNotice && (
              <span className="text-xs text-emerald-400 font-medium animate-in fade-in">
                {refreshNotice}
              </span>
            )}

            {/* Profile User Chip Button */}
            <button
              onClick={() => setActiveTab('profile')}
              title="Xem hồ sơ cá nhân"
              className={`hidden sm:flex items-center gap-2 px-3 py-1.5 rounded-xl border text-xs font-semibold transition-all duration-200 cursor-pointer hover:scale-105 active:scale-95 ${
                activeTab === 'profile'
                  ? 'bg-[#122e54] border-[#38bdf8] text-white shadow-sm'
                  : 'bg-[#0c2340] border-[#1d4ed8]/40 text-slate-300 hover:bg-[#122e54] hover:text-white'
              }`}
            >
              <Avatar
                name={currentUser?.fullName || "Enterprise User"}
                src={currentUser?.avatarUrl}
                size="xs"
              />
              <span className="max-w-[140px] truncate">{currentUser?.fullName || 'Hồ sơ cá nhân'}</span>
            </button>

            {/* Logout button */}
            <button
              onClick={handleLogout}
              disabled={isLoggingOut}
              title="Đăng xuất khỏi hệ thống"
              className="h-9 w-9 rounded-xl flex items-center justify-center text-slate-400 hover:text-rose-300 hover:bg-rose-950/40 border border-transparent hover:border-rose-500/30 transition-all duration-200 cursor-pointer hover:scale-105 active:scale-95 disabled:opacity-50"
            >
              {isLoggingOut ? <RefreshCw className="h-4 w-4 animate-spin text-rose-400" /> : <LogOut className="h-4 w-4" />}
            </button>
          </div>
        </header>

        {/* Workspace Body: Dynamic View */}
        <main className="flex flex-1 overflow-hidden">
          {activeTab === 'chat' && <EnterpriseChatModule />}
          {activeTab === 'profile' && (
            <UserProfileView
              onBackToWorkspace={() => setActiveTab('chat')}
              onLogout={handleLogout}
              isLoggingOut={isLoggingOut}
              onProfileUpdated={(updated) => setCurrentUser(updated)}
            />
          )}
          {activeTab !== 'chat' && activeTab !== 'profile' && (
            <div className="flex-1 flex flex-col items-center justify-center p-8 text-center bg-[#071e3d] text-slate-300">
              <div className="w-16 h-16 rounded-2xl bg-[#0c2340] border border-[#1d4ed8]/40 flex items-center justify-center text-[#38bdf8] mb-4 shadow-xl">
                {activeTab === 'contacts' && <Users className="w-8 h-8" />}
                {activeTab === 'calls' && <PhoneCall className="w-8 h-8" />}
                {activeTab === 'remote' && <Monitor className="w-8 h-8" />}
                {activeTab === 'admin' && <ShieldAlert className="w-8 h-8" />}
              </div>
              <h3 className="text-lg font-bold text-white mb-2">
                {activeTab === 'contacts' && 'Danh Bạ Doanh Nghiệp Tập Trung'}
                {activeTab === 'calls' && 'Hội Thoại & Video Call WebRTC'}
                {activeTab === 'remote' && 'Hỗ Trợ Từ Xa & Điều Khiển Màn Hình'}
                {activeTab === 'admin' && 'Trung Tâm Giám Sát An Ninh & Audit Forensics'}
              </h3>
              <p className="text-xs text-slate-400 max-w-md mb-6 leading-relaxed">
                Module này đã được đặc tả luồng E2E và kết nối với các microservice chuyên biệt. Bấm vào tab "Nhắn tin & AI Copilot" để trải nghiệm module nhắn tin 3 cột hoàn chỉnh.
              </p>
              <button
                onClick={() => setActiveTab('chat')}
                className="px-5 py-2.5 rounded-lg bg-[#0052cc] hover:bg-[#0065ff] text-white font-bold text-xs shadow-md transition-all duration-200 hover:scale-105 active:scale-95 cursor-pointer"
              >
                Mở Module Nhắn Tin Doanh Nghiệp & AI Copilot
              </button>
            </div>
          )}
        </main>
      </div>
    </div>
  );
};

export default App;
