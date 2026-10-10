import Link from "next/link";
import { ArrowRight, Pill } from "lucide-react";

import { getApiAssetUrl } from "@/lib/api/client";
import { cn } from "@/lib/utils";

interface ProductCardProps {
  name: string;
  form: string;
  composition: string;
  /** Price line, e.g. "Starting from ₹120" or "Contact for details". */
  packSize: string;
  slug: string;
  imageUrl?: string | null;
  categoryName?: string;
  /** "grid" (default) or horizontal "list" layout. */
  view?: "grid" | "list";
  moq?: number | null;
}

export function ProductCard({
  name,
  form,
  composition,
  packSize,
  slug,
  imageUrl,
  categoryName,
  moq,
  view = "grid",
}: ProductCardProps) {
  const productImageUrl = getApiAssetUrl(imageUrl);
  const isList = view === "list";
  const hasPrice = packSize.toLowerCase().startsWith("starting");

  return (
    <Link
      href={`/products/${slug}`}
      className="group block h-full rounded-3xl focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-primary/25"
    >
      <article
        className={cn(
          "h-full overflow-hidden rounded-3xl border border-border bg-card transition-all duration-300",
          "hover:-translate-y-1.5 hover:border-primary/40 hover:shadow-[0_24px_50px_rgba(8,127,91,0.14)]",
          isList && "flex flex-col sm:flex-row",
        )}
      >
        {/* ---------- IMAGE ---------- */}
        <div
          className={cn(
            "relative flex shrink-0 items-center justify-center overflow-hidden bg-gradient-to-br from-primary-light via-muted to-background",
            isList ? "h-48 sm:h-auto sm:w-56" : "h-52",
          )}
        >
          <div
            aria-hidden="true"
            className="absolute -right-8 -top-8 size-32 rounded-full bg-primary/10 blur-2xl transition-transform duration-500 group-hover:scale-150"
          />

          {productImageUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={productImageUrl}
              alt={name}
              loading="lazy"
              className="relative h-full w-full object-contain p-7 transition-transform duration-500 group-hover:scale-110"
            />
          ) : (
            <div className="relative flex size-20 items-center justify-center rounded-2xl bg-background shadow-md transition-transform duration-500 group-hover:-rotate-6 group-hover:scale-110">
              <Pill aria-hidden="true" className="size-9 text-primary" />
            </div>
          )}

          <span className="absolute left-3 top-3 rounded-full bg-background/90 px-3 py-1 text-[10px] font-bold uppercase tracking-wider text-primary shadow-sm backdrop-blur">
            {form}
          </span>
        </div>

        {/* ---------- DETAILS ---------- */}
        <div className="flex min-w-0 flex-1 flex-col p-5">
          {categoryName && (
            <p className="mb-1.5 truncate text-[11px] font-semibold uppercase tracking-[0.12em] text-primary">
              {categoryName}
            </p>
          )}

          <h2 className="line-clamp-2 text-base font-bold leading-snug text-foreground transition-colors group-hover:text-primary">
            {name}
          </h2>

          <p className="mt-2 line-clamp-2 text-[13px] leading-5 text-muted-foreground">
            {composition}
          </p>

          {typeof moq === "number" && moq > 0 && (
            <p className="mt-2 text-xs font-medium text-muted-foreground">
              Minimum order: {moq}
            </p>
          )}

          <div className="mt-auto flex items-end justify-between gap-3 border-t border-dashed border-border pt-4">
            <div className="min-w-0">
              <p className="text-[10px] font-semibold uppercase tracking-[0.12em] text-muted-foreground">
                Price
              </p>
              <p
                className={cn(
                  "mt-0.5 truncate text-sm",
                  hasPrice
                    ? "font-bold text-foreground"
                    : "font-medium text-muted-foreground",
                )}
              >
                {packSize}
              </p>
            </div>

            <span className="flex size-10 shrink-0 items-center justify-center rounded-full bg-primary-light text-primary transition-all duration-300 group-hover:bg-primary group-hover:text-primary-foreground">
              <ArrowRight className="size-[18px] transition-transform duration-300 group-hover:translate-x-0.5" />
            </span>
          </div>
        </div>
      </article>
    </Link>
  );
}
