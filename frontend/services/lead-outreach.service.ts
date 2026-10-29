import { apiClient } from "@/lib/api/client";
import type { ExternalLead, LeadCampaign, LeadImportPreview, LeadImportSummary, LeadPage, LeadPermissionStatus } from "@/types/lead-outreach";

export const leadOutreachService = {
  importWorkbook(file: File) {
    const data = new FormData();
    data.set("file", file);
    return apiClient<LeadImportSummary>("/admin/leads/imports", { method: "POST", body: data });
  },
  getImportPreview(id: number) {
    return apiClient<LeadImportPreview>(`/admin/leads/imports/${id}`);
  },
  confirmImport(id: number, confirmEmailEligibility: boolean) {
    return apiClient<LeadImportSummary>(`/admin/leads/imports/${id}/confirm`, {
      method: "POST",
      body: JSON.stringify({ confirmEmailEligibility }),
    });
  },
  getLeads(params: { search: string; permission: string; page: number; pageSize: number }) {
    const query = new URLSearchParams({
      search: params.search,
      permission: params.permission,
      page: String(params.page),
      pageSize: String(params.pageSize),
    });
    return apiClient<LeadPage>(`/admin/leads?${query.toString()}`);
  },
  updatePermission(id: number, permissionStatus: LeadPermissionStatus, evidence?: string) {
    return apiClient<Pick<ExternalLead, "id" | "permissionStatus" | "permissionEvidence" | "isSuppressed" | "canReceiveMarketingEmail">>(
      `/admin/leads/${id}/permission`,
      { method: "PUT", body: JSON.stringify({ permissionStatus, evidence }) },
    );
  },
  confirmUnknownLeads(confirmEmailEligibility: boolean) {
    return apiClient<{ updatedCount: number }>("/admin/leads/confirm-unknown", {
      method: "POST",
      body: JSON.stringify({ confirmEmailEligibility }),
    });
  },
  queueCampaign(input: { name: string; subject: string; bodyText: string; leadIds: number[]; confirmSend: boolean }) {
    return apiClient<{ id: number; status: string; recipientCount: number; excludedCount: number; message: string }>(
      "/admin/lead-campaigns/send", { method: "POST", body: JSON.stringify(input) },
    );
  },
  getCampaigns() {
    return apiClient<LeadCampaign[]>("/admin/lead-campaigns");
  },
};
