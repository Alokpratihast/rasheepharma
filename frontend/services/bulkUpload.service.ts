import { apiClient } from "@/lib/api/client";
import type {
  BulkUploadResponse,
  BulkUploadStatus,
} from "@/types/bulkUpload";

export const bulkUploadService = {
  async upload(
    excelFile: File,
    imageFiles: File[] = [],
  ): Promise<BulkUploadResponse> {
    const formData = new FormData();

    formData.append("excelFile", excelFile);

    imageFiles.forEach((image) => {
      formData.append("images", image);
    });

    return apiClient<BulkUploadResponse>(
      "/admin/bulk-upload",
      {
        method: "POST",
        body: formData,
      },
    );
  },

  async getStatus(
    jobId: number,
  ): Promise<BulkUploadStatus> {
    return apiClient<BulkUploadStatus>(
      `/admin/bulk-upload/${jobId}`,
    );
  },
};