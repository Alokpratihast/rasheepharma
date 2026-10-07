import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import {
  ArrowRight,
  Building2,
  CheckCircle2,
  ChevronRight,
  FileText,
  FlaskConical,
  Headset,
  Package,
  Pill,
  ShieldCheck,
  Tag,
  Globe2,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { ProductCard } from "@/components/product/ProductCard";
import { ProductGallery } from "@/components/product/ProductGallery";
import { ProductPurchaseActions } from "@/components/product/ProductPurchaseActions";
import { getApiAssetUrl } from "@/lib/api/client";
import { formatPrice } from "@/lib/format";
import { productService } from "@/services/product.service";
import type { ProductList } from "@/types/product";

interface ProductDetailPageProps {
  params: Promise<{
    slug: string;
  }>;
}

export async function generateMetadata({
  params,
}: ProductDetailPageProps): Promise<Metadata> {
  const { slug } = await params;

  try {
    const product = await productService.getBySlug(slug);

    return {
      title: `${product.name} | RashePharma`,
      description:
        product.description ||
        `${product.name} pharmaceutical product from RashePharma.`,
    };
  } catch {
    return {
      title: "Product | RashePharma",
    };
  }
}

/** One cell of the variants table (label shows on mobile only). */
function Cell({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <div>
      <p className="mb-1 text-[10px] font-semibold uppercase tracking-wide text-muted-foreground md:hidden">
        {label}
      </p>
      {children}
    </div>
  );
}

export default async function ProductDetailPage({
  params,
}: ProductDetailPageProps) {
  const { slug } = await params;

  let product;

  try {
    product = await productService.getBySlug(slug);
  } catch {
    notFound();
  }

  if (!product || !product.isActive) {
    notFound();
  }

  /* ---------- images (primary first) ---------- */
  const galleryImages = [...product.images]
    .sort(
      (a, b) =>
        Number(b.isPrimary) - Number(a.isPrimary) ||
        a.displayOrder - b.displayOrder,
    )
    .map((image) => ({
      id: image.id,
      url: getApiAssetUrl(image.imageUrl) ?? "",
      alt: image.altText ?? product.name,
    }))
    .filter((image) => image.url);

  /* ---------- variants ---------- */
  const activeVariants = product.variants.filter((v) => v.isActive);
  const availableVariants = activeVariants.filter((v) => v.stockQuantity > 0);

  const pricedVariants = activeVariants.filter((v) => v.price > 0);
  const lowestPrice =
    pricedVariants.length > 0
      ? Math.min(...pricedVariants.map((v) => v.price))
      : null;

  const inStock = availableVariants.length > 0;

  /* ---------- related products ---------- */
  let related: ProductList[] = [];
  try {
    const all = await productService.getAll();
    related = all
      .filter(
        (p) =>
          p.isActive &&
          p.id !== product.id &&
          p.categoryName === product.categoryName,
      )
      .slice(0, 4);
  } catch {
    related = [];
  }

  const facts = [
    { label: "Dosage form", value: product.dosageForm, Icon: Pill },
    { label: "Manufacturer", value: product.manufacturer, Icon: Building2 },
    { label: "Brand", value: product.brandName, Icon: Tag },
    { label: "Category", value: product.categoryName, Icon: Package },
  ].filter((fact) => fact.value);

  const categoryHref = `/products?category=${encodeURIComponent(
    product.categoryName,
  )}`;

  return (
    <main className="min-h-screen bg-background">
      {/* =================== BREADCRUMB =================== */}
      <section className="border-b border-border bg-muted/40">
        <Container>
          <nav
            aria-label="Breadcrumb"
            className="flex min-h-14 items-center gap-1.5 overflow-hidden text-sm"
          >
            <Link
              href="/products"
              className="shrink-0 font-medium text-muted-foreground transition-colors hover:text-primary"
            >
              Products
            </Link>
            <ChevronRight className="size-4 shrink-0 text-muted-foreground/60" />
            <Link
              href={categoryHref}
              className="shrink-0 truncate text-muted-foreground transition-colors hover:text-primary"
            >
              {product.categoryName}
            </Link>
            <ChevronRight className="size-4 shrink-0 text-muted-foreground/60" />
            <span
              aria-current="page"
              className="truncate font-semibold text-foreground"
            >
              {product.name}
            </span>
          </nav>
        </Container>
      </section>

      {/* =================== PRODUCT HERO =================== */}
      <section className="py-8 sm:py-12">
        <Container>
          <div className="grid gap-10 lg:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)] lg:items-start lg:gap-14">
            {/* Gallery */}
            <div className="lg:sticky lg:top-32">
              <ProductGallery
                images={galleryImages}
                productName={product.name}
                featured={product.isFeatured}
              />
            </div>

            {/* Info */}
            <div className="min-w-0">
              {/* Chips */}
              <div className="flex flex-wrap items-center gap-2">
                <Link
                  href={categoryHref}
                  className="rounded-full bg-primary-light px-3.5 py-1.5 text-xs font-bold text-primary transition-colors hover:bg-primary/15"
                >
                  {product.categoryName}
                </Link>

                {product.dosageForm && (
                  <span className="rounded-full border border-border bg-background px-3.5 py-1.5 text-xs font-medium text-muted-foreground">
                    {product.dosageForm}
                  </span>
                )}

                {inStock ? (
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-500/10 px-3.5 py-1.5 text-xs font-bold text-emerald-600 dark:text-emerald-400">
                    <CheckCircle2 className="size-3.5" />
                    In stock
                  </span>
                ) : (
                  <span className="rounded-full bg-muted px-3.5 py-1.5 text-xs font-semibold text-muted-foreground">
                    Contact for availability
                  </span>
                )}
              </div>

              {/* Title */}
              <h1 className="mt-5 text-3xl font-extrabold leading-[1.1] tracking-[-0.03em] text-foreground sm:text-4xl lg:text-[44px]">
                {product.name}
              </h1>

              {product.genericName && (
                <p className="mt-3 text-base font-medium text-muted-foreground sm:text-lg">
                  {product.genericName}
                </p>
              )}

              {/* Price */}
              <div className="mt-6 flex flex-wrap items-end gap-x-4 gap-y-1">
                {lowestPrice !== null ? (
                  <>
                    <div>
                      <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                        Starting from
                      </p>
                      <p className="mt-1 text-4xl font-extrabold tracking-tight text-primary">
                        {formatPrice(lowestPrice)}
                      </p>
                    </div>
                    {activeVariants.length > 1 && (
                      <p className="pb-1.5 text-sm text-muted-foreground">
                        {activeVariants.length} variants available
                      </p>
                    )}
                  </>
                ) : (
                  <p className="text-xl font-bold text-foreground">
                    Contact us for pricing
                  </p>
                )}
              </div>

              {/* Quick facts */}
              {facts.length > 0 && (
                <dl className="mt-7 grid grid-cols-1 gap-3 sm:grid-cols-2">
                  {facts.map(({ label, value, Icon }) => (
                    <div
                      key={label}
                      className="flex items-center gap-3 rounded-2xl border border-border bg-card p-3.5"
                    >
                      <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary-light text-primary">
                        <Icon className="size-[18px]" />
                      </span>
                      <div className="min-w-0">
                        <dt className="text-[10px] font-semibold uppercase tracking-[0.12em] text-muted-foreground">
                          {label}
                        </dt>
                        <dd className="truncate text-sm font-bold text-foreground">
                          {value}
                        </dd>
                      </div>
                    </div>
                  ))}
                </dl>
              )}

              {/* Purchase */}
              <div className="mt-7">
                <ProductPurchaseActions variants={product.variants} />
              </div>

              {/* Trust row */}
              <ul className="mt-6 grid grid-cols-1 gap-3 text-sm sm:grid-cols-3">
                {[
                  { Icon: ShieldCheck, text: "Quality focused" },
                  { Icon: Globe2, text: "Global supply" },
                  { Icon: Headset, text: "Dedicated B2B support" },
                ].map(({ Icon, text }) => (
                  <li
                    key={text}
                    className="flex items-center gap-2.5 text-muted-foreground"
                  >
                    <Icon className="size-[18px] text-primary" />
                    {text}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </Container>
      </section>

      {/* =================== COMPOSITION + DESCRIPTION =================== */}
      {(product.composition || product.description) && (
        <section className="pb-12 sm:pb-16">
          <Container>
            <div className="grid gap-5 md:grid-cols-2">
              {product.composition && (
                <article className="rounded-3xl border border-border bg-card p-6 sm:p-8">
                  <div className="flex items-center gap-3">
                    <span className="flex size-11 items-center justify-center rounded-2xl bg-primary-light text-primary">
                      <FlaskConical className="size-5" />
                    </span>
                    <h2 className="text-lg font-bold text-foreground">
                      Composition
                    </h2>
                  </div>
                  <p className="mt-4 text-sm leading-7 text-muted-foreground">
                    {product.composition}
                  </p>
                </article>
              )}

              {product.description && (
                <article className="rounded-3xl border border-border bg-card p-6 sm:p-8">
                  <div className="flex items-center gap-3">
                    <span className="flex size-11 items-center justify-center rounded-2xl bg-sky-500/10 text-sky-600 dark:text-sky-400">
                      <FileText className="size-5" />
                    </span>
                    <h2 className="text-lg font-bold text-foreground">
                      Description
                    </h2>
                  </div>
                  <p className="mt-4 whitespace-pre-line text-sm leading-7 text-muted-foreground">
                    {product.description}
                  </p>
                </article>
              )}
            </div>
          </Container>
        </section>
      )}

      {/* =================== VARIANTS TABLE =================== */}
      <section className="border-y border-border bg-muted/40 py-12 sm:py-16">
        <Container>
          <div className="mb-8 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
            <div>
              <p className="text-xs font-bold uppercase tracking-[0.16em] text-primary">
                Specifications
              </p>
              <h2 className="mt-2 text-2xl font-extrabold tracking-tight text-foreground sm:text-3xl">
                Available variants
              </h2>
              <p className="mt-2 text-sm text-muted-foreground">
                Pack sizes, strengths, SKU and pricing details.
              </p>
            </div>

            {activeVariants.length > 0 && (
              <span className="inline-flex w-fit items-center gap-2 rounded-full bg-card px-4 py-2 text-xs font-bold text-primary shadow-sm ring-1 ring-border">
                <Package className="size-3.5" />
                {activeVariants.length} active variant
                {activeVariants.length !== 1 ? "s" : ""}
              </span>
            )}
          </div>

          {activeVariants.length > 0 ? (
            <div className="overflow-hidden rounded-3xl border border-border bg-card shadow-[0_10px_40px_rgba(7,63,50,0.05)]">
              <div className="hidden grid-cols-6 gap-4 border-b border-border bg-muted/60 px-6 py-4 text-[10px] font-bold uppercase tracking-[0.13em] text-muted-foreground md:grid">
                <span>Strength</span>
                <span>Pack size</span>
                <span>SKU</span>
                <span>MOQ</span>
                <span>Stock</span>
                <span>Price</span>
              </div>

              <div className="divide-y divide-border">
                {activeVariants.map((variant) => (
                  <div
                    key={variant.id}
                    className="grid grid-cols-2 gap-4 px-5 py-5 transition-colors hover:bg-primary-light/40 md:grid-cols-6 md:items-center md:gap-4 md:px-6"
                  >
                    <Cell label="Strength">
                      <p className="font-bold text-foreground">
                        {variant.strength || "Standard"}
                      </p>
                    </Cell>

                    <Cell label="Pack size">
                      <p className="text-sm text-muted-foreground">
                        {variant.packSize || "—"}
                      </p>
                    </Cell>

                    <Cell label="SKU">
                      <p className="break-all font-mono text-xs font-medium text-muted-foreground">
                        {variant.sku || "—"}
                      </p>
                    </Cell>

                    <Cell label="MOQ">
                      <p className="text-sm text-muted-foreground">
                        {variant.moq ?? "—"}
                      </p>
                    </Cell>

                    <Cell label="Stock">
                      {variant.stockQuantity > 0 ? (
                        <span className="inline-flex rounded-full bg-emerald-500/10 px-3 py-1 text-xs font-bold text-emerald-600 dark:text-emerald-400">
                          {variant.stockQuantity} available
                        </span>
                      ) : (
                        <span className="inline-flex rounded-full bg-muted px-3 py-1 text-xs font-medium text-muted-foreground">
                          Contact us
                        </span>
                      )}
                    </Cell>

                    <Cell label="Price">
                      <p className="text-lg font-extrabold text-primary">
                        {variant.price > 0
                          ? formatPrice(variant.price)
                          : "Contact"}
                      </p>
                    </Cell>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div className="rounded-3xl border border-dashed border-border bg-card p-10 text-center">
              <div className="mx-auto flex size-14 items-center justify-center rounded-2xl bg-primary-light text-primary">
                <FileText className="size-6" />
              </div>
              <p className="mt-4 font-bold text-foreground">
                Variant information unavailable
              </p>
              <p className="mx-auto mt-1 max-w-md text-sm text-muted-foreground">
                Please contact us for pack size, availability and pricing
                details.
              </p>
            </div>
          )}
        </Container>
      </section>

      {/* =================== RELATED PRODUCTS =================== */}
      {related.length > 0 && (
        <section className="py-12 sm:py-16">
          <Container>
            <div className="mb-8 flex items-end justify-between gap-4">
              <div>
                <p className="text-xs font-bold uppercase tracking-[0.16em] text-primary">
                  More from {product.categoryName}
                </p>
                <h2 className="mt-2 text-2xl font-extrabold tracking-tight text-foreground sm:text-3xl">
                  Related products
                </h2>
              </div>
              <Link
                href={categoryHref}
                className="hidden items-center gap-1.5 text-sm font-semibold text-primary hover:underline sm:inline-flex"
              >
                View all
                <ArrowRight className="size-4" />
              </Link>
            </div>

            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
              {related.map((item) => (
                <ProductCard
                  key={item.id}
                  name={item.name}
                  form={item.dosageForm ?? "Product"}
                  composition={
                    item.genericName ??
                    item.composition ??
                    "Pharmaceutical product"
                  }
                  packSize={
                    item.startingPrice !== null
                      ? `Starting from ${formatPrice(item.startingPrice)}`
                      : "Contact for details"
                  }
                  slug={item.slug}
                  imageUrl={item.primaryImageUrl}
                  categoryName={item.categoryName}
                />
              ))}
            </div>
          </Container>
        </section>
      )}

      {/* =================== QUOTE CTA =================== */}
      <section className="pb-16 sm:pb-20">
        <Container>
          <div className="relative isolate overflow-hidden rounded-[32px] bg-brand-dark px-6 py-10 text-white shadow-[0_24px_60px_rgba(7,63,50,0.25)] sm:px-12 sm:py-14">
            <div
              aria-hidden="true"
              className="pointer-events-none absolute inset-0 -z-10"
            >
              <div className="absolute -left-24 -top-24 size-80 rounded-full bg-primary/50 blur-[90px]" />
              <div className="absolute -bottom-32 right-0 size-72 rounded-full bg-sky-600/35 blur-[90px]" />
            </div>

            <div className="flex flex-col gap-8 lg:flex-row lg:items-center lg:justify-between">
              <div className="max-w-2xl">
                <span className="inline-flex rounded-full border border-white/15 bg-white/10 px-3.5 py-1.5 text-[10px] font-bold uppercase tracking-[0.16em] text-white/85 backdrop-blur">
                  Business enquiry
                </span>
                <h2 className="mt-4 text-3xl font-extrabold tracking-tight sm:text-4xl">
                  Interested in{" "}
                  <span className="bg-gradient-to-r from-teal-300 to-emerald-200 bg-clip-text text-transparent">
                    {product.name}
                  </span>
                  ?
                </h2>
                <p className="mt-3 text-sm leading-7 text-white/70 sm:text-base">
                  Contact RashePharma for availability, bulk pricing and
                  long-term supply requirements.
                </p>
              </div>

              <div className="flex shrink-0 flex-wrap gap-3">
                <Link
                  href="/contact"
                  className="inline-flex h-12 items-center gap-2 rounded-full bg-white px-7 font-bold text-brand-dark shadow-[0_14px_34px_rgba(0,0,0,0.3)] transition-all hover:-translate-y-0.5 hover:bg-emerald-50"
                >
                  Request a Quote
                  <ArrowRight className="size-4" />
                </Link>
                <Link
                  href="/products"
                  className="inline-flex h-12 items-center rounded-full border border-white/35 px-7 font-bold text-white transition-all hover:-translate-y-0.5 hover:border-white hover:bg-white/10"
                >
                  View Products
                </Link>
              </div>
            </div>
          </div>
        </Container>
      </section>
    </main>
  );
}
