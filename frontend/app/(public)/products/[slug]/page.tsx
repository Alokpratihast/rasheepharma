

import Link from "next/link";
import { notFound } from "next/navigation";
import {
  ArrowLeft,
  CheckCircle2,
  Package,
  Pill,
  Building2,
  FileText,
  ShieldCheck,
  Boxes,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { Button } from "@/components/ui/button";
import { getApiAssetUrl } from "@/lib/api/client";
import { productService } from "@/services/product.service";
import { ProductPurchaseActions } from "@/components/product/ProductPurchaseActions";

interface ProductDetailPageProps {
  params: Promise<{
    slug: string;
  }>;
}

export async function generateMetadata({
  params,
}: ProductDetailPageProps) {
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

  const primaryImage =
    product.images.find((image) => image.isPrimary) ??
    product.images[0];

  const primaryImageUrl = getApiAssetUrl(
    primaryImage?.imageUrl
  );

  const activeVariants = product.variants.filter(
    (variant) => variant.isActive
  );

  const availableVariants = activeVariants.filter(
    (variant) => variant.stockQuantity > 0
  );

  const lowestPrice =
    availableVariants.length > 0
      ? Math.min(
          ...availableVariants
            .filter((variant) => variant.price > 0)
            .map((variant) => variant.price)
        )
      : null;

  return (
    <main className="min-h-screen bg-white">
      {/* =================================================
          BREADCRUMB
      ================================================== */}

      <section className="border-b border-[#E9EEED] bg-[#F8FAFA]">
        <Container>
          <div className="flex min-h-14 items-center gap-2 overflow-hidden text-sm">
            <Link
              href="/products"
              className="inline-flex shrink-0 items-center gap-1.5 font-medium text-[#66706E] transition-colors hover:text-[#1B2A4A]"
            >
              <ArrowLeft className="size-4" />
              Products
            </Link>

            <span className="text-[#B4BCBA]">/</span>

            <Link
              href={`/products/category/${product.categoryId}`}
              className="truncate text-[#66706E] transition-colors hover:text-[#1B2A4A]"
            >
              {product.categoryName}
            </Link>

            <span className="text-[#B4BCBA]">/</span>

            <span className="truncate font-medium text-[#1B2A4A]">
              {product.name}
            </span>
          </div>
        </Container>
      </section>

      {/* =================================================
          PRODUCT HERO
      ================================================== */}

      <section className="bg-white py-8 sm:py-12 lg:py-14">
        <Container>
          <div className="grid gap-8 lg:grid-cols-[minmax(340px,0.85fr)_minmax(0,1.15fr)] lg:items-start lg:gap-12">
            {/* =================================================
                PRODUCT IMAGE
            ================================================== */}

            <div className="lg:sticky lg:top-24">
              <div className="relative overflow-hidden rounded-[28px] border border-[#E4E9E8] bg-[#F5F7F7] shadow-[0_12px_40px_rgba(27,42,74,0.06)]">
                {/* Image background decoration */}
                <div className="pointer-events-none absolute -right-20 -top-20 size-52 rounded-full bg-[#E8F4F4]" />

                <div className="pointer-events-none absolute -bottom-24 -left-20 size-56 rounded-full bg-[#EEF7F5]" />

                <div className="relative flex min-h-[400px] items-center justify-center p-8 sm:min-h-[480px] sm:p-12">
                  {primaryImageUrl ? (
                    <img
                      src={primaryImageUrl}
                      alt={
                        primaryImage?.altText ??
                        product.name
                      }
                      className="relative z-10 max-h-[440px] w-full object-contain drop-shadow-[0_18px_25px_rgba(27,42,74,0.10)]"
                    />
                  ) : (
                    <div className="relative z-10 flex flex-col items-center justify-center text-center">
                      <div className="flex size-24 items-center justify-center rounded-3xl bg-white shadow-sm">
                        <Pill className="size-11 text-[#3E8F96]" />
                      </div>

                      <p className="mt-5 text-sm font-semibold text-[#1B2A4A]">
                        Product Image
                      </p>

                      <p className="mt-1 text-xs text-[#7B8582]">
                        Image not available
                      </p>
                    </div>
                  )}
                </div>

                {/* Bottom image info */}
                <div className="relative z-10 border-t border-[#E3E8E7] bg-white/80 px-5 py-4 backdrop-blur-sm sm:px-6">
                  <div className="flex items-center justify-between gap-4">
                    <div>
                      <p className="text-[10px] font-semibold uppercase tracking-[0.14em] text-[#8A9391]">
                        Rashe Pharma
                      </p>

                      <p className="mt-0.5 text-xs font-medium text-[#1B2A4A]">
                        Pharmaceutical Product
                      </p>
                    </div>

                    <div className="flex size-9 items-center justify-center rounded-full bg-[#E8F4F4]">
                      <ShieldCheck className="size-4.5 text-[#3E8F96]" />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* =================================================
                PRODUCT INFORMATION
            ================================================== */}

            <div className="min-w-0">
              {/* Badges */}
              <div className="flex flex-wrap items-center gap-2">
                <span className="rounded-full bg-[#E8F4F4] px-3.5 py-1.5 text-xs font-semibold text-[#317C82]">
                  {product.categoryName}
                </span>

                {product.dosageForm && (
                  <span className="rounded-full border border-[#E2E6E5] bg-white px-3.5 py-1.5 text-xs font-medium text-[#626B69]">
                    {product.dosageForm}
                  </span>
                )}

                {product.isActive && (
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-[#EAF7EF] px-3.5 py-1.5 text-xs font-semibold text-[#23814A]">
                    <CheckCircle2 className="size-3.5" />
                    Available
                  </span>
                )}
              </div>

              {/* Title */}
              <h1 className="mt-5 max-w-3xl text-3xl font-bold leading-tight tracking-[-0.02em] text-[#1B2A4A] sm:text-4xl lg:text-[42px]">
                {product.name}
              </h1>

              {/* Generic */}
              {product.genericName && (
                <p className="mt-3 text-base font-medium text-[#6C7674] sm:text-lg">
                  {product.genericName}
                </p>
              )}

              {/* Short product stats */}
              <div className="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-3">
                <div className="rounded-2xl border border-[#E6EBEA] bg-[#FAFBFB] p-4">
                  <div className="mb-2 flex size-9 items-center justify-center rounded-xl bg-[#E8F4F4]">
                    <Package className="size-4.5 text-[#3E8F96]" />
                  </div>

                  <p className="text-[10px] font-semibold uppercase tracking-[0.12em] text-[#8A9391]">
                    Variants
                  </p>

                  <p className="mt-1 text-sm font-bold text-[#1B2A4A]">
                    {activeVariants.length}
                  </p>
                </div>

                <div className="rounded-2xl border border-[#E6EBEA] bg-[#FAFBFB] p-4">
                  <div className="mb-2 flex size-9 items-center justify-center rounded-xl bg-[#FFF2E8]">
                    <Boxes className="size-4.5 text-[#F5821F]" />
                  </div>

                  <p className="text-[10px] font-semibold uppercase tracking-[0.12em] text-[#8A9391]">
                    Stock
                  </p>

                  <p className="mt-1 text-sm font-bold text-[#1B2A4A]">
                    {availableVariants.length > 0
                      ? "Available"
                      : "Contact us"}
                  </p>
                </div>

                <div className="col-span-2 rounded-2xl border border-[#E6EBEA] bg-[#FAFBFB] p-4 sm:col-span-1">
                  <div className="mb-2 flex size-9 items-center justify-center rounded-xl bg-[#E8F4F4]">
                    <Pill className="size-4.5 text-[#3E8F96]" />
                  </div>

                  <p className="text-[10px] font-semibold uppercase tracking-[0.12em] text-[#8A9391]">
                    Starting price
                  </p>

                  <p className="mt-1 text-sm font-bold text-[#F5821F]">
                    {lowestPrice !== null
                      ? `₹${lowestPrice.toLocaleString("en-IN")}`
                      : "Contact"}
                  </p>
                </div>
              </div>

              {/* Composition */}
              {product.composition && (
                <div className="mt-7">
                  <div className="mb-2.5 flex items-center gap-2">
                    <div className="h-5 w-1 rounded-full bg-[#3E8F96]" />

                    <p className="text-sm font-bold text-[#1B2A4A]">
                      Composition
                    </p>
                  </div>

                  <div className="rounded-2xl border border-[#E5EAE9] bg-[#F8FAFA] px-5 py-4">
                    <p className="text-sm leading-7 text-[#596360]">
                      {product.composition}
                    </p>
                  </div>
                </div>
              )}

              {/* Description */}
              {product.description && (
                <div className="mt-6">
                  <div className="mb-2.5 flex items-center gap-2">
                    <div className="h-5 w-1 rounded-full bg-[#F5821F]" />

                    <p className="text-sm font-bold text-[#1B2A4A]">
                      Description
                    </p>
                  </div>

                  <p className="max-w-3xl text-sm leading-7 text-[#626B69]">
                    {product.description}
                  </p>
                </div>
              )}

              {/* Manufacturer */}
              {product.manufacturer && (
                <div className="mt-7 flex items-center gap-4 rounded-2xl border border-[#DDE8E6] bg-[#F7FBFA] p-5">
                  <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-white shadow-sm">
                    <Building2 className="size-5.5 text-[#3E8F96]" />
                  </div>

                  <div className="min-w-0">
                    <p className="text-[10px] font-semibold uppercase tracking-[0.14em] text-[#899391]">
                      Manufacturer
                    </p>

                    <p className="mt-1 truncate text-base font-bold text-[#1B2A4A]">
                      {product.manufacturer}
                    </p>
                  </div>
                </div>
              )}

              {/* Purchase Card */}
              <div className="mt-7">
                <ProductPurchaseActions
                  variants={product.variants}
                />
              </div>
            </div>
          </div>
        </Container>
      </section>

      {/* =================================================
          AVAILABLE VARIANTS
      ================================================== */}

      <section className="border-y border-[#E6EBEA] bg-[#F7F9F9] py-12 sm:py-16">
        <Container>
          <div className="mb-7 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
            <div>
              <div className="flex items-center gap-2">
                <div className="h-6 w-1 rounded-full bg-[#3E8F96]" />

                <h2 className="text-2xl font-bold tracking-tight text-[#1B2A4A] sm:text-3xl">
                  Available Variants
                </h2>
              </div>

              <p className="mt-2 text-sm text-[#707A77]">
                Pack sizes, strengths, SKU and pricing details.
              </p>
            </div>

            {activeVariants.length > 0 && (
              <span className="inline-flex w-fit items-center gap-2 rounded-full bg-white px-4 py-2 text-xs font-semibold text-[#3E8F96] shadow-sm ring-1 ring-[#E0E7E5]">
                <Package className="size-3.5" />
                {activeVariants.length} active variant
                {activeVariants.length !== 1 ? "s" : ""}
              </span>
            )}
          </div>

          {product.variants.length > 0 ? (
            <div className="overflow-hidden rounded-[22px] border border-[#E2E8E6] bg-white shadow-[0_8px_30px_rgba(27,42,74,0.04)]">
              {/* Desktop Header */}
              <div className="hidden grid-cols-5 gap-4 border-b border-[#E7EBEA] bg-[#F9FAFA] px-6 py-4 text-[10px] font-bold uppercase tracking-[0.13em] text-[#818A87] md:grid">
                <span>Strength</span>
                <span>Pack Size</span>
                <span>SKU</span>
                <span>Stock</span>
                <span>Price</span>
              </div>

              <div className="divide-y divide-[#E9EDEC]">
                {product.variants
                  .filter((variant) => variant.isActive)
                  .map((variant) => (
                    <div
                      key={variant.id}
                      className="grid gap-4 px-5 py-5 transition-colors hover:bg-[#FBFCFC] md:grid-cols-5 md:items-center md:gap-5 md:px-6"
                    >
                      {/* Strength */}
                      <div>
                        <p className="mb-1 text-[10px] font-semibold uppercase tracking-wide text-[#909895] md:hidden">
                          Strength
                        </p>

                        <p className="font-semibold text-[#1B2A4A]">
                          {variant.strength || "Standard"}
                        </p>
                      </div>

                      {/* Pack */}
                      <div>
                        <p className="mb-1 text-[10px] font-semibold uppercase tracking-wide text-[#909895] md:hidden">
                          Pack Size
                        </p>

                        <div className="flex items-center gap-2 text-sm text-[#596360]">
                          <span className="flex size-7 items-center justify-center rounded-lg bg-[#E8F4F4]">
                            <Package className="size-3.5 text-[#3E8F96]" />
                          </span>

                          {variant.packSize || "—"}
                        </div>
                      </div>

                      {/* SKU */}
                      <div>
                        <p className="mb-1 text-[10px] font-semibold uppercase tracking-wide text-[#909895] md:hidden">
                          SKU
                        </p>

                        <p className="font-mono text-xs font-medium text-[#596360]">
                          {variant.sku || "—"}
                        </p>
                      </div>

                      {/* Stock */}
                      <div>
                        <p className="mb-1 text-[10px] font-semibold uppercase tracking-wide text-[#909895] md:hidden">
                          Stock
                        </p>

                        {variant.stockQuantity > 0 ? (
                          <span className="inline-flex rounded-full bg-[#EAF7EF] px-3 py-1.5 text-xs font-semibold text-[#23814A]">
                            {variant.stockQuantity} available
                          </span>
                        ) : (
                          <span className="inline-flex rounded-full bg-[#F3F4F4] px-3 py-1.5 text-xs font-medium text-[#777F7D]">
                            Contact for availability
                          </span>
                        )}
                      </div>

                      {/* Price */}
                      <div>
                        <p className="mb-1 text-[10px] font-semibold uppercase tracking-wide text-[#909895] md:hidden">
                          Price
                        </p>

                        <p className="text-lg font-bold text-[#F5821F]">
                          {variant.price > 0
                            ? `₹${variant.price.toLocaleString(
                                "en-IN"
                              )}`
                            : "Contact"}
                        </p>
                      </div>
                    </div>
                  ))}
              </div>
            </div>
          ) : (
            <div className="rounded-[22px] border border-dashed border-[#D8DFDD] bg-white p-10 text-center">
              <div className="mx-auto flex size-14 items-center justify-center rounded-2xl bg-[#E8F4F4]">
                <FileText className="size-6 text-[#3E8F96]" />
              </div>

              <p className="mt-4 font-semibold text-[#1B2A4A]">
                Variant information unavailable
              </p>

              <p className="mx-auto mt-1 max-w-md text-sm text-[#737D7A]">
                Please contact us for pack size, availability and
                pricing details.
              </p>
            </div>
          )}
        </Container>
      </section>

      {/* =================================================
          ENQUIRY CTA
      ================================================== */}

      <section className="bg-white py-12 sm:py-16">
        <Container>
          <div className="relative overflow-hidden rounded-[28px] bg-[#1B2A4A] px-6 py-9 shadow-[0_18px_50px_rgba(27,42,74,0.14)] sm:px-10 sm:py-11">
            {/* Decorative circles */}
            <div className="pointer-events-none absolute -right-20 -top-28 size-72 rounded-full border border-white/10" />

            <div className="pointer-events-none absolute -bottom-32 right-24 size-64 rounded-full border border-white/5" />

            <div className="relative z-10 flex flex-col gap-7 lg:flex-row lg:items-center lg:justify-between">
              <div className="max-w-2xl">
                <span className="inline-flex rounded-full bg-white/10 px-3 py-1.5 text-[10px] font-semibold uppercase tracking-[0.14em] text-white/80">
                  Business Enquiry
                </span>

                <h2 className="mt-4 text-2xl font-bold tracking-tight text-white sm:text-3xl">
                  Interested in this product?
                </h2>

                <p className="mt-2 max-w-xl text-sm leading-6 text-white/65 sm:text-base">
                  Contact RashePharma for product availability,
                  pricing, bulk requirements and business enquiries.
                </p>
              </div>

              <div className="flex shrink-0 flex-wrap gap-3">
                <Link href="/contact">
                  <Button
                    size="lg"
                    className="h-12 rounded-xl bg-[#F5821F] px-6 font-semibold text-white shadow-lg shadow-black/10 hover:bg-[#DF7115]"
                  >
                    Enquire Now
                  </Button>
                </Link>

                <Link href="/products">
                  <Button
                    size="lg"
                    variant="outline"
                    className="h-12 rounded-xl border-white/20 bg-white/5 px-6 font-semibold text-white hover:bg-white/10 hover:text-white"
                  >
                    View Products
                  </Button>
                </Link>
              </div>
            </div>
          </div>
        </Container>
      </section>
    </main>
  );
}

