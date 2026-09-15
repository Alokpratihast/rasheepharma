import { ProductsPageClient } from "@/components/product/ProductsPageClient";
import { productService } from "@/services/product.service";
import type { ProductList } from "@/types/product";

export default async function ProductsPage() {
  let products: ProductList[] = [];
  let errorMessage = "";

  try {
    products = await productService.getAll();
  } catch {
    errorMessage =
      "Unable to load products right now. Please try again.";
  }

  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-14">
      <div className="mx-auto w-full max-w-7xl px-5 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
            Product Catalogue
          </p>

          <h1 className="mt-2 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
            All Products
          </h1>

          <p className="mt-2 max-w-2xl text-sm leading-6 text-[#595959]">
            Browse our pharmaceutical portfolio by product, category and
            dosage form.
          </p>
        </div>

        {errorMessage ? (
          <div className="rounded-xl border border-dashed border-[#d9d9d9] bg-white py-16 text-center">
            <p className="text-sm font-medium text-[#1B2A4A]">
              {errorMessage}
            </p>
          </div>
        ) : (
          <ProductsPageClient products={products} />
        )}
      </div>
    </main>
  );
}