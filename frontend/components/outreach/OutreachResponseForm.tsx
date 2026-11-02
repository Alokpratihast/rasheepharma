"use client";

import { useEffect, useState } from "react";
import { CheckCircle2, Mail, MessageSquareText } from "lucide-react";
import { apiClient } from "@/lib/api/client";

type LinkStatus = { alreadyResponded: boolean; message: string };

export default function OutreachResponseForm({ token }: { token: string }) {
  const [status, setStatus] = useState<LinkStatus | null>(() => token ? null : {
    alreadyResponded: true,
    message: "This response link is missing or invalid.",
  });
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(Boolean(token));
  const [sending, setSending] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    let active = true;
    if (!token) return;
    apiClient<LinkStatus>(`/lead-outreach/responses/validate?token=${encodeURIComponent(token)}`)
      .then((result) => { if (active) setStatus(result); })
      .catch((reason: unknown) => { if (active) setError(reason instanceof Error ? reason.message : "This response link could not be verified."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [token]);

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault(); setError(""); setSending(true);
    try {
      const result = await apiClient<{ message: string }>("/lead-outreach/responses", { method: "POST", body: JSON.stringify({ token, message }) });
      setSuccess(result.message); setStatus({ alreadyResponded: true, message: result.message }); setMessage("");
    } catch (reason) { setError(reason instanceof Error ? reason.message : "Your response could not be sent."); }
    finally { setSending(false); }
  }

  return <main className="min-h-[70vh] bg-slate-50 px-4 py-14 sm:py-20"><section className="mx-auto max-w-2xl overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-xl shadow-slate-200/60"><div className="bg-gradient-to-br from-emerald-900 to-emerald-700 p-8 text-white sm:p-10"><div className="mb-5 flex size-12 items-center justify-center rounded-2xl bg-white/10"><Mail className="size-6"/></div><p className="text-xs font-bold uppercase tracking-[0.2em] text-emerald-200">Rashe Lifesciences</p><h1 className="mt-3 text-3xl font-bold">Reply to our team</h1><p className="mt-3 max-w-lg text-sm leading-6 text-emerald-50">Send your question or product requirements securely. Our team will follow up with you.</p></div><div className="p-6 sm:p-10">{loading ? <div className="h-28 animate-pulse rounded-2xl bg-slate-100" aria-label="Verifying response link"/> : success ? <div className="rounded-2xl border border-emerald-200 bg-emerald-50 p-5 text-emerald-900"><CheckCircle2 className="mb-3 size-7"/><p className="font-semibold">Response received</p><p className="mt-1 text-sm">{success}</p></div> : status?.alreadyResponded ? <div className="rounded-2xl border border-slate-200 bg-slate-50 p-5 text-slate-700"><p className="font-semibold">This link can no longer accept a reply</p><p className="mt-1 text-sm">{status.message}</p></div> : error && !status ? <div className="rounded-2xl border border-rose-200 bg-rose-50 p-5 text-sm text-rose-800">{error}</div> : <form onSubmit={submit} className="space-y-4"><label htmlFor="reply-message" className="block text-sm font-semibold text-slate-800">Your message</label><div className="relative"><MessageSquareText className="absolute left-3 top-3 size-4 text-slate-400"/><textarea id="reply-message" required minLength={2} maxLength={5000} rows={7} value={message} onChange={(event) => setMessage(event.target.value)} placeholder="How can our team help?" className="w-full rounded-xl border border-slate-300 p-3 pl-10 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"/><p className="mt-1 text-right text-xs text-slate-500">{message.length}/5000</p></div>{error && <p role="alert" className="rounded-lg bg-rose-50 p-3 text-sm text-rose-800">{error}</p>}<button disabled={sending || message.trim().length < 2} className="inline-flex min-h-12 w-full items-center justify-center rounded-xl bg-emerald-700 px-5 font-semibold text-white transition hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-50">{sending ? "Sending reply…" : "Send reply"}</button><p className="text-center text-xs leading-5 text-slate-500">This private link expires after 30 days and accepts one reply.</p></form>}</div></section></main>;
}
