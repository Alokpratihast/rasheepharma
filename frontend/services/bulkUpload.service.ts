import { ApiError, apiClient } from "@/lib/api/client";
import type {
  BulkUploadResponse,
  BulkUploadStatus,
} from "@/types/bulkUpload";

const CHUNK_SIZE = 8 * 1024 * 1024;
const CHUNK_CONCURRENCY = 4;

type UploadTarget = {
  fileName: string;
  fileType: "Excel" | "Image";
  fileSize: number;
  blobName: string;
  uploadUrl: string;
};

type UploadTargetsResponse = {
  excelFile: UploadTarget;
  images: UploadTarget[];
};

type UploadFile = {
  file: File;
  target: UploadTarget;
};

async function putChunk(
  target: UploadTarget,
  chunkIndex: number,
  body: Blob,
): Promise<void> {
  const endpoint = `${target.uploadUrl}/chunks/${chunkIndex}?fileSize=${target.fileSize}`;

  for (let attempt = 0; attempt < 3; attempt += 1) {
    try {
      await apiClient<null>(endpoint, {
        method: "PUT",
        headers: { "Content-Type": "application/octet-stream" },
        body,
      });
      return;
    } catch (error) {
      const retryable =
        !(error instanceof ApiError) ||
        error.status === 408 ||
        error.status === 429 ||
        error.status >= 500;
      if (!retryable || attempt === 2) throw error;
    }

    await new Promise((resolve) =>
      window.setTimeout(resolve, 250 * 2 ** attempt),
    );
  }
}

async function uploadFileInChunks(
  { file, target }: UploadFile,
  onBytesUploaded: (bytes: number) => void,
): Promise<void> {
  const chunkCount = Math.ceil(file.size / CHUNK_SIZE);
  let nextChunk = 0;

  const workers = Array.from(
    { length: Math.min(CHUNK_CONCURRENCY, chunkCount) },
    async () => {
      while (true) {
        const index = nextChunk++;
        if (index >= chunkCount) return;

        const start = index * CHUNK_SIZE;
        const end = Math.min(start + CHUNK_SIZE, file.size);
        await putChunk(target, index, file.slice(start, end));
        onBytesUploaded(end - start);
      }
    },
  );

  await Promise.all(workers);
}

export const bulkUploadService = {
  async upload(
    excelFile: File,
    imageFiles: File[] = [],
    onProgress?: (percent: number) => void,
  ): Promise<BulkUploadResponse> {
    const totalBytes = [excelFile, ...imageFiles].reduce(
      (sum, file) => sum + file.size,
      0,
    );
    let uploadedBytes = 0;
    const reportProgress = (bytes: number) => {
      uploadedBytes += bytes;
      onProgress?.(
        totalBytes === 0
          ? 0
          : Math.min(99, Math.floor((uploadedBytes / totalBytes) * 100)),
      );
    };

    const targets = await apiClient<UploadTargetsResponse>(
      "/admin/bulk-upload/targets",
      {
        method: "POST",
        body: JSON.stringify({
          excelFile: {
            fileName: excelFile.name,
            contentType: excelFile.type,
            fileSize: excelFile.size,
          },
          images: imageFiles.map((file) => ({
            fileName: file.name,
            contentType: file.type,
            fileSize: file.size,
          })),
        }),
      },
    );

    const files: UploadFile[] = [
      { file: excelFile, target: targets.excelFile },
      ...imageFiles.map((file, index) => ({
        file,
        target: targets.images[index],
      })),
    ];

    // Cap active chunk requests to keep disk, API, and VPS memory pressure bounded.
    for (const upload of files) {
      await uploadFileInChunks(upload, reportProgress);
    }

    const completed = await apiClient<BulkUploadResponse>(
      "/admin/bulk-upload/complete",
      {
        method: "POST",
        body: JSON.stringify({
          excelFile: {
            fileName: excelFile.name,
            contentType: excelFile.type,
            fileSize: excelFile.size,
            blobName: targets.excelFile.blobName,
          },
          images: imageFiles.map((file, index) => ({
            fileName: file.name,
            contentType: file.type,
            fileSize: file.size,
            blobName: targets.images[index].blobName,
          })),
        }),
      },
    );

    onProgress?.(100);
    return completed;
  },

  async getStatus(jobId: number): Promise<BulkUploadStatus> {
    return apiClient<BulkUploadStatus>(`/admin/bulk-upload/${jobId}`);
  },
};
