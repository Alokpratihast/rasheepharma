export interface BulkUploadResponse {
  jobId: number;
}

export interface BulkUploadStatus {
  jobId: number;
  fileName: string;
  status: string;
  totalRecords: number;
  processedRecords: number;
  successCount: number;
  errorCount: number;
  errorMessage: string | null;
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
}

export interface BulkUploadError {
  id: number;
  bulkUploadJobId: number;
  sheetName: string;
  rowNumber: number;
  productSlug: string | null;
  errorMessage: string;
  createdAt: string;
}

export interface BulkUploadFile {
  id: number;
  bulkUploadJobId: number;
  originalFileName: string;
  blobName: string;
  fileUrl: string;
  fileType: string;
  contentType: string | null;
  fileSize: number;
  createdAt: string;
}