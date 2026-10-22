"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, LoaderCircle, RefreshCw } from "lucide-react";
import { productService } from "@/services/product.service";
import type { ProductDetails } from "@/types/product";
import { ProductForm } from "@/components/admin/products/ProductForm";
import { ProductVariantList } from "@/components/admin/products/ProductVariantList";
import { ProductImageList } from "@/components/admin/products/ProductImageList";
import { getAdminErrorMessage } from "@/lib/admin-error-message";

export default function EditProductPage() {
  const params = useParams();
  const productId = Number(params.id);

  const [product, setProduct] =
    useState<ProductDetails | null>(null);

  const [error, setError] = useState<string | null>(null);
  const [retryKey, setRetryKey] = useState(0);
  const [loadedRequestKey, setLoadedRequestKey] = useState<string | null>(null);
  const requestKey = `${productId}:${retryKey}`;
  const invalidProductId = !productId || Number.isNaN(productId);
  const loading = !invalidProductId && loadedRequestKey !== requestKey;
  const visibleError = invalidProductId
    ? "This product link is invalid. Return to the product list and open the item again."
    : error;

  useEffect(() => {
    let active = true;

    const loadProduct = async () => {
      try {
        const data =
          await productService.getById(productId);

        if (active) {
          setProduct(data);
          setError(null);
        }
      } catch (loadError) {
        console.error("Failed to load product:", loadError);

        if (active) {
          setError(getAdminErrorMessage(loadError, "We could not load this product. Check your connection and try again."));
        }
      } finally {
        if (active) setLoadedRequestKey(requestKey);
      }
    };

    if (!invalidProductId) void loadProduct();
    return () => { active = false; };
  }, [productId, retryKey, requestKey, invalidProductId]);

  if (loading) {
    return (
      <main className="min-h-screen bg-[#f5f7f6] px-4 py-6 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-5xl" aria-busy="true">
          <div role="status" aria-label="Loading product" className="mb-6 flex items-center gap-2 text-sm font-medium text-slate-600">
            <LoaderCircle aria-hidden="true" className="size-4 animate-spin text-[#007f68]" /> Loading product details…
          </div>
          <div aria-hidden="true" className="space-y-5">
            <div className="h-28 animate-pulse rounded-2xl border border-slate-200 bg-white" />
            <div className="h-72 animate-pulse rounded-2xl border border-slate-200 bg-white" />
            <div className="h-52 animate-pulse rounded-2xl border border-slate-200 bg-white" />
          </div>
        </div>
      </main>
    );
  }

  if (visibleError || !product) {
    return (
      <main className="min-h-screen bg-[#f5f7f6] px-4 py-6 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-5xl">
          <div role="alert" className="rounded-2xl border border-red-200 bg-white p-6 shadow-sm">
            <h1 className="text-xl font-semibold text-[#1B2A4A]">Could not open this product</h1>
            <p className="mt-2 text-sm leading-6 text-slate-600">{visibleError ?? "This product may have been removed. Return to the list and choose another product."}</p>
            <button
              type="button"
              onClick={() => setRetryKey((value) => value + 1)}
              className="mt-5 inline-flex min-h-10 items-center gap-2 rounded-lg bg-[#1B2A4A] px-4 py-2 text-sm font-medium text-white transition hover:bg-[#142039]"
            >
              <RefreshCw aria-hidden="true" className="size-4" /> Try again
            </button>
            <Link href="/admin/products" className="ml-4 inline-flex min-h-10 items-center gap-2 text-sm font-semibold text-[#007f68] hover:underline">
              <ArrowLeft aria-hidden="true" className="size-4" /> Back to products
            </Link>
          </div>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-[#f5f7f6] px-4 py-6 sm:px-6 lg:px-8">
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
