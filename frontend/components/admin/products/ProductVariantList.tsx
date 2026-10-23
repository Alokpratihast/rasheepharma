"use client";

import { useEffect, useState } from "react";
import { LoaderCircle, RefreshCw } from "lucide-react";
import { productVariantService } from "@/services/productVariantService";
import type { ProductVariant } from "@/types/product";
import { ProductVariantForm } from "@/components/admin/products/ProductVariantForm";
import { getAdminErrorMessage } from "@/lib/admin-error-message";

interface ProductVariantListProps {
  productId: number;
}

export function ProductVariantList({
  productId,
}: ProductVariantListProps) {
  const [variants, setVariants] = useState<ProductVariant[]>([]);
  const [loadedRequestKey, setLoadedRequestKey] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [retryKey, setRetryKey] = useState(0);

  const [showForm, setShowForm] = useState(false);
  const [editingVariant, setEditingVariant] =
    useState<ProductVariant | null>(null);

  const requestKey = `${productId}:${retryKey}`;
  const loading = loadedRequestKey !== requestKey;

  useEffect(() => {
    let isActive = true;
    const loadVariants = async () => {
      try {
        const data =
          await productVariantService.getByProductId(productId);
        if (!isActive) return;
        setVariants(data);
        setError(null);
      } catch (error) {
        if (!isActive) return;
        console.error(
          "Failed to load product variants:",
          error,
        );
        setError(getAdminErrorMessage(error, "We could not load variants. Check your connection and try again."));
      } finally {
        if (isActive) setLoadedRequestKey(requestKey);
      }
    };

    void loadVariants();

    return () => {
      isActive = false;
    };
  }, [productId, retryKey, requestKey]);

  const handleAdd = () => {
    setEditingVariant(null);
    setShowForm(true);
  };

  const handleEdit = (variant: ProductVariant) => {
    setEditingVariant(variant);
    setShowForm(true);
  };

  const handleDelete = async (variant: ProductVariant) => {
    const confirmed = window.confirm(
      `Are you sure you want to delete this variant?`,
    );

    if (!confirmed) {
      return;
    }

    try {
      setError(null);

      await productVariantService.delete(variant.id);

      setVariants((currentVariants) =>
        currentVariants.filter(
          (item) => item.id !== variant.id,
        ),
      );
    } catch (error) {
      console.error(
        "Failed to delete product variant:",
        error,
      );

      setError(getAdminErrorMessage(error, "We could not delete this variant. Check the product details and try again."));
    }
  };

  const handleSuccess = (savedVariant: ProductVariant) => {
    setVariants((currentVariants) => {
      const existingVariant = currentVariants.find(
        (item) => item.id === savedVariant.id,
      );

      if (existingVariant) {
        return currentVariants.map((item) =>
          item.id === savedVariant.id
            ? savedVariant
            : item,
        );
      }

      return [...currentVariants, savedVariant];
    });

    setShowForm(false);
    setEditingVariant(null);
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingVariant(null);
  };

  return (
    <section className="mt-8">
      <div className="mb-5 flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold text-[#1B2A4A]">
            Product Variants
          </h2>

          <p className="mt-1 text-sm text-gray-500">
            Manage strength, packaging, pricing and inventory.
          </p>
        </div>

        {!showForm && (
          <button
            type="button"
            onClick={handleAdd}
            className="rounded-lg bg-[#1B2A4A] px-4 py-2.5 text-sm font-medium text-white hover:bg-[#142039]"
          >
            Add Variant
          </button>
        )}
      </div>

      {showForm && (
        <div className="mb-6">
          <ProductVariantForm
            key={String(productId) + ':' + (editingVariant?.id ?? 'new')}
            productId={productId}
            variant={editingVariant}
            onSuccess={handleSuccess}
            onCancel={handleCancel}
          />
        </div>
      )}

      {error && (
        <div role="alert" className="mb-5 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          <p>{error}</p>
          {!loading && variants.length === 0 && (
            <button type="button" onClick={() => setRetryKey((value) => value + 1)} className="mt-3 inline-flex items-center gap-2 font-semibold text-red-900 hover:underline">
              <RefreshCw aria-hidden="true" className="size-4" /> Retry loading variants
            </button>
          )}
        </div>
      )}

      {loading ? (
        <div role="status" aria-label="Loading product variants" className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <div aria-hidden="true" className="space-y-3">
            <div className="h-5 w-40 animate-pulse rounded bg-slate-100" />
            <div className="h-12 animate-pulse rounded bg-slate-100" />
            <div className="h-12 animate-pulse rounded bg-slate-100" />
          </div>
          <p className="mt-4 flex items-center gap-2 text-sm text-slate-500"><LoaderCircle aria-hidden="true" className="size-4 animate-spin" /> Loading variants…</p>
        </div>
      ) : variants.length === 0 ? (
        <div className="rounded-xl bg-white p-8 text-center shadow-sm">
          <p className="text-sm text-gray-500">
            No variants added yet.
          </p>

          {!showForm && (
            <button
              type="button"
              onClick={handleAdd}
              className="mt-4 text-sm font-medium text-[#1B2A4A] hover:underline"
            >
              Add the first variant
            </button>
          )}
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl bg-white shadow-sm">
          <div className="overflow-x-auto">
            <table className="min-w-full">
              <thead className="border-b border-gray-200 bg-gray-50">
                <tr>
                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Strength
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Pack Size
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Price
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    MOQ
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    SKU
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Stock
                  </th>

                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Status
                  </th>

                  <th className="px-5 py-3 text-right text-xs font-semibold uppercase tracking-wide text-gray-500">
                    Actions
                  </th>
                </tr>
              </thead>

              <tbody className="divide-y divide-gray-100">
                {variants.map((variant) => (
                  <tr key={variant.id}>
                    <td className="px-5 py-4 text-sm font-medium text-gray-900">
                      {variant.strength || "—"}
                    </td>

                    <td className="px-5 py-4 text-sm text-gray-600">
                      {variant.packSize || "—"}
                    </td>

                    <td className="px-5 py-4 text-sm font-medium text-gray-900">
                      {variant.currency === "USD"
                      ? `$${variant.price.toFixed(2)}`
                      : `₹${variant.price.toFixed(2)}`}
                    </td>

                    <td className="px-5 py-4 text-sm text-gray-600">
                      {variant.moq ?? "—"}
                    </td>

                    <td className="px-5 py-4 text-sm text-gray-600">
                      {variant.sku || "—"}
                    </td>

                    <td className="px-5 py-4 text-sm text-gray-600">
                      {variant.stockQuantity}
                    </td>

                    <td className="px-5 py-4">
                      <span
                        className={`rounded-full px-2.5 py-1 text-xs font-medium ${
                          variant.isActive
                            ? "bg-green-100 text-green-700"
                            : "bg-gray-100 text-gray-600"
                        }`}
                      >
                        {variant.isActive
                          ? "Active"
                          : "Inactive"}
                      </span>
                    </td>

                    <td className="px-5 py-4">
                      <div className="flex justify-end gap-3">
                        <button
                          type="button"
                          onClick={() =>
                            handleEdit(variant)
                          }
                          className="text-sm font-medium text-[#1B2A4A] hover:underline"
                        >
                          Edit
                        </button>

                        <button
                          type="button"
                          onClick={() =>
                            handleDelete(variant)
                          }
                          className="text-sm font-medium text-red-600 hover:underline"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  );
}
