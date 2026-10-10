"use client";

import { Loader2, ShoppingBag } from "lucide-react";

import { Button } from "@/components/ui/button";
import type { Cart } from "@/types/cart";

interface CheckoutSummaryProps {
  cart: Cart;
  onPlaceOrder: () => void;
  isPlacingOrder: boolean;
  disabled?: boolean;
}

export function CheckoutSummary({
  cart,
  onPlaceOrder,
  isPlacingOrder,
  disabled = false,
}: CheckoutSummaryProps) {
  const minimumOrderValue = 200;
  const isBelowMinimum = cart.totalAmount < minimumOrderValue;

  const formatUsd = (amount: number) =>
    new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount);

  return (
    <section className="rounded-2xl border border-[#e5e8e7] bg-white p-5 sm:p-6">
      <div className="flex items-center gap-3">
        <div className="flex size-10 items-center justify-center rounded-xl bg-[#E8F4F4]">
          <ShoppingBag className="size-5 text-[#3E8F96]" />
        </div>

        <div>
          <h2 className="text-lg font-bold text-[#1B2A4A]">
            Order Summary
          </h2>

          <p className="mt-1 text-sm text-[#777]">
            Review your items before placing the order.
          </p>
        </div>
      </div>

      <div className="mt-6 divide-y divide-[#e5e8e7]">
        {cart.items.map((item) => (
          <div
            key={item.productVariantId}
            className="flex gap-4 py-4 first:pt-0 last:pb-0"
          >
            <div className="min-w-0 flex-1">
              <h3 className="font-semibold text-[#1B2A4A]">
                {item.productName}
              </h3>

              {(item.strength || item.packSize) && (
                <p className="mt-1 text-xs text-[#777]">
                  {[item.strength, item.packSize]
                    .filter(Boolean)
                    .join(" • ")}
                </p>
              )}

              <div className="mt-2 flex flex-wrap items-center gap-3 text-xs text-[#666]">
                <span>
                  Qty:{" "}
                  <span className="font-semibold text-[#1B2A4A]">
                    {item.quantity}
                  </span>
                </span>

                <span>
                  {formatUsd(item.unitPrice)} / unit
                </span>
              </div>
            </div>

            <div className="shrink-0 text-right">
              <p className="font-bold text-[#1B2A4A]">
                {formatUsd(item.totalPrice)}
              </p>
            </div>
          </div>
        ))}
      </div>

      <div className="mt-6 border-t border-[#e5e8e7] pt-5">
        <div className="flex items-center justify-between text-sm text-[#666]">
          <span>Items</span>
          <span>{cart.items.length}</span>
        </div>

        <div className="mt-2 flex items-center justify-between text-sm text-[#666]">
          <span>Total Quantity</span>
          <span>
            {cart.items.reduce(
              (total, item) => total + item.quantity,
              0
            )}
          </span>
        </div>

        <div className="mt-4 flex items-center justify-between border-t border-[#e5e8e7] pt-4">
          <span className="font-semibold text-[#1B2A4A]">
            Total Amount
          </span>

          <span className="text-xl font-bold text-[#F5821F]">
            {formatUsd(cart.totalAmount)}
          </span>
        </div>
      </div>

      {isBelowMinimum && (
        <div className="mt-4 rounded-xl border border-[#f3d6b8] bg-[#fff8f1] p-3">
          <p className="text-sm font-semibold text-[#9a4f0b]">
            Minimum order value is $200.00 USD
          </p>

          <p className="mt-1 text-xs leading-5 text-[#777]">
            Please add{" "}
            <span className="font-semibold text-[#9a4f0b]">
              {formatUsd(minimumOrderValue - cart.totalAmount)}
            </span>{" "}
            more to continue.
          </p>
        </div>
      )}

      <Button
        type="button"
        size="lg"
        onClick={onPlaceOrder}
        disabled={
          disabled ||
          isPlacingOrder ||
          isBelowMinimum
        }
        className="mt-6 h-12 w-full rounded-lg bg-[#F5821F] text-white hover:bg-[#df7115]"
      >
        {isPlacingOrder ? (
          <>
            <Loader2 className="mr-2 size-5 animate-spin" />
            Redirecting to Payment...
          </>
        ) : (
          "Proceed to Payment"
        )}
      </Button>

      <p className="mt-3 text-center text-xs text-[#888]">
        By proceeding to payment, you confirm that the selected
        delivery address is correct.
      </p>
    </section>
  );
}