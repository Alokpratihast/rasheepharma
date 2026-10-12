import type { Metadata } from "next";
import Link from "next/link";
import { Suspense } from "react";
import { ChevronRight, FolderTree, Pill } from "lucide-react";

import { Container } from "@/components/ui/container";
import { ProductsPageClient } from "@/components/product/ProductsPageClient";
import { productService } from "@/services/product.service";
import type { ProductList } from "@/types/product";

export const metadata: Metadata = {
  title: "Products | RashePharma",
  description:
    "Browse the RashePharma pharmaceutical portfolio by product, category and dosage form.",
};

function ProductsSkeleton() {
  return (
    <div className="grid gap-8 lg:grid-cols-[272px_minmax(0,1fr)]">
      <div className="hidden h-[520px] animate-pulse rounded-3xl bg-muted lg:block" />
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 xl:grid-cols-3">
        {Array.from({ length: 6 }).map((_, i) => (
          <div
            key={i}
            className="h-[360px] animate-pulse rounded-3xl bg-muted"
          />
        ))}
      </div>
    </div>
  );
}

export default async function ProductsPage() {
  let products: ProductList[] = [];
  let errorMessage = "";

  try {
    products = await productService.getAll();
  } catch {
    errorMessage = "Unable to load products right now. Please try again.";
  }

  const categoryCount = new Set(
    products.map((p) => p.categoryName?.trim()).filter(Boolean),
  ).size;

  return (
    <main className="min-h-screen bg-background">
      {/* ================= BANNER ================= */}
      <section className="relative isolate overflow-hidden bg-brand-dark text-white">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute inset-0 -z-10"
        >
          <div className="absolute -left-32 -top-32 size-[420px] rounded-full bg-primary/45 blur-[100px]" />
          <div className="absolute -bottom-40 right-0 size-[380px] rounded-full bg-sky-600/30 blur-[100px]" />
          <div
            className="absolute inset-0 opacity-60"
            style={{
              backgroundImage:
                "linear-gradient(rgba(255,255,255,.05) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,.05) 1px, transparent 1px)",
              backgroundSize: "56px 56px",
              maskImage:
                "radial-gradient(circle at 50% 30%, #000, transparent 75%)",
              WebkitMaskImage:
                "radial-gradient(circle at 50% 30%, #000, transparent 75%)",
            }}
          />
        </div>

        <Container>
          <div className="pb-20 pt-10 sm:pb-24 sm:pt-12">
            <nav aria-label="Breadcrumb" className="text-xs text-white/60">
              <ol className="flex items-center gap-1.5">
                <li>
                  <Link href="/" className="transition-colors hover:text-white">
                    Home
                  </Link>
                </li>
                <ChevronRight className="size-3.5" />
                <li aria-current="page" className="text-white/90">
                  Products
                </li>
              </ol>
            </nav>

            <div className="mt-6 flex flex-col gap-8 lg:flex-row lg:items-end lg:justify-between">
              <div className="animate-in fade-in slide-in-from-bottom-4 duration-700">
                <p className="text-xs font-bold uppercase tracking-[0.18em] text-emerald-300">
                  Product Catalogue
                </p>
                <h1 className="mt-3 text-4xl font-extrabold tracking-[-0.03em] sm:text-5xl">
                  Our{" "}
                  <span className="bg-gradient-to-r from-teal-300 to-emerald-200 bg-clip-text text-transparent">
                    Products
                  </span>
                </h1>
                <p className="mt-4 max-w-xl text-base leading-7 text-white/70">
                  Browse our pharmaceutical portfolio by product, category and
                  dosage form, then request a quote for what you need.
                </p>
              </div>

              {products.length > 0 && (
                <div className="flex gap-3 animate-in fade-in slide-in-from-bottom-4 duration-700 [animation-delay:150ms] [animation-fill-mode:backwards]">
                  <div className="flex items-center gap-3 rounded-2xl border border-white/15 bg-white/10 px-5 py-3.5 backdrop-blur-xl">
                    <span className="flex size-10 items-center justify-center rounded-xl bg-emerald-400/20">
                      <Pill className="size-5 text-emerald-300" />
                    </span>
                    <span>
                      <span className="block text-2xl font-extrabold leading-none">
                        {products.length}
                      </span>
                      <span className="text-xs text-white/65">Products</span>
                    </span>
                  </div>
                  <div className="flex items-center gap-3 rounded-2xl border border-white/15 bg-white/10 px-5 py-3.5 backdrop-blur-xl">
                    <span className="flex size-10 items-center justify-center rounded-xl bg-sky-400/20">
                      <FolderTree className="size-5 text-sky-300" />
                    </span>
                    <span>
                      <span className="block text-2xl font-extrabold leading-none">
                        {categoryCount}
                      </span>
                      <span className="text-xs text-white/65">Categories</span>
                    </span>
                  </div>
                </div>
              )}
            </div>
          </div>
        </Container>

        <svg
          aria-hidden="true"
          viewBox="0 0 1440 60"
          preserveAspectRatio="none"
          className="absolute inset-x-0 -bottom-px h-8 w-full fill-background sm:h-12"
        >
          <path d="M0,30 C240,70 480,0 720,24 C960,48 1200,70 1440,26 L1440,60 L0,60 Z" />
        </svg>
      </section>

      {/* ================= CONTENT ================= */}
      <section className="pb-20 pt-6 sm:pt-8">
        <Container>
          {errorMessage ? (
            <div className="rounded-3xl border border-dashed border-border bg-card py-20 text-center">
              <p className="text-sm font-medium text-foreground">
                {errorMessage}
              </p>
            </div>
          ) : (
            <Suspense fallback={<ProductsSkeleton />}>
              <ProductsPageClient products={products} />
            </Suspense>
          )}
        </Container>
      </section>
    </main>
  );
}
