import type { Metadata } from "next";
import Link from "next/link";
import { ArrowRight, MapPin } from "lucide-react";
import { notFound } from "next/navigation";
import { Container } from "@/components/ui/container";
import { cityLandingPageSeeder } from "@/data/seed/cityLandingPageSeeder";
import type { LocationSeoPage, LocationSeoRouteStatus } from "@/types/location-seo-page";

const apiBase=(process.env.NEXT_PUBLIC_API_BASE_URL??"http://localhost:5104/api").replace(/\/+$/,"");
const siteUrl=(process.env.NEXT_PUBLIC_SITE_URL??"https://www.rashelifesciences.in").replace(/\/+$/,"");
export const revalidate=60;
interface Props{params:Promise<{country:string}>}
type CityEntry=(typeof cityLandingPageSeeder)[number]|LocationSeoPage;

async function getPages():Promise<CityEntry[]>{
  try{
    const [pagesResponse,statusResponse]=await Promise.all([
      fetch(`${apiBase}/location-pages`,{next:{revalidate:60},signal:AbortSignal.timeout(5000)}),
      fetch(`${apiBase}/location-pages/status`,{next:{revalidate:60},signal:AbortSignal.timeout(5000)}),
    ]);
    const saved=pagesResponse.ok?await pagesResponse.json() as LocationSeoPage[]:[];
    const statuses=statusResponse.ok?await statusResponse.json() as LocationSeoRouteStatus[]:[];
    const byKey=new Map(saved.map(p=>[`${p.countrySlug}/${p.citySlug}`,p]));
    const statusByKey=new Map(statuses.map(p=>[`${p.countrySlug}/${p.citySlug}`,p]));
    const seedPages=cityLandingPageSeeder.flatMap(seed=>{
      const key=`${seed.countrySlug}/${seed.citySlug}`;const status=statusByKey.get(key);
      if(status&&!status.isPublished)return [];
      return [byKey.get(key)??seed];
    });
    const seedKeys=new Set(cityLandingPageSeeder.map(p=>`${p.countrySlug}/${p.citySlug}`));
    return [...seedPages,...saved.filter(p=>!seedKeys.has(`${p.countrySlug}/${p.citySlug}`))];
  }catch{return cityLandingPageSeeder;}
}

export async function generateMetadata({params}:Props):Promise<Metadata>{
  const {country}=await params;const pages=await getPages();const details=pages.find(p=>p.countrySlug===country.toLowerCase());
  if(!details)return {title:"Location not found | RashePharma",robots:{index:false,follow:false}};
  const title=`B2B Pharmaceutical Enquiries in ${details.country} | RashePharma`;
  const description=`Discuss pharmaceutical product requirements and potential business partnerships for your ${details.country}-based organisation with RashePharma.`;
  return {title,description,alternates:{canonical:`${siteUrl}/countries/${details.countrySlug}`},openGraph:{type:"website",title,description,siteName:"RashePharma"}};
}

export default async function CountryLandingPage({params}:Props){
  const {country}=await params;const pages=(await getPages()).filter(p=>p.countrySlug===country.toLowerCase());
  if(!pages.length)notFound();const details=pages[0];const enquiryHref=`/b2b/enquiry?countryCode=${details.countryCode}&quoteCurrency=${details.quoteCurrency}`;
  return <main className="min-h-screen bg-slate-50"><section className="border-b border-emerald-100 bg-gradient-to-br from-emerald-50 via-white to-cyan-50 py-16 sm:py-24"><Container><p className="inline-flex items-center gap-2 text-sm font-bold uppercase tracking-wider text-emerald-800"><MapPin className="size-4"/>B2B business enquiries</p><h1 className="mt-4 max-w-3xl text-4xl font-bold tracking-tight text-slate-950 sm:text-5xl">Pharmaceutical product and partnership enquiries in {details.country}</h1><p className="mt-5 max-w-2xl text-lg leading-8 text-slate-700">Share your organisation’s product and market requirements with RashePharma. Availability, documentation and commercial fit are reviewed for each enquiry.</p>{details.quoteCurrency==="USD"&&<p className="mt-3 text-sm font-medium text-emerald-900">International B2B quotations can be requested in USD.</p>}<Link href={enquiryHref} className="mt-8 inline-flex min-h-12 items-center gap-2 rounded-xl bg-emerald-700 px-6 font-semibold text-white hover:bg-emerald-800">Send a business enquiry<ArrowRight className="size-4"/></Link></Container></section><section className="py-14 sm:py-20"><Container><h2 className="text-3xl font-bold text-slate-950">Choose your business location</h2><p className="mt-3 text-slate-600">City-specific pages make it easy to start an enquiry with your location included.</p><ul className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">{pages.map(p=><li key={p.citySlug}><Link href={`/countries/${p.countrySlug}/${p.citySlug}`} className="group flex items-center justify-between rounded-2xl border border-slate-200 bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:border-emerald-300 hover:shadow-md"><span><b className="block text-slate-900">{p.city}</b><span className="mt-1 block text-sm text-slate-500">{p.region}</span></span><ArrowRight className="size-5 text-emerald-700 transition group-hover:translate-x-1"/></Link></li>)}</ul></Container></section></main>;
}
