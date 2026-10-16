import { apiClient, getApiAssetUrl, refreshAccessToken, ApiError } from "@/lib/api/client";
import { getStoredToken } from "@/lib/auth/session";
import type { BlogPost, BlogPostPage, BlogPostWriteInput, PublicBlogPost, PublicBlogPostPage } from "@/types/blog";

const PUBLIC_ENDPOINT = "/blog/posts";
const ADMIN_ENDPOINT = "/admin/blog/posts";

export const blogService = {
  getPublished(page = 1, pageSize = 12) {
    return apiClient<PublicBlogPostPage>(`${PUBLIC_ENDPOINT}?page=${page}&pageSize=${pageSize}`);
  },
  getPublishedBySlug(slug: string) {
    return apiClient<PublicBlogPost>(`${PUBLIC_ENDPOINT}/${encodeURIComponent(slug)}`);
  },
  getAdminPage(page = 1, pageSize = 25) {
    return apiClient<BlogPostPage>(`${ADMIN_ENDPOINT}?page=${page}&pageSize=${pageSize}`);
  },
  getById(id: number) {
    return apiClient<BlogPost>(`${ADMIN_ENDPOINT}/${id}`);
  },
  create(input: BlogPostWriteInput) {
    return apiClient<BlogPost>(ADMIN_ENDPOINT, { method: "POST", body: JSON.stringify(input) });
  },
  update(id: number, input: BlogPostWriteInput) {
    return apiClient<BlogPost>(`${ADMIN_ENDPOINT}/${id}`, { method: "PUT", body: JSON.stringify(input) });
  },
  publish(id: number) {
    return apiClient<BlogPost>(`${ADMIN_ENDPOINT}/${id}/publish`, { method: "POST" });
  },
  unpublish(id: number) {
    return apiClient<BlogPost>(`${ADMIN_ENDPOINT}/${id}/unpublish`, { method: "POST" });
  },
  delete(id: number) {
    return apiClient<void>(`${ADMIN_ENDPOINT}/${id}`, { method: "DELETE" });
  },
  uploadCover(file: File, displayFile: File) {
    const data = new FormData();
    data.append("file", file);
    data.append("displayFile", displayFile);
    return apiClient<{ url: string; originalUrl: string }>(`${ADMIN_ENDPOINT}/images`, { method: "POST", body: data });
  },
  async getCoverPreview(id: number, signal?: AbortSignal): Promise<Blob> {
    const url = getApiAssetUrl(`/api${ADMIN_ENDPOINT}/${id}/image`);
    if (!url) throw new Error("Cover preview is unavailable.");

    const request = (token: string | null) => fetch(url, {
      credentials: "include",
      cache: "no-store",
      signal,
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });

    let response = await request(getStoredToken());
    if (response.status === 401) {
      const refreshedToken = await refreshAccessToken();
      if (refreshedToken) response = await request(refreshedToken);
    }

    if (!response.ok) {
      throw new ApiError("Could not load the cover image preview.", response.status);
    }

    const contentType = response.headers.get("content-type")?.split(";", 1)[0].trim().toLowerCase();
    if (!contentType || !["image/jpeg", "image/png", "image/webp"].includes(contentType)) {
      throw new Error("The cover preview has an unsupported image type.");
    }

    return response.blob();
  },
  async getOriginalCoverPreview(id: number): Promise<Blob> {
    const url = getApiAssetUrl(`/api${ADMIN_ENDPOINT}/${id}/image?original=true`);
    if (!url) throw new Error("Original cover image is unavailable.");
    const request = (token: string | null) => fetch(url, {
      credentials: "include",
      cache: "no-store",
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    let response = await request(getStoredToken());
    if (response.status === 401) {
      const refreshedToken = await refreshAccessToken();
      if (refreshedToken) response = await request(refreshedToken);
    }
    if (!response.ok) throw new ApiError("Could not load the original cover image.", response.status);
    const contentType = response.headers.get("content-type")?.split(";", 1)[0].trim().toLowerCase();
    if (!contentType || !["image/jpeg", "image/png", "image/webp"].includes(contentType)) {
      throw new Error("The original cover has an unsupported image type.");
    }
    return response.blob();
  },
};
