import Link from "next/link";
import { ArrowRight, Search } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";
import { productService } from "@/services/product.service";
import type { ProductList } from "@/types/product";

export async function HeroSection() {
  let products : ProductList[] = [];

  try {
    products = await productService.getAll();
  } catch {
    products = [];
  }

  /*
   * Build popular categories dynamically from database products.
   *
   * Example:
   *
   * Pharmaceutical Tablets  → 16 products
   * Pharmaceutical Capsules → 7 products
   * Pharmaceutical Injection → 3 products
   *
   * The categories with the highest product count
   * will be shown as popular search suggestions.
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

  const popularCategories = Array.from(
    categoryCounts.entries()
  )
    .sort((a, b) => b[1] - a[1])
    .slice(0, 4)
    .map(([categoryName]) => categoryName);

  return (
    <section className="relative overflow-hidden border-b border-border bg-gradient-to-b from-[#edf7f7] via-white to-white">
      {/* Background decoration */}
      <div
        aria-hidden="true"
        className="absolute left-1/2 top-0 size-[420px] -translate-x-1/2 rounded-full bg-[#3E8F96]/10 blur-3xl"
      />

      <Container className="relative">
        <div className="mx-auto flex max-w-4xl flex-col items-center px-0 py-16 text-center sm:py-20 lg:py-24">
          {/* Eyebrow */}
          <div className="text-xs font-semibold uppercase tracking-[0.18em] text-[#F5821F]">
            QUALITY PHARMACEUTICALS
          </div>

          {/* Heading */}
          <h1 className="mt-4 max-w-3xl text-4xl font-semibold tracking-tight text-[#1B2A4A] sm:text-5xl lg:text-6xl">
            Quality Healthcare,
            <span className="block text-[#3E8F96]">
              Built for Trust.
            </span>
          </h1>

          {/* Description */}
          <p className="mt-5 max-w-2xl text-base leading-7 text-[#595959] sm:text-lg">
            Explore RashePharma&apos;s pharmaceutical range, discover products
            by name or composition, and connect with us for global healthcare
            and B2B requirements.
          </p>

          {/* Hero Search */}
          <form
            action="/products"
            method="GET"
            className="mt-8 w-full max-w-2xl"
          >
            <div className="flex h-14 items-center rounded-xl border border-[#d9e4e4] bg-white px-4 shadow-sm transition-shadow focus-within:shadow-md">
              <Search className="size-5 shrink-0 text-[#7a7a7a]" />

              <input
                type="search"
                name="search"
                placeholder="Search product or salt (e.g. cefixime)"
                aria-label="Search pharmaceutical products"
                className="ml-3 w-full bg-transparent text-sm text-[#1B2A4A] outline-none placeholder:text-[#999] sm:text-base"
              />

              <Button
                type="submit"
                size="sm"
                className="hidden shrink-0 rounded-lg bg-[#F5821F] px-5 text-white hover:bg-[#df7115] sm:inline-flex"
              >
                Search
              </Button>
            </div>
          </form>

          {/* Search suggestions */}
          {popularCategories.length > 0 && (
            <div className="mt-5 flex flex-wrap items-center justify-center gap-2">
              <span className="mr-1 text-xs font-medium text-[#777]">
                Popular:
              </span>

              {popularCategories.map((categoryName) => (
                <Link
                  key={categoryName}
                  href={`/products?search=${encodeURIComponent(
                    categoryName
                  )}`}
                  className="rounded-full border border-[#dcdcdc] bg-white px-3 py-1.5 text-xs font-medium text-[#555] transition-colors hover:border-[#3E8F96] hover:text-[#3E8F96]"
                >
                  {categoryName}
                </Link>
              ))}
            </div>
          )}

          {/* CTA */}
          <div className="mt-8 flex flex-col items-center gap-3 sm:flex-row">
            <Link href="/products">
              <Button
                size="lg"
                className="h-11 rounded-lg bg-[#F5821F] px-6 text-white hover:bg-[#df7115]"
              >
                Explore Products
                <ArrowRight className="size-4" />
              </Button>
            </Link>

            <Link href="/b2b/partner">
              <Button
                variant="outline"
                size="lg"
                className="h-11 rounded-lg border-[#3E8F96]/30 px-6 text-[#1B2A4A] hover:border-[#3E8F96] hover:text-[#3E8F96]"
              >
                Become a Partner
              </Button>
            </Link>
          </div>

          {/* Supporting stats */}
          <div className="mt-10 grid w-full max-w-2xl grid-cols-3 divide-x divide-[#dde5e5] rounded-xl border border-[#e5ecec] bg-white/80 py-4 shadow-sm">
            <div className="px-3">
              <p className="text-lg font-semibold text-[#1B2A4A]">
                Quality
              </p>

              <p className="mt-1 text-xs text-[#777]">
                Focused
              </p>
            </div>

            <div className="px-3">
              <p className="text-lg font-semibold text-[#1B2A4A]">
                Global
              </p>

              <p className="mt-1 text-xs text-[#777]">
                Reach
              </p>
            </div>

            <div className="px-3">
              <p className="text-lg font-semibold text-[#1B2A4A]">
                B2B
              </p>

              <p className="mt-1 text-xs text-[#777]">
                Support
              </p>
            </div>
          </div>
        </div>
      </Container>
    </section>
  );
}