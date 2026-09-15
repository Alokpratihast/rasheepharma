import Link from "next/link";
import { ArrowLeft, PackageOpen } from "lucide-react";
import { notFound } from "next/navigation";

import { Container } from "@/components/ui/container";
import { ProductGrid } from "@/components/product/ProductGrid";
import { categoryService } from "@/services/category.service";
import { productService } from "@/services/product.service";
import type { ProductList } from "@/types/product";

interface CategoryProductsPageProps {
  params: Promise<{
    slug: string;
  }>;
}

export default async function CategoryProductsPage({
  params,
}: CategoryProductsPageProps) {
  const { slug } = await params;

  /* =================================================
     GET CATEGORY
  ================================================== */

  let category;

  try {
    category = await categoryService.getBySlug(slug);
  } catch {
    notFound();
  }

  if (!category || !category.isActive) {
    notFound();
  }

  /* =================================================
     GET PRODUCTS
  ================================================== */

  let products: ProductList[] = [];

  try {
    products = await productService.getAll();
  } catch {
    products = [];
  }

  /* =================================================
     FILTER PRODUCTS BY CATEGORY
     
     ProductList currently contains categoryName,
     so we match it with the DB category name.
  ================================================== */

  const categoryProducts = products.filter(
    (product) =>
      product.isActive &&
      product.categoryName.toLowerCase() === category.name.toLowerCase()
  );

  /* =================================================
     MAP PRODUCTS FOR PRODUCT GRID
  ================================================== */

  const gridItems = categoryProducts.map((product) => ({
    id: product.id,
    name: product.name,
    form: product.dosageForm || "Product",
    composition: product.genericName || "Pharmaceutical product",
    packSize:
      product.startingPrice !== null
        ? `Starting from ₹${product.startingPrice.toLocaleString("en-IN")}`
        : "Contact for details",
    slug: product.slug,
  }));

  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-14">
      <Container>
        {/* =================================================
            BREADCRUMB
        ================================================== */}

        <div className="mb-6 flex items-center gap-2 text-sm text-muted-foreground">
          <Link
            href="/categories"
            className="inline-flex items-center gap-1 transition-colors hover:text-primary"
          >
            <ArrowLeft className="size-4" />
            Categories
          </Link>

          <span>/</span>

          <span className="text-[#1B2A4A]">
            {category.name}
          </span>
        </div>

        {/* =================================================
            PAGE HEADER
        ================================================== */}

        <div className="max-w-3xl">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
            Product Catalogue
          </p>

          <h1 className="mt-2 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
            {category.name}
          </h1>

          <p className="mt-3 text-sm leading-6 text-[#595959] sm:text-base">
            Explore pharmaceutical products available in the{" "}
            {category.name} category.
          </p>
        </div>

        {/* =================================================
            RESULT COUNT
        ================================================== */}

        <div className="mt-8 flex items-center justify-between border-b border-border pb-4">
          <p className="text-sm text-muted-foreground">
            <span className="font-semibold text-[#1B2A4A]">
              {categoryProducts.length}
            </span>{" "}
            {categoryProducts.length === 1 ? "product" : "products"}
          </p>

          <Link
            href="/products"
            className="text-sm font-medium text-primary hover:underline"
          >
            View all products
          </Link>
        </div>

        {/* =================================================
            PRODUCTS
        ================================================== */}

        {gridItems.length > 0 ? (
          <div className="mt-8">
            <ProductGrid products={gridItems} />
          </div>
        ) : (
          <div className="mt-8 rounded-xl border border-dashed border-border bg-white p-12 text-center">
            <PackageOpen className="mx-auto size-10 text-muted-foreground" />

            <h2 className="mt-4 text-lg font-semibold text-[#1B2A4A]">
              No products found
            </h2>

            <p className="mx-auto mt-2 max-w-md text-sm leading-6 text-muted-foreground">
              There are currently no active products available in the{" "}
              {category.name} category.
            </p>

            <Link
              href="/products"
              className="mt-5 inline-block text-sm font-medium text-primary hover:underline"
            >
              Browse all products
            </Link>
          </div>
        )}
      </Container>
    </main>
  );
}