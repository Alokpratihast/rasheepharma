export interface CreateEnquiryItem {
  productVariantId: number;
  quantity: number;
  message?: string | null;
}

export interface CreateEnquiry {
  customerName: string;
  email: string;
  phoneNumber?: string | null;
  country: string;
  businessType?: string | null;
  message?: string | null;
  items: CreateEnquiryItem[];
}

export interface EnquiryDetails {
  id: number;
  enquiryNumber: string;
  customerName: string;
  email: string;
  phoneNumber: string | null;
  country: string;
  businessType: string | null;
  message: string | null;
  status: string;
  createdAt: string;
  updatedAt: string | null;
  items: EnquiryItem[];
}

export interface EnquiryItem {
  id: number;
  productVariantId: number;
  productName: string;
  strength: string | null;
  packSize: string | null;
  quantity: number;
  message: string | null;
}