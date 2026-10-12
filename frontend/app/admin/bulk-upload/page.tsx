"use client";

import {
  ChangeEvent,
  FormEvent,
  useEffect,
  useState,
} from "react";
import {
  CheckCircle2,
  FileSpreadsheet,
  ImagePlus,
  Loader2,
  Upload,
  X,
  AlertCircle,
} from "lucide-react";

import { bulkUploadService } from "@/services/bulkUpload.service";
import type { BulkUploadStatus } from "@/types/bulkUpload";

const POLLING_INTERVAL = 3000;

export default function BulkUploadPage() {
  const [excelFile, setExcelFile] = useState<File | null>(null);
  const [imageFiles, setImageFiles] = useState<File[]>([]);

  const [jobId, setJobId] = useState<number | null>(null);
  const [jobStatus, setJobStatus] =
    useState<BulkUploadStatus | null>(null);

  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isLoadingStatus, setIsLoadingStatus] =
    useState(false);

  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const isProcessing =
    jobStatus?.status === "Pending" ||
    jobStatus?.status === "Processing";

  useEffect(() => {
    if (!jobId || !isProcessing) {
      return;
    }

    let cancelled = false;

    const fetchStatus = async () => {
      try {
        setIsLoadingStatus(true);

        const status =
          await bulkUploadService.getStatus(jobId);

        if (!cancelled) {
          setJobStatus(status);
        }
      } catch (statusError) {
        if (!cancelled) {
          setError(
            statusError instanceof Error
              ? statusError.message
              : "Unable to fetch upload status.",
          );
        }
      } finally {
        if (!cancelled) {
          setIsLoadingStatus(false);
        }
      }
    };

    fetchStatus();

    const interval = window.setInterval(
      fetchStatus,
      POLLING_INTERVAL,
    );

    return () => {
      cancelled = true;
      window.clearInterval(interval);
    };
  }, [jobId, isProcessing]);

  const handleExcelChange = (
    event: ChangeEvent<HTMLInputElement>,
  ) => {
    const file = event.target.files?.[0];

    if (!file) {
      return;
    }

    const isExcel = file.name.toLowerCase().endsWith(".xlsx");

    if (!isExcel) {
      setError(
        "Please select a valid Excel file (.xlsx).",
      );
      setExcelFile(null);
      return;
    }

    setError("");
    setMessage("");
    setExcelFile(file);
  };

  const handleImagesChange = (
    event: ChangeEvent<HTMLInputElement>,
  ) => {
    const files = Array.from(event.target.files ?? []);

    setError("");
    setMessage("");
    setImageFiles(files);
  };

  const removeImage = (index: number) => {
    setImageFiles((current) =>
      current.filter(
        (_, imageIndex) => imageIndex !== index,
      ),
    );
  };

  const handleUpload = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    setError("");
    setMessage("");
    setJobStatus(null);
    setJobId(null);

    if (!excelFile) {
      setError("Please select an Excel file.");
      return;
    }

    try {
      setUploadProgress(0);
      setIsUploading(true);

      const response =
        await bulkUploadService.upload(excelFile, imageFiles, setUploadProgress);

      setJobId(response.jobId);

      setMessage(
        `Bulk upload started successfully. Job ID: ${response.jobId}`,
      );

      setExcelFile(null);
      setImageFiles([]);
    } catch (uploadError) {
      setError(
        uploadError instanceof Error
          ? uploadError.message
          : "Bulk upload failed.",
      );
    } finally {
      setIsUploading(false);
    }
  };

  const totalRecords =
    jobStatus?.totalRecords ?? 0;

  const processedRecords =
    jobStatus?.processedRecords ?? 0;

  const progress =
    totalRecords > 0
      ? Math.min(
          100,
          Math.round(
            (processedRecords / totalRecords) * 100,
          ),
        )
      : 0;

  const isCompleted =
    jobStatus?.status === "Completed";

  const hasErrors =
    jobStatus?.status ===
      "CompletedWithErrors" ||
    jobStatus?.status === "Failed";

  return (
    <div className="min-h-full bg-gray-50 p-6">
      <div className="mx-auto max-w-5xl">
        {/* Header */}
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-[#1B2A4A]">
            Bulk Product Upload
          </h1>

          <p className="mt-1 text-sm text-gray-500">
            Upload products, variants and product images
            using an Excel file.
          </p>
        </div>

        <form
          onSubmit={handleUpload}
          className="space-y-6"
        >
          {/* Excel */}
          <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
            <div className="mb-4 flex items-center gap-3">
              <div className="flex size-10 items-center justify-center rounded-lg bg-green-50 text-green-600">
                <FileSpreadsheet className="size-5" />
              </div>

              <div>
                <h2 className="font-semibold text-gray-900">
                  Excel File
                </h2>

                <p className="text-sm text-gray-500">
                  Upload the bulk product Excel file.
                </p>
              </div>
            </div>

            <label
              htmlFor="excel-file"
              className="flex cursor-pointer flex-col items-center justify-center rounded-xl border-2 border-dashed border-gray-300 px-6 py-10 transition hover:border-[#1B2A4A] hover:bg-gray-50"
            >
              <FileSpreadsheet className="mb-3 size-10 text-gray-400" />

              <span className="text-sm font-medium text-gray-700">
                Click to select Excel file
              </span>

              <span className="mt-1 text-xs text-gray-500">
                .xlsx only
              </span>

              <input
                id="excel-file"
                type="file"
                accept=".xlsx"
                onChange={handleExcelChange}
                className="hidden"
                disabled={isUploading}
              />
            </label>

            {excelFile && (
              <div className="mt-4 flex items-center justify-between rounded-lg border border-green-200 bg-green-50 px-4 py-3">
                <div className="flex min-w-0 items-center gap-3">
                  <FileSpreadsheet className="size-5 shrink-0 text-green-600" />

                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-gray-900">
                      {excelFile.name}
                    </p>

                    <p className="text-xs text-gray-500">
                      {(
                        excelFile.size /
                        1024 /
                        1024
                      ).toFixed(2)}{" "}
                      MB
                    </p>
                  </div>
                </div>

                <button
                  type="button"
                  onClick={() =>
                    setExcelFile(null)
                  }
                  className="rounded-md p-1 text-gray-400 transition hover:bg-white hover:text-red-500"
                >
                  <X className="size-5" />
                </button>
              </div>
            )}
          </div>

          {/* Images */}
          <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
            <div className="mb-4 flex items-center gap-3">
              <div className="flex size-10 items-center justify-center rounded-lg bg-blue-50 text-blue-600">
                <ImagePlus className="size-5" />
              </div>

              <div>
                <h2 className="font-semibold text-gray-900">
                  Product Images
                </h2>

                <p className="text-sm text-gray-500">
                  Select multiple images. File names must
                  match the Excel image references.
                </p>
              </div>
            </div>

            <label
              htmlFor="image-files"
              className="flex cursor-pointer flex-col items-center justify-center rounded-xl border-2 border-dashed border-gray-300 px-6 py-10 transition hover:border-[#1B2A4A] hover:bg-gray-50"
            >
              <ImagePlus className="mb-3 size-10 text-gray-400" />

              <span className="text-sm font-medium text-gray-700">
                Click to select product images
              </span>

              <span className="mt-1 text-xs text-gray-500">
                Multiple images are supported
              </span>

              <input
                id="image-files"
                type="file"
                accept="image/*"
                multiple
                onChange={handleImagesChange}
                className="hidden"
                disabled={isUploading}
              />
            </label>

            {imageFiles.length > 0 && (
              <div className="mt-4">
                <div className="mb-3 flex items-center justify-between">
                  <p className="text-sm font-semibold text-gray-900">
                    Selected Images
                  </p>

                  <span className="rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-600">
                    {imageFiles.length} files
                  </span>
                </div>

                <div className="max-h-72 space-y-2 overflow-y-auto">
                  {imageFiles.map(
                    (file, index) => (
                      <div
                        key={`${file.name}-${index}`}
                        className="flex items-center justify-between rounded-lg border border-gray-200 px-3 py-2"
                      >
                        <div className="flex min-w-0 items-center gap-3">
                          <ImagePlus className="size-4 shrink-0 text-gray-400" />

                          <div className="min-w-0">
                            <p className="truncate text-sm text-gray-700">
                              {file.name}
                            </p>

                            <p className="text-xs text-gray-400">
                              {(
                                file.size /
                                1024 /
                                1024
                              ).toFixed(2)}{" "}
                              MB
                            </p>
                          </div>
                        </div>

                        <button
                          type="button"
                          onClick={() =>
                            removeImage(index)
                          }
                          className="rounded-md p-1 text-gray-400 transition hover:bg-red-50 hover:text-red-500"
                        >
                          <X className="size-4" />
                        </button>
                      </div>
                    ),
                  )}
                </div>
              </div>
            )}
          </div>

          {/* Upload message */}
          {message && (
            <div className="flex items-center gap-3 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
              <CheckCircle2 className="size-5 shrink-0" />
              <span>{message}</span>
            </div>
          )}

          {error && (
            <div className="flex items-center gap-3 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
              <AlertCircle className="size-5 shrink-0" />
              <span>{error}</span>
            </div>
          )}

          {/* Upload button */}
          <div className="flex justify-end">
            <button
              type="submit"
              disabled={
                isUploading ||
                !excelFile ||
                isProcessing
              }
              className="inline-flex items-center gap-2 rounded-lg bg-[#1B2A4A] px-6 py-3 text-sm font-semibold text-white transition hover:bg-[#142038] disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isUploading ? (
                <Loader2 className="size-4 animate-spin" />
              ) : (
                <Upload className="size-4" />
              )}

              {isUploading
                ? `Uploading to secure storage... ${uploadProgress}%`
                : "Upload Products"}
            </button>
          </div>
        </form>

        {/* Job Status */}
        {jobStatus && (
          <div className="mt-6 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
            <div className="mb-5 flex items-start justify-between gap-4">
              <div>
                <h2 className="font-semibold text-gray-900">
                  Upload Job #{jobStatus.jobId}
                </h2>

                <p className="mt-1 text-sm text-gray-500">
                  {jobStatus.fileName}
                </p>
              </div>

              <div
                className={`rounded-full px-3 py-1 text-xs font-semibold ${
                  isCompleted
                    ? "bg-green-100 text-green-700"
                    : hasErrors
                      ? "bg-red-100 text-red-700"
                      : "bg-blue-100 text-blue-700"
                }`}
              >
                {jobStatus.status}
              </div>
            </div>

            {/* Progress */}
            <div className="mb-6">
              <div className="mb-2 flex items-center justify-between text-sm">
                <span className="font-medium text-gray-700">
                  Progress
                </span>

                <span className="font-semibold text-[#1B2A4A]">
                  {progress}%
                </span>
              </div>

              <div className="h-3 overflow-hidden rounded-full bg-gray-100">
                <div
                  className="h-full rounded-full bg-[#1B2A4A] transition-all duration-500"
                  style={{
                    width: `${progress}%`,
                  }}
                />
              </div>

              <p className="mt-2 text-xs text-gray-500">
                {processedRecords} of{" "}
                {totalRecords} records processed
              </p>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <div className="rounded-lg bg-gray-50 p-4">
                <p className="text-xs text-gray-500">
                  Total Records
                </p>

                <p className="mt-1 text-xl font-bold text-gray-900">
                  {jobStatus.totalRecords}
                </p>
              </div>

              <div className="rounded-lg bg-green-50 p-4">
                <p className="text-xs text-green-600">
                  Successful
                </p>

                <p className="mt-1 text-xl font-bold text-green-700">
                  {jobStatus.successCount}
                </p>
              </div>

              <div className="rounded-lg bg-red-50 p-4">
                <p className="text-xs text-red-600">
                  Errors
                </p>

                <p className="mt-1 text-xl font-bold text-red-700">
                  {jobStatus.errorCount}
                </p>
              </div>
            </div>

            {/* Processing indicator */}
            {isProcessing && (
              <div className="mt-5 flex items-center gap-2 text-sm text-blue-600">
                <Loader2 className="size-4 animate-spin" />

                {isLoadingStatus
                  ? "Checking upload status..."
                  : "Upload is being processed..."}
              </div>
            )}

            {/* Completion */}
            {isCompleted && (
              <div className="mt-5 flex items-center gap-2 text-sm font-medium text-green-600">
                <CheckCircle2 className="size-5" />
                Bulk upload completed successfully.
              </div>
            )}

            {/* Errors */}
            {hasErrors && (
              <div className="mt-5 rounded-lg border border-red-200 bg-red-50 p-4">
                <div className="flex gap-3">
                  <AlertCircle className="size-5 shrink-0 text-red-600" />

                  <div>
                    <p className="text-sm font-semibold text-red-700">
                      Upload completed with errors
                    </p>

                    {jobStatus.errorMessage && (
                      <p className="mt-1 text-sm text-red-600">
                        {jobStatus.errorMessage}
                      </p>
                    )}
                  </div>
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}