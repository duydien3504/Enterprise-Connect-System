import { useState, useEffect, useCallback, useMemo } from 'react';
import { contactsApi } from '../api/contactsApi';
import { tokenStorage } from '@/core/storage/tokenStorage';
import type {
  ContactItem,
  UserListItem,
  RelationStatus,
} from '../types/contact.types';

export type SubTabType = 'accepted' | 'pending' | 'directory' | 'blocked';
export type PendingSubTab = 'received' | 'sent';

export interface FeedbackNotification {
  type: 'success' | 'error';
  text: string;
}

export function useContactsManager() {
  const currentUser = tokenStorage.getUser();
  const currentUserId = currentUser?.id;

  // Sub-tab states
  const [activeSubTab, setActiveSubTab] = useState<SubTabType>('accepted');
  const [pendingSubTab, setPendingSubTab] = useState<PendingSubTab>('received');

  // Contact list datasets
  const [acceptedContacts, setAcceptedContacts] = useState<ContactItem[]>([]);
  const [pendingReceivedContacts, setPendingReceivedContacts] = useState<ContactItem[]>([]);
  const [pendingSentContacts, setPendingSentContacts] = useState<ContactItem[]>([]);
  const [blockedContacts, setBlockedContacts] = useState<ContactItem[]>([]);

  // Directory dataset & filters
  const [directoryUsers, setDirectoryUsers] = useState<UserListItem[]>([]);
  const [directorySearchQuery, setDirectorySearchQuery] = useState('');
  const [directoryDeptFilter, setDirectoryDeptFilter] = useState('');
  const [directoryPage, setDirectoryPage] = useState(1);
  const [directoryTotalPages, setDirectoryTotalPages] = useState(1);
  const [directoryTotalCount, setDirectoryTotalCount] = useState(0);

  // Status flags
  const [isLoading, setIsLoading] = useState(false);
  const [actionLoadingId, setActionLoadingId] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<FeedbackNotification | null>(null);

  // Auto clear feedback after 4 seconds
  useEffect(() => {
    if (!feedback) return;
    const timer = setTimeout(() => setFeedback(null), 4000);
    return () => clearTimeout(timer);
  }, [feedback]);

  // Load Contacts list based on active tab
  const loadContacts = useCallback(async () => {
    try {
      setIsLoading(true);
      // Concurrently fetch all relationship lists to keep badges & cross-referencing accurate
      const [accepted, received, sent, blocked] = await Promise.all([
        contactsApi.getContacts('accepted').catch(() => [] as ContactItem[]),
        contactsApi.getContacts('pending_received').catch(() => [] as ContactItem[]),
        contactsApi.getContacts('pending_sent').catch(() => [] as ContactItem[]),
        contactsApi.getContacts('blocked').catch(() => [] as ContactItem[]),
      ]);

      setAcceptedContacts(accepted);
      setPendingReceivedContacts(received);
      setPendingSentContacts(sent);
      setBlockedContacts(blocked);
    } catch {
      setFeedback({ type: 'error', text: 'Không thể tải danh sách danh bạ. Vui lòng thử lại.' });
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Load Directory Users
  const loadDirectoryUsers = useCallback(
    async (query = directorySearchQuery, dept = directoryDeptFilter, page = directoryPage) => {
      try {
        setIsLoading(true);
        const result = await contactsApi.searchUsers({
          name: query.trim() || undefined,
          departmentName: dept.trim() || undefined,
          page,
          pageSize: 12,
        });

        setDirectoryUsers(result.items || []);
        setDirectoryTotalCount(result.totalCount || 0);
        setDirectoryPage(result.page || 1);
        setDirectoryTotalPages(result.totalPages || 1);
      } catch {
        setFeedback({ type: 'error', text: 'Không thể tra cứu danh bạ nhân sự. Vui lòng thử lại.' });
      } finally {
        setIsLoading(false);
      }
    },
    [directorySearchQuery, directoryDeptFilter, directoryPage]
  );

  // Initial load
  useEffect(() => {
    loadContacts();
  }, [loadContacts]);

  // Trigger search when sub-tab is directory or search params change with debounce
  useEffect(() => {
    if (activeSubTab !== 'directory') return;

    const handler = setTimeout(() => {
      loadDirectoryUsers(directorySearchQuery, directoryDeptFilter, directoryPage);
    }, 350);

    return () => clearTimeout(handler);
  }, [activeSubTab, directorySearchQuery, directoryDeptFilter, directoryPage, loadDirectoryUsers]);

  // Cross-reference map for quick lookup: targetUserId -> { status, contactId }
  const relationLookup = useMemo(() => {
    const map = new Map<string, { status: RelationStatus; contactId?: string }>();

    for (const c of acceptedContacts) {
      map.set(c.userId, { status: 'accepted', contactId: c.contactId });
    }
    for (const c of pendingReceivedContacts) {
      map.set(c.userId, { status: 'pending_received', contactId: c.contactId });
    }
    for (const c of pendingSentContacts) {
      map.set(c.userId, { status: 'pending_sent', contactId: c.contactId });
    }
    for (const c of blockedContacts) {
      map.set(c.userId, { status: 'blocked', contactId: c.contactId });
    }

    return map;
  }, [acceptedContacts, pendingReceivedContacts, pendingSentContacts, blockedContacts]);

  const getRelationStatus = useCallback(
    (targetUserId: string): { status: RelationStatus; contactId?: string } => {
      if (currentUserId && targetUserId.toLowerCase() === currentUserId.toLowerCase()) {
        return { status: 'self' };
      }
      return relationLookup.get(targetUserId) || { status: 'none' };
    },
    [currentUserId, relationLookup]
  );

  // Mutations
  const handleSendRequest = async (receiverId: string) => {
    try {
      setActionLoadingId(receiverId);
      await contactsApi.sendRequest(receiverId);
      setFeedback({ type: 'success', text: 'Đã gửi lời mời kết bạn thành công.' });
      await loadContacts();
    } catch {
      setFeedback({ type: 'error', text: 'Không thể gửi lời mời kết bạn. Vui lòng kiểm tra lại.' });
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleRespondRequest = async (contactId: string, accept: boolean) => {
    try {
      setActionLoadingId(contactId);
      await contactsApi.respondRequest(contactId, accept);
      setFeedback({
        type: 'success',
        text: accept ? 'Đã chấp nhận kết nối bạn bè.' : 'Đã từ chối lời mời kết bạn.',
      });
      await loadContacts();
    } catch {
      setFeedback({ type: 'error', text: 'Không thể xử lý phản hồi. Vui lòng thử lại.' });
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleRemoveContact = async (targetUserId: string, fullName?: string) => {
    try {
      setActionLoadingId(targetUserId);
      await contactsApi.removeContact(targetUserId);
      setFeedback({
        type: 'success',
        text: fullName ? `Đã xóa ${fullName} khỏi danh bạ.` : 'Đã xóa liên hệ khỏi danh bạ.',
      });
      await loadContacts();
    } catch {
      setFeedback({ type: 'error', text: 'Không thể xóa liên hệ. Vui lòng thử lại.' });
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleBlockUser = async (targetUserId: string, fullName?: string) => {
    try {
      setActionLoadingId(targetUserId);
      await contactsApi.blockUser(targetUserId);
      setFeedback({
        type: 'success',
        text: fullName ? `Đã chặn ${fullName}.` : 'Đã chặn người dùng.',
      });
      await loadContacts();
      if (directoryUsers.length > 0) {
        await loadDirectoryUsers(directorySearchQuery, directoryDeptFilter, directoryPage);
      }
    } catch {
      setFeedback({ type: 'error', text: 'Không thể chặn người dùng. Vui lòng thử lại.' });
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleUnblockUser = async (targetUserId: string, fullName?: string) => {
    try {
      setActionLoadingId(targetUserId);
      await contactsApi.unblockUser(targetUserId);
      setFeedback({
        type: 'success',
        text: fullName ? `Đã bỏ chặn ${fullName}.` : 'Đã bỏ chặn thành công.',
      });
      await loadContacts();
      if (directoryUsers.length > 0) {
        await loadDirectoryUsers(directorySearchQuery, directoryDeptFilter, directoryPage);
      }
    } catch {
      setFeedback({ type: 'error', text: 'Không thể bỏ chặn người dùng. Vui lòng thử lại.' });
    } finally {
      setActionLoadingId(null);
    }
  };

  return {
    currentUser,
    activeSubTab,
    setActiveSubTab,
    pendingSubTab,
    setPendingSubTab,
    // Data
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
    // Flags
    isLoading,
    actionLoadingId,
    feedback,
    setFeedback,
    // Methods
    getRelationStatus,
    loadContacts,
    loadDirectoryUsers,
    handleSendRequest,
    handleRespondRequest,
    handleRemoveContact,
    handleBlockUser,
    handleUnblockUser,
  };
}

