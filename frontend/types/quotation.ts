export interface CreateQuotationItem {
  productVariantId: number;
  quantity: number;
  unitPrice: number;
}

export interface CreateQuotation {
  enquiryId: number;
  currency: string;
  validUntil: string;
  notes?: string | null;
  items: CreateQuotationItem[];
}

export interface QuotationItem {
  id: number;
  productVariantId: number;
  productName: string;
  strength: string | null;
  packSize: string | null;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface QuotationDetails {
  id: number;
  quoteNumber: string;
  enquiryId: number;
  totalAmount: number;
  currency: string;
  status: string;
  validUntil: string;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
  items: QuotationItem[];
}

export interface QuotationList {
  id: number;
  quoteNumber: string;
  enquiryId: number;
  totalAmount: number;
  currency: string;
  status: string;
  validUntil: string;
  createdAt: string;
}