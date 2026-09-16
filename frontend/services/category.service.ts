import { apiClient } from "@/lib/api/client";
import type {
  Category,
  CategoryNavigation,
} from "@/types/category";

const CATEGORY_ENDPOINT = "/Categories";

export const categoryService = {
  async getAll(): Promise<Category[]> {
    return apiClient<Category[]>(CATEGORY_ENDPOINT);
  },

  async getNavigation(): Promise<CategoryNavigation[]> {
    return apiClient<CategoryNavigation[]>(
      `${CATEGORY_ENDPOINT}/navigation`,
    );
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