import { apiClient } from "@/lib/api/client";
import type {
  ProductDetails,
  ProductList,
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
};