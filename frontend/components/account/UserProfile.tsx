"use client";

import { useState } from "react";
import Link from "next/link";
import {
  UserRound,
  LogOut,
  ChevronDown,
} from "lucide-react";

import { useAuth } from "@/components/providers/AuthProvider";

export function UserProfile() {
  const [open, setOpen] = useState(false);

  const {
    user,
    isAuthenticated,
    logout,
  } = useAuth();

  if (!isAuthenticated || !user) {
    return (
      <Link
        href="/login"
        aria-label="Sign in"
        className="flex size-9 items-center justify-center rounded-lg text-[#1B2A4A] transition-colors hover:bg-[#F3F6F5] hover:text-primary"
      >
        <UserRound className="size-4" />
      </Link>
    );
  }

  function handleLogout() {
    setOpen(false);
    logout();
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() =>
          setOpen((current) => !current)
        }
        aria-expanded={open}
        aria-haspopup="true"
        aria-label="Open account menu"
        className="flex items-center gap-1.5 rounded-lg px-2 py-1.5 text-[#1B2A4A] transition-colors hover:bg-[#F3F6F5]"
      >
        <div className="flex size-8 items-center justify-center rounded-full bg-[#EAF5F3]">
          <UserRound className="size-4 text-[#3E8F96]" />
        </div>

        <span className="hidden max-w-24 truncate text-sm font-medium xl:block">
          {user.firstName}
        </span>

        <ChevronDown
          className={[
            "size-3.5 transition-transform",
            open ? "rotate-180" : "",
          ].join(" ")}
        />
      </button>

      {open && (
        <div className="absolute right-0 top-full z-50 mt-2 w-60 overflow-hidden rounded-xl border border-[#e5e8e7] bg-white shadow-[0_15px_40px_rgba(27,42,74,0.12)]">
          {/* User information */}
          <div className="border-b border-[#edf0ef] px-4 py-3">
            <p className="truncate text-sm font-semibold text-[#1B2A4A]">
              {user.firstName} {user.lastName}
            </p>

            <p className="mt-0.5 truncate text-xs text-[#777]">
              {user.email}
            </p>

            <span className="mt-2 inline-flex rounded-full bg-[#EAF5F3] px-2 py-1 text-[10px] font-semibold uppercase tracking-wide text-[#3E8F96]">
              {user.role}
            </span>
          </div>

          {/* Account */}
          <div className="p-2">
            <Link
              href="/profile"
              onClick={() => setOpen(false)}
              className="flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm text-[#1B2A4A] transition-colors hover:bg-[#F5F8F7]"
            >
              <UserRound className="size-4 text-[#3E8F96]" />
              My Profile
            </Link>

            <button
              type="button"
              onClick={handleLogout}
              className="flex w-full items-center gap-2 rounded-lg px-3 py-2.5 text-left text-sm text-red-600 transition-colors hover:bg-red-50"
            >
              <LogOut className="size-4" />
              Logout
            </button>
          </div>
        </div>
      )}
    </div>
  );
}