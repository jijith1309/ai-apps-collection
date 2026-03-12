import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { FileUploadModule, FileUploadHandlerEvent } from 'primeng/fileupload';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { DocumentsService } from '../services/documents.service';
import { DocumentItem } from '../models/document.models';
import { DatePipe, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [
    RouterLink, ButtonModule, CardModule, TableModule, TagModule,
    FileUploadModule, MessageModule, TooltipModule, DatePipe, DecimalPipe
  ],
  template: `
    <div class="max-w-6xl mx-auto p-6 flex flex-col gap-6">

      <!-- Page header -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-color">Documents</h1>
          <p class="text-muted-color text-sm mt-1">Upload files to build your knowledge base</p>
        </div>
        <a routerLink="/chat">
          <p-button label="Open Chat" icon="pi pi-comments" />
        </a>
      </div>

      <!-- Upload zone -->
      <div class="rounded-2xl border-2 border-dashed border-surface-border bg-surface-card p-10 text-center"
           [style.border-color]="docsService.uploading() ? 'var(--p-primary-color)' : ''">
        <div class="flex flex-col items-center gap-4">
          <div class="w-16 h-16 rounded-2xl flex items-center justify-center"
               style="background: var(--p-primary-50)">
            <i class="pi pi-cloud-upload text-3xl" style="color: var(--p-primary-color)"></i>
          </div>
          <div>
            <p class="font-semibold text-color text-lg m-0">Upload a Document</p>
            <p class="text-muted-color text-sm mt-1 m-0">PDF, DOC, DOCX · Max 20 MB</p>
          </div>
          <p-fileupload
            mode="basic"
            accept=".pdf,.doc,.docx"
            [maxFileSize]="20000000"
            [multiple]="false"
            [customUpload]="true"
            (uploadHandler)="onUpload($event)"
            [auto]="true"
            chooseLabel="Choose File"
            chooseIcon="pi pi-folder-open"
            [disabled]="docsService.uploading()"
          />
          @if (docsService.uploading()) {
            <p class="text-sm m-0" style="color: var(--p-primary-color)">
              <i class="pi pi-spin pi-spinner mr-1"></i> Uploading and generating embeddings…
            </p>
          }
          @if (docsService.uploadError()) {
            <p-message severity="error" [text]="docsService.uploadError()!" />
          }
        </div>
      </div>

      <!-- Documents table -->
      <p-card>
        <ng-template #title>
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-2">
              <i class="pi pi-list" style="color: var(--p-primary-color)"></i>
              <span>Uploaded Documents</span>
              <span class="text-sm font-normal text-muted-color ml-1">
                ({{ docsService.documents().length }})
              </span>
            </div>
            <p-button icon="pi pi-refresh" label="Refresh" severity="secondary"
              size="small" variant="text" [loading]="docsService.loading()"
              (onClick)="docsService.refreshDocuments()" />
          </div>
        </ng-template>

        <p-table [value]="docsService.documents()" [loading]="docsService.loading()" stripedRows>
          <ng-template #header>
            <tr>
              <th>File Name</th>
              <th>Type</th>
              <th>Size</th>
              <th>Uploaded</th>
              <th>Status</th>
              <th></th>
            </tr>
          </ng-template>
          <ng-template #body let-doc>
            <tr>
              <td>
                <div class="flex items-center gap-2">
                  <i [class]="'pi ' + fileIcon(doc.contentType) + ' text-muted-color'"></i>
                  <span class="font-medium text-color">{{ doc.fileName }}</span>
                </div>
              </td>
              <td class="text-muted-color text-sm">{{ formatType(doc.contentType) }}</td>
              <td class="text-sm text-muted-color">{{ (doc.fileSizeBytes / 1024) | number:'1.0-0' }} KB</td>
              <td class="text-sm text-muted-color">{{ doc.uploadedAt | date:'MMM d, y · h:mm a' }}</td>
              <td>
                <div class="flex items-center gap-2">
                  <p-tag [value]="statusLabel(doc.embeddingStatus)"
                    [severity]="statusSeverity(doc.embeddingStatus)"
                    [rounded]="true"
                    [pTooltip]="doc.embeddingErrorMessage ?? ''" />
                </div>
              </td>
              <td>
                @if (doc.embeddingStatus !== 'Completed') {
                  <p-button
                    icon="pi pi-refresh"
                    [label]="doc.embeddingStatus === 'Failed' ? 'Retry' : 'Generate'"
                    size="small"
                    [severity]="doc.embeddingStatus === 'Failed' ? 'danger' : 'secondary'"
                    variant="text"
                    pTooltip="Regenerate embedding"
                    (onClick)="retryEmbedding(doc)" />
                }
              </td>
            </tr>
          </ng-template>
          <ng-template #emptymessage>
            <tr>
              <td colspan="6">
                <div class="flex flex-col items-center justify-center py-16 text-muted-color">
                  <i class="pi pi-inbox text-5xl mb-4 opacity-40"></i>
                  <p class="font-semibold text-color m-0">No documents yet</p>
                  <p class="text-sm mt-1 m-0">Upload a PDF, DOC, or DOCX to get started</p>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </p-card>
    </div>
  `
})
export class DocumentsComponent implements OnInit {
  docsService = inject(DocumentsService);

  ngOnInit(): void {
    this.docsService.loadDocuments();
  }

  async onUpload(event: FileUploadHandlerEvent): Promise<void> {
    const file = event.files[0];
    if (file) await this.docsService.uploadFile(file);
  }

  fileIcon(contentType: string): string {
    if (contentType.includes('pdf')) return 'pi-file-pdf';
    return 'pi-file-word';
  }

  formatType(contentType: string): string {
    if (contentType.includes('pdf')) return 'PDF';
    if (contentType.includes('wordprocessingml')) return 'DOCX';
    return 'DOC';
  }

  statusLabel(status: string): string {
    switch (status) {
      case 'Completed': return 'Completed';
      case 'Processing': return 'Processing';
      case 'Pending': return 'Pending';
      case 'Failed': return 'Failed';
      default: return status;
    }
  }

  statusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Completed': return 'success';
      case 'Processing': return 'info';
      case 'Pending': return 'warn';
      case 'Failed': return 'danger';
      default: return 'secondary';
    }
  }

  async retryEmbedding(doc: DocumentItem): Promise<void> {
    await this.docsService.retryEmbedding(doc.documentId);
  }
}
