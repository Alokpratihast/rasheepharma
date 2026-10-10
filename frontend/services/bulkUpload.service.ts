import { apiClient } from "@/lib/api/client";
import type {
  BulkUploadResponse,
  BulkUploadStatus,
} from "@/types/bulkUpload";

const BLOCK_SIZE = 8 * 1024 * 1024;
const BLOCK_CONCURRENCY = 4;
const STORAGE_API_VERSION = "2023-11-03";

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

function createBlockId(index: number): string {
  // Fixed-width ASCII IDs keep all Azure block IDs the same decoded length.
  return btoa(`block-${index.toString().padStart(10, "0")}`);
}

async function putBlock(
  target: UploadTarget,
  blockId: string,
  block: Blob,
): Promise<void> {
  const url = new URL(target.uploadUrl);
  url.searchParams.set("comp", "block");
  url.searchParams.set("blockid", blockId);

  for (let attempt = 0; attempt < 3; attempt += 1) {
    try {
      const response = await fetch(url, {
        method: "PUT",
        mode: "cors",
        credentials: "omit",
        headers: {
          "x-ms-version": STORAGE_API_VERSION,
          "Content-Type": "application/octet-stream",
        },
        body: block,
      });

      if (response.ok) return;
      const isRetryable =
        response.status === 408 ||
        response.status === 429 ||
        response.status >= 500;

      if (!isRetryable || attempt === 2) {
        throw new Error(
          `A file block could not be uploaded (HTTP ${response.status}).`,
        );
      }
    } catch (error) {
      if (attempt === 2 || (error instanceof Error && error.message.startsWith("A file block"))) {
        throw error;
      }
    }

    // Exponential backoff avoids hammering Blob Storage during transient failures.
    await new Promise((resolve) =>
      window.setTimeout(resolve, 250 * 2 ** attempt),
    );
  }
}
async function commitBlocks(
  target: UploadTarget,
  blockIds: string[],
  contentType: string,
): Promise<void> {
  const url = new URL(target.uploadUrl);
  url.searchParams.set("comp", "blocklist");

  const blockList = [
    "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
    "<BlockList>",
    ...blockIds.map((id) => `<Latest>${id}</Latest>`),
    "</BlockList>",
  ].join("");

  const response = await fetch(url, {
    method: "PUT",
    mode: "cors",
    credentials: "omit",
    headers: {
      "x-ms-version": STORAGE_API_VERSION,
      "x-ms-blob-content-type": contentType || "application/octet-stream",
      "Content-Type": "application/xml",
    },
    body: blockList,
  });

  if (!response.ok) {
    throw new Error(
      `Azure could not finalize ${target.fileName} (HTTP ${response.status}).`,
    );
  }
}

async function uploadFileInBlocks(
  { file, target }: UploadFile,
  onBytesUploaded: (bytes: number) => void,
): Promise<void> {
  const blockCount = Math.ceil(file.size / BLOCK_SIZE);
  const blockIds = Array.from(
    { length: blockCount },
    (_, index) => createBlockId(index),
  );
  let nextBlock = 0;

  // A bounded worker pool limits memory/network pressure to about four blocks.
  const workers = Array.from(
    { length: Math.min(BLOCK_CONCURRENCY, blockCount) },
    async () => {
      while (true) {
        const index = nextBlock++;
        if (index >= blockCount) return;

        const start = index * BLOCK_SIZE;
        const end = Math.min(start + BLOCK_SIZE, file.size);
        await putBlock(
          target,
          blockIds[index],
          file.slice(start, end),
        );
        onBytesUploaded(end - start);
      }
    },
  );

  await Promise.all(workers);
  await commitBlocks(target, blockIds, file.type);
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

    // Upload one file at a time; each file uses a small fixed block pool.
    for (const upload of files) {
      await uploadFileInBlocks(upload, reportProgress);
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
    return apiClient<BulkUploadStatus>(
      `/admin/bulk-upload/${jobId}`,
    );
  },
};