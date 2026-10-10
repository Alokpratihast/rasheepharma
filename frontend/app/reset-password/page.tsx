"use client";

import {
  FormEvent,
  Suspense,
  useState,
} from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { LockKeyhole, CheckCircle2, Eye, EyeOff } from "lucide-react";

import { Button } from "@/components/ui/button";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ||
  "http://localhost:5104/api";

function ResetPasswordForm() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token") ?? "";

  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] =
    useState("");

  const [showPassword, setShowPassword] =
    useState(false);
  const [showConfirmPassword, setShowConfirmPassword] =
    useState(false);

  const [isLoading, setIsLoading] =
    useState(false);

  const [errorMessage, setErrorMessage] =
    useState("");

  const [successMessage, setSuccessMessage] =
    useState("");

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    setErrorMessage("");
    setSuccessMessage("");

    if (!token) {
      setErrorMessage(
        "Invalid password reset link."
      );
      return;
    }

    if (!newPassword) {
      setErrorMessage(
        "Please enter your new password."
      );
      return;
    }

    if (newPassword.length < 8) {
      setErrorMessage(
        "Password must be at least 8 characters."
      );
      return;
    }

    if (newPassword !== confirmPassword) {
      setErrorMessage(
        "Passwords do not match."
      );
      return;
    }

    try {
      setIsLoading(true);

      const response = await fetch(
        `${API_BASE_URL}/Auth/reset-password`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            token,
            newPassword,
          }),
        }
      );

      const data =
        await response.json().catch(() => null);

      if (!response.ok) {
        throw new Error(
          data?.message ||
            "Unable to reset your password."
        );
      }

      setSuccessMessage(
        data?.message ||
          "Password reset successfully."
      );

      setNewPassword("");
      setConfirmPassword("");
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : "Unable to reset your password. Please try again."
      );
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="min-h-screen bg-[#fafafa] px-4 py-10 sm:px-6 sm:py-16">
      <div className="mx-auto flex w-full max-w-md items-center justify-center">
        <div className="w-full rounded-2xl border border-[#e5e8e7] bg-white p-6 shadow-sm sm:p-8">

          {/* Header */}
          <div className="mb-7">
            <div className="mb-4 flex size-12 items-center justify-center rounded-xl bg-[#EAF5F3]">
              <LockKeyhole className="size-5 text-[#3E8F96]" />
            </div>

            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
              Account Recovery
            </p>

            <h1 className="mt-2 text-2xl font-semibold tracking-tight text-[#1B2A4A] sm:text-3xl">
              Reset your password
            </h1>

            <p className="mt-2 text-sm leading-6 text-[#595959]">
              Create a new password for your
              Rashe Pharma account.
            </p>
          </div>

          {/* Success */}
          {successMessage && (
            <div
              role="status"
              className="mb-5 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm leading-5 text-green-700"
            >
              <div className="flex items-start gap-2">
                <CheckCircle2 className="mt-0.5 size-4 shrink-0" />
                <span>{successMessage}</span>
              </div>
            </div>
          )}

          {/* Error */}
          {(errorMessage || !token) && (
            <div
              role="alert"
              className="mb-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm leading-5 text-red-700"
            >
              {errorMessage || "Invalid password reset link."}
            </div>
          )}

          {!successMessage && (
            <form onSubmit={handleSubmit}>

              {/* New Password */}
              <div>
                <label
                  htmlFor="new-password"
                  className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
                >
                  New Password
                  <span className="ml-1 text-[#F5821F]">
                    *
                  </span>
                </label>

                <div className="relative">
                  <LockKeyhole className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-[#7c8790]" />

                  <input
                    id="new-password"
                    name="newPassword"
                    type={
                      showPassword
                        ? "text"
                        : "password"
                    }
                    required
                    minLength={8}
                    autoComplete="new-password"
                    value={newPassword}
                    onChange={(event) => {
                      setNewPassword(
                        event.target.value
                      );
                      setErrorMessage("");
                    }}
                    placeholder="Enter new password"
                    className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white pl-10 pr-11 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
                  />

                  <button
                    type="button"
                    onClick={() =>
                      setShowPassword(
                        (value) => !value
                      )
                    }
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-[#7c8790] hover:text-[#3E8F96]"
                    aria-label={
                      showPassword
                        ? "Hide password"
                        : "Show password"
                    }
                  >
                    {showPassword ? (
                      <EyeOff className="size-4" />
                    ) : (
                      <Eye className="size-4" />
                    )}
                  </button>
                </div>

                <p className="mt-1.5 text-xs text-[#777]">
                  Minimum 8 characters.
                </p>
              </div>

              {/* Confirm Password */}
              <div className="mt-5">
                <label
                  htmlFor="confirm-password"
                  className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
                >
                  Confirm Password
                  <span className="ml-1 text-[#F5821F]">
                    *
                  </span>
                </label>

                <div className="relative">
                  <LockKeyhole className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-[#7c8790]" />

                  <input
                    id="confirm-password"
                    name="confirmPassword"
                    type={
                      showConfirmPassword
                        ? "text"
                        : "password"
                    }
                    required
                    minLength={8}
                    autoComplete="new-password"
                    value={confirmPassword}
                    onChange={(event) => {
                      setConfirmPassword(
                        event.target.value
                      );
                      setErrorMessage("");
                    }}
                    placeholder="Confirm new password"
                    className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white pl-10 pr-11 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
                  />

                  <button
                    type="button"
                    onClick={() =>
                      setShowConfirmPassword(
                        (value) => !value
                      )
                    }
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-[#7c8790] hover:text-[#3E8F96]"
                    aria-label={
                      showConfirmPassword
                        ? "Hide password"
                        : "Show password"
                    }
                  >
                    {showConfirmPassword ? (
                      <EyeOff className="size-4" />
                    ) : (
                      <Eye className="size-4" />
                    )}
                  </button>
                </div>
              </div>

              {/* Submit */}
              <Button
                type="submit"
                disabled={isLoading}
                size="lg"
                className="mt-6 h-11 w-full rounded-lg bg-[#F5821F] text-white hover:bg-[#df7115] disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isLoading
                  ? "Resetting..."
                  : "Reset Password"}
              </Button>
            </form>
          )}

          {/* Login */}
          <div className="mt-6 border-t border-[#edf0ef] pt-5 text-center">
            <Link
              href="/login"
              className="text-sm font-semibold text-[#3E8F96] hover:underline"
            >
              Back to Login
            </Link>
          </div>
        </div>
      </div>
    </main>
  );
}
export default function ResetPasswordPage() {
  return (
    <Suspense
      fallback={
        <main className="min-h-screen bg-[#fafafa]" aria-busy="true" />
      }
    >
      <ResetPasswordForm />
    </Suspense>
  );
}
