import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom, interval } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { DocumentItem } from '../models/document.models';

@Injectable({ providedIn: 'root' })
export class DocumentsService {
  private api = inject(ApiService);

  documents = signal<DocumentItem[]>([]);
  loading = signal(false);
  uploading = signal(false);
  uploadError = signal<string | null>(null);

  async loadDocuments(): Promise<void> {
    this.loading.set(true);
    try {
      const res = await firstValueFrom(this.api.get<DocumentItem[]>('documents'));
      if (res.success && res.data) this.documents.set(res.data);
    } finally {
      this.loading.set(false);
    }
  }

  async uploadFile(file: File): Promise<void> {
    this.uploading.set(true);
    this.uploadError.set(null);
    try {
      const formData = new FormData();
      formData.append('file', file);
      const res = await firstValueFrom(this.api.postForm<DocumentItem>('documents/upload', formData));
      if (res.success && res.data) {
        this.documents.update(docs => [res.data!, ...docs]);
      } else {
        this.uploadError.set(res.message ?? 'Upload failed.');
      }
    } catch {
      this.uploadError.set('Upload failed. Please try again.');
    } finally {
      this.uploading.set(false);
    }
  }

  async retryEmbedding(documentId: number): Promise<void> {
    const res = await firstValueFrom(this.api.post<DocumentItem>(`documents/${documentId}/retry-embedding`, {}));
    if (res.success && res.data) {
      this.documents.update(docs => docs.map(d => d.documentId === documentId ? res.data! : d));
    }
  }

  async refreshDocuments(): Promise<void> {
    const res = await firstValueFrom(this.api.get<DocumentItem[]>('documents'));
    if (res.success && res.data) this.documents.set(res.data);
  }
}
