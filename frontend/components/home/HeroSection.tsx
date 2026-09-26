




import Image from "next/image";
import Link from "next/link";
import {
  ArrowRight,
  Building2,
  Globe2,
  Handshake,
  Pill,
  Search,
  ShieldCheck,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";
import { productService } from "@/services/product.service";
import type { ProductList } from "@/types/product";

export async function HeroSection() {
  let products: ProductList[] = [];

  try {
    products = await productService.getAll();
  } catch {
    products = [];
  }

  /*
   * Build popular categories dynamically from database products.
   */
  const categoryCounts = new Map<string, number>();

  for (const product of products) {
    const categoryName = product.categoryName?.trim();

    if (!categoryName) {
      continue;
    }

    categoryCounts.set(
      categoryName,
      (categoryCounts.get(categoryName) ?? 0) + 1
    );
  }

  const popularCategories = Array.from(categoryCounts.entries())
    .sort((a, b) => b[1] - a[1])
    .slice(0, 4)
    .map(([categoryName]) => categoryName);

  return (
    <section className="relative overflow-hidden border-b border-[#e5eeee] bg-white">
      {/* =========================================================
          BACKGROUND DECORATION
      ========================================================== */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 z-0 overflow-hidden"
      >
        <div className="absolute -left-40 top-20 size-[420px] rounded-full bg-[#dff5f6] blur-3xl opacity-70" />

        <div className="absolute -right-40 -top-32 size-[600px] rounded-full bg-[#e8f8f9] blur-3xl opacity-80" />

        <div className="absolute bottom-[-250px] left-[30%] size-[500px] rounded-full bg-[#f3fafb] blur-3xl" />
      </div>

      <Container className="relative z-10">
        {/* =========================================================
            MAIN HERO
        ========================================================== */}
        <div className="grid items-center gap-10 pt-10 pb-12 sm:pt-12 sm:pb-14 lg:grid-cols-[0.95fr_1.05fr] lg:gap-8 lg:pt-12 lg:pb-14">
          {/* =======================================================
              LEFT CONTENT
          ======================================================== */}
          <div className="relative z-20 max-w-[680px]">
            {/* Eyebrow */}
            <div className="mb-5 flex items-center gap-3">
              <span className="h-px w-8 bg-[#F5821F]" />

              <span className="text-xs font-bold tracking-[0.28em] text-[#F5821F]">
                QUALITY PHARMACEUTICALS
              </span>
            </div>

            {/* Heading */}
            <h1 className="max-w-[680px] text-[42px] font-extrabold leading-[1.05] tracking-[-0.04em] text-[#1B2A4A] sm:text-[54px] lg:text-[64px]">
              Quality Healthcare,
              <span className="block bg-gradient-to-r from-[#16899B] to-[#32A7B5] bg-clip-text text-transparent">
                Built for Trust.
              </span>
            </h1>

            {/* Description */}
            <p className="mt-6 max-w-[610px] text-base leading-8 text-[#61758D] sm:text-lg">
              Explore RashePharma&apos;s pharmaceutical range, discover
              products by name or composition, and connect with us for global
              healthcare and B2B requirements.
            </p>

            {/* =====================================================
                SEARCH
            ====================================================== */}
            <form
              action="/products"
              method="GET"
              className="mt-8 w-full max-w-[650px]"
            >
              <div className="flex min-h-14 items-center rounded-2xl border border-[#d8e9ea] bg-white p-1.5 shadow-[0_12px_35px_rgba(27,42,74,0.10)] transition-all duration-300 focus-within:border-[#3E9BA7] focus-within:shadow-[0_16px_40px_rgba(21,155,176,0.14)]">
                <div className="flex min-w-0 flex-1 items-center gap-3 px-3 sm:px-4">
                  <Search className="size-5 shrink-0 text-[#16899B]" />

                  <input
                    type="search"
                    name="search"
                    placeholder="Search product or salt (e.g. cefixime)"
                    aria-label="Search pharmaceutical products"
                    className="w-full min-w-0 bg-transparent py-3 text-sm text-[#1B2A4A] outline-none placeholder:text-[#8C9AAA] sm:text-[15px]"
                  />
                </div>

                <Button
                  type="submit"
                  className="hidden h-11 shrink-0 rounded-xl bg-[#F5821F] px-6 font-semibold text-white shadow-[0_6px_16px_rgba(245,130,31,0.20)] transition-all duration-200 hover:-translate-y-0.5 hover:bg-[#df7115] sm:inline-flex"
                >
                  Search
                  <ArrowRight className="ml-1.5 size-4" />
                </Button>
              </div>
            </form>

            {/* =====================================================
                POPULAR CATEGORIES
            ====================================================== */}
            {popularCategories.length > 0 && (
              <div className="mt-5 flex flex-wrap items-center gap-2.5">
                <span className="mr-1 text-sm font-bold text-[#1B2A4A]">
                  Popular:
                </span>

                {popularCategories.map((categoryName) => (
                  <Link
                    key={categoryName}
                    href={`/products?search=${encodeURIComponent(
                      categoryName
                    )}`}
                    className="rounded-full border border-[#d3edf0] bg-[#edf9fa] px-4 py-2 text-xs font-semibold text-[#16899B] transition-all duration-200 hover:-translate-y-0.5 hover:border-[#b7e1e5] hover:bg-[#def5f7]"
                  >
                    {categoryName}
                  </Link>
                ))}
              </div>
            )}

            {/* =====================================================
                CTA BUTTONS
            ====================================================== */}
            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              <Link
                href="/products"
                className="inline-flex h-12 items-center justify-center rounded-xl bg-[#F5821F] px-7 font-bold text-white shadow-[0_8px_20px_rgba(245,130,31,0.22)] transition-all duration-200 hover:-translate-y-0.5 hover:bg-[#df7115] hover:shadow-[0_12px_25px_rgba(245,130,31,0.28)]"
              >
                Explore Products
                <ArrowRight className="ml-2 size-4" />
              </Link>

              <Link
                href="/b2b/enquiry"
                className="inline-flex h-12 items-center justify-center rounded-xl border border-[#16899B] bg-white px-7 font-bold text-[#1B2A4A] transition-all duration-200 hover:-translate-y-0.5 hover:bg-[#f0fafb] hover:text-[#16899B]"
              >
                Become a Partner
              </Link>
            </div>
          </div>

          {/* =======================================================
              RIGHT VISUAL
          ======================================================== */}
          <div className="relative min-h-[460px] sm:min-h-[500px] lg:min-h-[590px]">
            {/* Soft glow */}
            <div className="absolute right-0 top-10 h-[500px] w-[500px] rounded-full bg-[#dff5f6] blur-3xl opacity-80" />

            {/* =====================================================
                MAIN SCIENTIST IMAGE
            ====================================================== */}
            <div className="absolute right-0 top-0 h-[460px] w-full max-w-[590px] overflow-hidden rounded-[32px] bg-[#eaf7f8] shadow-[0_25px_70px_rgba(27,42,74,0.14)] sm:h-[520px] sm:rounded-[38px] lg:h-[570px]">
              <Image
                src="/images/header1.jpg"
                alt="Pharmaceutical research and quality"
                fill
                priority
                sizes="(max-width: 1024px) 100vw, 590px"
                className="object-cover object-center"
              />

              {/* White left fade */}
              <div className="absolute inset-0 bg-gradient-to-r from-white/70 via-white/15 to-transparent" />

              {/* Bottom teal fade */}
              <div className="absolute inset-0 bg-gradient-to-t from-[#087C8D]/20 via-transparent to-transparent" />

              {/* Dot pattern */}
              <div className="absolute left-[-10px] top-24 grid grid-cols-6 gap-2 opacity-40">
                {Array.from({ length: 36 }).map((_, index) => (
                  <span
                    key={index}
                    className="size-1.5 rounded-full bg-[#16899B]"
                  />
                ))}
              </div>
            </div>

            {/* =====================================================
                QUALITY ASSURED
            ====================================================== */}
            <div className="absolute right-2 top-8 z-30 flex items-center gap-3 rounded-2xl border border-white/80 bg-white/95 px-4 py-3 shadow-[0_15px_35px_rgba(27,42,74,0.14)] backdrop-blur-md sm:right-5 sm:px-5">
              <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-[#e3f6f7]">
                <ShieldCheck className="size-6 text-[#16899B]" />
              </div>

              <div>
                <p className="text-sm font-bold text-[#1B2A4A]">
                  Quality Assured
                </p>

                <p className="mt-0.5 text-[11px] text-[#71839A]">
                  Consistent standards
                </p>
              </div>
            </div>

            {/* =====================================================
                GLOBAL SUPPLY
            ====================================================== */}
            <div className="absolute right-8 top-[145px] z-30 flex items-center gap-3 rounded-2xl border border-white/80 bg-white/95 px-4 py-3 shadow-[0_15px_35px_rgba(27,42,74,0.14)] backdrop-blur-md sm:right-12 sm:px-5">
              <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-[#e3f6f7]">
                <Globe2 className="size-6 text-[#16899B]" />
              </div>

              <div>
                <p className="text-sm font-bold text-[#1B2A4A]">
                  Global Supply
                </p>

                <p className="mt-0.5 text-[11px] text-[#71839A]">
                  Healthcare partnerships
                </p>
              </div>
            </div>

            {/* =====================================================
                B2B FOCUSED
            ====================================================== */}
            <div className="absolute right-2 top-[245px] z-30 flex items-center gap-3 rounded-2xl border border-white/80 bg-white/95 px-4 py-3 shadow-[0_15px_35px_rgba(27,42,74,0.14)] backdrop-blur-md sm:right-7 sm:px-5">
              <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-[#fff1e7]">
                <Handshake className="size-6 text-[#F5821F]" />
              </div>

              <div>
                <p className="text-sm font-bold text-[#1B2A4A]">
                  B2B Focused
                </p>

                <p className="mt-0.5 text-[11px] text-[#71839A]">
                  Reliable partnerships
                </p>
              </div>
            </div>

            {/* =====================================================
                PRODUCT IMAGE FLOATING CARD
            ====================================================== */}
            <div className="absolute bottom-0 left-[-8px] z-40 h-[180px] w-[265px] overflow-hidden rounded-[26px] border-[6px] border-white bg-white shadow-[0_25px_55px_rgba(27,42,74,0.20)] sm:left-[-20px] sm:h-[225px] sm:w-[330px] sm:rounded-[28px]">
              <Image
                src="/images/rasheheader2.png"
                alt="Pharmaceutical products"
                fill
                sizes="330px"
                className="object-cover"
              />

              {/* Image overlay */}
              <div className="absolute inset-0 bg-gradient-to-t from-[#1B2A4A]/30 via-transparent to-transparent" />

              {/* Product information */}
              <div className="absolute bottom-3 left-3 flex items-center gap-2 rounded-xl bg-white/95 px-3 py-2 shadow-lg backdrop-blur">
                <div className="flex size-8 items-center justify-center rounded-lg bg-[#e4f7f8]">
                  <Pill className="size-4 text-[#16899B]" />
                </div>

                <div>
                  <p className="text-[11px] font-bold text-[#1B2A4A]">
                    Pharmaceutical Range
                  </p>

                  <p className="text-[9px] text-[#71839A]">
                    Quality-focused solutions
                  </p>
                </div>
              </div>
            </div>

            {/* =====================================================
                DECORATIVE HEALTHCARE BADGE
            ====================================================== */}
            <div className="absolute bottom-8 right-[-5px] z-30 flex size-[100px] items-center justify-center rounded-full border-[7px] border-white bg-[#fff5ec] shadow-[0_15px_35px_rgba(245,130,31,0.15)] sm:size-[125px] sm:border-[8px]">
              <div className="text-center">
                <Pill className="mx-auto size-5 text-[#F5821F]" />

                <p className="mt-1 text-[10px] font-bold text-[#1B2A4A]">
                  Healthcare
                </p>

                <p className="text-[9px] text-[#71839A]">
                  For Tomorrow
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* =========================================================
            TRUST FEATURE STRIP
        ========================================================== */}
        <div className="relative z-10 border-t border-[#e4eeee] bg-white/90 py-7">
          <div className="grid grid-cols-1 divide-y divide-[#e4eeee] md:grid-cols-2 md:divide-y-0 lg:grid-cols-4 lg:divide-x">
            {/* Quality */}
            <div className="flex items-center gap-4 px-3 py-5 lg:px-7 lg:py-2">
              <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-[#e4f7f8]">
                <ShieldCheck className="size-6 text-[#16899B]" />
              </div>

              <div>
                <p className="text-sm font-bold text-[#1B2A4A]">
                  Quality Focused
                </p>

                <p className="mt-1 text-xs leading-5 text-[#71839A]">
                  Committed to high standards
                </p>
              </div>
            </div>

            {/* Global */}
            <div className="flex items-center gap-4 px-3 py-5 lg:px-7 lg:py-2">
              <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-[#e4f7f8]">
                <Globe2 className="size-6 text-[#16899B]" />
              </div>

              <div>
                <p className="text-sm font-bold text-[#1B2A4A]">
                  Global Reach
                </p>

                <p className="mt-1 text-xs leading-5 text-[#71839A]">
                  Serving healthcare partners
                </p>
              </div>
            </div>

            {/* B2B */}
            <div className="flex items-center gap-4 px-3 py-5 lg:px-7 lg:py-2">
              <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-[#fff1e7]">
                <Building2 className="size-6 text-[#F5821F]" />
              </div>

              <div>
                <p className="text-sm font-bold text-[#1B2A4A]">
                  B2B Support
                </p>

                <p className="mt-1 text-xs leading-5 text-[#71839A]">
                  Dedicated to your growth
                </p>
              </div>
            </div>

            {/* Product Range */}
            <div className="flex items-center gap-4 px-3 py-5 lg:px-7 lg:py-2">
              <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-[#e4f7f8]">
                <Pill className="size-6 text-[#16899B]" />
              </div>

              <div>
                <p className="text-sm font-bold text-[#1B2A4A]">
                  Wide Range
                </p>

                <p className="mt-1 text-xs leading-5 text-[#71839A]">
                  Diverse pharmaceutical solutions
                </p>
              </div>
            </div>
          </div>
        </div>
      </Container>

      {/* =========================================================
          BOTTOM DECORATIVE CURVES
      ========================================================== */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute bottom-0 left-0 z-0 h-12 w-full overflow-hidden"
      >
        <div className="absolute -bottom-20 left-[-5%] h-24 w-[55%] rounded-[50%] bg-[#e5f7f8]" />

        <div className="absolute -bottom-24 right-[-5%] h-28 w-[55%] rounded-[50%] bg-[#eef9fa]" />
      </div>
    </section>
  );
}