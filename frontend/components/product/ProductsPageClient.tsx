"use client";

import { useEffect, useMemo, useRef, useState } from "react";
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
} from "@/components/product/ProductFilters";
import {
  ProductGrid,
  type ProductGridItem,
} from "@/components/product/ProductGrid";
import { cn } from "@/lib/utils";
import { productService } from "@/services/product.service";
import type { ProductPage, ProductPageQuery, ProductList } from "@/types/product";

interface Filters {
  search: string;
  category: string;
  form: string;
}

interface ProductsPageClientProps {
  initialPage: ProductPage;
  initialFilters: Filters;
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

function toQuery(
  filters: Filters,
  sort: SortKey,
  page: number,
): ProductPageQuery {
  const [sortBy, sortOrder] = sort.split("-") as [
    "featured" | "name" | "price",
    "asc" | "desc" | undefined,
  ];

  return {
    page,
    pageSize: PAGE_SIZE,
    search: filters.search.trim() || undefined,
    category: filters.category === ALL_CATEGORIES ? undefined : filters.category,
    form: filters.form === ALL_FORMS ? undefined : filters.form,
    sortBy,
    sortOrder: sortOrder ?? "desc",
  };
}

export function ProductsPageClient({
  initialPage,
  initialFilters,
}: ProductsPageClientProps) {
  const [filters, setFilters] = useState<Filters>(initialFilters);
  const [sort, setSort] = useState<SortKey>("featured");
  const [pageData, setPageData] = useState<ProductPage>(initialPage);
  const [products, setProducts] = useState<ProductList[]>(initialPage.items);
  const [view, setView] = useState<"grid" | "list">("grid");
  const [sheetOpen, setSheetOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState("");
  const skipInitialFetch = useRef(true);

  useEffect(() => {
    if (skipInitialFetch.current) {
      skipInitialFetch.current = false;
      return;
    }

    let active = true;
    const timeout = window.setTimeout(async () => {
      setLoading(true);
      setError("");

      try {
        const result = await productService.getPaged(toQuery(filters, sort, 1));
        if (!active) return;
        setPageData(result);
        setProducts(result.items);
      } catch {
        if (active) setError("Unable to load products. Please try again.");
      } finally {
        if (active) setLoading(false);
      }
    }, filters.search.trim() ? 300 : 0);

    return () => {
      active = false;
      window.clearTimeout(timeout);
    };
  }, [filters, sort]);

  function update(next: Filters) {
    setFilters(next);

    const params = new URLSearchParams();
    if (next.search.trim()) params.set("search", next.search.trim());
    if (next.category !== ALL_CATEGORIES) params.set("category", next.category);
    if (next.form !== ALL_FORMS) params.set("form", next.form);

    const queryString = params.toString();
    window.history.replaceState(
      null,
      "",
      queryString ? `?${queryString}` : window.location.pathname,
    );
  }

  function updateSort(next: SortKey) {
    setSort(next);
  }

  const clearFilters = () =>
    update({ search: "", category: ALL_CATEGORIES, form: ALL_FORMS });

  async function loadMore() {
    if (!pageData.hasNextPage || loadingMore) return;

    setLoadingMore(true);
    setError("");

    try {
      const result = await productService.getPaged(
        toQuery(filters, sort, pageData.page + 1),
      );
      setPageData(result);
      setProducts((current) => [...current, ...result.items]);
    } catch {
      setError("Unable to load more products. Please try again.");
    } finally {
      setLoadingMore(false);
    }
  }

  const items: ProductGridItem[] = useMemo(
    () =>
      products.map((product) => ({
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
      })),
    [products],
  );

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
    categories: pageData.categoryFacets,
    forms: pageData.formFacets,
    totalCount: pageData.categoryFacetTotalCount,
    formTotalCount: pageData.formFacetTotalCount,
    onSearchChange: (search: string) => update({ ...filters, search }),
    onCategoryChange: (category: string) => update({ ...filters, category }),
    onFormChange: (form: string) => update({ ...filters, form }),
    onClear: clearFilters,
  };

  const chip =
    "inline-flex items-center gap-1.5 rounded-full border border-primary/25 bg-primary-light py-1 pl-3 pr-1.5 text-xs font-semibold text-primary";

  return (
    <div className="grid gap-8 lg:grid-cols-[272px_minmax(0,1fr)] xl:grid-cols-[288px_minmax(0,1fr)]">
      <ProductFilters {...panelProps} />

      <MobileFilterSheet
        {...panelProps}
        open={sheetOpen}
        onClose={() => setSheetOpen(false)}
        resultCount={pageData.totalCount}
      />

      <div className="min-w-0">
        <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
          <p className="text-sm text-muted-foreground" aria-live="polite">
            Showing{" "}
            <span className="font-bold text-foreground">
              {products.length}
            </span>{" "}
            of{" "}
            <span className="font-bold text-foreground">
              {pageData.totalCount}
            </span>{" "}
            products
            {loading && <span className="ml-2">Updating…</span>}
          </p>

          <div className="flex items-center gap-2">
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

            <label className="relative">
              <span className="sr-only">Sort products</span>
              <ArrowDownAZ className="pointer-events-none absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <select
                value={sort}
                onChange={(event) => updateSort(event.target.value as SortKey)}
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

        {error && (
          <p role="alert" className="mb-4 text-sm text-red-600">
            {error}
          </p>
        )}

        <ProductGrid products={items} view={view} onClear={clearFilters} />

        {pageData.hasNextPage && (
          <div className="mt-10 flex flex-col items-center gap-3">
            <p className="text-xs text-muted-foreground">
              Showing {products.length} of {pageData.totalCount}
            </p>
            <button
              type="button"
              onClick={loadMore}
              disabled={loadingMore}
              className="h-12 rounded-full border border-primary/40 bg-background px-8 text-sm font-semibold text-primary transition-all hover:-translate-y-0.5 hover:bg-primary hover:text-primary-foreground hover:shadow-lg disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loadingMore ? "Loading…" : "Show more products"}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
