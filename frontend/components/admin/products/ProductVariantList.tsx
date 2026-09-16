"use client";

import { useEffect, useState } from "react";
import { productVariantService } from "@/services/productVariantService";
import type { ProductVariant } from "@/types/product";
import { ProductVariantForm } from "@/components/admin/products/ProductVariantForm";

interface ProductVariantListProps {
  productId: number;
}

export function ProductVariantList({
  productId,
}: ProductVariantListProps) {
  const [variants, setVariants] = useState<ProductVariant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [showForm, setShowForm] = useState(false);
  const [editingVariant, setEditingVariant] =
    useState<ProductVariant | null>(null);

  const loadVariants = async () => {
    try {
      setLoading(true);
      setError(null);

      const data =
        await productVariantService.getByProductId(productId);

      setVariants(data);
    } catch (error) {
      console.error(
        "Failed to load product variants:",
        error,
      );

      setError("Failed to load product variants.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadVariants();
  }, [productId]);

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

      setError("Failed to delete product variant.");
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
            productId={productId}
            variant={editingVariant}
            onSuccess={handleSuccess}
            onCancel={handleCancel}
          />
        </div>
      )}

      {error && (
        <div className="mb-5 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-600">
          {error}
        </div>
      )}

      {loading ? (
        <div className="rounded-xl bg-white p-6 shadow-sm">
          <p className="text-sm text-gray-500">
            Loading variants...
          </p>
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
                      {variant.currency || "INR"}{" "}
                      {variant.price.toFixed(2)}
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