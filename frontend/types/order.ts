export interface OrderItem {
  id: number;
  productVariantId: number;
  productName: string;
  strength?: string | null;
  packSize?: string | null;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface OrderStatusHistory {
  id: number;
  status: string;
  comment?: string | null;
  createdAt: string;
}

export interface Order {
  id: number;
  orderNumber: string;
  totalAmount: number;
  currency: string;
  status: string;

  customerId: number;
  customerName: string;
  customerEmail: string;
  customerPhone?: string | null;

  shippingAddressLine1: string;
  shippingAddressLine2?: string | null;
  shippingCity: string;
  shippingState?: string | null;
  shippingPostalCode: string;
  shippingCountry: string;
  createdAt: string;

  items: OrderItem[];
  statusHistory: OrderStatusHistory[];
}

export interface CreateOrderRequest {
  addressId: number;
}