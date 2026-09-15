"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

import { useAuth } from "@/components/providers/AuthProvider";

interface AdminGuardProps {
  children: React.ReactNode;
}

export function AdminGuard({
  children,
}: AdminGuardProps) {
  const router = useRouter();

  const {
    user,
    isAuthenticated,
    isLoading,
  } = useAuth();

  const isAdmin =
    user?.role?.trim().toLowerCase() === "admin";

  useEffect(() => {
    if (isLoading) {
      return;
    }

    if (!isAuthenticated) {
      router.replace("/login");
      return;
    }

    if (!isAdmin) {
      router.replace("/");
    }
  }, [
    isAuthenticated,
    isLoading,
    isAdmin,
    router,
  ]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#fafafa]">
        <p className="text-sm text-[#595959]">
          Checking authorization...
        </p>
      </div>
    );
  }

  if (!isAuthenticated || !isAdmin) {
    return null;
  }

  return <>{children}</>;
}