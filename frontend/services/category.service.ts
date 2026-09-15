import { apiClient } from "@/lib/api/client";
import type { Category } from "@/types/category";

const CATEGORY_ENDPOINT = "/Categories";

export const categoryService = {
  async getAll(): Promise<Category[]> {
    return apiClient<Category[]>(CATEGORY_ENDPOINT);
  },

  async getById(id: number): Promise<Category> {
    return apiClient<Category>(
      `${CATEGORY_ENDPOINT}/${id}`,
    );
  },

  async getBySlug(slug: string): Promise<Category> {
    return apiClient<Category>(
      `${CATEGORY_ENDPOINT}/slug/${encodeURIComponent(slug)}`,
    );
  },
};