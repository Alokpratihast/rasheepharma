"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import {
  Check,
  FileText,
  Minus,
  Plus,
  ShoppingCart,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/components/providers/AuthProvider";
import { cartService } from "@/services/cart.service";
import { formatPrice } from "@/lib/format";
import { calculateQuantityTotal } from "@/lib/quantity";
import { cn } from "@/lib/utils";

interface ProductVariant {
  id: number;
  strength?: string | null;
  packSize?: string | null;
  sku?: string | null;
  moq?: number | null;
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
    (variant) => variant.isActive && variant.stockQuantity > 0,
  );

  const [selectedVariantId, setSelectedVariantId] = useState<number | null>(
    activeVariants.length > 0 ? activeVariants[0].id : null,
  );
  const [quantity, setQuantity] = useState(1);
  const [quantityInput, setQuantityInput] = useState("1");
  const [isAdding, setIsAdding] = useState(false);

  const selectedVariant = activeVariants.find(
    (variant) => variant.id === selectedVariantId,
  );
  const parsedQuantity = Number(quantityInput);
  const isQuantityInputValid = Boolean(
    selectedVariant &&
      /^\d+$/.test(quantityInput) &&
      Number.isSafeInteger(parsedQuantity) &&
      parsedQuantity >= 1 &&
      parsedQuantity <= selectedVariant.stockQuantity,
  );
  const estimatedTotal = selectedVariant
    ? calculateQuantityTotal(
        selectedVariant.price,
        parsedQuantity,
        selectedVariant.stockQuantity,
      )
    : null;

  const decreaseQuantity = () => {
    const next = Math.max(1, quantity - 1);
    setQuantity(next);
    setQuantityInput(String(next));
  };

  const increaseQuantity = () => {
    if (!selectedVariant) return;
    const next = Math.min(selectedVariant.stockQuantity, quantity + 1);
    setQuantity(next);
    setQuantityInput(String(next));
  };

  const handleQuantityChange = (value: string) => {
    if (!/^\d*$/.test(value)) return;

    setQuantityInput(value);
    const next = Number(value);
    if (
      value.length > 0 &&
      Number.isSafeInteger(next) &&
      next >= 1 &&
      selectedVariant &&
      next <= selectedVariant.stockQuantity
    ) {
      setQuantity(next);
    }
  };

  const normalizeQuantityInput = () => {
    if (!selectedVariant) return;
    const parsed = Number(quantityInput);
    const normalized =
      Number.isSafeInteger(parsed) && parsed >= 1
        ? Math.min(parsed, selectedVariant.stockQuantity)
        : quantity;
    setQuantity(normalized);
    setQuantityInput(String(normalized));
  };

  const handleAddToCart = async () => {
    if (authLoading) {
      return;
    }

    if (!isAuthenticated) {
      router.push(
        `/login?redirect=${encodeURIComponent(window.location.pathname)}`,
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

    if (!isQuantityInputValid) {
      toast.error(`Enter a quantity from 1 to ${selectedVariant.stockQuantity}.`);
      return;
    }

    try {
      setIsAdding(true);

      await cartService.addToCart({
        productVariantId: selectedVariant.id,
        quantity: parsedQuantity,
      });

      toast.success("Product added to cart.");
      router.push("/cart");
    } catch (error) {
      console.error("Add to cart failed:", error);
      toast.error("Unable to add product to cart. Please try again.");
    } finally {
      setIsAdding(false);
    }
  };

  /* ---------- nothing in stock ---------- */
  if (activeVariants.length === 0) {
    return (
      <div className="rounded-3xl border border-dashed border-border bg-muted/40 p-6">
        <p className="font-bold text-foreground">Currently unavailable</p>
        <p className="mt-1 text-sm text-muted-foreground">
          This product isn&apos;t available to order online right now. Contact
          us for availability, bulk pricing and lead times.
        </p>
        <Link
          href="/contact"
          className="mt-4 inline-flex h-11 items-center gap-2 rounded-full bg-gradient-to-r from-primary to-teal-600 px-6 text-sm font-semibold text-white shadow-[0_8px_20px_rgba(8,127,91,0.3)] transition-all hover:-translate-y-0.5"
        >
          <FileText className="size-4" />
          Request a Quote
        </Link>
      </div>
    );
  }

  return (
    <div className="rounded-3xl border border-border bg-card p-5 shadow-[0_16px_50px_rgba(7,63,50,0.07)] sm:p-6">
      {/* Variant */}
      <div>
        <p className="mb-3 text-sm font-bold text-foreground">
          Select pack size / variant
        </p>

        <div className="grid gap-3 sm:grid-cols-2">
          {activeVariants.map((variant) => {
            const isSelected = variant.id === selectedVariantId;

            return (
              <button
                key={variant.id}
                type="button"
                aria-pressed={isSelected}
                onClick={() => {
                  setSelectedVariantId(variant.id);
                  setQuantity(1);
                  setQuantityInput("1");
                }}
                className={cn(
                  "relative flex flex-col justify-start rounded-2xl border-2 p-4 text-left transition-all duration-200",
                  isSelected
                    ? "border-primary bg-primary-light shadow-[0_8px_22px_rgba(8,127,91,0.14)]"
                    : "border-border bg-background hover:border-primary/40",
                )}
              >
                {isSelected && (
                  <span className="absolute right-3 top-3 flex size-5 items-center justify-center rounded-full bg-primary text-primary-foreground">
                    <Check className="size-3" />
                  </span>
                )}

                <p className="pr-6 text-sm font-bold text-foreground">
                  {variant.strength || "Standard"}
                </p>
                <p className="mt-1 text-xs text-muted-foreground">
                  {variant.packSize || "Pack size not specified"}
                </p>

                <div className="mt-auto flex items-end justify-between gap-2 pt-3">
                  <p className="text-base font-extrabold text-primary">
                    {variant.price > 0 ? formatPrice(variant.price) : "Contact"}
                  </p>
                  <p className="text-[11px] text-muted-foreground">
                    {variant.stockQuantity} in stock
                  </p>
                </div>

                {(variant.sku || variant.moq) && (
                  <p className="mt-2 truncate text-[11px] text-muted-foreground">
                    {variant.sku && <>SKU: {variant.sku}</>}
                    {variant.sku && variant.moq ? " · " : ""}
                    {variant.moq ? <>MOQ: {variant.moq}</> : null}
                  </p>
                )}
              </button>
            );
          })}
        </div>
      </div>

      {/* Selected variant */}
      {selectedVariant && (
        <div className="mt-6 border-t border-border pt-6">
          <div className="flex flex-wrap items-end justify-between gap-5">
            {/* Quantity */}
            <div>
              <p className="mb-2 text-sm font-bold text-foreground">Quantity</p>

              <div className="flex h-12 items-center rounded-full border border-border bg-background p-1">
                <button
                  type="button"
                  onClick={decreaseQuantity}
                  disabled={quantity <= 1 || isAdding}
                  className="flex size-10 items-center justify-center rounded-full text-foreground transition-colors hover:bg-muted disabled:cursor-not-allowed disabled:opacity-40"
                  aria-label="Decrease quantity"
                >
                  <Minus className="size-4" />
                </button>

                <input
                  type="text"
                  inputMode="numeric"
                  pattern="[0-9]*"
                  aria-label="Order quantity"
                  aria-describedby="quantity-help"
                  value={quantityInput}
                  onChange={(event) => handleQuantityChange(event.target.value)}
                  onBlur={normalizeQuantityInput}
                  onKeyDown={(event) => {
                    if (event.key === "Enter") {
                      event.currentTarget.blur();
                    }
                  }}
                  disabled={isAdding}
                  className="h-10 w-16 rounded-lg bg-transparent text-center text-sm font-bold text-foreground outline-none focus-visible:ring-2 focus-visible:ring-primary"
                />

                <button
                  type="button"
                  onClick={increaseQuantity}
                  disabled={
                    quantity >= selectedVariant.stockQuantity || isAdding
                  }
                  className="flex size-10 items-center justify-center rounded-full text-foreground transition-colors hover:bg-muted disabled:cursor-not-allowed disabled:opacity-40"
                  aria-label="Increase quantity"
                >
                  <Plus className="size-4" />
                </button>
              </div>
              <p
                id="quantity-help"
                aria-live="polite"
                className={cn(
                  "mt-2 text-xs",
                  isQuantityInputValid
                    ? "text-muted-foreground"
                    : "text-destructive",
                )}
              >
                {isQuantityInputValid
                  ? `Enter a whole number up to ${selectedVariant.stockQuantity} in stock.`
                  : `Enter a whole number from 1 to ${selectedVariant.stockQuantity}.`}
              </p>
            </div>

            {/* Total */}
            <div className="text-right">
              <p className="text-xs text-muted-foreground">Estimated total</p>
              <p className="mt-1 text-2xl font-extrabold tracking-tight text-foreground">
                {!isQuantityInputValid
                  ? "—"
                  : selectedVariant.price > 0
                    ? formatPrice(estimatedTotal ?? 0)
                    : "Contact"}
              </p>
              {isQuantityInputValid && selectedVariant.price > 0 ? (
                <p className="mt-1 text-xs text-muted-foreground">
                  {formatPrice(selectedVariant.price)} × {parsedQuantity} packs
                </p>
              ) : !isQuantityInputValid ? (
                <p className="mt-1 text-xs text-destructive">
                  Total updates when the quantity is valid.
                </p>
              ) : null}
            </div>
          </div>

          <div className="mt-5 flex flex-col gap-3 sm:flex-row">
            <Button
              type="button"
              size="lg"
              onClick={handleAddToCart}
              disabled={
                authLoading || isAdding || !selectedVariant || !isQuantityInputValid
              }
              className="h-12 flex-1 rounded-full bg-gradient-to-r from-primary to-teal-600 text-base font-semibold text-white shadow-[0_10px_26px_rgba(8,127,91,0.32)] transition-all hover:-translate-y-0.5 hover:shadow-[0_14px_32px_rgba(8,127,91,0.42)] disabled:translate-y-0"
            >
              <ShoppingCart className="mr-2 size-5" />
              {isAdding ? "Adding to Cart..." : "Add to Cart"}
            </Button>

            <Link
              href="/contact"
              className="inline-flex h-12 items-center justify-center gap-2 rounded-full border border-primary/40 px-6 text-sm font-semibold text-primary transition-all hover:-translate-y-0.5 hover:bg-primary-light"
            >
              <FileText className="size-4" />
              Request a Quote
            </Link>
          </div>

          <p className="mt-3 text-center text-xs text-muted-foreground">
            You can review your cart before placing the order.
          </p>
        </div>
      )}
    </div>
  );
}
