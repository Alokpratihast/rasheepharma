import Link from "next/link";
import { ArrowRight } from "lucide-react";

import { ProductCard } from "@/components/product/ProductCard";
import { Container } from "@/components/ui/container";
import { productService } from "@/services/product.service";
import type { ProductList } from "@/types/product";

const MAX_PRODUCTS = 8;

export async function FeaturedProducts() {
  let products: ProductList[] = [];

  try {
    products = await productService.getFeatured();
  } catch {
    // API unavailable: render the empty state instead of crashing the page.
    products = [];
  }

  const featured = products.slice(0, MAX_PRODUCTS);

  return (
    <section className="bg-primary-light/40 py-14 sm:py-20">
      <Container>
        {/* Heading */}
        <div className="flex items-end justify-between gap-4">
          <div className="max-w-2xl">
            <h2 className="text-3xl font-bold tracking-[-0.025em] text-brand-dark sm:text-4xl">
              Featured products
            </h2>

            <p className="mt-3 text-base leading-7 text-muted-foreground">
              Selected products from our pharmaceutical portfolio.
            </p>
          </div>

          <Link
            href="/products"
            className="group hidden h-11 shrink-0 items-center justify-center gap-2 rounded-full bg-brand-dark px-6 text-sm font-semibold text-white shadow-[0_10px_24px_rgba(7,63,50,0.22)] transition-all hover:-translate-y-0.5 hover:bg-primary sm:inline-flex"
          >
            See all products
            <ArrowRight className="size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
          </Link>
        </div>

        {/* Cards */}
        {featured.length > 0 ? (
          <div className="mt-10 grid grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-4">
            {featured.map((product) => (
              <ProductCard
                key={product.id}
                name={product.name}
                form={product.dosageForm ?? ""}
                composition={product.composition ?? ""}
                packSize={product.packSize ?? ""}
                moq={product.moq}
                slug={product.slug}
                imageUrl={product.primaryImageUrl}
              />
            ))}
          </div>
        ) : (
          <div className="mt-10 rounded-2xl border border-dashed border-border bg-white px-6 py-14 text-center">
            <p className="text-sm font-medium text-muted-foreground">
              Featured products will appear here once they&apos;re added.
            </p>

            <Link
              href="/products"
              className="mt-4 inline-flex text-sm font-semibold text-primary hover:underline"
            >
              Browse all products
            </Link>
          </div>
        )}

        {/* Mobile CTA */}
        <div className="mt-8 flex justify-center sm:hidden">
          <Link
            href="/products"
            className="inline-flex h-11 items-center justify-center gap-2 rounded-full bg-brand-dark px-6 text-sm font-semibold text-white"
          >
            See all products
            <ArrowRight className="size-4" />
          </Link>
        </div>
      </Container>
    </section>
  );
}
