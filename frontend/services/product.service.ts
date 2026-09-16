import { apiClient } from "@/lib/api/client";
import type {
  ProductCreateInput,
  ProductDetails,
  ProductList,
  ProductUpdateInput,
} from "@/types/product";

const PRODUCT_ENDPOINT = "/Products";

export const productService = {
  async getAll(): Promise<ProductList[]> {
    return apiClient<ProductList[]>(
      PRODUCT_ENDPOINT,
    );
  },

  async getFeatured(): Promise<ProductList[]> {
    return apiClient<ProductList[]>(
      `${PRODUCT_ENDPOINT}/featured`,
    );
  },

  async getById(id: number): Promise<ProductDetails> {
    return apiClient<ProductDetails>(
      `${PRODUCT_ENDPOINT}/${id}`,
    );
  },

  async getBySlug(
    slug: string,
  ): Promise<ProductDetails> {
    return apiClient<ProductDetails>(
      `${PRODUCT_ENDPOINT}/slug/${encodeURIComponent(slug)}`,
    );
  },

  async create(
    data: ProductCreateInput,
  ): Promise<ProductDetails> {
    return apiClient<ProductDetails>(
      PRODUCT_ENDPOINT,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  },

  async update(
    id: number,
    data: ProductUpdateInput,
  ): Promise<ProductDetails> {
    return apiClient<ProductDetails>(
      `${PRODUCT_ENDPOINT}/${id}`,
      {
        method: "PUT",
        body: JSON.stringify(data),
      },
    );
  },

  async delete(id: number): Promise<void> {
    await apiClient<void>(
      `${PRODUCT_ENDPOINT}/${id}`,
      {
        method: "DELETE",
      },
    );
  },
};