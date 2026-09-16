"use client";

import { useEffect, useState } from "react";
import { DashboardStatCard } from "@/components/admin/DashboardStatCard";
import { adminService } from "@/services/adminService";
import type { AdminDashboard } from "@/types/admin";

export default function AdminPage() {
  const [dashboard, setDashboard] = useState<AdminDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        const data = await adminService.getDashboard();

        setDashboard(data);
      } catch (error) {
        console.error("Failed to load dashboard:", error);

        setError("Failed to load dashboard data.");
      } finally {
        setLoading(false);
      }
    };

    loadDashboard();
  }, []);

  return (
    <main className="min-h-screen bg-[#f5f7f6] p-6">
      <div className="mx-auto max-w-7xl">
        <h1 className="text-3xl font-semibold text-[#1B2A4A]">
          Admin Dashboard
        </h1>

        <p className="mt-2 text-sm text-[#595959]">
          Welcome to the RashePharma admin panel.
        </p>

        {loading && (
          <p className="mt-8 text-sm text-gray-500">
            Loading dashboard...
          </p>
        )}

        {error && (
          <p className="mt-8 text-sm text-red-600">
            {error}
          </p>
        )}

        {!loading && !error && dashboard && (
          <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
            <DashboardStatCard
              title="Total Users"
              value={dashboard.totalUsers}
            />

            <DashboardStatCard
              title="Active Users"
              value={dashboard.activeUsers}
            />

            <DashboardStatCard
              title="Products"
              value={dashboard.totalProducts}
            />

            <DashboardStatCard
              title="Featured Products"
              value={dashboard.featuredProducts}
            />

            <DashboardStatCard
              title="Categories"
              value={dashboard.totalCategories}
            />

            <DashboardStatCard
              title="Enquiries"
              value={dashboard.totalEnquiries}
            />

            <DashboardStatCard
              title="Pending Enquiries"
              value={dashboard.pendingEnquiries}
            />

            <DashboardStatCard
              title="Orders"
              value={dashboard.totalOrders}
            />
          </div>
        )}
      </div>
    </main>
  );
}