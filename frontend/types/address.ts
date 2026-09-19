export interface Address {
  id: number;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state?: string | null;
  postalCode: string;
  country: string;
  addressType?: string | null;
  isDefault: boolean;
}

export interface CreateAddressRequest {
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state?: string | null;
  postalCode: string;
  country: string;
  addressType?: string | null;
  isDefault: boolean;
}

export interface UpdateAddressRequest {
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state?: string | null;
  postalCode: string;
  country: string;
  addressType?: string | null;
  isDefault: boolean;
}