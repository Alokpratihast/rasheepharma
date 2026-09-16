import { apiClient } from "@/lib/api/client";
import type {
  Category,
  CategoryCreateInput,
  CategoryDetails,
  CategoryNavigation,
  CategoryUpdateInput,
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

  async getById(id: number): Promise<CategoryDetails> {
    return apiClient<CategoryDetails>(
      `${CATEGORY_ENDPOINT}/${id}`,
    );
  },

  async getBySlug(
    slug: string,
  ): Promise<CategoryDetails> {
    return apiClient<CategoryDetails>(
      `${CATEGORY_ENDPOINT}/slug/${encodeURIComponent(slug)}`,
    );
  },

  async create(
    data: CategoryCreateInput,
  ): Promise<CategoryDetails> {
    return apiClient<CategoryDetails>(
      CATEGORY_ENDPOINT,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  },

  async update(
    id: number,
    data: CategoryUpdateInput,
  ): Promise<CategoryDetails> {
    return apiClient<CategoryDetails>(
      `${CATEGORY_ENDPOINT}/${id}`,
      {
        method: "PUT",
        body: JSON.stringify(data),
      },
    );
  },

  async delete(id: number): Promise<void> {
    await apiClient<void>(
      `${CATEGORY_ENDPOINT}/${id}`,
      {
        method: "DELETE",
      },
    );
  },
};