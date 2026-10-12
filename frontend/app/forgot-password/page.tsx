"use client";

import { FormEvent, useState } from "react";
import Link from "next/link";
import { ArrowLeft, LockKeyhole, Mail, Send } from "lucide-react";

import { Button } from "@/components/ui/button";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ||
  "http://localhost:5104/api";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    setErrorMessage("");
    setSuccessMessage("");

    const trimmedEmail = email.trim();

    if (!trimmedEmail) {
      setErrorMessage("Please enter your email address.");
      return;
    }

    try {
      setIsLoading(true);

      const response = await fetch(
        `${API_BASE_URL}/Auth/forgot-password`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            email: trimmedEmail,
          }),
        }
      );

      const data = await response.json().catch(() => null);

      if (!response.ok) {
        throw new Error(
          data?.message ||
            "Unable to process your request. Please try again."
        );
      }

      setSuccessMessage(
        data?.message ||
          "If an account exists with this email, you will receive a password reset link."
      );
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : "Unable to process your request. Please try again."
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
              Forgot your password?
            </h1>

            <p className="mt-2 text-sm leading-6 text-[#595959]">
              Enter your registered email address and we&apos;ll help you
              reset your password.
            </p>
          </div>

          {/* Success */}
          {successMessage && (
            <div
              role="status"
              className="mb-5 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm leading-5 text-green-700"
            >
              {successMessage}
            </div>
          )}

          {/* Error */}
          {errorMessage && (
            <div
              role="alert"
              className="mb-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm leading-5 text-red-700"
            >
              {errorMessage}
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit}>
            <div>
              <label
                htmlFor="forgot-password-email"
                className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
              >
                Email
                <span className="ml-1 text-[#F5821F]">*</span>
              </label>

              <div className="relative">
                <Mail className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-[#7c8790]" />

                <input
                  id="forgot-password-email"
                  name="email"
                  type="email"
                  required
                  autoComplete="email"
                  value={email}
                  onChange={(event) => {
                    setEmail(event.target.value);
                    setErrorMessage("");
                    setSuccessMessage("");
                  }}
                  placeholder="you@company.com"
                  className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white pl-10 pr-3 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
                />
              </div>
            </div>

            <Button
              type="submit"
              disabled={isLoading || !!successMessage}
              size="lg"
              className="mt-6 h-11 w-full rounded-lg bg-[#F5821F] text-white hover:bg-[#df7115] disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isLoading ? (
                "Sending..."
              ) : successMessage ? (
                "Reset Link Requested"
              ) : (
                <>
                  Send Reset Link
                  <Send className="size-4" />
                </>
              )}
            </Button>
          </form>

          {/* Back to Login */}
          <div className="mt-6 border-t border-[#edf0ef] pt-5 text-center">
            <Link
              href="/login"
              className="inline-flex items-center gap-2 text-sm font-semibold text-[#3E8F96] hover:underline"
            >
              <ArrowLeft className="size-4" />
              Back to Login
            </Link>
          </div>
        </div>
      </div>
    </main>
  );
}