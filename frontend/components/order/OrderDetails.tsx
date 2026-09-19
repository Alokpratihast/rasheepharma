"use client";

import {
  CheckCircle2,
  Clock3,
  Package,
  Truck,
} from "lucide-react";

import type { Order } from "@/types/order";

interface OrderDetailsProps {
  order: Order;
}

export function OrderDetails({ order }: OrderDetailsProps) {
  const formatDate = (date: string) => {
    return new Date(date).toLocaleString("en-IN", {
      day: "2-digit",
      month: "short",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const getStatusIcon = (status: string) => {
    const normalizedStatus = status.toLowerCase();

    if (
      normalizedStatus === "completed" ||
      normalizedStatus === "delivered"
    ) {
      return <CheckCircle2 className="size-5" />;
    }

    if (
      normalizedStatus === "shipped" ||
      normalizedStatus === "out_for_delivery"
    ) {
      return <Truck className="size-5" />;
    }

    return <Clock3 className="size-5" />;
  };

  return (
    <div className="space-y-6">
      {/* Order Header */}
      <section className="rounded-2xl border border-[#e5e8e7] bg-white p-5 sm:p-6">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <p className="text-sm text-[#777]">Order Number</p>

            <h1 className="mt-1 text-xl font-bold text-[#1B2A4A] sm:text-2xl">
              {order.orderNumber}
            </h1>

            <p className="mt-2 text-sm text-[#777]">
              Placed on {formatDate(order.createdAt)}
            </p>
          </div>

          <div className="inline-flex w-fit items-center gap-2 rounded-full bg-[#E8F4F4] px-3 py-2 text-sm font-semibold capitalize text-[#3E8F96]">
            {getStatusIcon(order.status)}
            {order.status}
          </div>
        </div>
      </section>

      {/* Order Items */}
      <section className="rounded-2xl border border-[#e5e8e7] bg-white p-5 sm:p-6">
        <div className="flex items-center gap-3">
          <div className="flex size-10 items-center justify-center rounded-xl bg-[#E8F4F4]">
            <Package className="size-5 text-[#3E8F96]" />
          </div>

          <div>
            <h2 className="text-lg font-bold text-[#1B2A4A]">
              Order Items
            </h2>

            <p className="mt-1 text-sm text-[#777]">
              {order.items.length}{" "}
              {order.items.length === 1 ? "item" : "items"} in
              this order.
            </p>
          </div>
        </div>

        <div className="mt-6 divide-y divide-[#e5e8e7]">
          {order.items.map((item) => (
            <div
              key={item.id}
              className="flex flex-col gap-3 py-4 first:pt-0 sm:flex-row sm:items-center sm:justify-between"
            >
              <div>
                <h3 className="font-semibold text-[#1B2A4A]">
                  {item.productName}
                </h3>

                {(item.strength || item.packSize) && (
                  <p className="mt-1 text-sm text-[#777]">
                    {[item.strength, item.packSize]
                      .filter(Boolean)
                      .join(" • ")}
                  </p>
                )}

                <div className="mt-2 flex flex-wrap gap-3 text-xs text-[#666]">
                  <span>
                    Qty:{" "}
                    <strong className="text-[#1B2A4A]">
                      {item.quantity}
                    </strong>
                  </span>

                  <span>
                    ₹
                    {item.unitPrice.toLocaleString("en-IN")} /
                    unit
                  </span>
                </div>
              </div>

              <div className="text-left sm:text-right">
                <p className="text-sm text-[#777]">Total</p>

                <p className="mt-1 font-bold text-[#1B2A4A]">
                  ₹{item.totalPrice.toLocaleString("en-IN")}
                </p>
              </div>
            </div>
          ))}
        </div>

        {/* Total */}
        <div className="mt-5 border-t border-[#e5e8e7] pt-5">
          <div className="flex items-center justify-between">
            <span className="font-semibold text-[#1B2A4A]">
              Order Total
            </span>

            <span className="text-xl font-bold text-[#F5821F]">
              {order.currency === "INR" ? "₹" : order.currency}{" "}
              {order.totalAmount.toLocaleString("en-IN")}
            </span>
          </div>
        </div>
      </section>

      {/* Shipping Address */}
      <section className="rounded-2xl border border-[#e5e8e7] bg-white p-5 sm:p-6">
        <h2 className="text-lg font-bold text-[#1B2A4A]">
          Delivery Address
        </h2>

        <div className="mt-4 rounded-xl bg-[#FAFAFA] p-4">
          <p className="text-sm leading-6 text-[#555]">
            {order.shippingAddressLine1}
            {order.shippingAddressLine2 && (
              <>
                <br />
                {order.shippingAddressLine2}
              </>
            )}
            <br />
            {order.shippingCity}
            {order.shippingState &&
              `, ${order.shippingState}`}{" "}
            - {order.shippingPostalCode}
            <br />
            {order.shippingCountry}
          </p>
        </div>
      </section>

      {/* Status History */}
      {order.statusHistory.length > 0 && (
        <section className="rounded-2xl border border-[#e5e8e7] bg-white p-5 sm:p-6">
          <h2 className="text-lg font-bold text-[#1B2A4A]">
            Order Status
          </h2>

          <div className="mt-5 space-y-4">
            {order.statusHistory.map((history) => (
              <div
                key={history.id}
                className="flex gap-3"
              >
                <div className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-[#E8F4F4] text-[#3E8F96]">
                  {getStatusIcon(history.status)}
                </div>

                <div className="min-w-0">
                  <div className="flex flex-wrap items-center gap-2">
                    <p className="font-semibold capitalize text-[#1B2A4A]">
                      {history.status}
                    </p>

                    <span className="text-xs text-[#999]">
                      {formatDate(history.createdAt)}
                    </span>
                  </div>

                  {history.comment && (
                    <p className="mt-1 text-sm text-[#666]">
                      {history.comment}
                    </p>
                  )}
                </div>
              </div>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}