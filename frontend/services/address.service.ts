import { apiClient } from "@/lib/api/client";

import type {
  Address,
  CreateAddressRequest,
  UpdateAddressRequest,
} from "@/types/address";

const ADDRESS_ENDPOINT = "/Addresses";

export const addressService = {
  async getAll(): Promise<Address[]> {
    return apiClient<Address[]>(ADDRESS_ENDPOINT, {
      method: "GET",
    });
  },

  async getById(id: number): Promise<Address> {
    return apiClient<Address>(`${ADDRESS_ENDPOINT}/${id}`, {
      method: "GET",
    });
  },

  async create(data: CreateAddressRequest): Promise<Address> {
    return apiClient<Address>(ADDRESS_ENDPOINT, {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  async update(
    id: number,
    data: UpdateAddressRequest
  ): Promise<Address> {
    return apiClient<Address>(`${ADDRESS_ENDPOINT}/${id}`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<void> {
    await apiClient<void>(`${ADDRESS_ENDPOINT}/${id}`, {
      method: "DELETE",
    });
  },
};