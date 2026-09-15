"use client";

import { useMemo, useState } from "react";

import { ProductFilters } from "@/components/product/ProductFilters";
import {
  ProductGrid,
  type ProductGridItem,
} from "@/components/product/ProductGrid";
import type { ProductList } from "@/types/product";

interface ProductsPageClientProps {
  products: ProductList[];
}

export function ProductsPageClient({
  products,
}: ProductsPageClientProps) {
  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("All Categories");
  const [form, setForm] = useState("All Forms");

  const filteredProducts = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase();

    return products.filter((product) => {
      const matchesSearch =
        normalizedSearch.length === 0 ||
        product.name.toLowerCase().includes(normalizedSearch) ||
        (product.genericName ?? "")
          .toLowerCase()
          .includes(normalizedSearch) ||
        product.categoryName
          .toLowerCase()
          .includes(normalizedSearch);

      const matchesCategory =
        category === "All Categories" ||
        product.categoryName.toLowerCase() ===
          category.toLowerCase();

      const matchesForm =
        form === "All Forms" ||
        (product.dosageForm ?? "").toLowerCase() ===
          form.toLowerCase();

      return (
        matchesSearch &&
        matchesCategory &&
        matchesForm
      );
    });
  }, [products, search, category, form]);

  const gridProducts: ProductGridItem[] =
    filteredProducts.map((product) => ({
      id: product.id,
      name: product.name,
      form: product.dosageForm ?? "Product",
      composition:
        product.genericName ??
        "Pharmaceutical product",
      packSize:
        product.startingPrice !== null
          ? `Starting from ₹${product.startingPrice}`
          : "Contact for details",
      slug: product.slug,
    }));

  const clearFilters = () => {
    setSearch("");
    setCategory("All Categories");
    setForm("All Forms");
  };

  return (
    <>
      <ProductFilters
        search={search}
        category={category}
        form={form}
        onSearchChange={setSearch}
        onCategoryChange={setCategory}
        onFormChange={setForm}
        onClear={clearFilters}
      />

      <div className="mb-4 flex items-center justify-between gap-4">
        <p className="text-sm text-[#595959]">
          {filteredProducts.length}{" "}
          {filteredProducts.length === 1
            ? "product"
            : "products"}{" "}
          found
        </p>

        {(search ||
          category !== "All Categories" ||
          form !== "All Forms") && (
          <button
            type="button"
            onClick={clearFilters}
            className="text-xs font-medium text-[#F5821F] hover:text-[#df7115]"
          >
            Clear filters
          </button>
        )}
      </div>

      <ProductGrid products={gridProducts} />
    </>
  );
}