"use client";

import Image from "next/image";
import { ImageIcon } from "lucide-react";
import { useEffect, useState } from "react";
import { blogService } from "@/services/blog.service";

type BlogCoverPreviewProps = {
  postId?: number;
  hasCoverImage: boolean;
  localPreviewUrl?: string | null;
  className?: string;
  alt?: string;
};

export function BlogCoverPreview({
  postId,
  hasCoverImage,
  localPreviewUrl,
  className = "",
  alt = "Blog cover preview",
}: BlogCoverPreviewProps) {
  const previewKey = `${postId ?? "new"}:${hasCoverImage}`;
  const [remotePreview, setRemotePreview] = useState<{
    key: string;
    src: string | null;
    failed: boolean;
  } | null>(null);
  const currentRemotePreview = remotePreview?.key === previewKey ? remotePreview : null;
  const src = localPreviewUrl ?? currentRemotePreview?.src ?? null;
  const failed = currentRemotePreview?.failed ?? false;

  useEffect(() => {
    let objectUrl: string | null = null;
    let active = true;
    const abortController = new AbortController();

    if (postId && hasCoverImage) {
      void blogService.getCoverPreview(postId, abortController.signal)
        .then((blob) => {
          if (!active) return;
          objectUrl = URL.createObjectURL(blob);
          setRemotePreview({ key: previewKey, src: objectUrl, failed: false });
        })
        .catch(() => {
          if (active && !abortController.signal.aborted) {
            setRemotePreview({ key: previewKey, src: null, failed: true });
          }
        });
    }

    return () => {
      active = false;
      abortController.abort();
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [hasCoverImage, postId, previewKey]);

  return (
    <div className={`overflow-hidden rounded-xl border border-gray-200 bg-gray-50 ${className}`}>
      {src ? (
        <Image
          src={src}
          alt={alt}
          width={1200}
          height={675}
          unoptimized
          className="h-full w-full object-contain"
        />
      ) : (
        <div className="flex h-full min-h-28 items-center justify-center gap-2 px-4 text-sm text-gray-500" role="status">
          <ImageIcon className="size-5 shrink-0 text-gray-400" aria-hidden="true" />
          <span>{failed ? "Preview unavailable" : postId && hasCoverImage ? "Loading cover preview…" : hasCoverImage ? "Save the article to preview this image" : "No cover image selected"}</span>
        </div>
      )}
    </div>
  );
}
