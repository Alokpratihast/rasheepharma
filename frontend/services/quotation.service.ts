import { apiClient } from "@/lib/api/client";
import type {
  CreateQuotation,
  QuotationDetails,
  QuotationList,
} from "@/types/quotation";

const QUOTATION_ENDPOINT = "/Quotations";

export const quotationService = {
  async create(
    data: CreateQuotation,
    token: string,
  ): Promise<QuotationDetails> {
    return apiClient<QuotationDetails>(
      QUOTATION_ENDPOINT,
      {
        method: "POST",
        token,
        body: JSON.stringify(data),
      },
    );
  },

  async getAll(token: string): Promise<QuotationList[]> {
    return apiClient<QuotationList[]>(
      QUOTATION_ENDPOINT,
      {
        method: "GET",
        token,
      },
    );
  },

  async getById(
    id: number,
    token: string,
  ): Promise<QuotationDetails> {
    return apiClient<QuotationDetails>(
      `${QUOTATION_ENDPOINT}/${id}`,
      {
        method: "GET",
        token,
      },
    );
  },

  async getByEnquiryId(
    enquiryId: number,
    token: string,
  ): Promise<QuotationList[]> {
    return apiClient<QuotationList[]>(
      `${QUOTATION_ENDPOINT}/enquiry/${enquiryId}`,
      {
        method: "GET",
        token,
      },
    );
  },
};