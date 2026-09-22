"use client";

import { Suspense } from "react";
import { useSearchParams } from "next/navigation";

function PaymentSuccessContent() {
  const searchParams = useSearchParams();

  const sessionId = searchParams.get("session_id");

  return (
    <main className="min-h-screen flex items-center justify-center p-6">
      <div className="text-center">
        <h1 className="text-3xl font-bold">
          Payment Successful
        </h1>

        <p className="mt-3 text-muted-foreground">
          Your payment has been successfully processed.
        </p>

        {sessionId && (
          <p className="mt-2 text-sm text-muted-foreground">
            Payment Session: {sessionId}
          </p>
        )}
      </div>
    </main>
  );
}

export default function PaymentSuccessPage() {
  return (
    <Suspense
      fallback={
        <main className="min-h-screen flex items-center justify-center">
          <p>Loading payment details...</p>
        </main>
      }
    >
      <PaymentSuccessContent />
    </Suspense>
  );
}