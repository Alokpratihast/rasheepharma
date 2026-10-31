export type LeadPermissionStatus = "Unknown" | "Allowed" | "Blocked";

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
  status: string;
  recipientCount: number;
  sentCount: number;
  failedCount: number;
  createdAtUtc: string;
  queuedAtUtc: string | null;
  completedAtUtc: string | null;
}
