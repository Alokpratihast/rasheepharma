"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { ArrowLeft, Mail, MessageSquareReply, Paperclip, Search, Send, Users } from "lucide-react";
import { toast } from "sonner";
import { Pagination } from "@/components/admin/Pagination";
import { leadOutreachService } from "@/services/lead-outreach.service";
import type { LeadCampaignDetailPage } from "@/types/lead-outreach";
import { formatDate, StatusBadge } from "@/components/admin/leads/CampaignHistory";

const recipientStatuses = ["All", "Pending", "Processing", "Queued", "Sent", "Failed", "Skipped"];

export function CampaignDetail({ campaignId }: { campaignId: number }) {
  const [data, setData] = useState<LeadCampaignDetailPage | null>(null);
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
        const result = await leadOutreachService.getCampaignDetail(campaignId, { search, status, page, pageSize });
        if (active) setData(result);
      } catch (reason) {
        if (!active) return;
        const message = reason instanceof Error ? reason.message : "Could not load campaign details.";
        setError(message);
        toast.error(message);
      } finally {
        if (active) setLoading(false);
      }
    }, search ? 250 : 0);
    return () => { active = false; window.clearTimeout(timer); };
  }, [campaignId, search, status, page, pageSize]);

  const campaign = data?.campaign;
  const counts = new Map((data?.statusCounts ?? []).map((item) => [item.status, item.count]));
  const accepted = counts.get("Sent") ?? 0;
  const failed = counts.get("Failed") ?? 0;
  const inProgress = (counts.get("Pending") ?? 0) + (counts.get("Processing") ?? 0) + (counts.get("Queued") ?? 0);
  const excluded = counts.get("Skipped") ?? 0;

  return <main className="mx-auto max-w-7xl space-y-6 p-4 sm:p-7">
    <Link href="/admin/outreach/campaigns" className="inline-flex items-center gap-2 text-sm font-semibold text-emerald-800 hover:underline"><ArrowLeft className="size-4"/>Campaign history</Link>
    <header className="flex flex-wrap items-start justify-between gap-4">
      <div><p className="text-xs font-bold uppercase tracking-[0.18em] text-emerald-700">Campaign report</p><h1 className="mt-1 text-3xl font-bold text-slate-900">{campaign?.name ?? "Campaign details"}</h1><p className="mt-2 text-sm text-slate-600">{campaign?.subject ?? "Recipient send record and processing status."}</p><div className="mt-3 flex flex-wrap gap-2">{campaign?.attachBrochure && <span className="inline-flex items-center gap-1 rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-800"><Paperclip className="size-3"/>Brochure attached</span>}{campaign && <StatusBadge status={campaign.status}/>}</div></div>
      <Link href="/admin/outreach" className="inline-flex min-h-10 items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 text-sm font-semibold text-slate-800 hover:bg-slate-50"><Send className="size-4"/>New campaign</Link>
    </header>
    {error && !campaign ? <div role="alert" className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">{error}</div> : <>
      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
        {[{ label: "Recipients", value: campaign?.recipientCount ?? 0, Icon: Users, tone: "text-slate-900" }, { label: "Accepted by provider", value: accepted, Icon: Mail, tone: "text-emerald-800" }, { label: "Still processing", value: inProgress, Icon: Send, tone: "text-amber-800" }, { label: "Failed", value: failed, Icon: Mail, tone: "text-rose-700" }, { label: "Replies", value: campaign?.respondedCount ?? 0, Icon: MessageSquareReply, tone: "text-blue-800" }].map(({ label, value, Icon, tone }) => <article key={label} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm"><div className="flex items-center justify-between"><p className="text-sm text-slate-600">{label}</p><Icon className="size-5 text-slate-400"/></div><p className={`mt-2 text-2xl font-bold ${tone}`}>{value.toLocaleString()}</p>{label === "Recipients" && excluded > 0 && <p className="mt-1 text-xs text-slate-500">{excluded} skipped/excluded</p>}</article>)}
      </section>
      {campaign && <section className="grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 text-sm text-slate-700 sm:grid-cols-3"><p><span className="block text-xs text-slate-500">Created</span>{formatDate(campaign.createdAtUtc)}</p><p><span className="block text-xs text-slate-500">Queued</span>{formatDate(campaign.queuedAtUtc)}</p><p><span className="block text-xs text-slate-500">Completed</span>{formatDate(campaign.completedAtUtc)}</p></section>}
      <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        <div className="border-b border-slate-200 p-4"><h2 className="font-semibold text-slate-900">Recipient send records</h2><p className="mt-1 text-sm text-slate-600">Search any recipient in this campaign, even when the campaign contains thousands of leads.</p></div>
        <div className="flex flex-col gap-3 border-b border-slate-200 p-4 sm:flex-row">
          <label className="relative flex-1"><Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400"/><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} placeholder="Search name, email, or product interest" className="h-11 w-full rounded-lg border border-slate-300 pl-9 pr-3 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"/></label>
          <select aria-label="Filter recipients by send status" value={status} onChange={(event) => { setStatus(event.target.value); setPage(1); }} className="h-11 rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-800">{recipientStatuses.map((item) => <option key={item} value={item}>{item}</option>)}</select>
        </div>
        {loading ? <div className="space-y-3 p-4" aria-label="Loading recipients">{[0, 1, 2, 3, 4].map((item) => <div key={item} className="h-14 animate-pulse rounded-lg bg-slate-100"/>)}</div> : error ? <div role="alert" className="m-4 rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">{error}</div> : (data?.recipients.length ?? 0) === 0 ? <div className="p-10 text-center"><Users className="mx-auto size-8 text-slate-400"/><h3 className="mt-3 font-semibold text-slate-900">No recipients match</h3><p className="mt-1 text-sm text-slate-600">Change the search or status filter.</p></div> : <div className="overflow-x-auto"><table className="min-w-[850px] w-full text-left text-sm"><thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr>{["Recipient", "Product interest", "Status", "Attempts", "Sent at", "Error"].map((item) => <th key={item} className="px-4 py-3 font-semibold">{item}</th>)}</tr></thead><tbody className="divide-y divide-slate-100">{data?.recipients.map((recipient) => <tr key={recipient.id} className="align-top hover:bg-slate-50"><td className="px-4 py-4"><p className="font-semibold text-slate-900">{recipient.name}</p><p className="mt-1 text-xs text-slate-600">{recipient.email || "No email"}</p></td><td className="max-w-60 px-4 py-4 text-slate-600">{recipient.productInterest || "—"}</td><td className="px-4 py-4"><StatusBadge status={recipient.status}/></td><td className="px-4 py-4 tabular-nums">{recipient.attemptCount}</td><td className="px-4 py-4 text-slate-600">{formatDate(recipient.sentAtUtc)}</td><td className="max-w-64 px-4 py-4 text-xs text-rose-700">{recipient.errorMessage || "—"}</td></tr>)}</tbody></table></div>}
        <div className="border-t border-slate-200 p-4"><Pagination page={page} pageSize={pageSize} totalItems={data?.total ?? 0} itemLabel="recipients" disabled={loading} onPageChange={setPage} onPageSizeChange={(value) => { setPageSize(value); setPage(1); }}/></div>
      </section>
      <p className="text-xs text-slate-500">“Sent” means accepted by your mail transport; this app does not currently display provider delivery, bounce, open, or click webhooks here.</p>
    </>}
  </main>;
}
