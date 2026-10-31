"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { AlertTriangle, CheckCircle2, FileSpreadsheet, Mail, Search, Send, ShieldCheck, Upload, Users } from "lucide-react";
import { toast } from "sonner";
import { Pagination } from "@/components/admin/Pagination";
import { leadOutreachService } from "@/services/lead-outreach.service";
import type { ExternalLead, LeadCampaign, LeadImportPreview, LeadPage } from "@/types/lead-outreach";

const inputClass = "h-10 w-full rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-900 outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100";

export function LeadOutreachManager() {
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<LeadImportPreview | null>(null);
  const [uploading, setUploading] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [confirmEmailEligibility, setConfirmEmailEligibility] = useState(false);
  const [leads, setLeads] = useState<LeadPage | null>(null);
  const [campaigns, setCampaigns] = useState<LeadCampaign[]>([]);
  const [search, setSearch] = useState("");
  const [permission, setPermission] = useState("All");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<number[]>([]);
  const [campaignName, setCampaignName] = useState("");
  const [subject, setSubject] = useState("A message from Rashe Lifesciences");
  const [bodyText, setBodyText] = useState("We would be glad to help with your pharmaceutical product requirements.\n\nReply to this email to speak with our team.");
  const [sending, setSending] = useState(false);

  const loadLeads = useCallback(async () => {
    setLoading(true);
    try {
      const result = await leadOutreachService.getLeads({ search, permission, page, pageSize });
      setLeads(result);
      setSelected((current) => current.filter((id) => result.leads.some((lead) => lead.id === id && lead.canReceiveMarketingEmail)));
    } catch (error) { toast.error(error instanceof Error ? error.message : "Could not load leads."); }
    finally { setLoading(false); }
  }, [search, permission, page, pageSize]);


  const loadCampaigns = useCallback(async () => {
    try { setCampaigns(await leadOutreachService.getCampaigns()); }
    catch (error) { toast.error(error instanceof Error ? error.message : "Could not load outreach history."); }
  }, []);
  // Initial load and filter changes: update state only after the async request
  // resolves, avoiding synchronous setState calls from inside an effect.
  useEffect(() => {
    let active = true;

    const fetchLeads = async () => {
      try {
        const result = await leadOutreachService.getLeads({ search, permission, page, pageSize });
        if (!active) return;
        setLeads(result);
        setSelected((current) =>
          current.filter((id) =>
            result.leads.some((lead) => lead.id === id && lead.canReceiveMarketingEmail),
          ),
        );
      } catch (error) {
        if (active) {
          toast.error(error instanceof Error ? error.message : "Could not load leads.");
        }
      } finally {
        if (active) setLoading(false);
      }
    };

    void fetchLeads();
    return () => {
      active = false;
    };
  }, [search, permission, page, pageSize]);

  // Load campaign history once on mount. The callback remains available
  // for the Refresh button and after a campaign is queued.
  useEffect(() => {
    let active = true;

    const fetchCampaigns = async () => {
      try {
        const result = await leadOutreachService.getCampaigns();
        if (active) setCampaigns(result);
      } catch (error) {
        if (active) {
          toast.error(error instanceof Error ? error.message : "Could not load outreach history.");
        }
      }
    };

    void fetchCampaigns();
    return () => {
      active = false;
    };
  }, []);

  const eligibleIds = useMemo(() => (leads?.leads ?? []).filter((lead) => lead.canReceiveMarketingEmail).map((lead) => lead.id), [leads]);
  const selectedEligible = selected.filter((id) => eligibleIds.includes(id));
  const allPageSelected = eligibleIds.length > 0 && eligibleIds.every((id) => selected.includes(id));

  const upload = async () => {
    if (!file) { toast.error("Choose an .xlsx workbook first."); return; }
    setUploading(true);
    try {
      const batch = await leadOutreachService.importWorkbook(file);
      setPreview(await leadOutreachService.getImportPreview(batch.id));
      setConfirmEmailEligibility(false);
      toast.success("Import preview is ready. Nothing has been imported or emailed yet.");
    } catch (error) { toast.error(error instanceof Error ? error.message : "Could not prepare the import preview."); }
    finally { setUploading(false); }
  };
  const confirmImport = async () => {
    if (!preview) return;
    if (!confirmEmailEligibility) { toast.error("Confirm that these leads are eligible for email outreach."); return; }
    setConfirming(true);
    try {
      const result = await leadOutreachService.confirmImport(preview.batch.id, confirmEmailEligibility);
      toast.success(`${result.importedRows.toLocaleString()} rows processed. No emails were sent.`);
      setPreview(null); setFile(null); setConfirmEmailEligibility(false); await loadLeads();
    } catch (error) { toast.error(error instanceof Error ? error.message : "Could not import these leads."); }
    finally { setConfirming(false); }
  };
  const denyLead = async (lead: ExternalLead) => {
    try {
      await leadOutreachService.updatePermission(lead.id, "Blocked");
      toast.success("Lead blocked from outreach.");
      await loadLeads();
    } catch (error) { toast.error(error instanceof Error ? error.message : "Could not update permission."); }
  };
  const confirmUnknownLeads = async () => {
    const count = leads?.counts.unknownEmail ?? 0;
    if (!count || !window.confirm(`Confirm ${count.toLocaleString()} Unknown email leads for Rashe Pharma outreach? Blocked and unsubscribed leads will stay excluded.`)) return;
    try {
      const result = await leadOutreachService.confirmUnknownLeads(true);
      toast.success(`${result.updatedCount.toLocaleString()} Unknown email leads are now eligible.`);
      await loadLeads();
    } catch (error) { toast.error(error instanceof Error ? error.message : "Could not confirm Unknown leads."); }
  };
  const queueCampaign = async () => {
    if (!selectedEligible.length) { toast.error("Select at least one eligible lead."); return; }
    if (!campaignName.trim() || !subject.trim() || !bodyText.trim()) { toast.error("Complete the campaign name, subject, and message."); return; }
    if (!window.confirm(`Queue this email for ${selectedEligible.length} permission-verified leads?`)) return;
    setSending(true);
    try {
      const result = await leadOutreachService.queueCampaign({ name: campaignName.trim(), subject: subject.trim(), bodyText: bodyText.trim(), leadIds: selectedEligible, confirmSend: true });
      toast.success(`${result.recipientCount} emails queued. Follow the progress below.`);
      setSelected([]); setCampaignName(""); await loadCampaigns();
    } catch (error) { toast.error(error instanceof Error ? error.message : "Could not queue this campaign."); }
    finally { setSending(false); }
  };

  return <main className="mx-auto max-w-7xl space-y-6 p-4 sm:p-7">
    <header className="flex flex-wrap items-end justify-between gap-4"><div><p className="text-xs font-bold uppercase tracking-[0.18em] text-emerald-700">Sales workspace</p><h1 className="mt-1 text-3xl font-bold text-slate-900">Leads &amp; Outreach</h1><p className="mt-2 max-w-2xl text-sm text-slate-600">Import external leads, review email permission, and send tracked campaign emails.</p></div><div className="flex items-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-900"><ShieldCheck className="size-5"/>Only permission-verified leads can be emailed</div></header>

    <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{[
      { label: "All leads", value: leads?.counts.total ?? 0, Icon: Users },
      { label: "Permission allowed", value: leads?.counts.allowed ?? 0, Icon: CheckCircle2 },
      { label: "Permission unknown", value: leads?.counts.unknown ?? 0, Icon: AlertTriangle },
      { label: "Unsubscribed", value: leads?.counts.suppressed ?? 0, Icon: Mail },
    ].map(({ label, value, Icon }) => <article key={label} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm"><div className="flex items-center justify-between"><p className="text-sm text-slate-600">{label}</p><Icon className="size-5 text-emerald-700"/></div><p className="mt-2 text-2xl font-bold text-slate-900">{value.toLocaleString()}</p></article>)}</section>

    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex items-start gap-3"><div className="rounded-xl bg-emerald-50 p-2.5 text-emerald-800"><FileSpreadsheet className="size-5"/></div><div><h2 className="text-lg font-semibold">Import lead workbook</h2><p className="mt-1 text-sm text-slate-600">Reads only <strong>Export</strong> and <strong>Product wise</strong>; Domestic is ignored. Matching email rows merge into one lead.</p></div></div>
      <div className="mt-4 flex flex-col gap-3 sm:flex-row"><input type="file" accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" onChange={(event) => setFile(event.target.files?.[0] ?? null)} className="block min-h-11 w-full rounded-lg border border-slate-300 p-2 text-sm file:mr-3 file:rounded-md file:border-0 file:bg-slate-100 file:px-3 file:py-1.5"/><button type="button" disabled={uploading || !file} onClick={() => void upload()} className="inline-flex min-h-11 items-center justify-center gap-2 rounded-lg bg-[#1B2A4A] px-5 text-sm font-semibold text-white disabled:opacity-50"><Upload className="size-4"/>{uploading ? "Preparing preview…" : "Review import"}</button></div>
      <p className="mt-3 text-xs text-slate-500">Maximum 10 MB and 20,000 rows. Import never sends email. Confirmed, non-blocked leads become eligible automatically; unsubscribed leads remain suppressed.</p>
      {preview && <div className="mt-5 rounded-xl border border-emerald-200 bg-emerald-50/60 p-4"><div className="flex flex-wrap items-start justify-between gap-3"><div><h3 className="font-semibold text-slate-900">Preview: {preview.batch.fileName}</h3><p className="mt-1 text-sm text-slate-600">{preview.batch.totalRows.toLocaleString()} rows · {preview.batch.validRows.toLocaleString()} valid · {preview.batch.invalidRows.toLocaleString()} invalid · {preview.batch.duplicateRows.toLocaleString()} repeated-email rows</p><p className="mt-2 text-sm font-medium text-amber-800">{preview.permissionNotice}</p><label className="mt-3 flex max-w-3xl items-start gap-2 text-sm text-slate-700"><input type="checkbox" checked={confirmEmailEligibility} onChange={(event) => setConfirmEmailEligibility(event.target.checked)} className="mt-1"/><span>I confirm these contacts are eligible to receive email outreach from Rashe Pharma. I understand that “Mail Sent On” is only a historical sent date, not proof of permission.</span></label><p className="mt-1 pl-6 text-xs text-slate-500">This confirmation applies to new and Unknown leads in this workbook. Previously blocked or unsubscribed leads remain ineligible.</p></div><div className="flex gap-2"><button type="button" onClick={() => { setPreview(null); setConfirmEmailEligibility(false); }} className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm">Cancel</button><button type="button" disabled={confirming || preview.batch.validRows === 0 || !confirmEmailEligibility} onClick={() => void confirmImport()} className="rounded-lg bg-emerald-700 px-4 py-2 text-sm font-semibold text-white disabled:opacity-50">{confirming ? "Importing…" : "Confirm import"}</button></div></div>
        <div className="mt-4 overflow-x-auto rounded-lg border border-slate-200 bg-white"><table className="min-w-full text-left text-xs"><thead className="bg-slate-50 text-slate-600"><tr>{["Sheet / row", "Name", "Email", "Phone", "Product interest", "Validation"].map((label) => <th key={label} className="px-3 py-2 font-semibold">{label}</th>)}</tr></thead><tbody className="divide-y divide-slate-100">{preview.rows.slice(0, 10).map((row) => <tr key={`${row.sheetName}-${row.sheetRow}`}><td className="whitespace-nowrap px-3 py-2">{row.sheetName} · {row.sheetRow}</td><td className="px-3 py-2">{row.name || "—"}</td><td className="px-3 py-2">{row.email || "—"}</td><td className="px-3 py-2">{row.phone || "—"}</td><td className="max-w-56 truncate px-3 py-2">{row.productInterest || "—"}</td><td className="px-3 py-2 text-amber-800">{row.validationError || (row.isDuplicate ? "Repeated email; merge" : "Ready")}</td></tr>)}</tbody></table></div><p className="mt-2 text-xs text-slate-500">Showing up to 10 rows. Historical sent dates are retained as notes and never trigger a resend.</p>
      </div>}
    </section>

    <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 p-5"><div><h2 className="text-lg font-semibold text-slate-900">Lead records</h2><p className="text-sm text-slate-500">Import confirmation enables outreach in bulk. You can deny individual leads; unsubscribe state is always enforced.</p></div><div className="flex flex-wrap gap-2"><label className="relative"><span className="sr-only">Search leads</span><Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400"/><input value={search} onChange={(event) => { setLoading(true); setSearch(event.target.value); setPage(1); }} placeholder="Search leads" className={`${inputClass} pl-9 sm:w-64`}/></label><select value={permission} onChange={(event) => { setLoading(true); setPermission(event.target.value); setPage(1); setSelected([]); }} className={`${inputClass} sm:w-44`} aria-label="Filter by permission"><option value="All">All permissions</option><option value="Allowed">Allowed</option><option value="Unknown">Unknown</option><option value="Blocked">Blocked</option></select>{(leads?.counts.unknownEmail ?? 0) > 0 && <button type="button" onClick={() => void confirmUnknownLeads()} className="min-h-10 rounded-lg border border-emerald-700 px-3 text-sm font-semibold text-emerald-800 hover:bg-emerald-50">Confirm {leads?.counts.unknownEmail.toLocaleString()} Unknown emails</button>}</div></div>
      {loading ? <div className="space-y-3 p-5" aria-label="Loading leads">{[1, 2, 3, 4].map((row) => <div key={row} className="h-14 animate-pulse rounded-lg bg-slate-100"/>)}</div> : !leads?.leads.length ? <div className="p-12 text-center"><Users className="mx-auto size-8 text-slate-300"/><p className="mt-3 font-medium text-slate-700">No leads found</p><p className="text-sm text-slate-500">Import a workbook or change the search/filter.</p></div> : <>
        <div className="overflow-x-auto"><table className="min-w-[1000px] w-full text-left text-sm"><thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr><th className="w-12 px-4 py-3"><input type="checkbox" aria-label="Select eligible leads on this page" checked={allPageSelected} onChange={(event) => setSelected((current) => event.target.checked ? [...new Set([...current, ...eligibleIds])] : current.filter((id) => !eligibleIds.includes(id)))} /></th><th className="px-4 py-3">Contact</th><th className="px-4 py-3">Product interest</th><th className="px-4 py-3">Source</th><th className="px-4 py-3">Permission</th><th className="px-4 py-3">Action</th></tr></thead><tbody className="divide-y divide-slate-100">{leads.leads.map((lead) => <tr key={lead.id} className="hover:bg-slate-50/70"><td className="px-4 py-4"><input type="checkbox" aria-label={`Select ${lead.name}`} disabled={!lead.canReceiveMarketingEmail} checked={selected.includes(lead.id)} onChange={(event) => setSelected((current) => event.target.checked ? [...current, lead.id] : current.filter((id) => id !== lead.id))}/></td><td className="px-4 py-4"><p className="font-semibold text-slate-900">{lead.name}</p><p className="text-slate-600">{lead.email || "No email"}</p><p className="text-xs text-slate-500">{lead.company || lead.phone || lead.country || ""}</p></td><td className="max-w-72 px-4 py-4 text-slate-600">{lead.productInterest || "—"}</td><td className="px-4 py-4 text-slate-600">{lead.source}</td><td className="px-4 py-4"><span className={`rounded-full px-2.5 py-1 text-xs font-medium ${lead.isSuppressed ? "bg-rose-50 text-rose-700" : lead.permissionStatus === "Allowed" ? "bg-emerald-50 text-emerald-700" : lead.permissionStatus === "Blocked" ? "bg-slate-200 text-slate-700" : "bg-amber-50 text-amber-800"}`}>{lead.isSuppressed ? "Unsubscribed" : lead.permissionStatus}</span>{lead.permissionEvidence && <p className="mt-1 max-w-64 truncate text-xs text-slate-500" title={lead.permissionEvidence}>{lead.permissionEvidence}</p>}</td><td className="px-4 py-4">{lead.isSuppressed ? <span className="text-xs text-slate-500">Unsubscribed</span> : lead.permissionStatus === "Allowed" ? <button type="button" onClick={() => void denyLead(lead)} className="text-xs font-semibold text-rose-700 hover:underline">Deny outreach</button> : <span className="text-xs text-slate-500">{lead.permissionStatus === "Blocked" ? "Denied" : "Not eligible"}</span>}</td></tr>)}</tbody></table></div>
        <div className="p-4"><Pagination page={page} pageSize={pageSize} totalItems={leads.total} itemLabel="leads" disabled={loading} onPageChange={(nextPage) => { setLoading(true); setPage(nextPage); }} onPageSizeChange={(value) => { setLoading(true); setPageSize(value); setPage(1); }}/></div>
      </>}
    </section>

    <section className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
      <div className="space-y-4 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><div className="flex items-center gap-3"><div className="rounded-xl bg-blue-50 p-2.5 text-blue-800"><Send className="size-5"/></div><div><h2 className="text-lg font-semibold">Compose outreach</h2><p className="text-sm text-slate-500">Selected eligible leads: {selectedEligible.length}</p></div></div><label className="block text-sm font-medium">Campaign name<input maxLength={160} value={campaignName} onChange={(event) => setCampaignName(event.target.value)} placeholder="e.g. Product follow-up" className={`${inputClass} mt-1`}/></label><label className="block text-sm font-medium">Email subject<input maxLength={200} value={subject} onChange={(event) => setSubject(event.target.value)} className={`${inputClass} mt-1`}/></label><label className="block text-sm font-medium">Message<textarea maxLength={12000} rows={6} value={bodyText} onChange={(event) => setBodyText(event.target.value)} className="mt-1 w-full rounded-lg border border-slate-300 p-3 text-sm outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100"/><span className="text-xs font-normal text-slate-500">Plain text only; message values are HTML-encoded in the branded email.</span></label><div className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900">Each recipient gets an individual email with an unsubscribe link. Uncertain SMTP failures are not retried automatically to avoid duplicate messages.</div><button type="button" disabled={sending || selectedEligible.length === 0} onClick={() => void queueCampaign()} className="inline-flex min-h-11 w-full items-center justify-center gap-2 rounded-lg bg-emerald-700 px-4 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"><Send className="size-4"/>{sending ? "Queueing…" : "Review and queue email"}</button></div>
      <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><div className="flex items-center justify-between"><div><h2 className="text-lg font-semibold">Recent campaigns</h2><p className="text-sm text-slate-500">Background worker delivery status</p></div><button type="button" onClick={() => void loadCampaigns()} className="text-sm font-semibold text-emerald-700">Refresh</button></div><div className="mt-4 space-y-3">{campaigns.length === 0 ? <p className="rounded-lg bg-slate-50 p-4 text-sm text-slate-500">No outreach campaigns yet.</p> : campaigns.map((campaign) => <CampaignCard key={campaign.id} campaign={campaign}/>)}</div></div>
    </section>
  </main>;
}

function CampaignCard({ campaign }: { campaign: LeadCampaign }) {
  return <article className="rounded-xl border border-slate-200 p-4"><div className="flex items-start justify-between gap-3"><div className="min-w-0"><p className="truncate font-semibold text-slate-900">{campaign.name}</p><p className="mt-1 truncate text-sm text-slate-600">{campaign.subject}</p></div><span className="shrink-0 rounded-full bg-blue-50 px-2.5 py-1 text-xs font-medium text-blue-800">{campaign.status}</span></div><p className="mt-3 text-xs text-slate-500">{campaign.sentCount}/{campaign.recipientCount} sent · {campaign.failedCount} failed · {new Date(campaign.createdAtUtc).toLocaleString()}</p></article>;
}
