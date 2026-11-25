import { apiClient } from "@/lib/api/client";
import type {
  ProductCreateInput,
  ProductDetails,
  ProductList,
  ProductPage,
  ProductPageQuery,
  ProductUpdateInput,
} from "@/types/product";

const PRODUCT_ENDPOINT = "/Products";

export const productService = {
  async getAll(): Promise<ProductList[]> {
    return apiClient<ProductList[]>(
      PRODUCT_ENDPOINT,
    );
  },

  async getPaged(query: ProductPageQuery = {}): Promise<ProductPage> {
    const params = new URLSearchParams();

    if (query.page !== undefined) params.set("page", String(query.page));
    if (query.pageSize !== undefined) params.set("pageSize", String(query.pageSize));
    if (query.search?.trim()) params.set("search", query.search.trim());
    if (query.category) params.set("category", query.category);
    if (query.form) params.set("form", query.form);
    if (query.sortBy) params.set("sortBy", query.sortBy);
    if (query.sortOrder) params.set("sortOrder", query.sortOrder);

    const queryString = params.toString();
    return apiClient<ProductPage>(
      `${PRODUCT_ENDPOINT}/paged${queryString ? `?${queryString}` : ""}`,
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