export type EmbeddingStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';

export interface DocumentItem {
  documentId: number;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedAt: string;
  embeddingStatus: EmbeddingStatus;
  embeddingErrorMessage?: string;
}
