"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { productService } from "@/services/product.service";
import type { ProductList as Product } from "@/types/product";
import { ProductTable } from "@/components/admin/products/ProductTable";
import { ProductFilters } from "@/components/admin/products/ProductFilters";

export function ProductList() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("");
  const [status, setStatus] = useState("");
  const [featured, setFeatured] = useState("");

  useEffect(() => {
    const loadProducts = async () => {
      try {
        setLoading(true);
        setError(null);

        const data = await productService.getAll();

        setProducts(data);
      } catch (error) {
        console.error("Failed to load products:", error);

        setError("Failed to load products.");
      } finally {
        setLoading(false);
      }
    };

    loadProducts();
  }, []);

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
    <main className="min-h-screen bg-[#f5f7f6] p-6">
      <div className="mx-auto max-w-7xl">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold text-[#1B2A4A]">
              Products
            </h1>

            <p className="mt-2 text-sm text-[#595959]">
              Manage your pharmaceutical products.
            </p>
          </div>

          <Link
            href="/admin/products/new"
            className="rounded-lg bg-[#1B2A4A] px-4 py-2.5 text-sm font-medium text-white hover:bg-[#142039]"
          >
            Add Product
          </Link>
        </div>

        {/* Loading */}
        {loading && (
          <div className="mt-8 rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-gray-500">
              Loading products...
            </p>
          </div>
        )}

        {/* Error */}
        {error && (
          <div className="mt-8 rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-red-600">
              {error}
            </p>
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

            <div className="mt-4 flex items-center justify-between">
              <p className="text-sm text-gray-600">
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
              <ProductTable
                products={filteredProducts}
              />
            </div>
          </>
        )}
      </div>
    </main>
  );
}