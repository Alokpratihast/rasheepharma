import { apiClient } from "@/lib/api/client";
import type { AdminDashboard } from "@/types/admin";

const ADMIN_ENDPOINT = "/Admin";

export const adminService = {
  async getDashboard(): Promise<AdminDashboard> {
    return apiClient<AdminDashboard>(
      `${ADMIN_ENDPOINT}/dashboard`
    );
  },
};