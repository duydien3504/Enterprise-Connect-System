import React, { useState, useEffect } from 'react';
import {
  MessageSquare,
  Users,
  PhoneCall,
  Monitor,
  ShieldAlert,
  Server,
  KeyRound,
  ExternalLink,
  CheckCircle2,
  XCircle,
  Radio,
  Send,
  LogOut,
} from 'lucide-react';
import { ENV } from '@/core/config/env';
import { signalRManager, type ConnectionState } from '@/core/signalr/signalRManager';
import { tokenStorage } from '@/core/storage/tokenStorage';
import { Button } from '@/shared/components/Button';
import { Input } from '@/shared/components/Input';
import { Avatar } from '@/shared/components/Avatar';
import { Badge } from '@/shared/components/Badge';

export const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'chat' | 'contacts' | 'calls' | 'remote' | 'admin'>('chat');
  const [signalRState, setSignalRState] = useState<ConnectionState>('Disconnected');
  const [email, setEmail] = useState('admin@enterprise.com');
  const [password, setPassword] = useState('Password123!');
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [loginLoading, setLoginLoading] = useState(false);
  const [gatewayStatus, setGatewayStatus] = useState<'checking' | 'healthy' | 'error'>('checking');
  const [messages, setMessages] = useState<Array<{ id: string; sender: string; text: string; time: string }>>([
    { id: '1', sender: 'System Gateway', text: 'Chào mừng bạn đến với EnterpriseConnect! API Gateway https://api.hugonef.id.vn đã sẵn sàng.', time: '10:00' },
  ]);
  const [inputMsg, setInputMsg] = useState('');

  // Check Gateway Health on mount
  useEffect(() => {
    fetch(`${ENV.API_BASE_URL}/health/live`)
      .then((res) => (res.ok ? setGatewayStatus('healthy') : setGatewayStatus('error')))
      .catch(() => setGatewayStatus('error'));

    const unsubscribe = signalRManager.subscribeState(setSignalRState);
    if (tokenStorage.getAccessToken()) {
      setIsLoggedIn(true);
    }
    return () => unsubscribe();
  }, []);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoginLoading(true);
    try {
      const res = await fetch(`${ENV.API_BASE_URL}/api/Auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });
      if (res.ok) {
        const data = await res.json();
        tokenStorage.setAccessToken(data.accessToken);
        setIsLoggedIn(true);
        // Connect SignalR
        try {
          await signalRManager.connectChat();
        } catch {
          // Ignore if token needs auth
        }
      } else {
        alert('Đăng nhập thất bại: Hãy kiểm tra thông tin tài khoản hoặc backend');
      }
    } catch {
      alert('Không thể kết nối API Gateway');
    } finally {
      setLoginLoading(false);
    }
  };

  const handleLogout = () => {
    tokenStorage.clear();
    setIsLoggedIn(false);
    signalRManager.disconnectChat();
  };

  const handleSendMessage = () => {
    if (!inputMsg.trim()) return;
    const newMsg = {
      id: Date.now().toString(),
      sender: isLoggedIn ? 'Tôi' : 'Khách vãng lai',
      text: inputMsg,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    };
    setMessages((prev) => [...prev, newMsg]);
    setInputMsg('');
  };

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-slate-950 text-slate-100">
      {/* 1. App Left Sidebar */}
      <aside className="w-18 flex flex-col items-center justify-between border-r border-slate-800 bg-slate-900/80 py-4 backdrop-blur-md">
        <div className="flex flex-col items-center gap-6">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-gradient-to-tr from-indigo-600 to-violet-600 shadow-lg shadow-indigo-500/20">
            <Radio className="h-6 w-6 text-white" />
          </div>

          <nav className="flex flex-col gap-2">
            {[
              { id: 'chat', icon: MessageSquare, label: 'Chat' },
              { id: 'contacts', icon: Users, label: 'Danh bạ' },
              { id: 'calls', icon: PhoneCall, label: 'Gọi thoại' },
              { id: 'remote', icon: Monitor, label: 'Remote' },
              { id: 'admin', icon: ShieldAlert, label: 'Admin' },
            ].map(({ id, icon: Icon, label }) => (
              <button
                key={id}
                onClick={() => setActiveTab(id as typeof activeTab)}
                title={label}
                className={`flex h-11 w-11 items-center justify-center rounded-xl transition-all ${
                  activeTab === id
                    ? 'bg-indigo-600 text-white shadow-md shadow-indigo-600/30'
                    : 'text-slate-400 hover:bg-slate-800 hover:text-slate-200'
                }`}
              >
                <Icon className="h-5 w-5" />
              </button>
            ))}
          </nav>
        </div>

        <div className="flex flex-col items-center gap-3">
          <Avatar name="Enterprise User" size="md" status={isLoggedIn ? 'online' : 'offline'} />
        </div>
      </aside>

      {/* 2. Main Content Area */}
      <div className="flex flex-1 flex-col overflow-hidden">
        {/* Header Bar */}
        <header className="flex h-16 items-center justify-between border-b border-slate-800 bg-slate-900/50 px-6 backdrop-blur-sm">
          <div className="flex items-center gap-3">
            <h1 className="text-lg font-bold tracking-tight text-white">EnterpriseConnect Client</h1>
            <Badge variant="primary" className="font-mono text-[11px]">
              React 19 + Vite + Tailwind
            </Badge>
          </div>

          {/* Connection Status & Swagger link */}
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2 rounded-full border border-slate-800 bg-slate-900/90 px-3 py-1.5 text-xs">
              <Server className="h-3.5 w-3.5 text-indigo-400" />
              <span className="font-mono text-slate-400">Gateway:</span>
              <span className="font-mono font-medium text-slate-200">api.hugonef.id.vn</span>
              {gatewayStatus === 'healthy' ? (
                <CheckCircle2 className="h-4 w-4 text-emerald-400" />
              ) : gatewayStatus === 'error' ? (
                <XCircle className="h-4 w-4 text-rose-400" />
              ) : (
                <span className="h-2 w-2 animate-pulse rounded-full bg-amber-400" />
              )}
            </div>

            <div className="flex items-center gap-2 rounded-full border border-slate-800 bg-slate-900/90 px-3 py-1.5 text-xs">
              <Radio className="h-3.5 w-3.5 text-violet-400" />
              <span className="font-mono text-slate-400">SignalR:</span>
              <Badge
                variant={
                  signalRState === 'Connected'
                    ? 'success'
                    : signalRState === 'Connecting'
                    ? 'warning'
                    : 'neutral'
                }
              >
                {signalRState}
              </Badge>
            </div>

            <a
              href={`${ENV.API_BASE_URL}/swagger`}
              target="_blank"
              rel="noreferrer"
              className="inline-flex items-center gap-1.5 rounded-lg border border-indigo-500/30 bg-indigo-500/10 px-3 py-1.5 text-xs font-medium text-indigo-300 hover:bg-indigo-500/20"
            >
              <span>Swagger Unified API</span>
              <ExternalLink className="h-3 w-3" />
            </a>

            {isLoggedIn && (
              <Button variant="ghost" size="sm" onClick={handleLogout} title="Đăng xuất">
                <LogOut className="h-4 w-4" />
              </Button>
            )}
          </div>
        </header>

        {/* Dynamic Body */}
        <main className="flex flex-1 overflow-hidden">
          {/* Left Column: Conversations List or Feature List */}
          <section className="w-80 border-r border-slate-800 bg-slate-900/30 p-4 flex flex-col gap-4">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold uppercase tracking-wider text-slate-400">
                {activeTab === 'chat' && 'Cuộc hội thoại'}
                {activeTab === 'contacts' && 'Danh bạ đồng nghiệp'}
                {activeTab === 'calls' && 'Cuộc gọi gần đây'}
                {activeTab === 'remote' && 'Phiên hỗ trợ từ xa'}
                {activeTab === 'admin' && 'Phân hệ quản trị'}
              </h2>
              <Badge variant="neutral">5 Services Online</Badge>
            </div>

            {/* Mock/Real conversation items */}
            <div className="flex-1 space-y-2 overflow-y-auto pr-1">
              {[
                { name: 'Kênh Thông Báo Chung', last: 'EnterpriseConnect System is Live', time: '10:12', unread: 2 },
                { name: 'Nguyễn Văn An (Dev)', last: 'Đã test xong API Gateway HTTPS', time: '09:45', unread: 0 },
                { name: 'Trần Thị Mai (QA)', last: '144 Unit Tests Passed', time: 'Hôm qua', unread: 0 },
              ].map((c, i) => (
                <div
                  key={i}
                  className="flex items-center gap-3 rounded-xl p-3 hover:bg-slate-800/60 cursor-pointer transition-colors border border-transparent hover:border-slate-700/50"
                >
                  <Avatar name={c.name} size="md" status={i === 1 ? 'online' : 'offline'} />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between">
                      <p className="truncate text-sm font-medium text-slate-200">{c.name}</p>
                      <span className="text-[11px] text-slate-500">{c.time}</span>
                    </div>
                    <p className="truncate text-xs text-slate-400">{c.last}</p>
                  </div>
                  {c.unread > 0 && <Badge variant="primary">{c.unread}</Badge>}
                </div>
              ))}
            </div>

            {/* Quick Auth Box if not logged in */}
            {!isLoggedIn && (
              <div className="rounded-xl border border-indigo-500/30 bg-indigo-950/20 p-3.5 space-y-3">
                <div className="flex items-center gap-2 text-xs font-semibold text-indigo-300">
                  <KeyRound className="h-4 w-4" />
                  <span>Đăng nhập thử nghiệm (Identity Service)</span>
                </div>
                <form onSubmit={handleLogin} className="space-y-2.5">
                  <Input
                    placeholder="email@enterprise.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="text-xs py-1.5"
                  />
                  <Input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="text-xs py-1.5"
                  />
                  <Button type="submit" size="sm" className="w-full" isLoading={loginLoading}>
                    Đăng nhập API Gateway
                  </Button>
                </form>
              </div>
            )}
          </section>

          {/* Right Column: Interactive Chat Box */}
          <section className="flex flex-1 flex-col bg-slate-950">
            {/* Chat Header */}
            <div className="flex h-14 items-center justify-between border-b border-slate-800/80 px-6 bg-slate-900/20">
              <div className="flex items-center gap-3">
                <Avatar name="Kênh Thông Báo Chung" size="sm" status="online" />
                <div>
                  <h3 className="text-sm font-semibold text-slate-200">Kênh Thông Báo Chung</h3>
                  <p className="text-[11px] text-emerald-400 flex items-center gap-1">
                    <span className="h-1.5 w-1.5 rounded-full bg-emerald-400" />
                    Kênh thời gian thực SignalR
                  </p>
                </div>
              </div>
            </div>

            {/* Messages Feed */}
            <div className="flex-1 space-y-4 overflow-y-auto p-6">
              {messages.map((m) => (
                <div
                  key={m.id}
                  className={`flex flex-col ${m.sender === 'Tôi' ? 'items-end' : 'items-start'}`}
                >
                  <div className="flex items-center gap-2 mb-1 text-xs text-slate-400">
                    <span className="font-semibold text-slate-300">{m.sender}</span>
                    <span>{m.time}</span>
                  </div>
                  <div
                    className={`max-w-md rounded-2xl px-4 py-2.5 text-sm leading-relaxed shadow-sm ${
                      m.sender === 'Tôi'
                        ? 'bg-indigo-600 text-white rounded-br-none'
                        : 'bg-slate-800/90 text-slate-200 border border-slate-700/60 rounded-bl-none'
                    }`}
                  >
                    {m.text}
                  </div>
                </div>
              ))}
            </div>

            {/* Message Input Form */}
            <div className="border-t border-slate-800 p-4 bg-slate-900/40">
              <form
                onSubmit={(e) => {
                  e.preventDefault();
                  handleSendMessage();
                }}
                className="flex items-center gap-3"
              >
                <Input
                  value={inputMsg}
                  onChange={(e) => setInputMsg(e.target.value)}
                  placeholder="Nhập nội dung tin nhắn gửi vào hệ thống..."
                  className="flex-1"
                />
                <Button type="submit" variant="primary" disabled={!inputMsg.trim()}>
                  <Send className="h-4 w-4" />
                  <span>Gửi</span>
                </Button>
              </form>
            </div>
          </section>
        </main>
      </div>
    </div>
  );
};

export default App;
