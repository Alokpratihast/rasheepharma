"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { Eye, EyeOff, LockKeyhole, Mail, UserPlus } from "lucide-react";
import type { Country } from "react-phone-number-input";

import { Button } from "@/components/ui/button";
import { PhoneCountryFields } from "@/components/forms/PhoneCountryFields";
import { authService } from "@/services/auth.service";

interface RegisterFormProps {
  onSuccess?: () => void;
}

export function RegisterForm({
  onSuccess,
}: RegisterFormProps) {
  const router = useRouter();

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState<string>();
  const [country, setCountry] = useState<Country>("IN");

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] =
    useState(false);

  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>
  ) {
    event.preventDefault();

    setErrorMessage("");
    setSuccessMessage("");

    if (!firstName.trim()) {
      setErrorMessage("Please enter your first name.");
      return;
    }

    if (!lastName.trim()) {
      setErrorMessage("Please enter your last name.");
      return;
    }

    if (!email.trim()) {
      setErrorMessage("Please enter your email address.");
      return;
    }

    if (!password) {
      setErrorMessage("Please enter a password.");
      return;
    }

    if (password.length < 6) {
      setErrorMessage(
        "Password must be at least 6 characters long."
      );
      return;
    }

    if (password !== confirmPassword) {
      setErrorMessage("Passwords do not match.");
      return;
    }

    try {
      setIsLoading(true);

      const countryName =
        new Intl.DisplayNames(["en"], {
          type: "region",
        }).of(country) ?? country;

      const response = await authService.register({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
        phoneNumber: phoneNumber ?? null,
        country: countryName,
        password,
      });

      console.log("Registration successful:", response);

      // Show success message
      setSuccessMessage(
        "Registration successful! Redirecting to login..."
      );

      // Keep existing callback if parent uses it
      onSuccess?.();

      // Redirect to login after showing success message
      setTimeout(() => {
        router.push("/login");
      }, 1500);

    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Unable to create your account. Please try again.";

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
          <UserPlus className="size-5 text-[#3E8F96]" />
        </div>

        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
          Create Account
        </p>

        <h1 className="mt-2 text-2xl font-semibold tracking-tight text-[#1B2A4A] sm:text-3xl">
          Create your account
        </h1>

        <p className="mt-2 text-sm leading-6 text-[#595959]">
          Register with RashePharma to manage your enquiries and
          account information.
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
          NAME
      ================================================== */}

      <div className="grid gap-5 sm:grid-cols-2">

        {/* First Name */}
        <div>
          <label
            htmlFor="register-first-name"
            className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
          >
            First Name
            <span className="ml-1 text-[#F5821F]">*</span>
          </label>

          <input
            id="register-first-name"
            name="firstName"
            type="text"
            required
            autoComplete="given-name"
            value={firstName}
            onChange={(event) =>
              setFirstName(event.target.value)
            }
            placeholder="First name"
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
          />
        </div>

        {/* Last Name */}
        <div>
          <label
            htmlFor="register-last-name"
            className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
          >
            Last Name
            <span className="ml-1 text-[#F5821F]">*</span>
          </label>

          <input
            id="register-last-name"
            name="lastName"
            type="text"
            required
            autoComplete="family-name"
            value={lastName}
            onChange={(event) =>
              setLastName(event.target.value)
            }
            placeholder="Last name"
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white px-3 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
          />
        </div>
      </div>

      {/* =================================================
          EMAIL
      ================================================== */}

      <div className="mt-5">
        <label
          htmlFor="register-email"
          className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
        >
          Email
          <span className="ml-1 text-[#F5821F]">*</span>
        </label>

        <div className="relative">
          <Mail className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-[#7c8790]" />

          <input
            id="register-email"
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
          PHONE + COUNTRY
      ================================================== */}

      <div className="mt-5">
        <PhoneCountryFields
          country={country}
          phoneNumber={phoneNumber}
          onCountryChange={setCountry}
          onPhoneChange={setPhoneNumber}
        />
      </div>

      {/* =================================================
          PASSWORD
      ================================================== */}

      <div className="mt-5">
        <label
          htmlFor="register-password"
          className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
        >
          Password
          <span className="ml-1 text-[#F5821F]">*</span>
        </label>

        <div className="relative">
          <LockKeyhole className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-[#7c8790]" />

          <input
            id="register-password"
            name="password"
            type={showPassword ? "text" : "password"}
            required
            autoComplete="new-password"
            value={password}
            onChange={(event) =>
              setPassword(event.target.value)
            }
            placeholder="Create a password"
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
          CONFIRM PASSWORD
      ================================================== */}

      <div className="mt-5">
        <label
          htmlFor="register-confirm-password"
          className="mb-1.5 block text-sm font-medium text-[#1B2A4A]"
        >
          Confirm Password
          <span className="ml-1 text-[#F5821F]">*</span>
        </label>

        <div className="relative">
          <LockKeyhole className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-[#7c8790]" />

          <input
            id="register-confirm-password"
            name="confirmPassword"
            type={
              showConfirmPassword ? "text" : "password"
            }
            required
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(event) =>
              setConfirmPassword(event.target.value)
            }
            placeholder="Confirm your password"
            className="h-11 w-full rounded-lg border border-[#dfe4e3] bg-white pl-10 pr-11 text-sm text-[#1B2A4A] outline-none transition-colors placeholder:text-[#999] focus:border-[#3E8F96]"
          />

          <button
            type="button"
            onClick={() =>
              setShowConfirmPassword(
                (current) => !current
              )
            }
            aria-label={
              showConfirmPassword
                ? "Hide confirm password"
                : "Show confirm password"
            }
            className="absolute right-3 top-1/2 flex size-7 -translate-y-1/2 items-center justify-center rounded-md text-[#718096] hover:bg-[#F3F6F5] hover:text-[#1B2A4A]"
          >
            {showConfirmPassword ? (
              <EyeOff className="size-4" />
            ) : (
              <Eye className="size-4" />
            )}
          </button>
        </div>
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
          "Creating account..."
        ) : successMessage ? (
          "Registration Successful"
        ) : (
          <>
            Create Account
            <UserPlus className="size-4" />
          </>
        )}
      </Button>

      {/* =================================================
          LOGIN
      ================================================== */}

      <div className="mt-6 border-t border-[#edf0ef] pt-5 text-center">
        <p className="text-sm text-[#777]">
          Already have an account?
        </p>

        <a
          href="/login"
          className="mt-1 inline-block text-sm font-semibold text-[#3E8F96] hover:underline"
        >
          Sign in
        </a>
      </div>
    </form>
  );
}