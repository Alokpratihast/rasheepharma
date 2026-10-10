import Link from "next/link";
import {
  ArrowRight,
  Building2,
  Globe2,
  Search,
  ShieldCheck,
} from "lucide-react";

import { HeroVisual } from "@/components/home/HeroVisual";
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

  const popularCategories = Array.from(categoryCounts.entries())
    .sort((a, b) => b[1] - a[1])
    .slice(0, 4)
    .map(([name]) => name);

  return (
    <section className="relative isolate overflow-hidden bg-brand-dark text-white">
      {/* ============ BACKGROUND ============ */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 -z-10"
      >
        <div className="hero-drift absolute -left-40 -top-40 size-[560px] rounded-full bg-primary/50 blur-[110px]" />
        <div className="hero-drift-b absolute -bottom-48 -right-32 size-[520px] rounded-full bg-sky-600/35 blur-[110px]" />
        <div className="hero-drift absolute right-[30%] top-[8%] size-[240px] rounded-full bg-emerald-400/20 blur-[90px]" />
        <div className="hero-grid absolute inset-0" />
      </div>

      <Container>
        <div className="grid items-center gap-14 pb-28 pt-14 sm:pt-16 lg:grid-cols-[1.02fr_0.98fr] lg:gap-10 lg:pb-32 lg:pt-20">
          {/* ============ LEFT: CONTENT ============ */}
          <div className="max-w-[640px]">
            <span
              className={`${enter} inline-flex items-center gap-2.5 rounded-full border border-white/15 bg-white/10 px-4 py-1.5 text-xs font-medium text-white/90 backdrop-blur`}
            >
              <span className="relative flex size-2">
                <span className="absolute inline-flex size-full animate-ping rounded-full bg-emerald-400 opacity-60" />
                <span className="relative inline-flex size-2 rounded-full bg-emerald-400" />
              </span>
              Quality Pharmaceuticals &middot; Worldwide supply
            </span>

            <h1
              className={`${enter} mt-6 text-[40px] font-extrabold leading-[1.05] tracking-[-0.035em] sm:text-[56px] lg:text-[66px]`}
              style={delay(100)}
            >
              Quality Healthcare,
              <span className="hero-grad block">Built for Trust.</span>
            </h1>

            <p
              className={`${enter} mt-6 max-w-[560px] text-base leading-8 text-white/75 sm:text-lg`}
              style={delay(200)}
            >
              Explore RashePharma&apos;s pharmaceutical range, discover
              products by name or composition, and connect with us for global
              healthcare and B2B requirements.
            </p>

            {/* SEARCH (plain GET form - works without JavaScript) */}
            <form
              action="/products"
              method="GET"
              role="search"
              className={`${enter} mt-8 hidden w-full max-w-[600px] sm:block`}
              style={delay(300)}
            >
              <div className="flex items-center gap-2 rounded-full border border-white/20 bg-white/10 p-1.5 pl-5 shadow-[0_18px_50px_rgba(0,0,0,0.25)] backdrop-blur-xl transition-all duration-300 focus-within:border-emerald-300/60 focus-within:bg-white/15 focus-within:shadow-[0_0_0_4px_rgba(94,234,212,0.18)]">
                <Search className="size-5 shrink-0 text-white/60" />
                <input
                  type="search"
                  name="search"
                  placeholder="Search product or salt (e.g. cefixime)"
                  aria-label="Search pharmaceutical products"
                  className="min-w-0 flex-1 bg-transparent py-3 text-sm text-white outline-none placeholder:text-white/50 sm:text-[15px]"
                />
                <Button
                  type="submit"
                  aria-label="Search"
                  className="h-11 shrink-0 rounded-full bg-gradient-to-r from-emerald-400 to-teal-400 px-4 font-semibold text-brand-dark shadow-[0_8px_22px_rgba(45,212,191,0.35)] transition-all hover:-translate-y-0.5 hover:shadow-[0_12px_28px_rgba(45,212,191,0.5)] sm:px-6"
                >
                  <span className="hidden sm:inline">Search</span>
                  <ArrowRight className="size-4 sm:ml-1.5" />
                </Button>
              </div>
            </form>

            {/* POPULAR */}
            {popularCategories.length > 0 && (
              <div
                className={`${enter} mt-6 flex flex-wrap items-center gap-2.5 sm:mt-5`}
                style={delay(380)}
              >
                <span className="mr-1 text-sm font-semibold text-white/80">
                  Popular:
                </span>
                {popularCategories.map((name) => (
                  <Link
                    key={name}
                    href={`/products?search=${encodeURIComponent(name)}`}
                    className="rounded-full border border-white/20 bg-white/5 px-4 py-1.5 text-xs font-semibold text-white/85 transition-all duration-200 hover:-translate-y-0.5 hover:border-emerald-300/60 hover:bg-white/15 hover:text-white"
                  >
                    {name}
                  </Link>
                ))}
              </div>
            )}

            {/* CTAs */}
            <div
              className={`${enter} mt-9 flex flex-col gap-3 sm:flex-row`}
              style={delay(460)}
            >
              <Link
                href="/products"
                className="inline-flex h-13 items-center justify-center rounded-full bg-white px-8 py-3.5 font-bold text-brand-dark shadow-[0_14px_34px_rgba(0,0,0,0.3)] transition-all duration-200 hover:-translate-y-0.5 hover:bg-emerald-50 hover:shadow-[0_18px_40px_rgba(0,0,0,0.38)]"
              >
                Explore Products
                <ArrowRight className="ml-2 size-4" />
              </Link>
              <Link
                href="/b2b/enquiry"
                className="inline-flex h-13 items-center justify-center rounded-full border border-white/35 px-8 py-3.5 font-bold text-white transition-all duration-200 hover:-translate-y-0.5 hover:border-white hover:bg-white/10"
              >
                Become a Partner
              </Link>
            </div>

            {/* MINI TRUST ROW */}
            <ul
              className={`${enter} mt-10 flex flex-wrap gap-x-7 gap-y-3 text-sm text-white/70`}
              style={delay(540)}
            >
              <li className="flex items-center gap-2">
                <ShieldCheck className="size-4 text-emerald-300" />
                Quality focused
              </li>
              <li className="flex items-center gap-2">
                <Globe2 className="size-4 text-sky-300" />
                Global reach
              </li>
              <li className="flex items-center gap-2">
                <Building2 className="size-4 text-amber-300" />
                B2B support
              </li>
            </ul>
          </div>

          {/* ============ RIGHT: VISUAL ============ */}
          <div
            className={`${enter} [animation-duration:900ms]`}
            style={delay(250)}
          >
            <HeroVisual />
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
