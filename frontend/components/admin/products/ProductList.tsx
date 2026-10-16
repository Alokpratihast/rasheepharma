"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { productService } from "@/services/product.service";
import type { ProductList as Product } from "@/types/product";
import { ProductTable } from "@/components/admin/products/ProductTable";
import { ProductFilters } from "@/components/admin/products/ProductFilters";
import { getAdminErrorMessage } from "@/lib/admin-error-message";
import { LoaderCircle, RefreshCw, SearchX } from "lucide-react";

export function ProductList() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("");
  const [status, setStatus] = useState("");
  const [featured, setFeatured] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    let active = true;

    const loadProducts = async () => {
      try {
        const data = await productService.getAll();

        if (active) {
          setProducts(data);
          setError(null);
        }
      } catch (loadError) {
        console.error("Failed to load products:", loadError);

        if (active) {
          setError(getAdminErrorMessage(loadError, "We could not load the product catalogue. Check your connection and try again."));
        }
      } finally {
        if (active) setLoading(false);
      }
    };

    setLoading(true);
    setError(null);
    void loadProducts();
    return () => { active = false; };
  }, [reloadKey]);

  const categories = useMemo(() => {
    return Array.from(
      new Set(
        products
          .map((product) => product.categoryName)
          .filter(Boolean),
      ),
    ).sort();
  }, [products]);

  const filteredProducts = useMemo(() => {
    const searchValue = search.trim().toLowerCase();

    return products.filter((product) => {
      const matchesSearch =
        !searchValue ||
        product.name.toLowerCase().includes(searchValue) ||
        product.slug.toLowerCase().includes(searchValue) ||
        product.genericName
          ?.toLowerCase()
          .includes(searchValue) ||
        product.composition
          ?.toLowerCase()
          .includes(searchValue);

      const matchesCategory =
        !category ||
        product.categoryName === category;

      const matchesStatus =
        !status ||
        (status === "active" && product.isActive) ||
        (status === "inactive" && !product.isActive);

      const matchesFeatured =
        !featured ||
        (featured === "featured" && product.isFeatured) ||
        (featured === "not-featured" && !product.isFeatured);

      return (
        matchesSearch &&
        matchesCategory &&
        matchesStatus &&
        matchesFeatured
      );
    });
  }, [
    products,
    search,
    category,
    status,
    featured,
  ]);

  return (
    <main className="min-h-screen bg-[#f5f7f6] px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-7xl">
        {/* Header */}
        <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
          <div>
            <h1 className="text-3xl font-semibold text-[#1B2A4A]">
              Products
            </h1>

            <p className="mt-2 text-sm text-[#595959]">
              Manage your pharmaceutical products.
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              onClick={() => setReloadKey((value) => value + 1)}
              disabled={loading}
              className="inline-flex min-h-10 items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:cursor-wait disabled:opacity-60"
            >
              <RefreshCw aria-hidden="true" className={`size-4 ${loading ? "animate-spin" : ""}`} />
              {loading ? "Loading…" : "Refresh"}
            </button>
            <Link
              href="/admin/products/new"
              className="inline-flex min-h-10 items-center justify-center rounded-lg bg-[#1B2A4A] px-4 py-2.5 text-sm font-medium text-white transition hover:bg-[#142039]"
            >
              Add product
            </Link>
          </div>
        </div>

        {/* Loading summary */}
        {loading && (
          <div role="status" aria-label="Loading product catalogue" className="mt-6 grid gap-3 sm:grid-cols-3">
            {Array.from({ length: 3 }, (_, index) => (
              <div key={index} className="h-[74px] animate-pulse rounded-xl border border-slate-200 bg-white" />
            ))}
            <span className="sr-only">Loading product catalogue…</span>
          </div>
        )}

        {!loading && !error && (
          <div className="mt-6 grid gap-3 sm:grid-cols-3">
            <div className="rounded-xl border border-slate-200 bg-white px-4 py-3"><p className="text-xs font-medium uppercase tracking-wide text-slate-500">All products</p><p className="mt-1 text-2xl font-bold text-[#1B2A4A]">{products.length}</p></div>
            <div className="rounded-xl border border-slate-200 bg-white px-4 py-3"><p className="text-xs font-medium uppercase tracking-wide text-slate-500">Active</p><p className="mt-1 text-2xl font-bold text-emerald-700">{products.filter((product) => product.isActive).length}</p></div>
            <div className="rounded-xl border border-slate-200 bg-white px-4 py-3"><p className="text-xs font-medium uppercase tracking-wide text-slate-500">Featured</p><p className="mt-1 text-2xl font-bold text-[#1B2A4A]">{products.filter((product) => product.isFeatured).length}</p></div>
          </div>
        )}

        {loading && (
          <div aria-hidden="true" className="mt-8 space-y-4">
            <div className="h-24 animate-pulse rounded-2xl border border-slate-200 bg-white" />
            <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
              <div className="h-12 animate-pulse bg-slate-100" />
              {Array.from({ length: 6 }, (_, index) => (
                <div key={index} className="flex h-[76px] items-center gap-6 border-t border-slate-100 px-6">
                  <div className="h-4 w-1/4 animate-pulse rounded bg-slate-100" />
                  <div className="h-4 w-1/6 animate-pulse rounded bg-slate-100" />
                  <div className="h-4 w-1/5 animate-pulse rounded bg-slate-100" />
                  <div className="h-4 w-16 animate-pulse rounded-full bg-slate-100" />
                </div>
              ))}
            </div>
            <p role="status" className="flex items-center justify-center gap-2 text-sm text-slate-500">
              <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
              Loading products…
            </p>
          </div>
        )}

        {/* Error */}
        {error && (
          <div role="alert" className="mt-8 rounded-2xl border border-red-200 bg-white p-6 shadow-sm">
            <p className="font-semibold text-red-900">Products could not be loaded</p>
            <p className="mt-2 text-sm leading-6 text-slate-700">{error}</p>
            <button type="button" onClick={() => setReloadKey((value) => value + 1)} className="mt-4 inline-flex min-h-10 items-center gap-2 rounded-lg bg-[#1B2A4A] px-4 py-2 text-sm font-semibold text-white transition hover:bg-[#142039]">
              <RefreshCw aria-hidden="true" className="size-4" /> Try again
            </button>
          </div>
        )}

        {/* Products */}
        {!loading && !error && (
          <>
            <ProductFilters
              search={search}
              category={category}
              status={status}
              featured={featured}
              categories={categories}
              onSearchChange={setSearch}
              onCategoryChange={setCategory}
              onStatusChange={setStatus}
              onFeaturedChange={setFeatured}
            />

            <div className="mt-4 flex flex-wrap items-center justify-between gap-3">
              <p aria-live="polite" className="text-sm text-gray-600">
                Showing{" "}
                <span className="font-semibold text-[#1B2A4A]">
                  {filteredProducts.length}
                </span>{" "}
                of{" "}
                <span className="font-semibold text-[#1B2A4A]">
                  {products.length}
                </span>{" "}
                products
              </p>
            </div>

            <div className="mt-4">
              {filteredProducts.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-14 text-center shadow-sm">
                  <SearchX aria-hidden="true" className="mx-auto size-9 text-slate-400" />
                  <h2 className="mt-3 font-semibold text-[#1B2A4A]">{products.length === 0 ? "No products yet" : "No products match these filters"}</h2>
                  <p className="mx-auto mt-1 max-w-md text-sm text-slate-500">{products.length === 0 ? "Add your first product to start building the catalogue." : "Try another search or clear the filters to see more products."}</p>
                  {products.length === 0 ? (
                    <Link href="/admin/products/new" className="mt-4 inline-flex rounded-lg bg-[#1B2A4A] px-4 py-2.5 text-sm font-semibold text-white hover:bg-[#142039]">Add your first product</Link>
                  ) : (
                    <button type="button" onClick={() => { setSearch(""); setCategory(""); setStatus(""); setFeatured(""); }} className="mt-4 text-sm font-semibold text-[#007f68] hover:underline">Clear filters</button>
                  )}
                </div>
              ) : (
              <ProductTable
                products={filteredProducts}
              />
              )}
            </div>
          </>
        )}
      </div>
    </main>
  );
}
