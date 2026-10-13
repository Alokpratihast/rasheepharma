import { apiClient } from "@/lib/api/client";
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
  uploadCover(file: File) {
    const data = new FormData();
    data.append("file", file);
    return apiClient<{ url: string }>(`${ADMIN_ENDPOINT}/images`, { method: "POST", body: data });
  },
};
