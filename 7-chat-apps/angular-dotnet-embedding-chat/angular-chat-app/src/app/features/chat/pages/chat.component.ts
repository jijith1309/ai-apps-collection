import { Component, inject, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { DatePipe } from '@angular/common';
import { ChatService } from '../services/chat.service';
import { DocumentsService } from '../../documents/services/documents.service';
import { MarkdownPipe } from '../../../shared/pipes/markdown.pipe';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    FormsModule, ButtonModule, InputTextModule,
    SelectModule, TooltipModule, DatePipe, MarkdownPipe
  ],
  template: `
    <div class="flex flex-col" style="height: calc(100vh - 4rem); background: #f3f4f6">

      <!-- Chat toolbar -->
      <div class="flex items-center justify-between px-6 py-3 shadow-sm" style="background: #ffffff; border-bottom: 1px solid #e5e7eb">
        <div class="flex items-center gap-3">
          <div>
            <h2 class="text-base font-semibold text-color m-0">Chat with Documents</h2>
            <p class="text-xs m-0" style="color: #6b7280">AI-powered document Q&amp;A</p>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <p-select
            [options]="documentOptions()"
            [(ngModel)]="selectedDocId"
            optionLabel="label"
            optionValue="value"
            placeholder="All documents"
            (onChange)="onDocSelect($event.value)"
            class="w-56"
          />
          <p-button icon="pi pi-trash" variant="text" severity="secondary"
            size="small" pTooltip="Clear chat" (onClick)="chatService.clearMessages()" />
        </div>
      </div>

      <!-- Messages area -->
      <div #scrollContainer class="flex-1 overflow-y-auto px-6 py-6 flex flex-col gap-4">

        @if (chatService.messages().length === 0) {
          <div class="flex flex-col items-center justify-center h-full gap-4 text-muted-color">
            <div class="w-20 h-20 rounded-3xl flex items-center justify-center shadow-inner"
                 style="background: var(--p-primary-50)">
              <i class="pi pi-comments text-4xl" style="color: var(--p-primary-color)"></i>
            </div>
            <div class="text-center">
              <p class="text-lg font-semibold text-color m-0">Ask anything</p>
              <p class="text-sm mt-1 m-0">Your documents are ready to answer questions</p>
            </div>
          </div>
        }

        @for (msg of chatService.messages(); track $index) {
          <div [class]="'flex gap-3 ' + (msg.role === 'user' ? 'flex-row-reverse' : 'flex-row')">
            <!-- Avatar -->
            <div class="w-8 h-8 rounded-full flex-shrink-0 flex items-center justify-center text-xs font-bold shadow-sm mt-1"
                 [style]="msg.role === 'user'
                   ? 'background: var(--p-primary-color); color: white'
                   : 'background: #e8f5e9; color: #2e7d32; border: 1px solid #c8e6c9'">
              <i [class]="'pi ' + (msg.role === 'user' ? 'pi-user' : 'pi-microchip-ai') + ' text-xs'"></i>
            </div>

            <!-- Bubble -->
            <div class="max-w-2xl flex flex-col"
                 [class]="msg.role === 'user' ? 'items-end' : 'items-start'">
              <div class="px-4 py-3 shadow-sm"
                   [style]="msg.role === 'user'
                     ? 'background: var(--p-primary-color); color: #ffffff; border-radius: 1rem 1rem 0.25rem 1rem'
                     : 'background: #ffffff; color: #111827; border: 1px solid #e5e7eb; border-radius: 1rem 1rem 1rem 0.25rem'">
                @if (msg.content) {
                  @if (msg.role === 'assistant') {
                    <div class="markdown-body text-sm leading-relaxed" [innerHTML]="msg.content | markdown"></div>
                  } @else {
                    <p class="whitespace-pre-wrap text-sm leading-relaxed m-0">{{ msg.content }}</p>
                  }
                } @else if (msg.role === 'assistant' && chatService.streaming()) {
                  <div class="flex gap-1 py-1">
                    <span class="w-2 h-2 rounded-full animate-bounce" style="background: var(--p-primary-color); animation-delay: 0ms"></span>
                    <span class="w-2 h-2 rounded-full animate-bounce" style="background: var(--p-primary-color); animation-delay: 150ms"></span>
                    <span class="w-2 h-2 rounded-full animate-bounce" style="background: var(--p-primary-color); animation-delay: 300ms"></span>
                  </div>
                }
              </div>
              <span class="text-xs mt-1 px-1" style="color: #6b7280">{{ msg.timestamp | date:'h:mm a' }}</span>
            </div>
          </div>
        }
      </div>

      <!-- Input bar -->
      <div class="px-6 py-4" style="background: #ffffff; border-top: 1px solid #e5e7eb">
        <div class="flex gap-3 max-w-4xl mx-auto">
          <input
            pInputText
            class="flex-1"
            placeholder="Ask about your documents…"
            [(ngModel)]="query"
            (keydown.enter)="send()"
            [disabled]="chatService.streaming()"
          />
          <p-button
            icon="pi pi-send"
            [loading]="chatService.streaming()"
            [disabled]="!query.trim()"
            (onClick)="send()"
            pTooltip="Send (Enter)"
          />
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host ::ng-deep .markdown-body p { margin: 0 0 0.5rem 0; }
    :host ::ng-deep .markdown-body p:last-child { margin-bottom: 0; }
    :host ::ng-deep .markdown-body ul, :host ::ng-deep .markdown-body ol { margin: 0.25rem 0 0.5rem 1.25rem; padding: 0; }
    :host ::ng-deep .markdown-body li { margin-bottom: 0.2rem; }
    :host ::ng-deep .markdown-body strong { font-weight: 600; }
    :host ::ng-deep .markdown-body h1, :host ::ng-deep .markdown-body h2, :host ::ng-deep .markdown-body h3 { font-weight: 600; margin: 0.5rem 0 0.25rem 0; }
    :host ::ng-deep .markdown-body h1 { font-size: 1rem; }
    :host ::ng-deep .markdown-body h2 { font-size: 0.95rem; }
    :host ::ng-deep .markdown-body h3 { font-size: 0.9rem; }
    :host ::ng-deep .markdown-body code { background: #f3f4f6; border-radius: 3px; padding: 0.1em 0.3em; font-size: 0.85em; }
    :host ::ng-deep .markdown-body pre { background: #f3f4f6; border-radius: 6px; padding: 0.75rem; overflow-x: auto; margin: 0.5rem 0; }
    :host ::ng-deep .markdown-body pre code { background: none; padding: 0; }
    :host ::ng-deep .markdown-body hr { border: none; border-top: 1px solid #e5e7eb; margin: 0.5rem 0; }
  `]
})
export class ChatComponent implements OnInit, AfterViewChecked {
  chatService = inject(ChatService);
  docsService = inject(DocumentsService);

  @ViewChild('scrollContainer') scrollContainer!: ElementRef<HTMLElement>;

  query = '';
  selectedDocId: number | null = null;

  documentOptions = () => [
    { label: 'All documents', value: null },
    ...this.docsService.documents()
      .filter(d => d.embeddingStatus === 'Completed')
      .map(d => ({ label: d.fileName, value: d.documentId }))
  ];

  ngOnInit(): void {
    this.docsService.loadDocuments();
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  async send(): Promise<void> {
    if (!this.query.trim()) return;
    const q = this.query;
    this.query = '';
    await this.chatService.sendMessage(q);
  }

  onDocSelect(docId: number | null): void {
    this.chatService.selectedDocumentId.set(docId);
  }

  private scrollToBottom(): void {
    const el = this.scrollContainer?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
  }
}
