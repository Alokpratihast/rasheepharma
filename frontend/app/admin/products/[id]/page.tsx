"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { productService } from "@/services/product.service";
import type { ProductDetails } from "@/types/product";
import { ProductForm } from "@/components/admin/products/ProductForm";
import { ProductVariantList } from "@/components/admin/products/ProductVariantList";
import { ProductImageList } from "@/components/admin/products/ProductImageList";

export default function EditProductPage() {
  const params = useParams();
  const router = useRouter();

  const productId = Number(params.id);

  const [product, setProduct] =
    useState<ProductDetails | null>(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadProduct = async () => {
      if (!productId || Number.isNaN(productId)) {
        setError("Invalid product ID.");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const data =
          await productService.getById(productId);

        setProduct(data);
      } catch (error) {
        console.error(
          "Failed to load product:",
          error,
        );

        setError("Failed to load product.");
      } finally {
        setLoading(false);
      }
    };

    loadProduct();
  }, [productId]);

  if (loading) {
    return (
      <main className="min-h-screen bg-[#f5f7f6] p-6">
        <div className="mx-auto max-w-5xl">
          <div className="rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-gray-500">
              Loading product...
            </p>
          </div>
        </div>
      </main>
    );
  }

  if (error || !product) {
    return (
      <main className="min-h-screen bg-[#f5f7f6] p-6">
        <div className="mx-auto max-w-5xl">
          <div className="rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-red-600">
              {error ?? "Product not found."}
            </p>

            <button
              type="button"
              onClick={() => router.push("/admin/products")}
              className="mt-4 rounded-lg bg-[#1B2A4A] px-4 py-2 text-sm font-medium text-white"
            >
              Back to Products
            </button>
          </div>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-[#f5f7f6] p-6">
      <div className="mx-auto max-w-5xl">
        <div className="mb-8">
          <h1 className="text-3xl font-semibold text-[#1B2A4A]">
            Edit Product
          </h1>

          <p className="mt-2 text-sm text-[#595959]">
            Update product information and manage its variants.
          </p>
        </div>

        <ProductForm
          product={product}
          onSuccess={(updatedProduct) => {
            setProduct(updatedProduct);
          }}
        />

        <ProductVariantList
          productId={product.id}
        />

        <ProductImageList
          productId={product.id}
        />
      </div>
    </main>
  );
}