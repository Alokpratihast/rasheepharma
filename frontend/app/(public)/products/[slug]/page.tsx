import Link from "next/link";
import { notFound } from "next/navigation";
import {
  ArrowLeft,
  CheckCircle2,
  Package,
  Pill,
  Building2,
  FileText,
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

  // Select primary product image.
  // If no primary image exists, use the first available image.
  const primaryImage =
    product.images.find((image) => image.isPrimary) ??
    product.images[0];

  const primaryImageUrl = getApiAssetUrl(
    primaryImage?.imageUrl
  );

  return (
    <main className="min-h-screen bg-white">
      {/* =================================================
          BREADCRUMB / BACK
      ================================================== */}

      <section className="border-b border-border bg-[#F7F8F8]">
        <Container>
          <div className="flex items-center gap-2 py-4 text-sm text-muted-foreground">
            <Link
              href="/products"
              className="inline-flex items-center gap-1 transition-colors hover:text-primary"
            >
              <ArrowLeft className="size-4" />
              Products
            </Link>

            <span>/</span>

            <Link
              href={`/products/category/${product.categoryId}`}
              className="hover:text-primary"
            >
              {product.categoryName}
            </Link>

            <span>/</span>

            <span className="truncate text-[#1B2A4A]">
              {product.name}
            </span>
          </div>
        </Container>
      </section>

      {/* =================================================
          PRODUCT HERO
      ================================================== */}

      <section className="py-10 sm:py-14">
        <Container>
          <div className="grid gap-8 lg:grid-cols-[360px_1fr]">
            {/* ================= PRODUCT IMAGE ================= */}

            <div className="relative flex min-h-[340px] items-center justify-center overflow-hidden rounded-2xl border border-border bg-[#F2F2F2]">
              {primaryImageUrl ? (
                <img
                  src={primaryImageUrl}
                  alt={
                    primaryImage?.altText ??
                    product.name
                  }
                  className="h-full w-full object-contain p-8"
                />
              ) : (
                <div className="flex flex-col items-center justify-center text-center">
                  <div className="mb-4 flex size-20 items-center justify-center rounded-2xl bg-white shadow-sm">
                    <Pill className="size-10 text-[#3E8F96]" />
                  </div>

                  <p className="text-sm font-medium text-[#1B2A4A]">
                    Product Image
                  </p>

                  <p className="mt-1 text-xs text-muted-foreground">
                    Image not available
                  </p>
                </div>
              )}
            </div>

            {/* ================= PRODUCT INFO ================= */}

            <div>
              <div className="mb-4 flex flex-wrap items-center gap-2">
                <span className="rounded-full bg-[#E8F4F4] px-3 py-1 text-xs font-medium text-[#3E8F96]">
                  {product.categoryName}
                </span>

                {product.dosageForm && (
                  <span className="rounded-full bg-[#F2F2F2] px-3 py-1 text-xs font-medium text-[#595959]">
                    {product.dosageForm}
                  </span>
                )}

                {product.isActive && (
                  <span className="inline-flex items-center gap-1 rounded-full bg-[#EAF7EF] px-3 py-1 text-xs font-medium text-green-700">
                    <CheckCircle2 className="size-3.5" />
                    Available
                  </span>
                )}
              </div>

              <h1 className="max-w-3xl text-3xl font-bold tracking-tight text-[#1B2A4A] sm:text-4xl">
                {product.name}
              </h1>

              {product.genericName && (
                <p className="mt-3 text-base text-[#595959]">
                  {product.genericName}
                </p>
              )}

              {product.composition && (
                <div className="mt-6">
                  <p className="mb-2 text-sm font-semibold text-[#1B2A4A]">
                    Composition
                  </p>

                  <p className="leading-7 text-[#595959]">
                    {product.composition}
                  </p>
                </div>
              )}

              {product.description && (
                <div className="mt-6">
                  <p className="mb-2 text-sm font-semibold text-[#1B2A4A]">
                    Description
                  </p>

                  <p className="leading-7 text-[#595959]">
                    {product.description}
                  </p>
                </div>
              )}

              {product.manufacturer && (
                <div className="mt-6 flex items-start gap-3 rounded-xl border border-border bg-[#FAFAFA] p-4">
                  <Building2 className="mt-0.5 size-5 shrink-0 text-[#3E8F96]" />

                  <div>
                    <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                      Manufacturer
                    </p>

                    <p className="mt-1 text-sm font-semibold text-[#1B2A4A]">
                      {product.manufacturer}
                    </p>
                  </div>
                </div>
              )}

              <ProductPurchaseActions
                variants={product.variants}
              />
            </div>
          </div>
        </Container>
      </section>

      {/* =================================================
          VARIANTS
      ================================================== */}

      <section className="border-t border-border bg-[#F7F8F8] py-10 sm:py-14">
        <Container>
          <div className="mb-6">
            <h2 className="text-2xl font-bold text-[#1B2A4A]">
              Available Variants
            </h2>

            <p className="mt-1 text-sm text-muted-foreground">
              Pack sizes, strengths and pricing details.
            </p>
          </div>

          {product.variants.length > 0 ? (
            <div className="overflow-hidden rounded-xl border border-border bg-white">
              <div className="hidden grid-cols-5 gap-4 border-b border-border bg-[#F2F2F2] px-5 py-4 text-xs font-semibold uppercase tracking-wide text-[#595959] md:grid">
                <span>Strength</span>
                <span>Pack Size</span>
                <span>SKU</span>
                <span>Stock</span>
                <span>Price</span>
              </div>

              <div className="divide-y divide-border">
                {product.variants
                  .filter((variant) => variant.isActive)
                  .map((variant) => (
                    <div
                      key={variant.id}
                      className="grid gap-3 px-5 py-5 md:grid-cols-5 md:items-center md:gap-4"
                    >
                      <div>
                        <p className="text-xs font-medium uppercase text-muted-foreground md:hidden">
                          Strength
                        </p>

                        <p className="font-medium text-[#1B2A4A]">
                          {variant.strength || "—"}
                        </p>
                      </div>

                      <div>
                        <p className="text-xs font-medium uppercase text-muted-foreground md:hidden">
                          Pack Size
                        </p>

                        <div className="flex items-center gap-2 text-sm text-[#595959]">
                          <Package className="size-4 text-[#3E8F96]" />
                          {variant.packSize || "—"}
                        </div>
                      </div>

                      <div>
                        <p className="text-xs font-medium uppercase text-muted-foreground md:hidden">
                          SKU
                        </p>

                        <p className="text-sm text-[#595959]">
                          {variant.sku || "—"}
                        </p>
                      </div>

                      <div>
                        <p className="text-xs font-medium uppercase text-muted-foreground md:hidden">
                          Stock
                        </p>

                        <p className="text-sm text-[#595959]">
                          {variant.stockQuantity > 0
                            ? `${variant.stockQuantity} available`
                            : "Contact for availability"}
                        </p>
                      </div>

                      <div>
                        <p className="text-xs font-medium uppercase text-muted-foreground md:hidden">
                          Price
                        </p>

                        <p className="font-semibold text-[#1B2A4A]">
                          {variant.price > 0
                            ? `₹${variant.price.toLocaleString("en-IN")}`
                            : "Contact for pricing"}
                        </p>
                      </div>
                    </div>
                  ))}
              </div>
            </div>
          ) : (
            <div className="rounded-xl border border-dashed border-border bg-white p-8 text-center">
              <FileText className="mx-auto size-8 text-muted-foreground" />

              <p className="mt-3 font-medium text-[#1B2A4A]">
                Variant information unavailable
              </p>

              <p className="mt-1 text-sm text-muted-foreground">
                Please contact us for pack size and pricing details.
              </p>
            </div>
          )}
        </Container>
      </section>

      {/* =================================================
          ENQUIRY CTA
      ================================================== */}

      <section className="py-12">
        <Container>
          <div className="rounded-2xl bg-[#1B2A4A] px-6 py-8 sm:px-10 sm:py-10">
            <div className="flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
              <div>
                <h2 className="text-2xl font-bold text-white">
                  Interested in this product?
                </h2>

                <p className="mt-2 max-w-2xl text-sm leading-6 text-white/70">
                  Contact RashePharma for product availability, pricing,
                  bulk requirements and business enquiries.
                </p>
              </div>

              <div className="flex flex-wrap gap-3">
                <Link href="/contact">
                  <Button
                    size="lg"
                    className="rounded-lg bg-[#F5821F] px-6 text-white hover:bg-[#df7115]"
                  >
                    Enquire Now
                  </Button>
                </Link>

                <Link href="/products">
                  <Button
                    size="lg"
                    variant="outline"
                    className="rounded-lg border-white/30 bg-transparent px-6 text-white hover:bg-white/10 hover:text-white"
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