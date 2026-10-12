"use client";

import { Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";

function PaymentSuccessContent() {
  const searchParams = useSearchParams();
  const router = useRouter();

  const sessionId = searchParams.get("session_id");

  const [countdown, setCountdown] = useState(5);

  useEffect(() => {
    const redirectTimer = setTimeout(() => {
      router.replace("/");
    }, 5000);

    const countdownTimer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(countdownTimer);
          return 0;
        }

        return prev - 1;
      });
    }, 1000);

    return () => {
      clearTimeout(redirectTimer);
      clearInterval(countdownTimer);
    };
  }, [router]);

  const handleBackToHome = () => {
    router.replace("/");
  };

  return (
    <main className="min-h-screen bg-background flex items-center justify-center px-6 py-12">
      <div className="w-full max-w-md">
        <div className="rounded-2xl border bg-card p-8 text-center shadow-sm sm:p-10">
          {/* Success Icon */}
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-green-100">
            <svg
              className="h-8 w-8 text-green-600"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M20 6 9 17l-5-5" />
            </svg>
          </div>

          {/* Heading */}
          <h1 className="mt-6 text-3xl font-bold tracking-tight">
            Payment Successful
          </h1>

          <p className="mt-3 text-muted-foreground">
            Your payment has been successfully processed and your order has
            been confirmed.
          </p>

          {/* Session Reference */}
          {sessionId && (
            <div className="mt-6 rounded-lg bg-muted/50 px-4 py-3">
              <p className="text-xs text-muted-foreground">
                Payment Reference
              </p>

              <p className="mt-1 break-all text-sm font-medium">
                {sessionId}
              </p>
            </div>
          )}

          {/* Redirect Information */}
          <p className="mt-6 text-sm text-muted-foreground">
            Redirecting you to the home page in{" "}
            <span className="font-semibold text-foreground">
              {countdown}
            </span>{" "}
            {countdown === 1 ? "second" : "seconds"}...
          </p>

          {/* Home Button */}
          <button
            type="button"
            onClick={handleBackToHome}
            className="mt-6 w-full rounded-lg bg-primary px-5 py-3 text-sm font-semibold text-primary-foreground transition-opacity hover:opacity-90"
          >
            Back to Home
          </button>
        </div>
      </div>
    </main>
  );
}

export default function PaymentSuccessPage() {
  return (
    <Suspense
      fallback={
        <main className="min-h-screen flex items-center justify-center px-6">
          <p className="text-muted-foreground">
            Loading payment details...
          </p>
        </main>
      }
    >
      <PaymentSuccessContent />
    </Suspense>
  );
}