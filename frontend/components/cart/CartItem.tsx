"use client";

import Image from "next/image";
import { Minus, Plus, Trash2 } from "lucide-react";

import type { CartItem as CartItemType } from "@/types/cart";

interface CartItemProps {
  item: CartItemType;
  onUpdateQuantity: (
    productVariantId: number,
    quantity: number,
  ) => void;
  onRemove: (productVariantId: number) => void;
  isUpdating?: boolean;
}

export function CartItem({
  item,
  onUpdateQuantity,
  onRemove,
  isUpdating = false,
}: CartItemProps) {
  const decreaseQuantity = () => {
    if (item.quantity <= 1) {
      return;
    }

    onUpdateQuantity(
      item.productVariantId,
      item.quantity - 1,
    );
  };

  const increaseQuantity = () => {
    onUpdateQuantity(
      item.productVariantId,
      item.quantity + 1,
    );
  };

  return (
    <article className="border-b border-[#edf0ef] py-5 last:border-b-0">
      <div className="flex gap-4 sm:gap-5">
        {/* Product Image */}
        <div className="relative flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-xl bg-[#F5F7F7] sm:size-24">
          {item.imageUrl ? (
            <Image
              src={item.imageUrl}
              alt={item.productName}
              fill
              sizes="96px"
              className="object-contain p-2"
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center px-2 text-center text-[10px] font-medium text-[#8A9391]">
              No Image
            </div>
          )}
        </div>

        {/* Product Information */}
        <div className="min-w-0 flex-1">
          <div className="flex items-start justify-between gap-3">
            <div className="min-w-0">
              <h3 className="text-sm font-semibold leading-5 text-[#1B2A4A] sm:text-base">
                {item.productName}
              </h3>

              {(item.strength || item.packSize) && (
                <p className="mt-1 text-xs text-[#777]">
                  {item.strength && item.strength}

                  {item.strength && item.packSize && " • "}

                  {item.packSize && item.packSize}
                </p>
              )}
            </div>

            {/* Remove */}
            <button
              type="button"
              onClick={() =>
                onRemove(item.productVariantId)
              }
              disabled={isUpdating}
              aria-label={`Remove ${item.productName}`}
              className="flex size-8 shrink-0 items-center justify-center rounded-lg text-[#9A4A4A] transition-colors hover:bg-red-50 hover:text-red-600 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <Trash2 className="size-4" />
            </button>
          </div>

          <div className="mt-4 flex flex-wrap items-center justify-between gap-3">
            {/* Unit Price */}
            <div>
              <p className="text-xs text-[#888]">
                Unit price
              </p>

              <p className="mt-0.5 text-sm font-semibold text-[#1B2A4A]">
                ₹{item.unitPrice.toFixed(2)}
              </p>
            </div>

            {/* Quantity */}
            <div>
              <p className="mb-1 text-xs text-[#888]">
                Quantity
              </p>

              <div className="flex h-9 items-center rounded-lg border border-[#dfe4e3] bg-white">
                <button
                  type="button"
                  onClick={decreaseQuantity}
                  disabled={
                    item.quantity <= 1 ||
                    isUpdating
                  }
                  aria-label="Decrease quantity"
                  className="flex size-8 items-center justify-center rounded-l-lg text-[#1B2A4A] transition-colors hover:bg-[#F3F6F5] disabled:cursor-not-allowed disabled:opacity-40"
                >
                  <Minus className="size-3.5" />
                </button>

                <span className="min-w-9 text-center text-sm font-medium text-[#1B2A4A]">
                  {item.quantity}
                </span>

                <button
                  type="button"
                  onClick={increaseQuantity}
                  disabled={isUpdating}
                  aria-label="Increase quantity"
                  className="flex size-8 items-center justify-center rounded-r-lg text-[#1B2A4A] transition-colors hover:bg-[#F3F6F5] disabled:cursor-not-allowed disabled:opacity-40"
                >
                  <Plus className="size-3.5" />
                </button>
              </div>
            </div>

            {/* Total */}
            <div className="text-right">
              <p className="text-xs text-[#888]">
                Total
              </p>

              <p className="mt-0.5 text-sm font-bold text-[#F5821F]">
                ₹{item.totalPrice.toFixed(2)}
              </p>
            </div>
          </div>
        </div>
      </div>
    </article>
  );
}