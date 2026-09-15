import { apiClient } from "@/lib/api/client";

import type {
  AddToCartRequest,
  Cart,
} from "@/types/cart";

const CART_ENDPOINT = "/Cart";

export const cartService = {
  // =========================================================
  // GET CURRENT USER CART
  // GET /api/Cart
  // =========================================================

  async getCart(): Promise<Cart> {
    return apiClient<Cart>(
      CART_ENDPOINT,
      {
        method: "GET",
      },
    );
  },

  // =========================================================
  // ADD ITEM TO CART
  // POST /api/Cart/items
  // =========================================================

  async addToCart(
    data: AddToCartRequest,
  ): Promise<Cart> {
    return apiClient<Cart>(
      `${CART_ENDPOINT}/items`,
      {
        method: "POST",
        body: JSON.stringify(data),
      },
    );
  },

  // =========================================================
  // UPDATE CART ITEM
  // PUT /api/Cart/items/{productVariantId}?quantity=...
  // =========================================================

  async updateItem(
    productVariantId: number,
    quantity: number,
  ): Promise<Cart> {
    return apiClient<Cart>(
      `${CART_ENDPOINT}/items/${productVariantId}?quantity=${quantity}`,
      {
        method: "PUT",
      },
    );
  },

  // =========================================================
  // REMOVE CART ITEM
  // DELETE /api/Cart/items/{productVariantId}
  // =========================================================

  async removeItem(
    productVariantId: number,
  ): Promise<void> {
    await apiClient<void>(
      `${CART_ENDPOINT}/items/${productVariantId}`,
      {
        method: "DELETE",
      },
    );
  },
};