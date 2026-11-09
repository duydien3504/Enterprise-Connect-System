import * as signalR from '@microsoft/signalr';
import { ENV } from '@/core/config/env';
import { tokenStorage } from '@/core/storage/tokenStorage';

export type ConnectionState = 'Disconnected' | 'Connecting' | 'Connected' | 'Reconnecting';

export type MessageListener = (message: any) => void;
export type TypingListener = (conversationId: string, userId: string, isTyping: boolean) => void;
export type PinnedListener = (messageId: string, isPinned: boolean, pinnedBy?: string) => void;
export type ReactedListener = (messageId: string, userId: string, emoji: string, isAdded: boolean) => void;
export type ToxicListener = (warningMessage: string) => void;

export class SignalRManager {
  private chatHub: signalR.HubConnection | null = null;
  private stateListeners: Set<(state: ConnectionState) => void> = new Set();
  private currentState: ConnectionState = 'Disconnected';

  private messageListeners: Set<MessageListener> = new Set();
  private typingListeners: Set<TypingListener> = new Set();
  private pinnedListeners: Set<PinnedListener> = new Set();
  private reactedListeners: Set<ReactedListener> = new Set();
  private toxicListeners: Set<ToxicListener> = new Set();
  private activeConversationGroups: Set<string> = new Set();

  public getState(): ConnectionState {
    return this.currentState;
  }

  public subscribeState(listener: (state: ConnectionState) => void): () => void {
    this.stateListeners.add(listener);
    listener(this.currentState);
    return () => this.stateListeners.delete(listener);
  }

  private setState(newState: ConnectionState) {
    this.currentState = newState;
    this.stateListeners.forEach((listener) => listener(newState));
  }

  public async connectChat(): Promise<signalR.HubConnection> {
    if (this.chatHub && this.chatHub.state === signalR.HubConnectionState.Connected) {
      return this.chatHub;
    }

    this.setState('Connecting');

    this.chatHub = new signalR.HubConnectionBuilder()
      .withUrl(ENV.SIGNALR.CHAT_HUB, {
        accessTokenFactory: () => tokenStorage.getAccessToken() || '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // Register Hub Events
    this.chatHub.on('ReceiveMessage', (message) => {
      this.messageListeners.forEach((listener) => listener(message));
    });

    this.chatHub.on('UserTyping', (conversationId, userId, isTyping) => {
      this.typingListeners.forEach((listener) => listener(conversationId, userId, isTyping));
    });

    this.chatHub.on('MessagePinned', (messageId, isPinned, pinnedBy) => {
      this.pinnedListeners.forEach((listener) => listener(messageId, isPinned, pinnedBy));
    });

    this.chatHub.on('MessageReacted', (messageId, userId, emoji, isAdded) => {
      this.reactedListeners.forEach((listener) => listener(messageId, userId, emoji, isAdded));
    });

    this.chatHub.on('ToxicWarning', (warningMessage) => {
      this.toxicListeners.forEach((listener) => listener(warningMessage));
    });

    this.chatHub.onreconnecting(() => this.setState('Reconnecting'));
    this.chatHub.onreconnected(async () => {
      this.setState('Connected');
      // Re-join any active conversation groups upon reconnection
      for (const convId of this.activeConversationGroups) {
        try {
          await this.chatHub?.invoke('JoinConversation', convId);
        } catch (err) {
          console.warn('[SignalR] Failed to re-join conversation:', convId, err);
        }
      }
    });

    this.chatHub.onclose(() => this.setState('Disconnected'));

    try {
      await this.chatHub.start();
      this.setState('Connected');
      return this.chatHub;
    } catch (err) {
      this.setState('Disconnected');
      throw err;
    }
  }

  public async disconnectChat(): Promise<void> {
    if (this.chatHub) {
      await this.chatHub.stop();
      this.chatHub = null;
      this.setState('Disconnected');
    }
  }

  public getChatConnection(): signalR.HubConnection | null {
    return this.chatHub;
  }

  public async joinConversation(conversationId: string): Promise<void> {
    if (!conversationId) return;
    this.activeConversationGroups.add(conversationId);
    if (this.chatHub && this.chatHub.state === signalR.HubConnectionState.Connected) {
      try {
        await this.chatHub.invoke('JoinConversation', conversationId);
      } catch (err) {
        console.warn('[SignalR] Failed to join conversation:', conversationId, err);
      }
    }
  }

  public async leaveConversation(conversationId: string): Promise<void> {
    if (!conversationId) return;
    this.activeConversationGroups.delete(conversationId);
    if (this.chatHub && this.chatHub.state === signalR.HubConnectionState.Connected) {
      try {
        await this.chatHub.invoke('LeaveConversation', conversationId);
      } catch (err) {
        console.warn('[SignalR] Failed to leave conversation:', conversationId, err);
      }
    }
  }

  public async sendTyping(conversationId: string, isTyping: boolean): Promise<void> {
    if (!conversationId) return;
    if (this.chatHub && this.chatHub.state === signalR.HubConnectionState.Connected) {
      try {
        await this.chatHub.invoke('SendTyping', conversationId, isTyping);
      } catch (err) {
        console.warn('[SignalR] Failed to send typing indicator:', err);
      }
    }
  }

  public onReceiveMessage(listener: MessageListener): () => void {
    this.messageListeners.add(listener);
    return () => this.messageListeners.delete(listener);
  }

  public onUserTyping(listener: TypingListener): () => void {
    this.typingListeners.add(listener);
    return () => this.typingListeners.delete(listener);
  }

  public onMessagePinned(listener: PinnedListener): () => void {
    this.pinnedListeners.add(listener);
    return () => this.pinnedListeners.delete(listener);
  }

  public onMessageReacted(listener: ReactedListener): () => void {
    this.reactedListeners.add(listener);
    return () => this.reactedListeners.delete(listener);
  }

  public onToxicWarning(listener: ToxicListener): () => void {
    this.toxicListeners.add(listener);
    return () => this.toxicListeners.delete(listener);
  }
}

export const signalRManager = new SignalRManager();
