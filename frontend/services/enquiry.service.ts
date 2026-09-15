import { apiClient } from "@/lib/api/client";
import type {
  CreateEnquiry,
  EnquiryDetails,
} from "@/types/enquiry";

const ENQUIRY_ENDPOINT = "/Enquiries";

export const enquiryService = {
  async create(
    data: CreateEnquiry,
    token: string,
  ): Promise<EnquiryDetails> {
    return apiClient<EnquiryDetails>(
      ENQUIRY_ENDPOINT,
      {
        method: "POST",
        token,
        body: JSON.stringify(data),
      },
    );
  },

  async getAll(
    token: string,
  ): Promise<EnquiryDetails[]> {
    return apiClient<EnquiryDetails[]>(
      ENQUIRY_ENDPOINT,
      {
        method: "GET",
        token,
      },
    );
  },

  async getById(
    id: number,
    token: string,
  ): Promise<EnquiryDetails> {
    return apiClient<EnquiryDetails>(
      `${ENQUIRY_ENDPOINT}/${id}`,
      {
        method: "GET",
        token,
      },
    );
  },

  async getByNumber(
    enquiryNumber: string,
    token: string,
  ): Promise<EnquiryDetails> {
    return apiClient<EnquiryDetails>(
      `${ENQUIRY_ENDPOINT}/number/${encodeURIComponent(
        enquiryNumber,
      )}`,
      {
        method: "GET",
        token,
      },
    );
  },
};