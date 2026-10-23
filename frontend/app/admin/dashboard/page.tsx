"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import {
  ArrowRight,
  BookOpen,
  Boxes,
  Clock3,
  FileSpreadsheet,
  MessageSquareText,
  Package,
  Plus,
  RefreshCw,
  ShoppingBag,
  Star,
  UsersRound,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { DashboardStatCard } from "@/components/admin/DashboardStatCard";
import { adminService } from "@/services/adminService";
import { getAdminErrorMessage } from "@/lib/admin-error-message";
import type { AdminDashboard } from "@/types/admin";

export default function AdminPage() {
  const [dashboard, setDashboard] = useState<AdminDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    let active = true;

    const loadDashboard = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await adminService.getDashboard();
        if (active) setDashboard(data);
      } catch (loadError) {
        console.error("Failed to load dashboard:", loadError);
        if (active) {
          setError(getAdminErrorMessage(loadError, "We could not load the dashboard right now."));
        }
      } finally {
        if (active) setLoading(false);
      }
    };

    void loadDashboard();
    return () => { active = false; };
  }, [reloadKey]);

  const stats = dashboard ? [
    { title: "Products", value: dashboard.totalProducts, detail: "All products in your catalogue", icon: Package },
    { title: "Featured products", value: dashboard.featuredProducts, detail: "Highlighted on the website", icon: Star },
    { title: "Categories", value: dashboard.totalCategories, detail: "Product groups and subgroups", icon: Boxes },
    { title: "Pending enquiries", value: dashboard.pendingEnquiries, detail: "Waiting for your response", icon: Clock3, highlighted: dashboard.pendingEnquiries > 0 },
    { title: "Total enquiries", value: dashboard.totalEnquiries, detail: "Customer and business enquiries", icon: MessageSquareText },
    { title: "Orders", value: dashboard.totalOrders, detail: "Orders received", icon: ShoppingBag },
    { title: "Total users", value: dashboard.totalUsers, detail: "Registered customer accounts", icon: UsersRound },
    { title: "Active users", value: dashboard.activeUsers, detail: `Of ${dashboard.totalUsers} registered users`, icon: UsersRound },
  ] : [];

  return (
    <main className="min-h-screen bg-[#f5f7f6] px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-7xl">
        <header className="flex flex-col justify-between gap-5 rounded-3xl bg-gradient-to-r from-[#102747] via-[#17385c] to-[#007f68] px-6 py-7 text-white shadow-sm sm:flex-row sm:items-center sm:px-8">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-emerald-200">RashePharma · Admin</p>
            <h1 className="mt-2 text-3xl font-bold tracking-tight sm:text-4xl">Dashboard</h1>
            <p className="mt-2 max-w-xl text-sm text-white/80">A quick view of your catalogue and the work that needs attention.</p>
          </div>
          <Link href="/admin/products/new" className="inline-flex min-h-11 items-center justify-center gap-2 self-start rounded-xl bg-white px-4 py-2.5 text-sm font-semibold text-[#17385c] transition hover:bg-emerald-50 sm:self-auto">
            <Plus aria-hidden="true" className="size-4" /> Add product
          </Link>
        </header>

        <section aria-labelledby="overview-heading" className="mt-8">
          <div className="mb-4 flex items-end justify-between gap-4">
            <div>
              <h2 id="overview-heading" className="text-xl font-bold text-[#1B2A4A]">Store overview</h2>
              <p className="mt-1 text-sm text-slate-500">Key numbers to help you decide what to do next.</p>
            </div>
            {!loading && !error && (
              <button type="button" onClick={() => setReloadKey((value) => value + 1)} className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 transition hover:border-slate-300 hover:bg-slate-50">
                <RefreshCw aria-hidden="true" className="size-4" /> Refresh
              </button>
            )}
          </div>

          {loading && (
            <div role="status" aria-label="Loading dashboard" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
              {Array.from({ length: 8 }, (_, index) => <div key={index} className="h-36 animate-pulse rounded-2xl border border-slate-200 bg-white" />)}
              <span className="sr-only">Loading dashboard figures…</span>
            </div>
          )}

          {error && (
            <div role="alert" className="rounded-2xl border border-red-200 bg-red-50 p-5 text-sm text-red-800">
              <p className="font-semibold">Dashboard data did not load</p>
              <p className="mt-1">{error}</p>
              <button type="button" onClick={() => setReloadKey((value) => value + 1)} className="mt-4 inline-flex items-center gap-2 rounded-lg bg-red-800 px-3 py-2 font-semibold text-white hover:bg-red-900">
                <RefreshCw aria-hidden="true" className="size-4" /> Try again
              </button>
            </div>
          )}

          {!loading && !error && dashboard && (
            <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
              {stats.map((stat) => <DashboardStatCard key={stat.title} {...stat} />)}
            </div>
          )}
        </section>

        <section aria-labelledby="shortcuts-heading" className="mt-9">
          <div className="mb-4">
            <h2 id="shortcuts-heading" className="text-xl font-bold text-[#1B2A4A]">Quick actions</h2>
            <p className="mt-1 text-sm text-slate-500">Go straight to a common task.</p>
          </div>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <QuickAction href="/admin/products/new" title="Add a product" description="Create a listing and add its variants and images." icon={Package} />
            <QuickAction href="/admin/bulk-upload" title="Bulk upload" description="Import several products from an Excel workbook." icon={FileSpreadsheet} />
            <QuickAction href="/admin/categories/new" title="Add a category" description="Organise products into a category or subcategory." icon={Boxes} />
            <QuickAction href="/admin/blog/new" title="Write an article" description="Create a draft for the public blog." icon={BookOpen} />
          </div>
        </section>
      </div>
    </main>
  );
}

function QuickAction({ href, title, description, icon: Icon }: {
  href: string;
  title: string;
  description: string;
  icon: LucideIcon;
}) {
  return (
    <Link href={href} className="group flex min-h-36 items-start gap-4 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:border-emerald-300 hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-700">
      <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-emerald-50 text-[#00856b]"><Icon aria-hidden="true" className="size-5" /></span>
      <span className="min-w-0 flex-1">
        <span className="flex items-center justify-between gap-2 font-semibold text-[#1B2A4A]">{title}<ArrowRight aria-hidden="true" className="size-4 shrink-0 text-slate-400 transition group-hover:translate-x-1 group-hover:text-[#00856b]" /></span>
        <span className="mt-2 block text-sm leading-5 text-slate-500">{description}</span>
      </span>
    </Link>
  );
}
