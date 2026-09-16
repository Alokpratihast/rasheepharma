import { apiClient } from "@/lib/api/client";
import type {
  ProductVariant,
  ProductVariantCreateInput,
  ProductVariantUpdateInput,
} from "@/types/product";

const VARIANT_ENDPOINT = "/ProductVariants";

export const productVariantService = {
  async getByProductId(
    productId: number,
  ): Promise<ProductVariant[]> {
    return apiClient<ProductVariant[]>(
      `${VARIANT_ENDPOINT}/product/${productId}`,
    );
  },

  async getById(id: number): Promise<ProductVariant> {
    return apiClient<ProductVariant>(
      `${VARIANT_ENDPOINT}/${id}`,
    );
  },

  async create(
    productId: number,
    data: ProductVariantCreateInput,
  ): Promise<ProductVariant> {
    return apiClient<ProductVariant>(
      `${VARIANT_ENDPOINT}/product/${productId}`,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  },

  async update(
    id: number,
    data: ProductVariantUpdateInput,
  ): Promise<ProductVariant> {
    return apiClient<ProductVariant>(
      `${VARIANT_ENDPOINT}/${id}`,
      {
        method: "PUT",
        body: JSON.stringify(data),
      },
    );
  },

  async delete(id: number): Promise<void> {
    await apiClient<void>(
      `${VARIANT_ENDPOINT}/${id}`,
      {
        method: "DELETE",
      },
    );
  },
};