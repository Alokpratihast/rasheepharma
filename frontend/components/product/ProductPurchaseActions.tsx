"use client";

import { useState } from "react";
import { Minus, Plus, ShoppingCart } from "lucide-react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/components/providers/AuthProvider";
import { cartService } from "@/services/cart.service";

interface ProductVariant {
  id: number;
  strength?: string | null;
  packSize?: string | null;
  sku?: string | null;
  stockQuantity: number;
  price: number;
  isActive: boolean;
}

interface ProductPurchaseActionsProps {
  variants: ProductVariant[];
}

export function ProductPurchaseActions({
  variants,
}: ProductPurchaseActionsProps) {
  const router = useRouter();

  const { isAuthenticated, isLoading: authLoading } = useAuth();

  const activeVariants = variants.filter(
    (variant) =>
      variant.isActive && variant.stockQuantity > 0
  );

  const [selectedVariantId, setSelectedVariantId] =
    useState<number | null>(
      activeVariants.length > 0
        ? activeVariants[0].id
        : null
    );

  const [quantity, setQuantity] = useState(1);
  const [isAdding, setIsAdding] = useState(false);

  const selectedVariant = activeVariants.find(
    (variant) => variant.id === selectedVariantId
  );

  const decreaseQuantity = () => {
    setQuantity((current) => Math.max(1, current - 1));
  };

  const increaseQuantity = () => {
    if (!selectedVariant) return;

    setQuantity((current) =>
      Math.min(
        selectedVariant.stockQuantity,
        current + 1
      )
    );
  };

  const handleAddToCart = async () => {
    if (authLoading) {
      return;
    }

    if (!isAuthenticated) {
      router.push(
        `/login?redirect=${encodeURIComponent(
          window.location.pathname
        )}`
      );
      return;
    }

    if (!selectedVariant) {
      toast.error("Please select a variant.");
      return;
    }

    if (selectedVariant.stockQuantity <= 0) {
      toast.error("This variant is currently out of stock.");
      return;
    }

    try {
      setIsAdding(true);

      await cartService.addToCart({
        productVariantId: selectedVariant.id,
        quantity,
      });

      toast.success("Product added to cart.");

      router.push("/cart");
    } catch (error) {
      console.error("Add to cart failed:", error);

      toast.error(
        "Unable to add product to cart. Please try again."
      );
    } finally {
      setIsAdding(false);
    }
  };

  if (activeVariants.length === 0) {
    return (
      <div className="mt-6 rounded-xl border border-dashed border-border bg-[#FAFAFA] p-5">
        <p className="font-medium text-[#1B2A4A]">
          Currently unavailable
        </p>

        <p className="mt-1 text-sm text-muted-foreground">
          Please contact us for availability and pricing.
        </p>
      </div>
    );
  }

  return (
    <div className="mt-8 rounded-2xl border border-[#e5e8e7] bg-[#FAFAFA] p-5">
      {/* Variant */}
      <div>
        <p className="mb-2 text-sm font-semibold text-[#1B2A4A]">
          Select Pack Size / Variant
        </p>

        <div className="grid gap-3 sm:grid-cols-2">
          {activeVariants.map((variant) => {
            const isSelected =
              variant.id === selectedVariantId;

            return (
              <button
                key={variant.id}
                type="button"
                onClick={() => {
                  setSelectedVariantId(variant.id);
                  setQuantity(1);
                }}
                className={`rounded-xl border p-4 text-left transition-all ${
                  isSelected
                    ? "border-[#3E8F96] bg-[#E8F4F4] ring-1 ring-[#3E8F96]"
                    : "border-[#dfe4e3] bg-white hover:border-[#3E8F96]/50"
                }`}
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-sm font-semibold text-[#1B2A4A]">
                      {variant.strength || "Standard"}
                    </p>

                    <p className="mt-1 text-xs text-[#666]">
                      {variant.packSize || "Pack size not specified"}
                    </p>

                    {variant.sku && (
                      <p className="mt-1 text-[11px] text-[#888]">
                        SKU: {variant.sku}
                      </p>
                    )}
                  </div>

                  <p className="text-sm font-bold text-[#F5821F]">
                    ₹{variant.price.toLocaleString("en-IN")}
                  </p>
                </div>

                <p className="mt-3 text-xs text-[#666]">
                  {variant.stockQuantity} available
                </p>
              </button>
            );
          })}
        </div>
      </div>

      {/* Selected Variant */}
      {selectedVariant && (
        <div className="mt-5 border-t border-[#e5e8e7] pt-5">
          <div className="flex flex-wrap items-end justify-between gap-5">
            {/* Quantity */}
            <div>
              <p className="mb-2 text-sm font-semibold text-[#1B2A4A]">
                Quantity
              </p>

              <div className="flex h-11 items-center rounded-lg border border-[#dfe4e3] bg-white">
                <button
                  type="button"
                  onClick={decreaseQuantity}
                  disabled={quantity <= 1 || isAdding}
                  className="flex size-10 items-center justify-center rounded-l-lg text-[#1B2A4A] hover:bg-[#F3F6F5] disabled:cursor-not-allowed disabled:opacity-40"
                  aria-label="Decrease quantity"
                >
                  <Minus className="size-4" />
                </button>

                <span className="min-w-12 text-center text-sm font-semibold text-[#1B2A4A]">
                  {quantity}
                </span>

                <button
                  type="button"
                  onClick={increaseQuantity}
                  disabled={
                    quantity >= selectedVariant.stockQuantity ||
                    isAdding
                  }
                  className="flex size-10 items-center justify-center rounded-r-lg text-[#1B2A4A] hover:bg-[#F3F6F5] disabled:cursor-not-allowed disabled:opacity-40"
                  aria-label="Increase quantity"
                >
                  <Plus className="size-4" />
                </button>
              </div>
            </div>

            {/* Total */}
            <div className="text-right">
              <p className="text-xs text-[#888]">
                Estimated total
              </p>

              <p className="mt-1 text-xl font-bold text-[#F5821F]">
                ₹
                {(
                  selectedVariant.price * quantity
                ).toLocaleString("en-IN")}
              </p>
            </div>
          </div>

          {/* Add to Cart */}
          <Button
            type="button"
            size="lg"
            onClick={handleAddToCart}
            disabled={
              authLoading ||
              isAdding ||
              !selectedVariant
            }
            className="mt-5 h-12 w-full rounded-lg bg-[#F5821F] text-white hover:bg-[#df7115]"
          >
            <ShoppingCart className="mr-2 size-5" />

            {isAdding
              ? "Adding to Cart..."
              : "Add to Cart"}
          </Button>

          <p className="mt-2 text-center text-xs text-[#888]">
            You can review your cart before placing the order.
          </p>
        </div>
      )}
    </div>
  );
}