import type { Metadata } from "next";
import Link from "next/link";
import { ArrowRight, BadgeCheck, Handshake, MapPin } from "lucide-react";
import { notFound } from "next/navigation";
import { Container } from "@/components/ui/container";
import { getCityLandingPage, type CityLandingPageSeed } from "@/data/seed/cityLandingPageSeeder";
import type { LocationSeoPage } from "@/types/location-seo-page";

const apiBase=(process.env.NEXT_PUBLIC_API_BASE_URL??"http://localhost:5104/api").replace(/\/+$/,"");
const siteUrl=(process.env.NEXT_PUBLIC_SITE_URL??"https://www.rashelifesciences.in").replace(/\/+$/,"");
export const revalidate=60;
interface Props { params: Promise<{country:string;city:string}> }

async function getPage(country:string,city:string):Promise<LocationSeoPage|CityLandingPageSeed|null>{
  try {
    const response=await fetch(`${apiBase}/location-pages/${encodeURIComponent(country)}/${encodeURIComponent(city)}`,{next:{revalidate:60},signal:AbortSignal.timeout(5000),headers:{Accept:"application/json"}});
    if(response.status===410)return null;
    if(response.ok)return await response.json() as LocationSeoPage;
  } catch { /* Keep seeded pages available during an API outage. */ }
  return getCityLandingPage(country,city)??null;
}

export async function generateMetadata({params}:Props):Promise<Metadata>{
  const {country,city}=await params; const page=await getPage(country,city);
  if(!page)return {title:"Location not found | RashePharma",robots:{index:false,follow:false}};
  const canonical=`${siteUrl}/countries/${page.countrySlug}/${page.citySlug}`;
  const isIndexable = "isIndexable" in page ? page.isIndexable : true;
  return {title:page.metaTitle,description:page.metaDescription,alternates:{canonical},robots:{index:isIndexable,follow:true},openGraph:{type:"website",url:canonical,title:page.metaTitle,description:page.metaDescription,siteName:"RashePharma"}};
}

export default async function CityLandingPage({params}:Props){
  const {country,city}=await params;const page=await getPage(country,city);if(!page)notFound();
  const enquiryHref=`/b2b/enquiry?countryCode=${page.countryCode}&city=${encodeURIComponent(page.city)}&quoteCurrency=${page.quoteCurrency}`;
  return <main className="min-h-screen bg-slate-50"><section className="border-b border-emerald-100 bg-gradient-to-br from-emerald-50 via-white to-cyan-50 py-14 sm:py-20"><Container><nav aria-label="Breadcrumb" className="mb-8 text-sm text-slate-600"><Link href="/">Home</Link><span className="px-2">/</span><Link href="/b2b/enquiry">B2B services</Link><span className="px-2">/</span><span aria-current="page">{page.city}, {page.country}</span></nav><div className="grid items-center gap-10 lg:grid-cols-[1.2fr_0.8fr]"><div><p className="inline-flex items-center gap-2 rounded-full border border-emerald-200 bg-white px-3 py-1.5 text-xs font-bold uppercase tracking-wider text-emerald-800"><MapPin className="size-4"/>B2B enquiries · {page.region}</p><h1 className="mt-5 text-4xl font-bold tracking-tight text-slate-950 sm:text-5xl">{page.heading}</h1><p className="mt-5 max-w-2xl text-lg leading-8 text-slate-700">{page.intro}</p><div className="mt-8 flex flex-col gap-3 sm:flex-row"><Link href={enquiryHref} className="inline-flex min-h-12 items-center justify-center gap-2 rounded-xl bg-emerald-700 px-6 font-semibold text-white hover:bg-emerald-800">Discuss a B2B requirement<ArrowRight className="size-4"/></Link><Link href="/products" className="inline-flex min-h-12 items-center justify-center rounded-xl border bg-white px-6 font-semibold">Browse product catalogue</Link></div><p className="mt-4 text-xs leading-5 text-slate-500">Availability, documentation and destination requirements are confirmed per enquiry.</p></div><aside className="rounded-3xl border bg-white p-7 shadow-xl"><span className="inline-flex size-12 items-center justify-center rounded-2xl bg-emerald-100 text-emerald-800"><Handshake className="size-6"/></span><h2 className="mt-5 text-2xl font-bold">A clear path to a business enquiry</h2><p className="mt-3 leading-7 text-slate-600">{page.buyerFocus}</p><ul className="mt-6 space-y-3 text-sm text-slate-700"><li className="flex gap-3"><BadgeCheck className="size-5 shrink-0 text-emerald-700"/>Share products and market requirements</li><li className="flex gap-3"><BadgeCheck className="size-5 shrink-0 text-emerald-700"/>Discuss documentation and commercial fit</li><li className="flex gap-3"><BadgeCheck className="size-5 shrink-0 text-emerald-700"/>Get next steps after team review</li></ul></aside></div></Container></section><section className="py-14 sm:py-20"><Container><div className="rounded-3xl bg-slate-950 p-8 text-white sm:flex sm:items-center sm:justify-between"><div><p className="font-semibold text-emerald-300">RashePharma · {page.city}</p><h2 className="mt-2 text-2xl font-bold">Ready to discuss your requirement?</h2><p className="mt-3 text-white/70">Send a B2B enquiry with your product and market requirements.</p></div><Link href={enquiryHref} className="mt-5 inline-flex min-h-12 items-center gap-2 rounded-xl bg-emerald-400 px-5 font-semibold text-slate-950 sm:mt-0">Start an enquiry<ArrowRight className="size-4"/></Link></div></Container></section></main>;
}
