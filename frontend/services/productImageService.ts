import { apiClient } from "@/lib/api/client";
import type { ProductImage } from "@/types/product";

interface ProductImageCreateInput {
  imageUrl: string;
  altText: string | null;
  isPrimary: boolean;
  displayOrder: number;
}

interface ProductImageUpdateInput {
  imageUrl: string;
  altText: string | null;
  isPrimary: boolean;
  displayOrder: number;
}

const IMAGE_ENDPOINT = "/ProductImages";

export const productImageService = {
  async getByProductId(
    productId: number,
  ): Promise<ProductImage[]> {
    return apiClient<ProductImage[]>(
      `${IMAGE_ENDPOINT}/product/${productId}`,
    );
  },

  async getById(id: number): Promise<ProductImage> {
    return apiClient<ProductImage>(
      `${IMAGE_ENDPOINT}/${id}`,
    );
  },

  async create(
    productId: number,
    data: ProductImageCreateInput,
  ): Promise<ProductImage> {
    return apiClient<ProductImage>(
      `${IMAGE_ENDPOINT}/product/${productId}`,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  },

  async update(
    id: number,
    data: ProductImageUpdateInput,
  ): Promise<ProductImage> {
    return apiClient<ProductImage>(
      `${IMAGE_ENDPOINT}/${id}`,
      {
        method: "PUT",
        body: JSON.stringify(data),
      },
    );
  },

  async delete(id: number): Promise<void> {
    await apiClient<void>(
      `${IMAGE_ENDPOINT}/${id}`,
      {
        method: "DELETE",
      },
    );
  },
};