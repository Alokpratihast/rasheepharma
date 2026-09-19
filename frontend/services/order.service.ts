import { apiClient } from "@/lib/api/client";

import type {
  CreateOrderRequest,
  Order,
} from "@/types/order";

const ORDER_ENDPOINT = "/Orders";

export const orderService = {
  async createOrder(data: CreateOrderRequest): Promise<Order> {
    return apiClient<Order>(ORDER_ENDPOINT, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  async getMyOrders(): Promise<Order[]> {
    return apiClient<Order[]>(`${ORDER_ENDPOINT}/my-orders`, {
      method: "GET",
    });
  },

  async getAllOrders(): Promise<Order[]> {
    return apiClient<Order[]>(`${ORDER_ENDPOINT}/admin`, {
      method: "GET",
    });
  },

  async getAdminOrderById(id: number): Promise<Order> {
  return apiClient<Order>(`${ORDER_ENDPOINT}/admin/${id}`, {
    method: "GET",
  });
},

  async getOrderById(id: number): Promise<Order> {
    return apiClient<Order>(`${ORDER_ENDPOINT}/${id}`, {
      method: "GET",
    });
  },

  async getOrderByNumber(orderNumber: string): Promise<Order> {
    return apiClient<Order>(
      `${ORDER_ENDPOINT}/number/${encodeURIComponent(orderNumber)}`,
      {
        method: "GET",
      }
    );
  },
};