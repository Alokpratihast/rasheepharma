import { ProductForm } from "@/components/admin/products/ProductForm";
import Link from "next/link";

export default function NewProductPage() {
  return (
    <main className="min-h-screen bg-[#f5f7f6] px-4 py-6 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-5xl">
        <div className="mb-6">
          <Link href="/admin/products" className="text-sm font-medium text-[#007f68] hover:underline">
            ← Back to products
          </Link>
          <div className="mt-3 flex flex-wrap items-end justify-between gap-3">
            <div>
              <h1 className="text-3xl font-bold tracking-tight text-[#1B2A4A]">
                Add Product
              </h1>

              <p className="mt-2 max-w-2xl text-sm text-slate-600">
                Add the product details and choose a category. Its URL slug is generated from the product name and can be edited.
              </p>
            </div>
            <span className="rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1.5 text-xs font-semibold text-emerald-800">New catalogue item</span>
          </div>
        </div>

        <ProductForm />
      </div>
    </main>
  );
}
