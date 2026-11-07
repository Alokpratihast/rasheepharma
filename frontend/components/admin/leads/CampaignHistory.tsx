"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { ArrowLeft, ChevronRight, Mail, MessageSquareReply, Search, Send } from "lucide-react";
import { toast } from "sonner";
import { Pagination } from "@/components/admin/Pagination";
import { leadOutreachService } from "@/services/lead-outreach.service";
import type { LeadCampaignHistoryPage } from "@/types/lead-outreach";

const statuses = ["All", "Queued", "Completed", "CompletedWithErrors"];

export function CampaignHistory() {
  const [data, setData] = useState<LeadCampaignHistoryPage | null>(null);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("All");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;
    const timer = window.setTimeout(async () => {
      setLoading(true);
      setError("");
      try {
        const result = await leadOutreachService.getCampaignHistory({ search, status, page, pageSize });
        if (active) setData(result);
      } catch (reason) {
        if (!active) return;
        const message = reason instanceof Error ? reason.message : "Could not load campaign history.";
        setError(message);
        toast.error(message);
      } finally {
        if (active) setLoading(false);
      }
    }, search ? 250 : 0);
    return () => { active = false; window.clearTimeout(timer); };
  }, [search, status, page, pageSize]);

  const campaigns = data?.campaigns ?? [];
  const sent = campaigns.reduce((sum, campaign) => sum + campaign.sentCount, 0);
  const failed = campaigns.reduce((sum, campaign) => sum + campaign.failedCount, 0);
  const replies = campaigns.reduce((sum, campaign) => sum + campaign.respondedCount, 0);

  return <main className="mx-auto max-w-7xl space-y-6 p-4 sm:p-7">
    <Link href="/admin/outreach" className="inline-flex items-center gap-2 text-sm font-semibold text-emerald-800 hover:underline"><ArrowLeft className="size-4"/>Back to Leads &amp; Outreach</Link>
    <header className="flex flex-wrap items-end justify-between gap-4">
      <div><p className="text-xs font-bold uppercase tracking-[0.18em] text-emerald-700">Sales workspace</p><h1 className="mt-1 text-3xl font-bold text-slate-900">Campaign history</h1><p className="mt-2 max-w-2xl text-sm text-slate-600">Review each outreach send and open a campaign to see its recipient-level processing record.</p></div>
      <Link href="/admin/outreach" className="inline-flex min-h-10 items-center gap-2 rounded-lg bg-[#1B2A4A] px-4 text-sm font-semibold text-white"><Send className="size-4"/>Create campaign</Link>
    </header>

    <section className="grid gap-4 sm:grid-cols-3">
      {[{ label: "Campaigns in this page", value: campaigns.length, Icon: Mail }, { label: "Accepted by mail provider", value: sent, Icon: Send }, { label: "Replies in this page", value: replies, Icon: MessageSquareReply }].map(({ label, value, Icon }) =>
        <article key={label} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm"><div className="flex items-center justify-between"><p className="text-sm text-slate-600">{label}</p><Icon className="size-5 text-emerald-700"/></div><p className="mt-2 text-2xl font-bold text-slate-900">{value.toLocaleString()}</p>{label === "Accepted by mail provider" && failed > 0 && <p className="mt-1 text-xs text-rose-700">{failed} failed on this page</p>}</article>
      )}
    </section>

    <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
      <div className="flex flex-col gap-3 border-b border-slate-200 p-4 sm:flex-row">
        <label className="relative flex-1"><Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400"/><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} placeholder="Search campaign name or subject" className="h-11 w-full rounded-lg border border-slate-300 pl-9 pr-3 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"/></label>
        <select aria-label="Filter campaigns by status" value={status} onChange={(event) => { setStatus(event.target.value); setPage(1); }} className="h-11 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-800">{statuses.map((item) => <option key={item} value={item}>{item === "CompletedWithErrors" ? "Completed with errors" : item}</option>)}</select>
      </div>
      {loading ? <div className="space-y-3 p-4" aria-label="Loading campaigns">{[0, 1, 2, 3].map((item) => <div key={item} className="h-24 animate-pulse rounded-xl bg-slate-100"/> )}</div> : error ? <div role="alert" className="m-4 rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">{error}</div> : campaigns.length === 0 ? <div className="p-10 text-center"><Mail className="mx-auto size-8 text-slate-400"/><h2 className="mt-3 font-semibold text-slate-900">No campaigns found</h2><p className="mt-1 text-sm text-slate-600">Try changing your search or create an outreach campaign.</p></div> : <div className="overflow-x-auto"><table className="min-w-[850px] w-full text-left text-sm"><thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr>{["Campaign", "Status", "Recipients", "Sent", "Failed", "Replies", "Created", ""].map((item) => <th key={item} className="px-4 py-3 font-semibold">{item}</th>)}</tr></thead><tbody className="divide-y divide-slate-100">{campaigns.map((campaign) => <tr key={campaign.id} className="hover:bg-slate-50"><td className="max-w-[300px] px-4 py-4"><Link href={`/admin/outreach/campaigns/${campaign.id}`} className="font-semibold text-slate-900 hover:text-emerald-800 hover:underline">{campaign.name}</Link><p className="mt-1 truncate text-xs text-slate-500">{campaign.subject}{campaign.attachBrochure ? " · brochure attached" : ""}</p></td><td className="px-4 py-4"><StatusBadge status={campaign.status}/></td><td className="px-4 py-4 tabular-nums">{campaign.recipientCount}</td><td className="px-4 py-4 font-medium text-emerald-800 tabular-nums">{campaign.sentCount}</td><td className="px-4 py-4 font-medium text-rose-700 tabular-nums">{campaign.failedCount}</td><td className="px-4 py-4 tabular-nums">{campaign.respondedCount}</td><td className="px-4 py-4 text-slate-600">{formatDate(campaign.createdAtUtc)}</td><td className="px-4 py-4"><Link aria-label={`View ${campaign.name}`} href={`/admin/outreach/campaigns/${campaign.id}`} className="inline-flex items-center gap-1 font-semibold text-emerald-800">Details<ChevronRight className="size-4"/></Link></td></tr>)}</tbody></table></div>}
      <div className="border-t border-slate-200 p-4"><Pagination page={page} pageSize={pageSize} totalItems={data?.total ?? 0} itemLabel="campaigns" disabled={loading} onPageChange={setPage} onPageSizeChange={(value) => { setPageSize(value); setPage(1); }}/></div>
    </section>
    <p className="text-xs text-slate-500">“Sent” means the worker handed the email to the configured mail provider; delivery, bounce, open and click events are not tracked in this campaign screen.</p>
  </main>;
}

export function StatusBadge({ status }: { status: string }) {
  const styles = status === "Completed" || status === "Sent" ? "bg-emerald-50 text-emerald-800" : status === "CompletedWithErrors" || status === "Failed" ? "bg-rose-50 text-rose-800" : status === "Skipped" ? "bg-slate-100 text-slate-700" : "bg-amber-50 text-amber-800";
  const label = status === "CompletedWithErrors" ? "Completed with errors" : status;
  return <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${styles}`}>{label}</span>;
}

export function formatDate(value: string | null | undefined) {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleString();
}
