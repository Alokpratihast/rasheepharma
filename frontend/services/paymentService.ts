import { apiClient } from "@/lib/api/client";

import type {
  CreateCheckoutSessionRequest,
  CreateCheckoutSessionResponse,
} from "@/types/payment";

const PAYMENT_ENDPOINT = "/Payments";

export const paymentService = {
  async createCheckoutSession(
    orderId: number
  ): Promise<CreateCheckoutSessionResponse> {
    const data: CreateCheckoutSessionRequest = {
      orderId,
    };

    return apiClient<CreateCheckoutSessionResponse>(
      `${PAYMENT_ENDPOINT}/create-checkout-session`,
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );
  },
};