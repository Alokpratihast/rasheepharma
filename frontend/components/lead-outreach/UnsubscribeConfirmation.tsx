"use client";

import { useState } from "react";
import { CheckCircle2, MailX } from "lucide-react";
import { apiClient } from "@/lib/api/client";

export function UnsubscribeConfirmation({ token }: { token: string }) {
  const [busy, setBusy] = useState(false);
  const [complete, setComplete] = useState(false);
  const [error, setError] = useState("");

  const unsubscribe = async () => {
    if (!token || busy) return;
    setBusy(true);
    setError("");
    try {
      await apiClient<{ message: string }>("/lead-outreach/unsubscribe", {
        method: "POST",
        body: JSON.stringify({ token }),
      });
      setComplete(true);
    } catch {
      setError("We could not process this request. Please contact our team for help.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <main className="mx-auto flex min-h-[60vh] max-w-2xl items-center px-5 py-16">
      <section className="w-full rounded-2xl border border-slate-200 bg-white p-8 shadow-sm sm:p-10">
        {complete ? (
          <>
            <CheckCircle2 aria-hidden="true" className="mb-5 size-11 text-emerald-700" />
            <h1 className="text-2xl font-bold text-slate-900">Email preference updated</h1>
            <p className="mt-3 text-slate-600">This address has been opted out of future marketing emails.</p>
          </>
        ) : (
          <>
            <MailX aria-hidden="true" className="mb-5 size-11 text-emerald-700" />
            <h1 className="text-2xl font-bold text-slate-900">Unsubscribe from marketing emails</h1>
            <p className="mt-3 text-slate-600">Confirm below to stop future marketing emails from Rashe Lifesciences.</p>
            {!token && <p className="mt-4 rounded-lg bg-amber-50 p-3 text-sm text-amber-900">This unsubscribe link is incomplete. Contact our team to update your preference.</p>}
            {error && <p role="alert" className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-800">{error}</p>}
            <button type="button" disabled={!token || busy} onClick={unsubscribe} className="mt-6 rounded-lg bg-[#1B2A4A] px-5 py-3 font-semibold text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60">
              {busy ? "Updating…" : "Confirm unsubscribe"}
            </button>
          </>
        )}
      </section>
    </main>
  );
}
