import * as signalR from '@microsoft/signalr';
import { ENV } from '@/core/config/env';
import { tokenStorage } from '@/core/storage/tokenStorage';

export type ConnectionState = 'Disconnected' | 'Connecting' | 'Connected' | 'Reconnecting';

export class SignalRManager {
  private chatHub: signalR.HubConnection | null = null;
  private stateListeners: Set<(state: ConnectionState) => void> = new Set();
  private currentState: ConnectionState = 'Disconnected';

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

    this.chatHub.onreconnecting(() => this.setState('Reconnecting'));
    this.chatHub.onreconnected(() => this.setState('Connected'));
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
}

export const signalRManager = new SignalRManager();
