"use client";

import type { FormEvent } from "react";
import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { ArrowUpRight, MapPin, Plus, Search } from "lucide-react";
import { Pagination } from "@/components/admin/Pagination";
import { cityLandingPageSeeder } from "@/data/seed/cityLandingPageSeeder";
import { getAdminErrorMessage } from "@/lib/admin-error-message";
import { locationSeoService } from "@/services/location-seo.service";
import type { LocationSeoPage, LocationSeoPageWriteInput } from "@/types/location-seo-page";

const empty: LocationSeoPageWriteInput = { country:"", countryCode:"", countrySlug:"", quoteCurrency:"USD", city:"", region:"", citySlug:"", metaTitle:"", metaDescription:"", heading:"", intro:"", buyerFocus:"", isPublished:false, isIndexable:true };
const key = (p: Pick<LocationSeoPageWriteInput,"countrySlug"|"citySlug">) => `${p.countrySlug}/${p.citySlug}`;
const slugify = (v:string) => v.toLowerCase().trim().normalize("NFKD").replace(/[\u0300-\u036f]/g,"").replace(/[^a-z0-9]+/g,"-").replace(/^-|-$/g,"");
const isStarterRoute = (p: Pick<LocationSeoPageWriteInput,"countrySlug"|"citySlug">) => cityLandingPageSeeder.some(seed => key(seed) === key(p));
const seeded = (s:(typeof cityLandingPageSeeder)[number]):LocationSeoPage => ({id:0,...s,isPublished:true,isIndexable:true,createdAtUtc:"",updatedAtUtc:""});
const asInput = (p:LocationSeoPage):LocationSeoPageWriteInput => ({country:p.country,countryCode:p.countryCode,countrySlug:p.countrySlug,quoteCurrency:p.quoteCurrency,city:p.city,region:p.region,citySlug:p.citySlug,metaTitle:p.metaTitle,metaDescription:p.metaDescription,heading:p.heading,intro:p.intro,buyerFocus:p.buyerFocus,isPublished:p.isPublished,isIndexable:p.isIndexable});
const mergePages = (saved: LocationSeoPage[]) => {
  const map = new Map(saved.map((page) => [key(page), page]));
  const initial = cityLandingPageSeeder.map((seed) => map.get(key(seed)) ?? seeded(seed));
  const seeds = new Set(cityLandingPageSeeder.map(key));
  return [...initial, ...saved.filter((page) => !seeds.has(key(page)))].sort(
    (left, right) => left.country.localeCompare(right.country) || left.city.localeCompare(right.city),
  );
};

export function LocationSeoManager() {
  const [pages,setPages]=useState<LocationSeoPage[]>([]); const [form,setForm]=useState(empty); const [id,setId]=useState<number|null>(null);
  const [search,setSearch]=useState(""); const [error,setError]=useState(""); const [notice,setNotice]=useState(""); const [loading,setLoading]=useState(true); const [saving,setSaving]=useState(false);
  const [page,setPage]=useState(1); const [pageSize,setPageSize]=useState(25);
  const load=useCallback(async()=>{try{const saved=await locationSeoService.getAdminPages();setPages(mergePages(saved));setError("");}catch(e){setError(getAdminErrorMessage(e,"Could not load location pages. Check the API connection."));}finally{setLoading(false);}},[]);
  useEffect(()=>{
    let active=true;
    locationSeoService.getAdminPages()
      .then((saved)=>{if(active){setPages(mergePages(saved));setError("");}})
      .catch((error:unknown)=>{if(active)setError(getAdminErrorMessage(error,"Could not load location pages. Check the API connection."));})
      .finally(()=>{if(active)setLoading(false);});
    return ()=>{active=false;};
  },[]);
  const shown=useMemo(()=>{const q=search.trim().toLowerCase();return pages.filter(p=>!q||`${p.city} ${p.country} ${p.metaTitle}`.toLowerCase().includes(q));},[pages,search]);
  const pageCount=Math.max(1,Math.ceil(shown.length/pageSize));
  const safePage=Math.min(page,pageCount);
  const visiblePages=shown.slice((safePage-1)*pageSize,safePage*pageSize);
  const set=<K extends keyof LocationSeoPageWriteInput>(field:K,value:LocationSeoPageWriteInput[K])=>setForm(v=>{
    const next={...v,[field]:value};
    if(field==="country"&&(!v.countrySlug||v.countrySlug===slugify(v.country))) next.countrySlug=slugify(String(value));
    if(field==="city"&&(!v.citySlug||v.citySlug===slugify(v.city))) next.citySlug=slugify(String(value));
    return next;
  });
  const clear=()=>{setForm(empty);setId(null);setError("");setNotice("");};
  const edit=(p:LocationSeoPage)=>{setForm(asInput(p));setId(p.id||null);setError("");setNotice("");};
  const save=async(e:FormEvent)=>{e.preventDefault();setSaving(true);setError("");setNotice("");try{const saved=id?await locationSeoService.update(id,form):await locationSeoService.create(form);setId(saved.id);setForm(asInput(saved));setNotice(saved.isPublished?"Page saved and published.":"Draft saved. Publish it when ready.");await load();}catch(err){setError(getAdminErrorMessage(err,"Could not save the page. Check the URL and SEO fields."));}finally{setSaving(false);}};
  const remove=async(p:LocationSeoPage)=>{if(!p.id){setError("Starter pages cannot be deleted. Save the page as unpublished to hide it.");return;}if(isStarterRoute(p)){setError("This URL belongs to a starter page. Unpublish it to hide the page without restoring the starter version.");return;}if(!window.confirm(`Delete ${p.city}, ${p.country}?`))return;try{await locationSeoService.delete(p.id);clear();setNotice("Page deleted.");await load();}catch(e){setError(getAdminErrorMessage(e,"Could not delete this page."));}};
  const input="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-900 outline-none focus:border-emerald-600 focus:ring-2 focus:ring-emerald-100";
  const text=(label:string,field:keyof LocationSeoPageWriteInput,max:number,area=false)=><label className="block text-sm font-medium text-slate-700">{label}{area?<textarea required maxLength={max} rows={3} value={String(form[field])} onChange={e=>set(field,e.target.value as never)} className={input}/>:<input required maxLength={max} value={String(form[field])} onChange={e=>set(field,e.target.value as never)} className={input}/>}</label>;
  return <main className="mx-auto max-w-7xl space-y-6 p-4 sm:p-7"><header className="flex flex-wrap items-end justify-between gap-4"><div><p className="text-xs font-bold uppercase tracking-widest text-emerald-700">SEO management</p><h1 className="mt-1 text-3xl font-bold text-slate-900">Location pages</h1><p className="mt-2 max-w-2xl text-sm text-slate-600">Manage city landing pages here. Saved pages live in the database; starter seeder entries are only initial content.</p></div><button type="button" onClick={clear} className="inline-flex h-11 items-center gap-2 rounded-xl bg-[#1B2A4A] px-4 text-sm font-semibold text-white"><Plus className="size-4"/>New city page</button></header>
  {(error||notice)&&<p role={error?"alert":"status"} className={`rounded-xl border p-3 text-sm ${error?"border-red-200 bg-red-50 text-red-800":"border-emerald-200 bg-emerald-50 text-emerald-800"}`}>{error||notice}</p>}
  <div className="grid gap-6 xl:grid-cols-[1fr_1fr]"><section className="overflow-hidden rounded-2xl border border-slate-200 bg-white"><div className="flex flex-wrap items-center justify-between gap-3 border-b p-4"><div><h2 className="font-semibold">Managed locations</h2><p className="text-xs text-slate-500">{shown.length} pages</p></div><label className="relative"><span className="sr-only">Search pages</span><Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400"/><input value={search} onChange={e=>{setSearch(e.target.value);setPage(1);}} placeholder="Search city/country" className="h-10 rounded-lg border pl-9 pr-3 text-sm"/></label></div>{loading?<div className="space-y-3 p-4">{[1,2,3].map(n=><div key={n} className="h-14 animate-pulse rounded bg-slate-100"/>)}</div>:<><ul className="divide-y">{visiblePages.map(p=><li key={key(p)} className="flex items-center justify-between gap-3 p-4"><div className="min-w-0"><div className="flex items-center gap-2"><MapPin className="size-4 text-emerald-700"/><b className="truncate">{p.city}, {p.country}</b><span className={`rounded-full px-2 py-0.5 text-xs ${p.isPublished?"bg-emerald-50 text-emerald-800":"bg-amber-50 text-amber-800"}`}>{p.isPublished?"Published":"Draft"}</span></div><p className="truncate pl-6 text-xs text-slate-500">/countries/{p.countrySlug}/{p.citySlug}</p></div><div className="flex shrink-0 gap-3"><button type="button" onClick={()=>edit(p)} className="text-sm font-semibold text-blue-700">Edit</button>{p.id>0&&!isStarterRoute(p)&&<button type="button" onClick={()=>void remove(p)} className="text-sm text-red-700">Delete</button>}{p.isPublished&&<Link target="_blank" href={`/countries/${p.countrySlug}/${p.citySlug}`} aria-label="Preview" className="text-slate-500"><ArrowUpRight className="size-4"/></Link>}</div></li>)}</ul>{shown.length>0&&<div className="p-4"><Pagination page={safePage} pageSize={pageSize} totalItems={shown.length} itemLabel="location pages" disabled={saving} onPageChange={setPage} onPageSizeChange={size=>{setPageSize(size);setPage(1);}}/></div>}</>}</section>
  <form onSubmit={save} className="space-y-4 rounded-2xl border border-slate-200 bg-white p-5"><div><h2 className="text-xl font-bold">{id?"Edit location page":"City SEO form"}</h2><p className="text-sm text-slate-500">Country and city slugs create the public URL.</p></div><div className="grid gap-4 sm:grid-cols-2">{text("Country *","country",100)}{text("Country code *","countryCode",2)}{text("Country URL slug *","countrySlug",100)}<label className="text-sm font-medium text-slate-700">Quote currency<select value={form.quoteCurrency} onChange={e=>set("quoteCurrency",e.target.value as "INR"|"USD")} className={input}><option>USD</option><option>INR</option></select></label>{text("City *","city",120)}{text("Region/state *","region",120)}{text("City URL slug *","citySlug",120)}</div><div className="border-t pt-4"><h3 className="font-semibold">Search engine fields</h3><div className="mt-3 space-y-3">{text("Meta title *","metaTitle",200)}{text("Meta description *","metaDescription",320,true)}<p className="text-right text-xs text-slate-500">{form.metaDescription.length}/320</p></div></div><div className="border-t pt-4"><h3 className="font-semibold">Page content</h3><div className="mt-3 space-y-3">{text("Page heading *","heading",200)}{text("Introduction *","intro",3000,true)}{text("Buyer focus *","buyerFocus",2000,true)}</div></div><div className="flex flex-wrap gap-5 border-t pt-4"><label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.isPublished} onChange={e=>set("isPublished",e.target.checked)} className="accent-emerald-700"/>Publish page</label><label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.isIndexable} onChange={e=>set("isIndexable",e.target.checked)} className="accent-emerald-700"/>Allow indexing</label></div><div className="flex justify-between border-t pt-4"><button type="button" onClick={clear} className="rounded-lg border px-4 py-2 text-sm">Clear</button><button disabled={saving} className="rounded-lg bg-[#1B2A4A] px-5 py-2.5 text-sm font-semibold text-white disabled:opacity-50">{saving?"Saving…":id?"Save changes":"Save page"}</button></div></form></div></main>;
}
