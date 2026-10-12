import { PackageSearch, X } from "lucide-react";

import { ProductCard } from "@/components/product/ProductCard";
import { cn } from "@/lib/utils";

export interface ProductGridItem {
  id: number;
  name: string;
  form: string;
  composition: string;
  packSize: string;
  slug: string;
  imageUrl?: string | null;
  categoryName?: string;
}

interface ProductGridProps {
  products: ProductGridItem[];
  view?: "grid" | "list";
  /** When provided, the empty state shows a "Clear filters" button. */
  onClear?: () => void;
}

export function ProductGrid({
  products,
  view = "grid",
  onClear,
}: ProductGridProps) {
  if (products.length === 0) {
    return (
      <div className="flex flex-col items-center rounded-3xl border border-dashed border-border bg-card px-6 py-20 text-center">
        <span className="flex size-16 items-center justify-center rounded-2xl bg-primary-light text-primary">
          <PackageSearch className="size-8" />
        </span>
        <p className="mt-5 text-lg font-bold text-foreground">
          No products found
        </p>
        <p className="mt-1.5 max-w-sm text-sm text-muted-foreground">
          We couldn&apos;t find anything matching your search or filters. Try a
          different keyword or clear the filters.
        </p>
        {onClear && (
          <button
            type="button"
            onClick={onClear}
            className="mt-6 inline-flex h-11 items-center gap-2 rounded-full bg-primary px-6 text-sm font-semibold text-primary-foreground transition-all hover:-translate-y-0.5 hover:shadow-lg"
          >
            <X className="size-4" />
            Clear filters
          </button>
        )}
      </div>
    );
  }

  return (
    <div
      className={cn(
        "grid gap-5",
        view === "list"
          ? "grid-cols-1"
          : "grid-cols-1 sm:grid-cols-2 xl:grid-cols-3",
      )}
    >
      {products.map((product, index) => (
        <div
          key={product.id}
          className="animate-in fade-in slide-in-from-bottom-3 duration-500 [animation-fill-mode:backwards]"
          style={{ animationDelay: `${Math.min(index, 8) * 45}ms` }}
        >
          <ProductCard
            name={product.name}
            form={product.form}
            composition={product.composition}
            packSize={product.packSize}
            slug={product.slug}
            imageUrl={product.imageUrl}
            categoryName={product.categoryName}
            view={view}
          />
        </div>
      ))}
    </div>
  );
}
