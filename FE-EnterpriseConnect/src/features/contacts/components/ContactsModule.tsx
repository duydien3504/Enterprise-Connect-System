import React, { useState } from 'react';
import {
  Users,
  UserCheck,
  Search,
  ShieldBan,
  CheckCircle2,
  AlertCircle,
  Sparkles,
} from 'lucide-react';
import { useContactsManager } from '../hooks/useContactsManager';
import { AcceptedContactsView } from './AcceptedContactsView';
import { PendingRequestsView } from './PendingRequestsView';
import { UserDirectoryView } from './UserDirectoryView';
import { BlockedUsersView } from './BlockedUsersView';
import { BlockConfirmModal } from './BlockConfirmModal';

interface ContactsModuleProps {
  onStartChat: (userId: string) => void;
}

export const ContactsModule: React.FC<ContactsModuleProps> = ({ onStartChat }) => {
  const {
    activeSubTab,
    setActiveSubTab,
    pendingSubTab,
    setPendingSubTab,
    // Datasets
    acceptedContacts,
    pendingReceivedContacts,
    pendingSentContacts,
    blockedContacts,
    directoryUsers,
    directorySearchQuery,
    setDirectorySearchQuery,
    directoryDeptFilter,
    setDirectoryDeptFilter,
    directoryPage,
    setDirectoryPage,
    directoryTotalPages,
    directoryTotalCount,
    // Status
    isLoading,
    actionLoadingId,
    feedback,
    // Operations
    getRelationStatus,
    loadContacts,
    loadDirectoryUsers,
    handleSendRequest,
    handleRespondRequest,
    handleRemoveContact,
    handleBlockUser,
    handleUnblockUser,
  } = useContactsManager();

  // Block / Unblock Confirmation Modal state
  const [modalState, setModalState] = useState<{
    isOpen: boolean;
    type: 'block' | 'unblock';
    targetUser: { id: string; fullName: string } | null;
  }>({
    isOpen: false,
    type: 'block',
    targetUser: null,
  });

  const handleOpenBlockModal = (id: string, fullName: string) => {
    setModalState({ isOpen: true, type: 'block', targetUser: { id, fullName } });
  };

  const handleOpenUnblockModal = (id: string, fullName: string) => {
    setModalState({ isOpen: true, type: 'unblock', targetUser: { id, fullName } });
  };

  const handleConfirmModal = async () => {
    if (!modalState.targetUser) return;
    if (modalState.type === 'block') {
      await handleBlockUser(modalState.targetUser.id, modalState.targetUser.fullName);
    } else {
      await handleUnblockUser(modalState.targetUser.id, modalState.targetUser.fullName);
    }
    setModalState({ isOpen: false, type: 'block', targetUser: null });
  };

  return (
    <div className="flex flex-1 h-full overflow-hidden bg-[#071e3d]">
      {/* 1. Left Sub-Sidebar (280px) */}
      <aside className="w-72 bg-[#07172b] border-r border-slate-800 flex flex-col shrink-0 select-none">
        {/* Module Header */}
        <div className="p-5 border-b border-slate-800/80 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-[#0052cc] to-[#1d4ed8] flex items-center justify-center text-white shadow-lg shadow-[#0052cc]/30">
              <Users className="w-5 h-5 text-white" />
            </div>
            <div>
              <h2 className="text-sm font-bold text-white tracking-tight">Danh Bạ Doanh Nghiệp</h2>
              <p className="text-[11px] text-[#38bdf8] font-medium flex items-center gap-1">
                <Sparkles className="w-3 h-3 text-[#38bdf8]" />
                <span>Mạng lưới nội bộ</span>
              </p>
            </div>
          </div>
        </div>

        {/* Sub-navigation List */}
        <nav className="p-3 space-y-1.5 flex-1 overflow-y-auto">
          {/* 1. Danh bạ bạn bè */}
          <button
            onClick={() => setActiveSubTab('accepted')}
            className={`w-full flex items-center justify-between px-3.5 py-3 rounded-xl text-xs font-semibold transition-all cursor-pointer ${
              activeSubTab === 'accepted'
                ? 'bg-[#0052cc] text-white shadow-md shadow-[#0052cc]/40'
                : 'text-slate-300 hover:bg-[#0c2340] hover:text-white'
            }`}
          >
            <div className="flex items-center gap-3">
              <Users className="w-4 h-4" />
              <span>Danh bạ bạn bè</span>
            </div>
            <span
              className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                activeSubTab === 'accepted'
                  ? 'bg-white/20 text-white'
                  : 'bg-slate-800 text-slate-300'
              }`}
            >
              {acceptedContacts.length}
            </span>
          </button>

          {/* 2. Lời mời kết bạn */}
          <button
            onClick={() => setActiveSubTab('pending')}
            className={`w-full flex items-center justify-between px-3.5 py-3 rounded-xl text-xs font-semibold transition-all cursor-pointer ${
              activeSubTab === 'pending'
                ? 'bg-[#0052cc] text-white shadow-md shadow-[#0052cc]/40'
                : 'text-slate-300 hover:bg-[#0c2340] hover:text-white'
            }`}
          >
            <div className="flex items-center gap-3">
              <UserCheck className="w-4 h-4" />
              <span>Lời mời kết bạn</span>
            </div>
            {pendingReceivedContacts.length > 0 ? (
              <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500 text-white animate-pulse">
                {pendingReceivedContacts.length}
              </span>
            ) : pendingSentContacts.length > 0 ? (
              <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-slate-800 text-slate-300">
                {pendingSentContacts.length}
              </span>
            ) : null}
          </button>

          {/* 3. Tra cứu người dùng toàn công ty */}
          <button
            onClick={() => setActiveSubTab('directory')}
            className={`w-full flex items-center justify-between px-3.5 py-3 rounded-xl text-xs font-semibold transition-all cursor-pointer ${
              activeSubTab === 'directory'
                ? 'bg-[#0052cc] text-white shadow-md shadow-[#0052cc]/40'
                : 'text-slate-300 hover:bg-[#0c2340] hover:text-white'
            }`}
          >
            <div className="flex items-center gap-3">
              <Search className="w-4 h-4" />
              <span>Tra cứu toàn công ty</span>
            </div>
            <span
              className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                activeSubTab === 'directory'
                  ? 'bg-white/20 text-white'
                  : 'bg-slate-800 text-slate-300'
              }`}
            >
              {directoryTotalCount > 0 ? directoryTotalCount : 'Mới'}
            </span>
          </button>

          {/* 4. Danh sách đã chặn */}
          <button
            onClick={() => setActiveSubTab('blocked')}
            className={`w-full flex items-center justify-between px-3.5 py-3 rounded-xl text-xs font-semibold transition-all cursor-pointer ${
              activeSubTab === 'blocked'
                ? 'bg-[#0052cc] text-white shadow-md shadow-[#0052cc]/40'
                : 'text-slate-300 hover:bg-[#0c2340] hover:text-white'
            }`}
          >
            <div className="flex items-center gap-3">
              <ShieldBan className="w-4 h-4" />
              <span>Danh sách đã chặn</span>
            </div>
            {blockedContacts.length > 0 && (
              <span
                className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                  activeSubTab === 'blocked'
                    ? 'bg-white/20 text-white'
                    : 'bg-rose-950/60 text-rose-300 border border-rose-800/40'
                }`}
              >
                {blockedContacts.length}
              </span>
            )}
          </button>
        </nav>

        {/* Footer Note */}
        <div className="p-4 border-t border-slate-800/80 bg-[#07172b]/50 text-slate-400 text-[11px] leading-relaxed">
          <p>Dữ liệu được đồng bộ trực tiếp từ Microservice Identity Service qua API Gateway.</p>
        </div>
      </aside>

      {/* 2. Right Workspace Panel */}
      <main className="flex-1 flex flex-col overflow-hidden relative">
        {/* Floating Feedback Notification */}
        {feedback && (
          <div
            className={`absolute top-4 right-4 z-50 flex items-center gap-2.5 px-4 py-2.5 rounded-xl shadow-2xl border text-xs font-semibold animate-in fade-in slide-in-from-top-3 ${
              feedback.type === 'success'
                ? 'bg-emerald-950/90 border-emerald-500/50 text-emerald-200 shadow-emerald-900/30'
                : 'bg-rose-950/90 border-rose-500/50 text-rose-200 shadow-rose-900/30'
            }`}
          >
            {feedback.type === 'success' ? (
              <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0" />
            ) : (
              <AlertCircle className="w-4 h-4 text-rose-400 shrink-0" />
            )}
            <span>{feedback.text}</span>
          </div>
        )}

        {/* Active Sub-View */}
        {activeSubTab === 'accepted' && (
          <AcceptedContactsView
            contacts={acceptedContacts}
            isLoading={isLoading}
            actionLoadingId={actionLoadingId}
            onStartChat={onStartChat}
            onRemoveContact={handleRemoveContact}
            onBlockUser={handleOpenBlockModal}
            onGoToDirectory={() => setActiveSubTab('directory')}
            onRefresh={loadContacts}
          />
        )}

        {activeSubTab === 'pending' && (
          <PendingRequestsView
            pendingSubTab={pendingSubTab}
            onSelectSubTab={setPendingSubTab}
            receivedList={pendingReceivedContacts}
            sentList={pendingSentContacts}
            isLoading={isLoading}
            actionLoadingId={actionLoadingId}
            onRespondRequest={handleRespondRequest}
            onCancelSentRequest={handleRemoveContact}
            onRefresh={loadContacts}
          />
        )}

        {activeSubTab === 'directory' && (
          <UserDirectoryView
            users={directoryUsers}
            totalCount={directoryTotalCount}
            currentPage={directoryPage}
            totalPages={directoryTotalPages}
            searchQuery={directorySearchQuery}
            departmentFilter={directoryDeptFilter}
            isLoading={isLoading}
            actionLoadingId={actionLoadingId}
            onSearchChange={setDirectorySearchQuery}
            onDepartmentChange={setDirectoryDeptFilter}
            onPageChange={(page) => {
              setDirectoryPage(page);
              loadDirectoryUsers(directorySearchQuery, directoryDeptFilter, page);
            }}
            getRelationStatus={getRelationStatus}
            onSendRequest={handleSendRequest}
            onRespondRequest={handleRespondRequest}
            onCancelSentRequest={handleRemoveContact}
            onPromptBlock={handleOpenBlockModal}
            onUnblockUser={handleOpenUnblockModal}
            onStartChat={onStartChat}
            onRefresh={() =>
              loadDirectoryUsers(directorySearchQuery, directoryDeptFilter, directoryPage)
            }
          />
        )}

        {activeSubTab === 'blocked' && (
          <BlockedUsersView
            blockedContacts={blockedContacts}
            isLoading={isLoading}
            actionLoadingId={actionLoadingId}
            onUnblockUser={handleOpenUnblockModal}
            onRefresh={loadContacts}
          />
        )}

        {/* Global Block / Unblock Confirmation Modal */}
        <BlockConfirmModal
          isOpen={modalState.isOpen}
          type={modalState.type}
          targetUser={modalState.targetUser}
          isLoading={Boolean(
            modalState.targetUser && actionLoadingId === modalState.targetUser.id
          )}
          onClose={() => setModalState({ isOpen: false, type: 'block', targetUser: null })}
          onConfirm={handleConfirmModal}
        />
      </main>
    </div>
  );
};
export default ContactsModule;

