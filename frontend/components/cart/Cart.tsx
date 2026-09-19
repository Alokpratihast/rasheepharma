"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  RefreshCw,
  ShoppingCart as ShoppingCartIcon,
} from "lucide-react";

import { CartItem } from "@/components/cart/CartItem";
import { CartSummary } from "@/components/cart/CartSummary";
import { cartService } from "@/services/cart.service";
import { useAuth } from "@/components/providers/AuthProvider";

import type { Cart as CartType } from "@/types/cart";

export function Cart() {

  const router = useRouter();
  const {
    isAuthenticated,
    isLoading: authLoading,
  } = useAuth();

  const [cart, setCart] = useState<CartType | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  const [updatingVariantId, setUpdatingVariantId] =
    useState<number | null>(null);

  /* =================================================
     LOAD CART
  ================================================== */

  const loadCart = useCallback(async () => {
    try {
      setErrorMessage("");

      const response = await cartService.getCart();

      setCart(response);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Unable to load your cart.";

      setErrorMessage(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  /* =================================================
     LOAD AFTER AUTH IS READY
  ================================================== */

  useEffect(() => {
    if (authLoading) {
      return;
    }

    if (!isAuthenticated) {
      setCart(null);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    loadCart();
  }, [
    authLoading,
    isAuthenticated,
    loadCart,
  ]);

  /* =================================================
     UPDATE QUANTITY
  ================================================== */

  async function handleUpdateQuantity(
    productVariantId: number,
    quantity: number,
  ) {
    if (quantity < 1) {
      return;
    }

    try {
      setErrorMessage("");
      setUpdatingVariantId(productVariantId);

      const updatedCart =
        await cartService.updateItem(
          productVariantId,
          quantity,
        );

      setCart(updatedCart);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Unable to update cart.";

      setErrorMessage(message);
    } finally {
      setUpdatingVariantId(null);
    }
  }

  /* =================================================
     REMOVE ITEM
  ================================================== */

  async function handleRemoveItem(
    productVariantId: number,
  ) {
    try {
      setErrorMessage("");
      setUpdatingVariantId(productVariantId);

      await cartService.removeItem(productVariantId);

      const updatedCart =
        await cartService.getCart();

      setCart(updatedCart);
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Unable to remove item from cart.";

      setErrorMessage(message);
    } finally {
      setUpdatingVariantId(null);
    }
  }

  /* =================================================
     AUTH CHECK
  ================================================== */

  if (authLoading) {
    return (
      <section className="rounded-2xl border border-[#e5e8e7] bg-white p-8 shadow-sm">
        <div className="flex min-h-56 items-center justify-center">
          <div className="text-center">
            <RefreshCw className="mx-auto size-6 animate-spin text-[#3E8F96]" />

            <p className="mt-3 text-sm text-[#666]">
              Checking your account...
            </p>
          </div>
        </div>
      </section>
    );
  }

  if (!isAuthenticated) {
    return (
      <section className="rounded-2xl border border-[#e5e8e7] bg-white px-6 py-16 shadow-sm">
        <div className="mx-auto max-w-md text-center">
          <div className="mx-auto flex size-14 items-center justify-center rounded-full bg-[#EAF5F3]">
            <ShoppingCartIcon className="size-6 text-[#3E8F96]" />
          </div>

          <h2 className="mt-5 text-xl font-semibold text-[#1B2A4A]">
            Please sign in to view your cart
          </h2>

          <p className="mt-2 text-sm leading-6 text-[#666]">
            Your cart is available after signing in to
            your account.
          </p>

          <a
            href="/login"
            className="mt-5 inline-flex h-10 items-center rounded-lg bg-[#F5821F] px-5 text-sm font-semibold text-white hover:bg-[#df7115]"
          >
            Sign In
          </a>
        </div>
      </section>
    );
  }

  /* =================================================
     CART LOADING
  ================================================== */

  if (isLoading) {
    return (
      <section className="rounded-2xl border border-[#e5e8e7] bg-white p-8 shadow-sm">
        <div className="flex min-h-56 items-center justify-center">
          <div className="text-center">
            <RefreshCw className="mx-auto size-6 animate-spin text-[#3E8F96]" />

            <p className="mt-3 text-sm text-[#666]">
              Loading your cart...
            </p>
          </div>
        </div>
      </section>
    );
  }

  /* =================================================
     ERROR
  ================================================== */

  if (errorMessage && !cart) {
    return (
      <section className="rounded-2xl border border-red-200 bg-white p-8 shadow-sm">
        <div className="mx-auto max-w-md text-center">
          <p className="text-sm font-semibold text-[#1B2A4A]">
            Unable to load cart
          </p>

          <p className="mt-2 text-sm text-red-600">
            {errorMessage}
          </p>

          <button
            type="button"
            onClick={() => {
              setIsLoading(true);
              loadCart();
            }}
            className="mt-5 inline-flex h-10 items-center gap-2 rounded-lg bg-[#F5821F] px-5 text-sm font-semibold text-white hover:bg-[#df7115]"
          >
            <RefreshCw className="size-4" />
            Try Again
          </button>
        </div>
      </section>
    );
  }

  /* =================================================
     EMPTY CART
  ================================================== */

  if (!cart || cart.items.length === 0) {
    return (
      <section className="rounded-2xl border border-[#e5e8e7] bg-white px-6 py-16 shadow-sm">
        <div className="mx-auto max-w-md text-center">
          <div className="mx-auto flex size-14 items-center justify-center rounded-full bg-[#EAF5F3]">
            <ShoppingCartIcon className="size-6 text-[#3E8F96]" />
          </div>

          <h2 className="mt-5 text-xl font-semibold text-[#1B2A4A]">
            Your cart is empty
          </h2>

          <p className="mt-2 text-sm leading-6 text-[#666]">
            Add products to your cart and they will
            appear here.
          </p>
        </div>
      </section>
    );
  }

  /* =================================================
     ITEM COUNT
  ================================================== */

  const itemCount = cart.items.reduce(
    (total, item) => total + item.quantity,
    0,
  );

  /* =================================================
     CART UI
  ================================================== */

  return (
    <div>
      {errorMessage && (
        <div className="mb-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {errorMessage}
        </div>
      )}

      <div className="grid items-start gap-6 lg:grid-cols-[1fr_360px]">
        {/* =================================================
            CART ITEMS
        ================================================== */}

        <section className="rounded-2xl border border-[#e5e8e7] bg-white px-5 shadow-sm sm:px-6">
          <div className="flex items-center justify-between border-b border-[#edf0ef] py-5">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
                Your Cart
              </p>

              <h2 className="mt-1 text-xl font-semibold text-[#1B2A4A]">
                Cart Items
              </h2>
            </div>

            <span className="rounded-full bg-[#EAF5F3] px-3 py-1 text-xs font-semibold text-[#3E8F96]">
              {itemCount} items
            </span>
          </div>

          <div>
            {cart.items.map((item) => (
              <CartItem
                key={item.id}
                item={item}
                onUpdateQuantity={
                  handleUpdateQuantity
                }
                onRemove={handleRemoveItem}
                isUpdating={
                  updatingVariantId ===
                  item.productVariantId
                }
              />
            ))}
          </div>
        </section>

        {/* =================================================
            SUMMARY
        ================================================== */}

        <CartSummary
          totalAmount={cart.totalAmount}
          itemCount={itemCount}
          onCheckout={() => {
            // Checkout flow will be implemented later.
            router.push("/checkout");
          }}
        />
      </div>
    </div>
  );
}