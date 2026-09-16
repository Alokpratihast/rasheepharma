import Image from "next/image";
import Link from "next/link";
import { ArrowRight, Pill } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";
import { getApiAssetUrl } from "@/lib/api/client";
import { productService } from "@/services/product.service";

export async function FeaturedProducts() {
  const featuredProducts = await productService.getFeatured();

  return (
    <section className="bg-[#fafafa] py-12 sm:py-16">
      <Container>
        {/* Section heading */}
        <div className="mb-5 flex items-end justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
              Product Range
            </p>

            <h2 className="mt-1.5 text-xl font-semibold tracking-tight text-[#1B2A4A] sm:text-2xl">
              Featured Products
            </h2>

            <p className="mt-1.5 text-sm text-[#595959]">
              Selected products from our pharmaceutical portfolio.
            </p>
          </div>

          <Link
            href="/products"
            className="hidden shrink-0 text-sm font-medium text-[#F5821F] hover:text-[#df7115] sm:block"
          >
            See more →
          </Link>
        </div>

        {/* Product cards */}
        {featuredProducts.length > 0 ? (
          <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
            {featuredProducts.map((product) => (
              <Link
                key={product.id}
                href={`/products/${product.slug}`}
                className="group min-w-0"
              >
                <article className="h-full overflow-hidden rounded-lg border border-[#e8e8e8] bg-white transition-all duration-200 hover:-translate-y-0.5 hover:border-[#3E8F96]/40 hover:shadow-md">
                  {/* Product image */}
                  <div className="relative flex h-36 items-center justify-center bg-[#F2F2F2]">
                    {product.primaryImageUrl ? (
                      <Image
                        src={getApiAssetUrl(product.primaryImageUrl)!}
                        alt={product.name}
                        fill
                        className="object-contain p-4 transition-transform duration-200 group-hover:scale-105"
                      />
                    ) : (
                      <div className="flex size-16 items-center justify-center rounded-xl bg-white shadow-sm transition-transform duration-200 group-hover:scale-105">
                        <Pill
                          aria-hidden="true"
                          className="size-8 text-[#3E8F96]"
                        />
                      </div>
                    )}
                  </div>

                  {/* Product content */}
                  <div className="p-4">
                    {/* Product name */}
                    <h3 className="line-clamp-1 text-sm font-semibold text-[#1B2A4A]">
                      {product.name}
                    </h3>

                    {/* Dosage form */}
                    {product.dosageForm && (
                      <p className="mt-1 text-[10px] font-semibold uppercase tracking-[0.1em] text-[#3E8F96]">
                        {product.dosageForm}
                      </p>
                    )}

                    {/* Composition */}
                    {product.composition && (
                      <p className="mt-2 line-clamp-2 min-h-10 text-xs leading-5 text-[#595959]">
                        {product.composition}
                      </p>
                    )}

                    {/* Product information */}
                    <div className="mt-3 border-t border-dashed border-[#e2e2e2] pt-3">
                      <div className="flex items-center justify-between gap-2">
                        {product.packSize && (
                          <div>
                            <p className="text-[9px] font-medium uppercase tracking-wide text-[#999]">
                              Pack Size
                            </p>

                            <p className="mt-0.5 text-[11px] font-medium text-[#595959]">
                              {product.packSize}
                            </p>
                          </div>
                        )}

                        {product.moq !== null && (
                          <div className="text-right">
                            <p className="text-[9px] font-medium uppercase tracking-wide text-[#999]">
                              MOQ
                            </p>

                            <p className="mt-0.5 text-[11px] font-medium text-[#595959]">
                              {product.moq}
                            </p>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                </article>
              </Link>
            ))}
          </div>
        ) : (
          <div className="rounded-lg border border-dashed border-[#dcdcdc] bg-white px-6 py-10 text-center">
            <p className="text-sm text-[#595959]">
              No featured products available.
            </p>
          </div>
        )}

        {/* Mobile CTA */}
        <div className="mt-5 flex justify-center sm:hidden">
          <Link href="/products">
            <Button
              variant="outline"
              className="border-[#3E8F96]/30 text-[#3E8F96]"
            >
              See all products
              <ArrowRight className="size-4" />
            </Button>
          </Link>
        </div>
      </Container>
    </section>
  );
}