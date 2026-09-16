"use client";

import { FormEvent, useEffect, useState } from "react";
import { productVariantService } from "@/services/productVariantService";
import type {
  ProductVariant,
  ProductVariantCreateInput,
  ProductVariantUpdateInput,
} from "@/types/product";

interface ProductVariantFormProps {
  productId: number;
  variant?: ProductVariant | null;
  onSuccess?: (variant: ProductVariant) => void;
  onCancel?: () => void;
}

export function ProductVariantForm({
  productId,
  variant,
  onSuccess,
  onCancel,
}: ProductVariantFormProps) {
  const isEditMode = Boolean(variant);

  const [strength, setStrength] = useState("");
  const [packSize, setPackSize] = useState("");
  const [price, setPrice] = useState("");
  const [currency, setCurrency] = useState("INR");
  const [moq, setMoq] = useState("");
  const [unitType, setUnitType] = useState("");
  const [sku, setSku] = useState("");
  const [stockQuantity, setStockQuantity] = useState("");
  const [isActive, setIsActive] = useState(true);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (variant) {
      setStrength(variant.strength ?? "");
      setPackSize(variant.packSize ?? "");
      setPrice(String(variant.price));
      setCurrency(variant.currency ?? "INR");
      setMoq(variant.moq !== null ? String(variant.moq) : "");
      setUnitType(variant.unitType ?? "");
      setSku(variant.sku ?? "");
      setStockQuantity(String(variant.stockQuantity));
      setIsActive(variant.isActive);
    } else {
      resetForm();
    }
  }, [variant]);

  const resetForm = () => {
    setStrength("");
    setPackSize("");
    setPrice("");
    setCurrency("INR");
    setMoq("");
    setUnitType("");
    setSku("");
    setStockQuantity("");
    setIsActive(true);
    setError(null);
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    try {
      setLoading(true);
      setError(null);

      if (!price || Number(price) < 0) {
        setError("Please enter a valid price.");
        return;
      }

      if (!stockQuantity || Number(stockQuantity) < 0) {
        setError("Please enter a valid stock quantity.");
        return;
      }

      if (isEditMode && variant) {
        const data: ProductVariantUpdateInput = {
          strength: strength || null,
          packSize: packSize || null,
          price: Number(price),
          currency: currency || null,
          moq: moq ? Number(moq) : null,
          unitType: unitType || null,
          sku: sku || null,
          stockQuantity: Number(stockQuantity),
          isActive,
        };

        const updatedVariant =
          await productVariantService.update(
            variant.id,
            data,
          );

        onSuccess?.(updatedVariant);
      } else {
        const data: ProductVariantCreateInput = {
          productId,
          strength: strength || null,
          packSize: packSize || null,
          price: Number(price),
          currency: currency || null,
          moq: moq ? Number(moq) : null,
          unitType: unitType || null,
          sku: sku || null,
          stockQuantity: Number(stockQuantity),
          isActive,
        };

        const createdVariant =
          await productVariantService.create(
            productId,
            data,
          );

        onSuccess?.(createdVariant);
        resetForm();
      }
    } catch (error) {
      console.error(
        "Failed to save product variant:",
        error,
      );

      setError(
        error instanceof Error
          ? error.message
          : "Failed to save product variant.",
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-xl bg-white p-6 shadow-sm"
    >
      <div className="mb-6">
        <h2 className="text-lg font-semibold text-[#1B2A4A]">
          {isEditMode
            ? "Edit Variant"
            : "Add Product Variant"}
        </h2>

        <p className="mt-1 text-sm text-gray-500">
          Add packaging, pricing and inventory information.
        </p>
      </div>

      {error && (
        <div className="mb-5 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-600">
          {error}
        </div>
      )}

      <div className="grid gap-5 md:grid-cols-2">
        <div>
          <label className="mb-1.5 block text-sm font-medium text-gray-700">
            Strength
          </label>

          <input
            type="text"
            value={strength}
            onChange={(event) =>
              setStrength(event.target.value)
            }
            placeholder="e.g. 500 mg"
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>

        <div>
          <label className="mb-1.5 block text-sm font-medium text-gray-700">
            Pack Size
          </label>

          <input
            type="text"
            value={packSize}
            onChange={(event) =>
              setPackSize(event.target.value)
            }
            placeholder="e.g. 10 Tablets"
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>

        <div>
          <label className="mb-1.5 block text-sm font-medium text-gray-700">
            Price
          </label>

          <input
            type="number"
            min="0"
            step="0.01"
            value={price}
            onChange={(event) =>
              setPrice(event.target.value)
            }
            placeholder="e.g. 25.00"
            required
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>

        <div>
          <label className="mb-1.5 block text-sm font-medium text-gray-700">
            Currency
          </label>

          <input
            type="text"
            value={currency}
            onChange={(event) =>
              setCurrency(event.target.value)
            }
            placeholder="INR"
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>

        <div>
          <label className="mb-1.5 block text-sm font-medium text-gray-700">
            MOQ
          </label>

          <input
            type="number"
            min="0"
            value={moq}
            onChange={(event) =>
              setMoq(event.target.value)
            }
            placeholder="e.g. 100"
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>

        <div>
          <label className="mb-1.5 block text-sm font-medium text-gray-700">
            Unit Type
          </label>

          <input
            type="text"
            value={unitType}
            onChange={(event) =>
              setUnitType(event.target.value)
            }
            placeholder="e.g. Strip, Bottle, Box"
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>

        <div>
          <label className="mb-1.5 block text-sm font-medium text-gray-700">
            SKU
          </label>

          <input
            type="text"
            value={sku}
            onChange={(event) =>
              setSku(event.target.value)
            }
            placeholder="e.g. PARA500-10"
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>

        <div>
          <label className="mb-1.5 block text-sm font-medium text-gray-700">
            Stock Quantity
          </label>

          <input
            type="number"
            min="0"
            value={stockQuantity}
            onChange={(event) =>
              setStockQuantity(event.target.value)
            }
            placeholder="e.g. 1000"
            required
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>
      </div>

      <div className="mt-6 flex items-center gap-3">
        <input
          id="variant-active"
          type="checkbox"
          checked={isActive}
          onChange={(event) =>
            setIsActive(event.target.checked)
          }
          className="h-4 w-4"
        />

        <label
          htmlFor="variant-active"
          className="text-sm font-medium text-gray-700"
        >
          Active
        </label>
      </div>

      <div className="mt-7 flex items-center gap-3">
        <button
          type="submit"
          disabled={loading}
          className="rounded-lg bg-[#1B2A4A] px-5 py-2.5 text-sm font-medium text-white hover:bg-[#142039] disabled:cursor-not-allowed disabled:opacity-60"
        >
          {loading
            ? "Saving..."
            : isEditMode
              ? "Update Variant"
              : "Add Variant"}
        </button>

        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-5 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}