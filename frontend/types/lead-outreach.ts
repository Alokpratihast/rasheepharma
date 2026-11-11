export type LeadPermissionStatus = "Unknown" | "Allowed" | "Blocked";
export type LeadOutreachStatus = "NeverSent" | "InProgress" | "Sent" | "Failed" | "Skipped";

export interface ExternalLead {
  id: number;
  name: string;
  email: string | null;
  phone: string | null;
  company: string | null;
  country: string | null;
  businessType: string | null;
  productInterest: string | null;
  source: string;
  permissionStatus: LeadPermissionStatus;
  permissionEvidence: string | null;
  historicalMailSentOn: string | null;
  isSuppressed: boolean;
  canReceiveMarketingEmail: boolean;
  outreachStatus: LeadOutreachStatus;
  updatedAtUtc: string;
}

export interface LeadPage {
  leads: ExternalLead[];
  page: number;
  pageSize: number;
  total: number;
  counts: { total: number; allowed: number; unknown: number; unknownEmail: number; suppressed: number };
}

export interface LeadImportSummary {
  id: number;
  fileName: string;
  status: string;
  totalRows: number;
  validRows: number;
  invalidRows: number;
  duplicateRows: number;
  importedRows: number;
  createdAtUtc: string;
}

export interface LeadImportPreview {
  batch: LeadImportSummary;
  page: number;
  pageSize: number;
  rows: Array<{
    sheetName: string;
    sheetRow: number;
    name: string;
    email: string | null;
    phone: string | null;
    productInterest: string | null;
    historicalMailSentOn: string | null;
    validationError: string | null;
    isDuplicate: boolean;
  }>;
  permissionNotice: string;
}

export interface LeadCampaign {
  id: number;
  name: string;
  subject: string;
  attachBrochure: boolean;
  status: string;
  recipientCount: number;
  sentCount: number;
  failedCount: number;
  respondedCount: number;
  createdAtUtc: string;
  queuedAtUtc: string | null;
  completedAtUtc: string | null;
}

export interface LeadCampaignResponse {
  id: number;
  name: string;
  email: string | null;
  message: string;
  submittedAtUtc: string;
}

export interface LeadCampaignHistoryPage {
  campaigns: LeadCampaign[];
  page: number;
  pageSize: number;
  total: number;
}

export interface LeadCampaignRecipient {
  id: number;
  name: string;
  email: string | null;
  productInterest: string | null;
  status: string;
  attemptCount: number;
  errorMessage: string | null;
  createdAtUtc: string;
  sentAtUtc: string | null;
}

export interface LeadCampaignDetailPage {
  campaign: LeadCampaign & { bodyText: string };
  recipients: LeadCampaignRecipient[];
  page: number;
  pageSize: number;
  total: number;
  statusCounts: Array<{ status: string; count: number }>;
}
