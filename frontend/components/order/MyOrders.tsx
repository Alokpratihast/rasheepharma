"use client";

import { useEffect, useState } from "react";
import {
  CalendarDays,
  ChevronRight,
  Clock3,
  Loader2,
  ShoppingBag,
} from "lucide-react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { orderService } from "@/services/order.service";
import type { Order } from "@/types/order";

export function MyOrders() {
  const router = useRouter();

  const [orders, setOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadOrders = async () => {
      try {
        setIsLoading(true);

        const data = await orderService.getMyOrders();

        setOrders(data);
      } catch (error) {
        console.error("Failed to load orders:", error);

        toast.error(
          "Unable to load your orders. Please try again."
        );
      } finally {
        setIsLoading(false);
      }
    };

    loadOrders();
  }, []);

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString("en-IN", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    });
  };

  const getStatusClass = (status: string) => {
    switch (status.toLowerCase()) {
      case "completed":
      case "delivered":
        return "bg-green-50 text-green-700";

      case "cancelled":
      case "rejected":
        return "bg-red-50 text-red-700";

      case "shipped":
      case "out_for_delivery":
        return "bg-blue-50 text-blue-700";

      default:
        return "bg-[#E8F4F4] text-[#3E8F96]";
    }
  };

  if (isLoading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="flex items-center gap-2 text-sm text-[#666]">
          <Loader2 className="size-5 animate-spin" />
          Loading your orders...
        </div>
      </div>
    );
  }

  if (orders.length === 0) {
    return (
      <div className="mx-auto max-w-xl rounded-2xl border border-[#e5e8e7] bg-white p-8 text-center sm:p-10">
        <div className="mx-auto flex size-14 items-center justify-center rounded-full bg-[#E8F4F4]">
          <ShoppingBag className="size-7 text-[#3E8F96]" />
        </div>

        <h1 className="mt-5 text-2xl font-bold text-[#1B2A4A]">
          No orders yet
        </h1>

        <p className="mt-2 text-sm leading-6 text-[#777]">
          You haven't placed any orders yet. Start shopping
          to see your orders here.
        </p>

        <Button
          type="button"
          onClick={() => router.push("/products")}
          className="mt-6 bg-[#3E8F96] text-white hover:bg-[#347b81]"
        >
          Start Shopping
        </Button>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-[#1B2A4A] sm:text-3xl">
          My Orders
        </h1>

        <p className="mt-2 text-sm text-[#777] sm:text-base">
          View and track all your orders.
        </p>
      </div>

      {/* Orders */}
      <div className="space-y-4">
        {orders.map((order) => (
          <section
            key={order.id}
            className="rounded-2xl border border-[#e5e8e7] bg-white p-5 transition hover:border-[#3E8F96]/40 sm:p-6"
          >
            {/* Top */}
            <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <p className="text-xs font-medium uppercase tracking-wide text-[#999]">
                  Order Number
                </p>

                <h2 className="mt-1 text-lg font-bold text-[#1B2A4A]">
                  {order.orderNumber}
                </h2>

                <div className="mt-2 flex flex-wrap items-center gap-3 text-xs text-[#777]">
                  <span className="flex items-center gap-1.5">
                    <CalendarDays className="size-3.5" />
                    {formatDate(order.createdAt)}
                  </span>
                </div>
              </div>

              {/* Status */}
              <span
                className={`inline-flex w-fit items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-semibold capitalize ${getStatusClass(
                  order.status
                )}`}
              >
                <Clock3 className="size-3.5" />
                {order.status}
              </span>
            </div>

            {/* Order Summary */}
            <div className="mt-5 border-t border-[#e5e8e7] pt-5">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <p className="text-xs text-[#888]">
                    Order Total
                  </p>

                  <p className="mt-1 text-xl font-bold text-[#F5821F]">
                    {order.currency === "INR"
                      ? "₹"
                      : order.currency}{" "}
                    {order.totalAmount.toLocaleString("en-IN")}
                  </p>
                </div>

                <Button
                  type="button"
                  variant="outline"
                  onClick={() =>
                    router.push(
                      `/orders/${encodeURIComponent(
                        order.orderNumber
                      )}`
                    )
                  }
                  className="border-[#3E8F96] text-[#3E8F96] hover:bg-[#E8F4F4]"
                >
                  View Order
                  <ChevronRight className="ml-1 size-4" />
                </Button>
              </div>
            </div>
          </section>
        ))}
      </div>
    </div>
  );
}