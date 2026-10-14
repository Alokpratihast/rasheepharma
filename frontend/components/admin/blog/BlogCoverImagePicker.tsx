"use client";

import { useEffect, useRef, useState, type PointerEvent } from "react";
import { toast } from "sonner";
import { blogService } from "@/services/blog.service";

const OUTPUT_WIDTH = 1200;
const OUTPUT_HEIGHT = 750;
const MAX_FILE_SIZE = 8 * 1024 * 1024;

type CropMode = "fit" | "fill";
type Point = { x: number; y: number };

export function BlogCoverImagePicker({
  disabled,
  onUploaded,
  onUploadingChange,
  postId,
  hasOriginal,
}: {
  disabled: boolean;
  onUploaded: (result: { url: string; originalUrl: string }, previewUrl: string) => void;
  onUploadingChange: (uploading: boolean) => void;
  postId?: number;
  hasOriginal: boolean;
}) {
  const [source, setSource] = useState<File | null>(null);
  const [sourceUrl, setSourceUrl] = useState<string | null>(null);
  const [mode, setMode] = useState<CropMode>("fit");
  const [zoom, setZoom] = useState(1);
  const [offset, setOffset] = useState<Point>({ x: 0, y: 0 });
  const [uploading, setUploading] = useState(false);
  const dragStart = useRef<{ pointer: Point; offset: Point } | null>(null);
  const previewFrame = useRef<HTMLDivElement>(null);

  useEffect(() => () => {
    if (sourceUrl) URL.revokeObjectURL(sourceUrl);
  }, [sourceUrl]);

  const selectFile = (file?: File) => {
    if (!file) return;
    if (!["image/jpeg", "image/png", "image/webp"].includes(file.type) || file.size > MAX_FILE_SIZE) {
      toast.error("Choose a JPEG, PNG, or WebP image smaller than 8 MB.");
      return;
    }
    setSource(file);
    setSourceUrl(URL.createObjectURL(file));
    setMode("fit");
    setZoom(1);
    setOffset({ x: 0, y: 0 });
  };

  const loadOriginal = async () => {
    if (!postId) return;
    try {
      const blob = await blogService.getOriginalCoverPreview(postId);
      const extension = blob.type === "image/png" ? "png" : blob.type === "image/webp" ? "webp" : "jpg";
      selectFile(new File([blob], `blog-cover-original.${extension}`, { type: blob.type }));
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not load the original image.");
    }
  };

  const onPointerDown = (event: PointerEvent<HTMLDivElement>) => {
    event.currentTarget.setPointerCapture(event.pointerId);
    dragStart.current = { pointer: { x: event.clientX, y: event.clientY }, offset };
  };

  const onPointerMove = (event: PointerEvent<HTMLDivElement>) => {
    if (!dragStart.current) return;
    setOffset({
      x: dragStart.current.offset.x + event.clientX - dragStart.current.pointer.x,
      y: dragStart.current.offset.y + event.clientY - dragStart.current.pointer.y,
    });
  };

  const makeDisplayFile = async () => {
    if (!source) throw new Error("Choose a cover image first.");
    const bitmap = await createImageBitmap(source, { imageOrientation: "from-image" });
    try {
      const canvas = document.createElement("canvas");
      canvas.width = OUTPUT_WIDTH;
      canvas.height = OUTPUT_HEIGHT;
      const context = canvas.getContext("2d");
      if (!context) throw new Error("Your browser could not prepare the cover image.");
      context.fillStyle = "#f8fafc";
      context.fillRect(0, 0, OUTPUT_WIDTH, OUTPUT_HEIGHT);
      const baseScale = mode === "fit"
        ? Math.min(OUTPUT_WIDTH / bitmap.width, OUTPUT_HEIGHT / bitmap.height)
        : Math.max(OUTPUT_WIDTH / bitmap.width, OUTPUT_HEIGHT / bitmap.height);
      const scale = baseScale * zoom;
      const width = bitmap.width * scale;
      const height = bitmap.height * scale;
      const frameWidth = previewFrame.current?.clientWidth ?? OUTPUT_WIDTH;
      const frameHeight = previewFrame.current?.clientHeight ?? OUTPUT_HEIGHT;
      context.drawImage(
        bitmap,
        (OUTPUT_WIDTH - width) / 2 + offset.x * OUTPUT_WIDTH / frameWidth,
        (OUTPUT_HEIGHT - height) / 2 + offset.y * OUTPUT_HEIGHT / frameHeight,
        width,
        height,
      );
      const blob = await new Promise<Blob>((resolve, reject) => {
        canvas.toBlob((value) => value ? resolve(value) : reject(new Error("Could not create the 16:10 cover image.")), "image/webp", 0.9);
      });
      const baseName = source.name.replace(/\.[^.]+$/, "") || "blog-cover";
      return new File([blob], `${baseName}-16x10.webp`, { type: "image/webp", lastModified: Date.now() });
    } finally {
      bitmap.close();
    }
  };

  const upload = async () => {
    if (!source) return;
    setUploading(true);
    onUploadingChange(true);
    try {
      const displayFile = await makeDisplayFile();
      const result = await blogService.uploadCover(source, displayFile);
      onUploaded(result, URL.createObjectURL(displayFile));
      setSource(null);
      setSourceUrl(null);
      toast.success("Original and 16:10 cover images uploaded.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not upload the cover image.");
    } finally {
      setUploading(false);
      onUploadingChange(false);
    }
  };

  return (
    <div className="space-y-3">
      <label className="block text-sm font-medium text-gray-700">
        Upload cover (JPEG, PNG, WebP; max 8 MB)
        <input
          type="file"
          accept="image/jpeg,image/png,image/webp"
          disabled={disabled || uploading}
          onChange={(event) => { selectFile(event.currentTarget.files?.[0]); event.currentTarget.value = ""; }}
          className="mt-1 block w-full rounded-lg border p-2"
        />
      </label>
      {!source && postId && hasOriginal && (
        <button type="button" disabled={disabled || uploading} onClick={() => void loadOriginal()} className="rounded-lg border px-3 py-2 text-sm text-[#1B2A4A] disabled:opacity-60">
          Adjust existing cover
        </button>
      )}

      {sourceUrl && source && (
        <section className="space-y-3 rounded-xl border border-slate-200 bg-white p-4" aria-label="Cover crop preview">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div>
              <p className="font-semibold text-[#1B2A4A]">16:10 cover preview</p>
              <p className="text-xs text-gray-500">Drag to position. The original image is saved separately.</p>
            </div>
            <div className="flex gap-2" role="group" aria-label="Image fitting mode">
              <button type="button" onClick={() => { setMode("fit"); setZoom(1); setOffset({ x: 0, y: 0 }); }} aria-pressed={mode === "fit"} className={`rounded-lg border px-3 py-2 text-sm ${mode === "fit" ? "border-emerald-700 bg-emerald-50 text-emerald-800" : ""}`}>
                Fit full image
              </button>
              <button type="button" onClick={() => { setMode("fill"); setZoom(1); setOffset({ x: 0, y: 0 }); }} aria-pressed={mode === "fill"} className={`rounded-lg border px-3 py-2 text-sm ${mode === "fill" ? "border-emerald-700 bg-emerald-50 text-emerald-800" : ""}`}>
                Fill frame
              </button>
            </div>
          </div>

          <div
            ref={previewFrame}
            className="relative aspect-[16/10] touch-none cursor-grab overflow-hidden rounded-lg bg-slate-50 active:cursor-grabbing"
            onPointerDown={onPointerDown}
            onPointerMove={onPointerMove}
            onPointerUp={() => { dragStart.current = null; }}
            onPointerCancel={() => { dragStart.current = null; }}
          >
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={sourceUrl}
              alt="Selected cover preview"
              draggable={false}
              className="pointer-events-none h-full w-full select-none"
              style={{ objectFit: mode === "fit" ? "contain" : "cover", objectPosition: `calc(50% + ${offset.x}px) calc(50% + ${offset.y}px)`, transform: `scale(${zoom})` }}
            />
            <span className="pointer-events-none absolute bottom-2 right-2 rounded bg-black/60 px-2 py-1 text-xs text-white">16:10</span>
          </div>

          <label className="block text-sm text-gray-700">
            Zoom
            <input className="ml-3 align-middle" type="range" min="1" max="3" step="0.05" value={zoom} onChange={(event) => setZoom(Number(event.target.value))} />
            <span className="ml-2 tabular-nums">{zoom.toFixed(1)}×</span>
          </label>
          <p className="text-xs text-gray-500">Fit full image keeps the whole photo visible. Fill frame crops edges to fill the card.</p>
          <div className="flex justify-end gap-2">
            <button type="button" disabled={uploading} onClick={() => { setSource(null); setSourceUrl(null); }} className="rounded-lg border px-4 py-2">Cancel</button>
            <button type="button" disabled={uploading || disabled} onClick={() => void upload()} className="rounded-lg bg-[#007f62] px-4 py-2 font-semibold text-white disabled:opacity-60">{uploading ? "Uploading…" : "Use this cover"}</button>
          </div>
        </section>
      )}
    </div>
  );
}
