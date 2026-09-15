"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import {
  Eye,
  EyeOff,
  LockKeyhole,
  Mail,
  LogIn,
} from "lucide-react";
import Link from "next/link";

import { Button } from "@/components/ui/button";
import { authService } from "@/services/auth.service";
import { useAuth } from "@/components/providers/AuthProvider";

interface LoginFormProps {
  onSuccess?: () => void;
}

export function LoginForm({
  onSuccess,
}: LoginFormProps) {
  const router = useRouter();

  const { login } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [showPassword, setShowPassword] = useState(false);

  const [isLoading, setIsLoading] = useState(false);

  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    setErrorMessage("");
    setSuccessMessage("");

    if (!email.trim()) {
      setErrorMessage("Please enter your email address.");
      return;
    }

    if (!password) {
      setErrorMessage("Please enter your password.");
      return;
    }

    try {
      setIsLoading(true);

      const response = await authService.login({
  email: email.trim(),
  password,
});

console.log("LOGIN RESPONSE:", response);
console.log("LOGIN ROLE:", response.role);

login(response);

setSuccessMessage(
  "Login successful! Redirecting...",
);

onSuccess?.();

const role = response.role?.trim().toLowerCase();

console.log("NORMALIZED ROLE:", role);

setTimeout(() => {
  if (role === "admin") {
    router.push("/admin/dashboard");
  } else {
    router.push("/");
  }
}, 1000);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Unable to sign in. Please try again.";

      setErrorMessage(message);
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="w-full rounded-2xl border border-[#e5e8e7] bg-white p-6 shadow-sm sm:p-8"
    >
      {/* =================================================
          HEADER
      ================================================== */}

      <div className="mb-7">
        <div className="mb-4 flex size-12 items-center justify-center rounded-xl bg-[#EAF5F3]">
          <LockKeyhole className="size-5 text-[#3E8F96]" />
        </div>

        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
          Account Access
        </p>

        <h1 className="mt-2 text-2xl font-semibold tracking-tight text-[#1B2A4A] sm:text-3xl">
          Welcome back
        </h1>

        <p className="mt-2 text-sm leading-6 text-[#595959]">
          Sign in to continue to your RashePharma account.
        </p>
      </div>

      {/* =================================================
          SUCCESS
      ================================================== */}

      {successMessage && (
        <div
          role="status"
          className="mb-5 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700"
        >
          {successMessage}
        </div>
      )}

      {/* =================================================
          ERROR
      ================================================== */}

      {errorMessage && (
        <div
          role="alert"
          className="mb-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
        >
          {errorMessage}
        </div>
      )}

      {/* =================================================
          EMAIL
      ================================================== */}

      <div>
        <label
          htmlFor="login-email"
          className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
        >
          Email
          <span className="ml-1 text-[#F5821F]">*</span>
        </label>

        <div className="relative">
          <Mail className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-[#7c8790]" />

          <input
            id="login-email"
            name="email"
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(event) =>
              setEmail(event.target.value)
            }
            placeholder="you@company.com"
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white pl-10 pr-3 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
          />
        </div>
      </div>

      {/* =================================================
          PASSWORD
      ================================================== */}

      <div className="mt-5">
        <div className="mb-1.5 flex items-center justify-between">
          <label
            htmlFor="login-password"
            className="text-sm font-medium text-[#1B2A4A]"
          >
            Password
            <span className="ml-1 text-[#F5821F]">*</span>
          </label>

          <button
            type="button"
            className="text-xs font-medium text-[#3E8F96] hover:underline"
          >
            Forgot password?
          </button>
        </div>

        <div className="relative">
          <LockKeyhole className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-[#7c8790]" />

          <input
            id="login-password"
            name="password"
            type={showPassword ? "text" : "password"}
            required
            autoComplete="current-password"
            value={password}
            onChange={(event) =>
              setPassword(event.target.value)
            }
            placeholder="Enter your password"
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white pl-10 pr-11 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
          />

          <button
            type="button"
            onClick={() =>
              setShowPassword((current) => !current)
            }
            aria-label={
              showPassword
                ? "Hide password"
                : "Show password"
            }
            className="absolute right-3 top-1/2 flex size-7 -translate-y-1/2 items-center justify-center rounded-md text-[#718096] hover:bg-[#F3F6F5] hover:text-[#1B2A4A]"
          >
            {showPassword ? (
              <EyeOff className="size-4" />
            ) : (
              <Eye className="size-4" />
            )}
          </button>
        </div>
      </div>

      {/* =================================================
          REMEMBER ME
      ================================================== */}

      <div className="mt-5 flex items-center">
        <label className="flex cursor-pointer items-center gap-2 text-sm text-[#595959]">
          <input
            type="checkbox"
            className="size-4 rounded border-[#cfd6d4] accent-[#3E8F96]"
          />
          Remember me
        </label>
      </div>

      {/* =================================================
          SUBMIT
      ================================================== */}

      <Button
        type="submit"
        disabled={isLoading || !!successMessage}
        size="lg"
        className="mt-6 h-11 w-full rounded-lg bg-[#F5821F] text-white hover:bg-[#df7115] disabled:cursor-not-allowed disabled:opacity-60"
      >
        {isLoading ? (
          "Signing in..."
        ) : successMessage ? (
          "Login Successful"
        ) : (
          <>
            Sign In
            <LogIn className="size-4" />
          </>
        )}
      </Button>

      {/* =================================================
          REGISTER
      ================================================== */}

      <div className="mt-6 border-t border-[#edf0ef] pt-5 text-center">
        <p className="text-sm text-[#777]">
          Don&apos;t have an account?
        </p>

        <Link
          href="/register"
          className="mt-1 inline-block text-sm font-semibold text-[#3E8F96] hover:underline"
        >
          Create an account
        </Link>
      </div>
    </form>
  );
}