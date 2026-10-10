import { afterEach, describe, expect, it, vi } from "vitest";

const apiClientMock = vi.hoisted(() => vi.fn());
vi.mock("@/lib/api/client", () => ({ apiClient: apiClientMock }));

import { bulkUploadService } from "@/services/bulkUpload.service";

const excelTarget = {
  fileName: "products.xlsx",
  fileType: "Excel" as const,
  fileSize: 8 * 1024 * 1024 + 1,
  blobName: "11111111111111111111111111111111.xlsx",
  uploadUrl: "https://storage.example.test/excel?sv=token",
};

function successResponse(): Response {
  return new Response(null, { status: 201 });
}

afterEach(() => {
  vi.unstubAllGlobals();
  vi.clearAllMocks();
});

describe("bulkUploadService direct Blob upload", () => {
  it("uploads blocks, commits them, and only then completes the job", async () => {
    apiClientMock.mockImplementation(async (endpoint: string) => {
      if (endpoint.endsWith("/targets")) {
        return { excelFile: excelTarget, images: [] };
      }

      return { jobId: 42 };
    });

    const fetchMock = vi.fn().mockResolvedValue(successResponse());
    vi.stubGlobal("fetch", fetchMock);
    const progress = vi.fn();
    const excelFile = new File(
      [new Uint8Array(8 * 1024 * 1024 + 1)],
      "products.xlsx",
      { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
    );

    const result = await bulkUploadService.upload(excelFile, [], progress);

    expect(result).toEqual({ jobId: 42 });
    expect(apiClientMock).toHaveBeenCalledTimes(2);
    expect(apiClientMock.mock.calls[0][0]).toBe("/admin/bulk-upload/targets");
    expect(apiClientMock.mock.calls[1][0]).toBe("/admin/bulk-upload/complete");

    // 8 MiB + 1 byte must use two block requests followed by one block-list commit.
    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(fetchMock.mock.calls[0][0].searchParams.get("comp")).toBe("block");
    expect(fetchMock.mock.calls[1][0].searchParams.get("comp")).toBe("block");
    expect(fetchMock.mock.calls[2][0].searchParams.get("comp")).toBe("blocklist");
    expect(progress).toHaveBeenLastCalledWith(100);
  });
});