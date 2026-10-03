"use client";

import { useCallback, useEffect, useState } from "react";
import { AlertCircle, Loader2, ShoppingBag } from "lucide-react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/components/providers/AuthProvider";

import { cartService } from "@/services/cart.service";
import { orderService } from "@/services/order.service";
import { paymentService } from "@/services/paymentService";

import { CheckoutAddress } from "./CheckoutAddress";
import { CheckoutSummary } from "./CheckoutSummary";

import type { Cart } from "@/types/cart";

export function Checkout() {
  const router = useRouter();

  const { isAuthenticated, isLoading: authLoading } = useAuth();

  const [cart, setCart] = useState<Cart | null>(null);
  const [selectedAddressId, setSelectedAddressId] =
    useState<number | null>(null);

  const [isLoadingCart, setIsLoadingCart] = useState(true);
  const [isPlacingOrder, setIsPlacingOrder] = useState(false);

  /**
   * Keep this callback stable because CheckoutAddress
   * uses it inside useEffect.
   */
  const handleAddressSelect = useCallback((addressId: number) => {
    setSelectedAddressId(addressId);
  }, []);

  /**
   * Load current user's cart.
   */
  useEffect(() => {
    if (authLoading) return;

    if (!isAuthenticated) {
      router.push(
        `/login?redirect=${encodeURIComponent("/checkout")}`
      );
      return;
    }

    const loadCart = async () => {
      try {
        setIsLoadingCart(true);

        const data = await cartService.getCart();

        setCart(data);
      } catch (error) {
        console.error("Failed to load cart:", error);

        toast.error(
          "Unable to load your cart. Please try again."
        );
      } finally {
        setIsLoadingCart(false);
      }
    };

    loadCart();
  }, [authLoading, isAuthenticated, router]);

  /**
   * Create order and start Stripe Checkout.
   *
   * Backend remains the source of truth for:
   * - Product prices
   * - Stock
   * - Order total
   * - Minimum order value
   * - Currency
   */
  const handlePlaceOrder = async () => {
    if (!selectedAddressId) {
      toast.error("Please select a delivery address.");
      return;
    }

    if (!cart || cart.items.length === 0) {
      toast.error("Your cart is empty.");
      return;
    }

    /**
     * Frontend UX validation.
     * Backend also validates the $200 minimum.
     */
    if (cart.totalAmount < 200) {
      toast.error("Minimum order value is $200 USD.");
      return;
    }

    try {
      setIsPlacingOrder(true);

      /**
       * Create the order using the selected address.
       * Final price, stock and total are calculated by the backend.
       */
      const order = await orderService.createOrder({
        addressId: selectedAddressId,
      });

      /**
       * Create Stripe Checkout Session for the newly created order.
       */
      const response =
        await paymentService.createCheckoutSession(order.id);

      /**
       * Redirect customer to Stripe Checkout.
       */
      window.location.assign(response.checkoutUrl);
    } catch (error) {
      console.error("Failed to start payment:", error);

      toast.error(
        "Unable to start payment. Please try again."
      );
    } finally {
      setIsPlacingOrder(false);
    }
  };

  /**
   * Authentication loading state.
   */
  if (authLoading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="flex items-center gap-2 text-sm text-[#666]">
          <Loader2 className="size-5 animate-spin" />
          Checking your account...
        </div>
      </div>
    );
  }

  /**
   * Cart loading state.
   */
  if (isLoadingCart) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="flex items-center gap-2 text-sm text-[#666]">
          <Loader2 className="size-5 animate-spin" />
          Loading checkout...
        </div>
      </div>
    );
  }

  /**
   * Empty / unavailable cart.
   */
  if (!cart || cart.items.length === 0) {
    return (
      <div className="mx-auto max-w-xl rounded-2xl border border-[#e5e8e7] bg-white p-8 text-center sm:p-10">
        <div className="mx-auto flex size-14 items-center justify-center rounded-full bg-[#E8F4F4]">
          <ShoppingBag className="size-7 text-[#3E8F96]" />
        </div>

        <h1 className="mt-5 text-2xl font-bold text-[#1B2A4A]">
          Your cart is empty
        </h1>

        <p className="mt-2 text-sm leading-6 text-[#777]">
          Add some products to your cart before proceeding
          to checkout.
        </p>

        <Button
          type="button"
          onClick={() => router.push("/products")}
          className="mt-6 bg-[#3E8F96] text-white hover:bg-[#347b81]"
        >
          Continue Shopping
        </Button>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-[#1B2A4A] sm:text-3xl">
          Checkout
        </h1>

        <p className="mt-2 text-sm text-[#777] sm:text-base">
          Confirm your delivery address and review your order.
        </p>
      </div>

      {/* Checkout Grid */}
      <div className="grid gap-6 lg:grid-cols-[1fr_420px]">
        {/* Left */}
        <div className="space-y-6">
          <CheckoutAddress
            selectedAddressId={selectedAddressId}
            onAddressSelect={handleAddressSelect}
          />

          {/* Information */}
          <div className="flex gap-3 rounded-xl border border-[#e5e8e7] bg-[#FAFAFA] p-4">
            <AlertCircle className="mt-0.5 size-5 shrink-0 text-[#3E8F96]" />

            <div>
              <p className="text-sm font-semibold text-[#1B2A4A]">
                Order information
              </p>

              <p className="mt-1 text-xs leading-5 text-[#777]">
                Your order will be created using the selected
                delivery address. Final pricing and stock are
                validated by the server when the order is placed.
              </p>
            </div>
          </div>
        </div>

        {/* Right */}
        <div className="lg:sticky lg:top-24 lg:self-start">
          <CheckoutSummary
            cart={cart}
            onPlaceOrder={handlePlaceOrder}
            isPlacingOrder={isPlacingOrder}
            disabled={!selectedAddressId}
          />
        </div>
      </div>
    </div>
  );
}