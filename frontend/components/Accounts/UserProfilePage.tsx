"use client";

import Link from "next/link";
import {
  UserRound,
  Mail,
  Phone,
  MapPin,
  ShoppingBag,
  MapPinned,
  LogOut,
} from "lucide-react";

import { useAuth } from "@/components/providers/AuthProvider";

export function UserProfilePage() {
  const { user, isAuthenticated, isLoading, logout } = useAuth();

  if (isLoading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <p className="text-sm text-gray-500">
          Loading profile...
        </p>
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return (
      <div className="flex min-h-[400px] flex-col items-center justify-center text-center">
        <UserRound className="size-12 text-gray-300" />

        <h1 className="mt-4 text-xl font-semibold text-[#1B2A4A]">
          Please sign in
        </h1>

        <p className="mt-2 text-sm text-gray-500">
          You need to sign in to view your profile.
        </p>

        <Link
          href="/login"
          className="mt-5 rounded-lg bg-[#3E8F96] px-5 py-2.5 text-sm font-medium text-white transition-colors hover:bg-[#347a80]"
        >
          Sign In
        </Link>
      </div>
    );
  }

  const fullName =
    `${user.firstName ?? ""} ${user.lastName ?? ""}`.trim();

  return (
    <section className="space-y-6">
      {/* Page Header */}
      <div>
        <p className="text-sm font-medium text-[#3E8F96]">
          My Account
        </p>

        <h1 className="mt-1 text-2xl font-semibold text-[#1B2A4A] sm:text-3xl">
          My Profile
        </h1>

        <p className="mt-2 text-sm text-gray-500">
          Manage your personal information and account.
        </p>
      </div>

      {/* Profile Header Card */}
      <div className="overflow-hidden rounded-2xl border border-[#e5e8e7] bg-white shadow-sm">
        <div className="h-24 bg-gradient-to-r from-[#EAF5F3] to-[#F5F8F7]" />

        <div className="px-5 pb-6 sm:px-7">
          <div className="-mt-10 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
            <div className="flex items-end gap-4">
              {/* Avatar */}
              <div className="flex size-20 shrink-0 items-center justify-center rounded-full border-4 border-white bg-[#EAF5F3] shadow-sm">
                <UserRound className="size-9 text-[#3E8F96]" />
              </div>

              {/* Name */}
              <div className="pb-1">
                <h2 className="text-xl font-semibold text-[#1B2A4A]">
                  {fullName || "User"}
                </h2>

                <p className="mt-0.5 text-sm text-gray-500">
                  {user.email}
                </p>
              </div>
            </div>

            {/* Role */}
            <span className="w-fit rounded-full bg-[#EAF5F3] px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-[#3E8F96]">
              {user.role}
            </span>
          </div>
        </div>
      </div>

      {/* Personal Information */}
      <div className="rounded-2xl border border-[#e5e8e7] bg-white p-5 shadow-sm sm:p-7">
        <div className="flex items-center gap-2">
          <UserRound className="size-5 text-[#3E8F96]" />

          <h2 className="text-lg font-semibold text-[#1B2A4A]">
            Personal Information
          </h2>
        </div>

        <div className="mt-6 grid gap-5 sm:grid-cols-2">
          {/* First Name */}
          <div>
            <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
              First Name
            </p>

            <div className="mt-2 rounded-lg border border-gray-200 bg-gray-50 px-4 py-3 text-sm text-gray-800">
              {user.firstName || "N/A"}
            </div>
          </div>

          {/* Last Name */}
          <div>
            <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
              Last Name
            </p>

            <div className="mt-2 rounded-lg border border-gray-200 bg-gray-50 px-4 py-3 text-sm text-gray-800">
              {user.lastName || "N/A"}
            </div>
          </div>

          {/* Email */}
          <div>
            <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
              Email Address
            </p>

            <div className="mt-2 flex items-center gap-3 rounded-lg border border-gray-200 bg-gray-50 px-4 py-3">
              <Mail className="size-4 shrink-0 text-[#3E8F96]" />

              <span className="break-all text-sm text-gray-800">
                {user.email || "N/A"}
              </span>
            </div>
          </div>

          {/* Phone */}
          <div>
            <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
              Phone Number
            </p>

            <div className="mt-2 flex items-center gap-3 rounded-lg border border-gray-200 bg-gray-50 px-4 py-3">
              <Phone className="size-4 shrink-0 text-[#3E8F96]" />

              <span className="text-sm text-gray-800">
                {user.phoneNumber || "N/A"}
              </span>
            </div>
          </div>

          {/* Country */}
          <div>
            <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
              Country
            </p>

            <div className="mt-2 flex items-center gap-3 rounded-lg border border-gray-200 bg-gray-50 px-4 py-3">
              <MapPin className="size-4 shrink-0 text-[#3E8F96]" />

              <span className="text-sm text-gray-800">
                {user.country || "N/A"}
              </span>
            </div>
          </div>

          {/* Account Status */}
          <div>
            <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
              Account Status
            </p>

            <div className="mt-2 flex items-center rounded-lg border border-gray-200 bg-gray-50 px-4 py-3">
              <span
                className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${
                  user.isActive
                    ? "bg-green-50 text-green-700"
                    : "bg-red-50 text-red-700"
                }`}
              >
                {user.isActive ? "Active" : "Inactive"}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Account Actions */}
      <div className="rounded-2xl border border-[#e5e8e7] bg-white p-5 shadow-sm sm:p-7">
        <h2 className="text-lg font-semibold text-[#1B2A4A]">
          Account
        </h2>

        <div className="mt-5 grid gap-3 sm:grid-cols-2">
          {/* My Orders */}
          <Link
            href="/orders"
            className="flex items-center gap-3 rounded-xl border border-gray-200 px-4 py-4 transition-colors hover:border-[#3E8F96] hover:bg-[#F5F8F7]"
          >
            <div className="flex size-10 items-center justify-center rounded-lg bg-[#EAF5F3]">
              <ShoppingBag className="size-5 text-[#3E8F96]" />
            </div>

            <div>
              <p className="text-sm font-semibold text-[#1B2A4A]">
                My Orders
              </p>

              <p className="mt-0.5 text-xs text-gray-500">
                View your orders and order history
              </p>
            </div>
          </Link>

          {/* Addresses */}
          <Link
            href="/addresses"
            className="flex items-center gap-3 rounded-xl border border-gray-200 px-4 py-4 transition-colors hover:border-[#3E8F96] hover:bg-[#F5F8F7]"
          >
            <div className="flex size-10 items-center justify-center rounded-lg bg-[#EAF5F3]">
              <MapPinned className="size-5 text-[#3E8F96]" />
            </div>

            <div>
              <p className="text-sm font-semibold text-[#1B2A4A]">
                My Addresses
              </p>

              <p className="mt-0.5 text-xs text-gray-500">
                Manage your delivery addresses
              </p>
            </div>
          </Link>
        </div>

        {/* Logout */}
        <button
          type="button"
          onClick={logout}
          className="mt-5 inline-flex items-center gap-2 rounded-lg border border-red-200 px-4 py-2.5 text-sm font-medium text-red-600 transition-colors hover:bg-red-50"
        >
          <LogOut className="size-4" />
          Logout
        </button>
      </div>
    </section>
  );
}