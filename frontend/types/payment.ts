export interface CreateCheckoutSessionRequest {
  orderId: number;
}

export interface CreateCheckoutSessionResponse {
  checkoutUrl: string;
}