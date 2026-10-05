import Link from "next/link";
import { ArrowUpRight, Pill } from "lucide-react";

import { getApiAssetUrl } from "@/lib/api/client";

interface ProductCardProps {
  name: string;
  form: string;
  composition: string;
  packSize: string;
  slug: string;
  imageUrl?: string | null;
  /** Optional: shown next to pack size when provided (home page). */
  moq?: number | null;
}

export function ProductCard({
  name,
  form,
  composition,
  packSize,
  slug,
  imageUrl,
  moq,
}: ProductCardProps) {
  const productImageUrl = getApiAssetUrl(imageUrl);

  return (
    <Link
      href={`/products/${slug}`}
      className="group block h-full rounded-2xl"
    >
      <article className="flex h-full flex-col overflow-hidden rounded-2xl border border-border bg-white transition-all duration-300 group-hover:-translate-y-1 group-hover:border-primary/40 group-hover:shadow-[0_18px_44px_rgba(8,127,91,0.14)]">
        {/* Image */}
        <div className="relative flex h-44 items-center justify-center bg-gradient-to-b from-primary-light/70 to-white">
          {productImageUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={productImageUrl}
              alt={name}
              loading="lazy"
              className="h-full w-full object-contain p-6 transition-transform duration-300 group-hover:scale-105"
            />
          ) : (
            <div className="flex size-20 items-center justify-center rounded-2xl bg-white shadow-sm transition-transform duration-300 group-hover:scale-105">
              <Pill
                aria-hidden="true"
                className="size-9 text-primary"
              />
            </div>
          )}

          {form && (
            <span className="absolute left-3 top-3 rounded-full bg-white/90 px-2.5 py-1 text-[11px] font-semibold text-primary ring-1 ring-primary/15 backdrop-blur">
              {form}
            </span>
          )}
        </div>

        {/* Details */}
        <div className="flex flex-1 flex-col p-4 sm:p-5">
          <h3 className="line-clamp-2 text-[15px] font-semibold leading-5 text-brand-dark transition-colors group-hover:text-primary">
            {name}
          </h3>

          <p className="mt-2 line-clamp-2 min-h-10 text-sm leading-5 text-muted-foreground">
            {composition}
          </p>

          <div className="mt-auto pt-4">
            <div className="flex items-end justify-between gap-3 border-t border-dashed border-border pt-4">
              <dl className="flex gap-5 text-sm">
                <div>
                  <dt className="text-xs text-muted-foreground">
                    Pack size
                  </dt>
                  <dd className="mt-0.5 font-medium text-foreground">
                    {packSize || "-"}
                  </dd>
                </div>

                {moq !== null && moq !== undefined && (
                  <div>
                    <dt className="text-xs text-muted-foreground">
                      MOQ
                    </dt>
                    <dd className="mt-0.5 font-medium text-foreground">
                      {moq}
                    </dd>
                  </div>
                )}
              </dl>

              <span
                aria-hidden="true"
                className="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary-light text-primary transition-colors duration-300 group-hover:bg-primary group-hover:text-white"
              >
                <ArrowUpRight className="size-4" />
              </span>
            </div>
          </div>
        </div>
      </article>
    </Link>
  );
}
