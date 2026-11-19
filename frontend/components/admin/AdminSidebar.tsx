"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useState } from "react";

import {
  Boxes,
  ClipboardList,
  FileText,
  LayoutDashboard,
  LogOut,
  Package,
  ShoppingCart,
  Users,
  Globe,
  MapPin,
  Handshake,
  BookOpen,
  Upload,
  ChevronLeft,
  ChevronRight,
  Menu,
  X,
} from "lucide-react";

import { useAuth } from "@/components/providers/AuthProvider";

const menuItems = [
  {
    label: "Dashboard",
    href: "/admin/dashboard",
    icon: LayoutDashboard,
  },
  {
    label: "Products",
    href: "/admin/products",
    icon: Package,
  },

  {
    label: "Bulk Upload",
    href: "/admin/bulk-upload",
    icon: Upload,
  },
  {
    label: "Categories",
    href: "/admin/categories",
    icon: Boxes,
  },
  {
    label: "Enquiries",
    href: "/admin/enquiries",
    icon: ClipboardList,
  },
  {
    label: "Quotations",
    href: "/admin/quotations",
    icon: FileText,
  },
  {
    label: "Orders",
    href: "/admin/orders",
    icon: ShoppingCart,
  },
  {
    label: "Users",
    href: "/admin/users",
    icon: Users,
  },
  {
    label: "Website Content",
    href: "/admin/content",
    icon: Globe,
  },
  {
    label: "Location SEO",
    href: "/admin/locations",
    icon: MapPin,
  },
  // Blog has its own workflow and does not alter the existing Website Content screen.
  {
    label: "Blog",
    href: "/admin/blog",
    icon: BookOpen,
  },
  {
    label: "Partners",
    href: "/admin/partners",
    icon: Handshake,
  },
];

export function AdminSidebar() {
  const pathname = usePathname();
  const router = useRouter();
  const { logout } = useAuth();

  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleLogout = () => {
    setMobileOpen(false);
    logout();
    router.push("/login");
  };

  return (
    <>
      <header className="fixed inset-x-0 top-0 z-40 flex h-14 items-center justify-between border-b border-slate-200 bg-white px-4 lg:hidden">
        <div>
          <p className="font-bold text-[#1B2A4A]">RashePharma</p>
          <p className="text-[11px] text-slate-500">Admin Panel</p>
        </div>
        <button
          type="button"
          onClick={() => setMobileOpen((open) => !open)}
          aria-label={mobileOpen ? "Close navigation" : "Open navigation"}
          aria-expanded={mobileOpen}
          aria-controls="admin-navigation"
          className="flex size-10 items-center justify-center rounded-lg border border-slate-200 text-slate-700 hover:bg-slate-50"
        >
          {mobileOpen ? <X aria-hidden="true" className="size-5" /> : <Menu aria-hidden="true" className="size-5" />}
        </button>
      </header>

      {mobileOpen && (
        <button
          type="button"
          aria-label="Close navigation menu"
          onClick={() => setMobileOpen(false)}
          className="fixed inset-0 z-40 bg-slate-950/40 lg:hidden"
        />
      )}

      <aside
        id="admin-navigation"
        className={`fixed inset-y-0 left-0 z-50 flex h-screen flex-col border-r border-gray-200 bg-white transition-all duration-300 ${
          collapsed ? "w-64 lg:w-20" : "w-64"
        } ${mobileOpen ? "translate-x-0" : "-translate-x-full"} lg:translate-x-0`}
      >
      {/* Header / Logo */}
      <div
        className={`relative flex h-16 shrink-0 items-center border-b border-gray-200 ${
          collapsed ? "justify-center px-3" : "px-6"
        }`}
      >
        {!collapsed ? (
          <div>
            <h1 className="text-lg font-bold text-[#1B2A4A]">
              RashePharma
            </h1>

            <p className="text-xs text-gray-500">
              Admin Panel
            </p>
          </div>
        ) : (
          <div className="flex size-10 items-center justify-center rounded-lg bg-[#1B2A4A] text-lg font-bold text-white">
            R
          </div>
        )}

        {/* Collapse / Expand */}
        <button
          type="button"
          onClick={() => setCollapsed((value) => !value)}
          aria-label={
            collapsed ? "Expand sidebar" : "Collapse sidebar"
          }
          title={
            collapsed ? "Expand sidebar" : "Collapse sidebar"
          }
          className="absolute -right-3 top-1/2 hidden size-7 -translate-y-1/2 items-center justify-center rounded-full border border-gray-200 bg-white text-gray-500 shadow-sm transition hover:bg-gray-100 hover:text-[#1B2A4A] lg:flex"
        >
          {collapsed ? (
            <ChevronRight className="size-4" />
          ) : (
            <ChevronLeft className="size-4" />
          )}
        </button>
      </div>

      {/* Navigation */}
      <nav className="min-h-0 flex-1 overflow-y-auto p-3">
        <div className="space-y-1">
          {menuItems.map((item) => {
            const Icon = item.icon;

            const isActive =
              pathname === item.href ||
              pathname.startsWith(`${item.href}/`);

            return (
              <Link
                key={item.href}
                href={item.href}
                onClick={() => setMobileOpen(false)}
                title={collapsed ? item.label : undefined}
                className={`flex items-center rounded-lg py-2.5 text-sm font-medium transition ${
                  collapsed
                    ? "justify-center px-2"
                    : "gap-3 px-3"
                } ${
                  isActive
                    ? "bg-[#1B2A4A] text-white"
                    : "text-gray-600 hover:bg-gray-100 hover:text-[#1B2A4A]"
                }`}
              >
                <Icon className="size-5 shrink-0" />

                {!collapsed && (
                  <span className="truncate">
                    {item.label}
                  </span>
                )}
              </Link>
            );
          })}
        </div>
      </nav>

      {/* Bottom Actions */}
      <div className="shrink-0 border-t border-gray-200 bg-white p-3">
        {/* View Website */}
        <Link
          href="/"
          onClick={() => setMobileOpen(false)}
          title={collapsed ? "View Website" : undefined}
          className={`flex items-center rounded-lg py-2.5 text-sm font-medium text-gray-600 transition hover:bg-gray-100 hover:text-[#1B2A4A] ${
            collapsed
              ? "justify-center px-2"
              : "gap-3 px-3"
          }`}
        >
          <Globe className="size-5 shrink-0" />

          {!collapsed && <span>View Website</span>}
        </Link>

        {/* Logout */}
        <button
          type="button"
          onClick={handleLogout}
          title={collapsed ? "Logout" : undefined}
          className={`mt-1 flex w-full items-center rounded-lg py-2.5 text-sm font-medium text-gray-600 transition hover:bg-gray-100 hover:text-red-600 ${
            collapsed
              ? "justify-center px-2"
              : "gap-3 px-3"
          }`}
        >
          <LogOut className="size-5 shrink-0" />

          {!collapsed && <span>Logout</span>}
        </button>
      </div>
      </aside>
    </>
  );
}

