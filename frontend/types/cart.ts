export interface CartItem {
  id: number;
  productVariantId: number;
  productName: string;
  strength: string | null;
  packSize: string | null;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  imageUrl: string | null;
}

export interface Cart {
  id: number;
  items: CartItem[];
  totalAmount: number;
}

export interface AddToCartRequest {
  productVariantId: number;
  quantity: number;
}