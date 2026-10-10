"use client";

import { useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import {
  ArrowDownAZ,
  ChevronDown,
  LayoutGrid,
  List,
  SlidersHorizontal,
  X,
} from "lucide-react";

import {
  ALL_CATEGORIES,
  ALL_FORMS,
  MobileFilterSheet,
  ProductFilters,
  type FacetOption,
} from "@/components/product/ProductFilters";
import {
  ProductGrid,
  type ProductGridItem,
} from "@/components/product/ProductGrid";
import { cn } from "@/lib/utils";
import type { ProductList } from "@/types/product";

interface ProductsPageClientProps {
  products: ProductList[];
}

interface Filters {
  search: string;
  category: string;
  form: string;
}

type SortKey =
  | "featured"
  | "name-asc"
  | "name-desc"
  | "price-asc"
  | "price-desc";

const PAGE_SIZE = 12;

const SORT_OPTIONS: { value: SortKey; label: string }[] = [
  { value: "featured", label: "Featured first" },
  { value: "name-asc", label: "Name: A to Z" },
  { value: "name-desc", label: "Name: Z to A" },
  { value: "price-asc", label: "Price: Low to High" },
  { value: "price-desc", label: "Price: High to Low" },
];

/** Does a product match the filters? `skip` ignores one filter (for facet counts). */
function matches(
  product: ProductList,
  filters: Filters,
  skip?: "category" | "form",
) {
  const q = filters.search.trim().toLowerCase();

  const matchesSearch =
    q.length === 0 ||
    product.name.toLowerCase().includes(q) ||
    (product.genericName ?? "").toLowerCase().includes(q) ||
    (product.composition ?? "").toLowerCase().includes(q) ||
    product.categoryName.toLowerCase().includes(q);

  const matchesCategory =
    skip === "category" ||
    filters.category === ALL_CATEGORIES ||
    product.categoryName.toLowerCase() === filters.category.toLowerCase();

  const matchesForm =
    skip === "form" ||
    filters.form === ALL_FORMS ||
    (product.dosageForm ?? "").toLowerCase() === filters.form.toLowerCase();

  return matchesSearch && matchesCategory && matchesForm;
}

function countBy(
  items: ProductList[],
  pick: (p: ProductList) => string | null | undefined,
): FacetOption[] {
  const map = new Map<string, number>();
  for (const item of items) {
    const key = pick(item)?.trim();
    if (!key) continue;
    map.set(key, (map.get(key) ?? 0) + 1);
  }
  return Array.from(map.entries())
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => a.name.localeCompare(b.name));
}

function sortProducts(list: ProductList[], sort: SortKey) {
  const copy = [...list];
  const price = (p: ProductList) => p.startingPrice ?? Number.POSITIVE_INFINITY;

  switch (sort) {
    case "name-asc":
      return copy.sort((a, b) => a.name.localeCompare(b.name));
    case "name-desc":
      return copy.sort((a, b) => b.name.localeCompare(a.name));
    case "price-asc":
      return copy.sort((a, b) => price(a) - price(b));
    case "price-desc":
      return copy.sort((a, b) => {
        // products without a price always go last
        if (a.startingPrice === null) return 1;
        if (b.startingPrice === null) return -1;
        return b.startingPrice - a.startingPrice;
      });
    default:
      return copy.sort(
        (a, b) =>
          Number(b.isFeatured) - Number(a.isFeatured) ||
          a.name.localeCompare(b.name),
      );
  }
}

export function ProductsPageClient({ products }: ProductsPageClientProps) {
  const searchParams = useSearchParams();

  const [filters, setFilters] = useState<Filters>(() => {
    const category = searchParams.get("category");
    const form = searchParams.get("form");
    return {
      search: searchParams.get("search") ?? "",
      category:
        category &&
        products.some(
          (p) => p.categoryName.toLowerCase() === category.toLowerCase(),
        )
          ? category
          : ALL_CATEGORIES,
      form: form ?? ALL_FORMS,
    };
  });
  const [sort, setSort] = useState<SortKey>("featured");
  const [view, setView] = useState<"grid" | "list">("grid");
  const [visible, setVisible] = useState(PAGE_SIZE);
  const [sheetOpen, setSheetOpen] = useState(false);

  /* Keep the URL shareable without triggering a navigation. */
  function update(next: Filters) {
    setFilters(next);
    setVisible(PAGE_SIZE);

    const params = new URLSearchParams();
    if (next.search.trim()) params.set("search", next.search.trim());
    if (next.category !== ALL_CATEGORIES) params.set("category", next.category);
    if (next.form !== ALL_FORMS) params.set("form", next.form);

    const qs = params.toString();
    window.history.replaceState(
      null,
      "",
      qs ? `?${qs}` : window.location.pathname,
    );
  }

  const clearFilters = () =>
    update({ search: "", category: ALL_CATEGORIES, form: ALL_FORMS });

  /* ---------- derived data ---------- */
  const categoryFacets = useMemo(
    () =>
      countBy(
        products.filter((p) => matches(p, filters, "category")),
        (p) => p.categoryName,
      ),
    [products, filters],
  );

  const formFacets = useMemo(
    () =>
      countBy(
        products.filter((p) => matches(p, filters, "form")),
        (p) => p.dosageForm,
      ),
    [products, filters],
  );

  const filtered = useMemo(
    () => sortProducts(products.filter((p) => matches(p, filters)), sort),
    [products, filters, sort],
  );

  const shown = filtered.slice(0, visible);

  const items: ProductGridItem[] = shown.map((product) => ({
    id: product.id,
    name: product.name,
    form: product.dosageForm ?? "Product",
    composition:
      product.genericName ?? product.composition ?? "Pharmaceutical product",
    packSize:
      product.startingPrice !== null
        ? `Starting from $${product.startingPrice.toLocaleString("en-US", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          })}`
        : "Contact for details",
    slug: product.slug,
    imageUrl: product.primaryImageUrl,
    categoryName: product.categoryName,
  }));

  const hasFilters =
    filters.search.trim().length > 0 ||
    filters.category !== ALL_CATEGORIES ||
    filters.form !== ALL_FORMS;

  const activeFilterCount =
    Number(filters.search.trim().length > 0) +
    Number(filters.category !== ALL_CATEGORIES) +
    Number(filters.form !== ALL_FORMS);

  const panelProps = {
    search: filters.search,
    category: filters.category,
    form: filters.form,
    categories: categoryFacets,
    forms: formFacets,
    totalCount: products.filter((p) => matches(p, filters, "category")).length,
    onSearchChange: (search: string) => update({ ...filters, search }),
    onCategoryChange: (category: string) => update({ ...filters, category }),
    onFormChange: (form: string) => update({ ...filters, form }),
    onClear: clearFilters,
  };

  const chip =
    "inline-flex items-center gap-1.5 rounded-full border border-primary/25 bg-primary-light py-1 pl-3 pr-1.5 text-xs font-semibold text-primary";

  return (
    <div className="grid gap-8 lg:grid-cols-[272px_minmax(0,1fr)] xl:grid-cols-[288px_minmax(0,1fr)]">
      {/* ============ SIDEBAR (desktop) ============ */}
      <ProductFilters {...panelProps} />

      {/* ============ MOBILE SHEET ============ */}
      <MobileFilterSheet
        {...panelProps}
        open={sheetOpen}
        onClose={() => setSheetOpen(false)}
        resultCount={filtered.length}
      />

      {/* ============ RESULTS ============ */}
      <div className="min-w-0">
        {/* Toolbar */}
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
          <p className="text-sm text-muted-foreground">
            Showing{" "}
            <span className="font-bold text-foreground">{filtered.length}</span>{" "}
            {filtered.length === 1 ? "product" : "products"}
          </p>

          <div className="flex items-center gap-2">
            {/* Mobile filters button */}
            <button
              type="button"
              onClick={() => setSheetOpen(true)}
              className="inline-flex h-10 items-center gap-2 rounded-full border border-border bg-card px-4 text-sm font-semibold text-foreground transition-colors hover:border-primary/50 lg:hidden"
            >
              <SlidersHorizontal className="size-4" />
              Filters
              {activeFilterCount > 0 && (
                <span className="flex size-5 items-center justify-center rounded-full bg-primary text-[11px] text-primary-foreground">
                  {activeFilterCount}
                </span>
              )}
            </button>

            {/* Sort */}
            <label className="relative">
              <span className="sr-only">Sort products</span>
              <ArrowDownAZ className="pointer-events-none absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <select
                value={sort}
                onChange={(e) => setSort(e.target.value as SortKey)}
                className="h-10 appearance-none rounded-full border border-border bg-card pl-10 pr-9 text-sm font-medium text-foreground outline-none transition-colors hover:border-primary/50 focus:border-primary"
              >
                {SORT_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
              <ChevronDown className="pointer-events-none absolute right-3.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            </label>

            {/* View toggle */}
            <div
              role="group"
              aria-label="Layout"
              className="hidden h-10 items-center rounded-full border border-border bg-card p-1 sm:flex"
            >
              {(
                [
                  { value: "grid", label: "Grid view", Icon: LayoutGrid },
                  { value: "list", label: "List view", Icon: List },
                ] as const
              ).map(({ value, label, Icon }) => (
                <button
                  key={value}
                  type="button"
                  aria-label={label}
                  aria-pressed={view === value}
                  onClick={() => setView(value)}
                  className={cn(
                    "flex size-8 items-center justify-center rounded-full transition-colors",
                    view === value
                      ? "bg-primary text-primary-foreground"
                      : "text-muted-foreground hover:text-foreground",
                  )}
                >
                  <Icon className="size-4" />
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Active filter chips */}
        {hasFilters && (
          <div className="mb-5 flex flex-wrap items-center gap-2">
            {filters.search.trim() && (
              <span className={chip}>
                &ldquo;{filters.search.trim()}&rdquo;
                <button
                  type="button"
                  aria-label="Remove search"
                  onClick={() => update({ ...filters, search: "" })}
                  className="flex size-5 items-center justify-center rounded-full hover:bg-primary/15"
                >
                  <X className="size-3" />
                </button>
              </span>
            )}
            {filters.category !== ALL_CATEGORIES && (
              <span className={chip}>
                {filters.category}
                <button
                  type="button"
                  aria-label="Remove category filter"
                  onClick={() => update({ ...filters, category: ALL_CATEGORIES })}
                  className="flex size-5 items-center justify-center rounded-full hover:bg-primary/15"
                >
                  <X className="size-3" />
                </button>
              </span>
            )}
            {filters.form !== ALL_FORMS && (
              <span className={chip}>
                {filters.form}
                <button
                  type="button"
                  aria-label="Remove dosage form filter"
                  onClick={() => update({ ...filters, form: ALL_FORMS })}
                  className="flex size-5 items-center justify-center rounded-full hover:bg-primary/15"
                >
                  <X className="size-3" />
                </button>
              </span>
            )}
            <button
              type="button"
              onClick={clearFilters}
              className="px-2 text-xs font-semibold text-muted-foreground underline-offset-4 hover:text-primary hover:underline"
            >
              Clear all
            </button>
          </div>
        )}

        <ProductGrid products={items} view={view} onClear={clearFilters} />

        {/* Load more */}
        {filtered.length > visible && (
          <div className="mt-10 flex flex-col items-center gap-3">
            <p className="text-xs text-muted-foreground">
              Showing {shown.length} of {filtered.length}
            </p>
            <button
              type="button"
              onClick={() => setVisible((v) => v + PAGE_SIZE)}
              className="h-12 rounded-full border border-primary/40 bg-background px-8 text-sm font-semibold text-primary transition-all hover:-translate-y-0.5 hover:bg-primary hover:text-primary-foreground hover:shadow-lg"
            >
              Show more products
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
