import Image from "next/image";
import Link from "next/link";
import { ArrowRight, Check, Globe2, Search } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";
import { productService } from "@/services/product.service";
import type { ProductList } from "@/types/product";

import "./hero.css";

/** Staggered entrance (tw-animate-css utilities + per-element delay). */
const enter =
  "animate-in fade-in slide-in-from-bottom-6 duration-700 [animation-fill-mode:backwards]";

const delay = (ms: number): React.CSSProperties => ({
  animationDelay: `${ms}ms`,
});

/** Shown only when no product categories could be loaded. */
const FALLBACK_CHIPS = ["Tablets", "Capsules", "Injections", "Respules"];

const tile = "relative overflow-hidden rounded-3xl p-5 sm:p-6";
const glassTile = `${tile} border border-white/20 bg-white/10 backdrop-blur`;

export async function HeroSection() {
  let products: ProductList[] = [];

  try {
    products = await productService.getAll();
  } catch {
    products = [];
  }

  /* Popular categories - built from real products (same logic as before). */
  const categoryCounts = new Map<string, number>();

  for (const product of products) {
    const name = product.categoryName?.trim();
    if (!name) continue;
    categoryCounts.set(name, (categoryCounts.get(name) ?? 0) + 1);
  }

  const fromProducts = Array.from(categoryCounts.entries())
    .sort((a, b) => b[1] - a[1])
    .slice(0, 4)
    .map(([name]) => name);

  const chips = fromProducts.length > 0 ? fromProducts : FALLBACK_CHIPS;

  return (
    <section className="relative isolate overflow-hidden bg-brand-dark text-white">
      {/* ============ BACKGROUND: glows + global supply routes ============ */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 -z-10"
      >
        <div className="hero-drift absolute -left-40 -top-40 size-[520px] rounded-full bg-primary/45 blur-[110px]" />
        <div className="hero-drift-b absolute -bottom-48 right-[-8%] size-[520px] rounded-full bg-teal-500/25 blur-[110px]" />
        <div className="hero-grid absolute inset-0 opacity-50" />

        <svg
          viewBox="0 0 900 700"
          preserveAspectRatio="xMaxYMin slice"
          className="absolute inset-y-0 right-0 h-full w-full opacity-70 lg:w-[70%] lg:opacity-100"
        >
          <g fill="none" stroke="rgba(255,255,255,0.09)">
            <circle cx="880" cy="34" r="150" />
            <circle cx="880" cy="34" r="290" />
            <circle cx="880" cy="34" r="440" />
            <circle cx="880" cy="34" r="600" />
          </g>
          <g
            fill="none"
            stroke="#5eead4"
            strokeOpacity="0.6"
            strokeDasharray="4 8"
          >
            <path className="hero-route" d="M880 34 Q650 90 470 330" />
            <path className="hero-route" d="M880 34 Q940 280 850 520" />
            <path className="hero-route" d="M880 34 Q690 360 560 640" />
            <path className="hero-route" d="M880 34 Q600 20 330 170" />
          </g>
          <g fill="#5eead4">
            <circle cx="470" cy="330" r="5" />
            <circle cx="850" cy="520" r="5" />
            <circle cx="560" cy="640" r="5" />
            <circle cx="330" cy="170" r="5" />
          </g>
          <circle className="hero-ping" cx="880" cy="34" r="10" fill="#F97316" />
          <circle cx="880" cy="34" r="7" fill="#F97316" />
          <text
            x="862"
            y="39"
            textAnchor="end"
            fill="#fed7aa"
            fontSize="14"
            fontWeight="600"
          >
            Bangalore
          </text>
        </svg>
      </div>

      <Container>
        <div className="grid items-center gap-12 pb-24 pt-12 sm:pt-14 lg:grid-cols-[1.05fr_0.95fr] lg:gap-14 lg:pb-32 lg:pt-16">
          {/* ============ LEFT: CONTENT ============ */}
          <div className="max-w-[640px]">
            <p
              className={`${enter} text-xs font-semibold uppercase tracking-[0.16em] text-orange-400`}
            >
              Rashe Lifesciences &middot; Bangalore
            </p>

            <h1
              className={`${enter} mt-5 text-[38px] font-bold leading-[1.02] tracking-[-0.035em] sm:text-[54px] lg:text-[66px]`}
              style={delay(100)}
            >
              Medicines you can{" "}
              <span className="hero-grad">trust,</span> delivered
              worldwide.
            </h1>

            <p
              className={`${enter} mt-6 max-w-[520px] text-base leading-8 text-white/75 sm:text-lg`}
              style={delay(200)}
            >
              Search our range by name or composition, or talk to us about
              bulk and export supply.
            </p>

            {/* SEARCH (plain GET form - works without JavaScript) */}
            <form
              action="/products"
              method="GET"
              role="search"
              className={`${enter} mt-8 hidden w-full max-w-[560px] sm:block`}
              style={delay(300)}
            >
              <div className="flex items-center gap-2 rounded-2xl bg-white p-2 pl-5 shadow-[0_18px_50px_rgba(0,0,0,0.3)] focus-within:ring-4 focus-within:ring-teal-300/30">
                <Search className="size-5 shrink-0 text-slate-400" />
                <input
                  type="search"
                  name="search"
                  placeholder="Search product or salt (e.g. cefixime)"
                  aria-label="Search pharmaceutical products"
                  className="min-w-0 flex-1 bg-transparent py-3 text-[15px] text-slate-900 outline-none placeholder:text-slate-500"
                />
                <Button
                  type="submit"
                  aria-label="Search"
                  className="h-12 shrink-0 rounded-xl bg-primary px-6 font-semibold text-white hover:bg-primary-dark"
                >
                  Search
                </Button>
              </div>
            </form>

            {/* POPULAR */}
            <div
              className={`${enter} mt-6 flex flex-wrap items-center gap-2.5 sm:mt-5`}
              style={delay(380)}
            >
              {chips.map((name) => (
                <Link
                  key={name}
                  href={`/products?search=${encodeURIComponent(name)}`}
                  className="rounded-full border border-white/20 bg-white/10 px-4 py-1.5 text-sm text-white/90 transition-colors hover:border-teal-300/60 hover:bg-white/20"
                >
                  {name}
                </Link>
              ))}
            </div>

            {/* TRUST ROW */}
            <ul
              className={`${enter} mt-9 flex flex-wrap gap-x-7 gap-y-3 text-sm text-white/75`}
              style={delay(460)}
            >
              {["WHO-GMP plants", "ISO 9001:2015", "15 yrs experience"].map(
                (item) => (
                  <li key={item} className="flex items-center gap-2">
                    <Check className="size-4 text-teal-300" />
                    {item}
                  </li>
                ),
              )}
            </ul>
          </div>

          {/* ============ RIGHT: BENTO ============ */}
          <div
            className={`${enter} grid grid-cols-2 gap-3 [animation-duration:900ms] sm:gap-4 lg:grid-cols-[1.2fr_1fr] lg:grid-rows-[190px_190px_130px]`}
            style={delay(250)}
          >
            {/* photo */}
            <div
              className={`${tile} hero-float row-span-2 min-h-[280px] border border-white/15 bg-primary-dark p-0`}
            >
              <Image
                src="/images/header1.jpg"
                alt="Pharmaceutical research and quality"
                fill
                priority
                sizes="(max-width: 1024px) 50vw, 300px"
                className="object-cover object-[50%_35%]"
              />
              <div className="absolute inset-0 bg-gradient-to-t from-brand-dark/80 via-transparent to-transparent" />
              <span className="absolute left-5 top-5 text-xs font-semibold uppercase tracking-[0.12em] text-white">
                Our range
              </span>
              <span className="absolute bottom-4 left-4 inline-flex items-center gap-2 rounded-full bg-white/15 px-3.5 py-1.5 text-xs font-semibold text-white backdrop-blur">
                <Globe2 className="size-3.5 text-teal-300" />
                Worldwide supply
              </span>
            </div>

            {/* certified */}
            <div className={`${tile} hero-float-b bg-white text-slate-900`}>
              <p className="text-xs font-semibold uppercase tracking-[0.12em] text-primary">
                Certified
              </p>
              <p className="mt-3 text-2xl font-bold tracking-tight sm:text-3xl">
                WHO-GMP
              </p>
              <p className="mt-1 text-sm text-slate-600">ISO 9001:2015</p>
            </div>

            {/* years */}
            <div className={`${glassTile} hero-float-c`}>
              <p className="text-xs font-semibold uppercase tracking-[0.12em] text-teal-200">
                Team
              </p>
              <p className="mt-2 text-4xl font-bold tracking-tight">15 yrs</p>
              <p className="text-sm text-white/75">in pharmacy</p>
            </div>

            {/* B2B */}
            <Link
              href="/b2b/enquiry"
              className={`${tile} group col-span-2 flex items-center justify-between gap-4 bg-orange-500 text-slate-950 transition-colors hover:bg-orange-400`}
            >
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.12em]">
                  For distributors
                </p>
                <p className="mt-2 text-xl font-bold tracking-tight sm:text-2xl">
                  Request a bulk quote
                </p>
              </div>
              <span className="inline-flex shrink-0 items-center gap-1.5 rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white">
                Enquire now
                <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
              </span>
            </Link>
          </div>
        </div>
      </Container>

      {/* ============ CURVED DIVIDER ============ */}
      <svg
        aria-hidden="true"
        viewBox="0 0 1440 80"
        preserveAspectRatio="none"
        className="absolute inset-x-0 -bottom-px h-10 w-full fill-background sm:h-16"
      >
        <path d="M0,42 C240,92 480,2 720,32 C960,62 1200,92 1440,36 L1440,80 L0,80 Z" />
      </svg>
    </section>
  );
}
