import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { TokenService } from '../../../core/services/token.service';
import { ChatMessage } from '../models/chat.models';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private api = inject(ApiService);
  private tokenService = inject(TokenService);

  messages = signal<ChatMessage[]>([]);
  streaming = signal(false);
  selectedDocumentId = signal<number | null>(null);

  private eventSource: EventSource | null = null;

  async sendMessage(query: string): Promise<void> {
    if (!query.trim() || this.streaming()) return;

    this.messages.update(msgs => [
      ...msgs,
      { role: 'user', content: query, timestamp: new Date() }
    ]);

    const assistantMessage: ChatMessage = { role: 'assistant', content: '', timestamp: new Date() };
    this.messages.update(msgs => [...msgs, assistantMessage]);
    this.streaming.set(true);

    const params: Record<string, string | number | boolean> = { query };
    const docId = this.selectedDocumentId();
    if (docId !== null) params['documentId'] = docId;

    const token = this.tokenService.getToken();
    const url = this.buildSseUrl(params, token);

    this.eventSource?.close();
    this.eventSource = new EventSource(url);

    this.eventSource.onmessage = (event) => {
      if (event.data === '[DONE]') {
        this.streaming.set(false);
        this.eventSource?.close();
        return;
      }
      this.messages.update(msgs => {
        const updated = [...msgs];
        const last = updated[updated.length - 1];
        if (last.role === 'assistant') {
          updated[updated.length - 1] = { ...last, content: last.content + event.data };
        }
        return updated;
      });
    };

    this.eventSource.onerror = () => {
      this.streaming.set(false);
      this.eventSource?.close();
      this.messages.update(msgs => {
        const updated = [...msgs];
        const last = updated[updated.length - 1];
        if (last.role === 'assistant' && !last.content) {
          updated[updated.length - 1] = { ...last, content: 'Error: could not reach the server.' };
        }
        return updated;
      });
    };
  }

  private buildSseUrl(params: Record<string, string | number | boolean>, token: string | null): string {
    const base = this.api.getSseUrl('chat/stream', params);
    return token ? `${base}&access_token=${encodeURIComponent(token)}` : base;
  }

  clearMessages(): void {
    this.messages.set([]);
  }
}
