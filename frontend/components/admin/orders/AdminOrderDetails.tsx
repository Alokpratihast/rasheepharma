"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import {
  ArrowLeft,
  CalendarDays,
  Clock3,
  Loader2,
  Mail,
  MapPin,
  Package,
  Phone,
  Save,
  UserRound,
} from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { orderService } from "@/services/order.service";
import type { Order } from "@/types/order";

export function AdminOrderDetails() {
  const params = useParams();
  const router = useRouter();

  const [order, setOrder] = useState<Order | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const [selectedStatus, setSelectedStatus] = useState("");
  const [comment, setComment] = useState("");
  const [isUpdating, setIsUpdating] = useState(false);

  const orderId = Number(params.id);

  useEffect(() => {
    const loadOrder = async () => {
      try {
        setIsLoading(true);

        const data = await orderService.getAdminOrderById(orderId);

        setOrder(data);
        setSelectedStatus(data.status);
      } catch (error) {
        console.error("Failed to load order:", error);
        toast.error("Unable to load order details.");
      } finally {
        setIsLoading(false);
      }
    };

    if (orderId) {
      loadOrder();
    }
  }, [orderId]);

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
      <div className="flex min-h-[500px] items-center justify-center">
        <div className="flex items-center gap-2 text-sm text-gray-500">
          <Loader2 className="size-5 animate-spin" />
          Loading order details...
        </div>
      </div>
    );
  }

  if (!order) {
    return (
      <div className="flex min-h-[400px] flex-col items-center justify-center">
        <Package className="size-12 text-gray-400" />

        <h1 className="mt-4 text-xl font-semibold text-gray-800">
          Order not found
        </h1>

        <Button
          type="button"
          onClick={() => router.push("/admin/orders")}
          className="mt-5"
        >
          Back to Orders
        </Button>
      </div>
    );
  }

  return (
    <section className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <button
            type="button"
            onClick={() => router.push("/admin/orders")}
            className="mb-3 inline-flex items-center gap-2 text-sm text-gray-500 hover:text-gray-800"
          >
            <ArrowLeft className="size-4" />
            Back to Orders
          </button>

          <h1 className="text-2xl font-semibold text-gray-900">
            Order Details
          </h1>

          <p className="mt-1 text-sm text-gray-500">
            {order.orderNumber}
          </p>
        </div>

        <span
          className={`inline-flex w-fit items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-semibold capitalize ${getStatusClass(
            order.status
          )}`}
        >
          <Clock3 className="size-3.5" />
          {order.status}
        </span>
      </div>

      {/* Update Order Status */}
<div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
  <div className="flex items-center gap-2">
    <Clock3 className="size-5 text-[#3E8F96]" />

    <h2 className="font-semibold text-gray-900">
      Update Order Status
    </h2>
  </div>

  <div className="mt-5 grid gap-5 md:grid-cols-2">
    {/* Status */}
    <div>
      <label
        htmlFor="order-status"
        className="mb-2 block text-sm font-medium text-gray-700"
      >
        Status
      </label>

      <select
        id="order-status"
        value={selectedStatus}
        onChange={(e) => setSelectedStatus(e.target.value)}
        disabled={isUpdating}
        className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-800 outline-none focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20 disabled:cursor-not-allowed disabled:bg-gray-50"
      >
        <option value="Pending">Pending</option>
        <option value="Processing">Processing</option>
        <option value="Shipped">Shipped</option>
        <option value="Delivered">Delivered</option>
        <option value="Completed">Completed</option>
        <option value="Cancelled">Cancelled</option>
      </select>
    </div>

    {/* Comment */}
    <div>
      <label
        htmlFor="order-comment"
        className="mb-2 block text-sm font-medium text-gray-700"
      >
        Comment
      </label>

      <input
        id="order-comment"
        type="text"
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        placeholder="Add a comment..."
        disabled={isUpdating}
        className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm text-gray-800 outline-none placeholder:text-gray-400 focus:border-[#3E8F96] focus:ring-2 focus:ring-[#3E8F96]/20 disabled:cursor-not-allowed disabled:bg-gray-50"
      />
    </div>
  </div>

  <div className="mt-5 flex justify-end">
    <Button
      type="button"
      disabled={
        isUpdating ||
        !selectedStatus ||
        selectedStatus === order.status
      }
      onClick={async () => {
        try {
          setIsUpdating(true);

          await orderService.updateStatus(order.id, {
            status: selectedStatus,
            comment: comment.trim() || undefined,
          });

          setOrder((prev) =>
            prev
              ? {
                  ...prev,
                  status: selectedStatus,
                }
              : prev
          );

          setComment("");

          toast.success("Order status updated successfully.");
        } catch (error) {
          console.error("Failed to update order status:", error);
          toast.error("Unable to update order status.");
        } finally {
          setIsUpdating(false);
        }
      }}
      className="inline-flex items-center gap-2 bg-[#3E8F96] hover:bg-[#347a80]"
    >
      {isUpdating ? (
        <Loader2 className="size-4 animate-spin" />
      ) : (
        <Save className="size-4" />
      )}

      {isUpdating ? "Updating..." : "Update Status"}
    </Button>
  </div>
</div>

      {/* Customer Details */}
      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <div className="flex items-center gap-2">
          <UserRound className="size-5 text-[#3E8F96]" />

          <h2 className="font-semibold text-gray-900">
            Customer Details
          </h2>
        </div>

        <div className="mt-5 grid gap-5 sm:grid-cols-3">
          {/* Customer Name */}
          <div className="flex items-start gap-3">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-gray-50">
              <UserRound className="size-5 text-gray-500" />
            </div>

            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
                Name
              </p>

              <p className="mt-1 font-medium text-gray-900">
                {order.customerName || "N/A"}
              </p>
            </div>
          </div>

          {/* Customer Email */}
          <div className="flex items-start gap-3">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-gray-50">
              <Mail className="size-5 text-gray-500" />
            </div>

            <div className="min-w-0">
              <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
                Email
              </p>

              <p className="mt-1 break-all font-medium text-gray-900">
                {order.customerEmail || "N/A"}
              </p>
            </div>
          </div>

          {/* Customer Phone */}
          <div className="flex items-start gap-3">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-gray-50">
              <Phone className="size-5 text-gray-500" />
            </div>

            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
                Phone
              </p>

              <p className="mt-1 font-medium text-gray-900">
                {order.customerPhone || "N/A"}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Order Information */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Order Items */}
        <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm lg:col-span-2">
          <div className="flex items-center gap-2">
            <Package className="size-5 text-[#3E8F96]" />

            <h2 className="font-semibold text-gray-900">
              Order Items
            </h2>
          </div>

          <div className="mt-5 divide-y divide-gray-100">
            {order.items.map((item) => (
              <div
                key={item.id}
                className="flex flex-col gap-3 py-4 sm:flex-row sm:items-center sm:justify-between"
              >
                <div>
                  <p className="font-medium text-gray-900">
                    {item.productName}
                  </p>

                  <p className="mt-1 text-sm text-gray-500">
                    {item.strength && `${item.strength} · `}
                    {item.packSize}
                  </p>

                  <p className="mt-1 text-xs text-gray-400">
                    Quantity: {item.quantity}
                  </p>
                </div>

                <div className="text-left sm:text-right">
                  <p className="text-sm text-gray-500">
                    ₹{item.unitPrice.toLocaleString("en-IN")} ×{" "}
                    {item.quantity}
                  </p>

                  <p className="mt-1 font-semibold text-gray-900">
                    ₹
                    {(item.unitPrice * item.quantity).toLocaleString(
                      "en-IN"
                    )}
                  </p>
                </div>
              </div>
            ))}
          </div>

          {/* Total */}
          <div className="mt-4 flex items-center justify-between border-t border-gray-200 pt-5">
            <span className="font-medium text-gray-600">
              Order Total
            </span>

            <span className="text-xl font-bold text-[#F5821F]">
              {order.currency === "INR" ? "₹" : order.currency}{" "}
              {order.totalAmount.toLocaleString("en-IN")}
            </span>
          </div>
        </div>

        {/* Shipping Address */}
        <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
          <div className="flex items-center gap-2">
            <MapPin className="size-5 text-[#3E8F96]" />

            <h2 className="font-semibold text-gray-900">
              Shipping Address
            </h2>
          </div>

          <div className="mt-5 space-y-1 text-sm leading-6 text-gray-600">
            <p>{order.shippingAddressLine1}</p>

            {order.shippingAddressLine2 && (
              <p>{order.shippingAddressLine2}</p>
            )}

            <p>
              {order.shippingCity}
              {order.shippingState &&
                `, ${order.shippingState}`}
            </p>

            <p>{order.shippingPostalCode}</p>

            <p>{order.shippingCountry}</p>
          </div>
        </div>
      </div>

      {/* Status History */}
      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <div className="flex items-center gap-2">
          <CalendarDays className="size-5 text-[#3E8F96]" />

          <h2 className="font-semibold text-gray-900">
            Order History
          </h2>
        </div>

        <div className="mt-5 space-y-4">
          {order.statusHistory.map((history) => (
            <div
              key={history.id}
              className="flex gap-4 border-l-2 border-gray-200 pl-4"
            >
              <div>
                <p className="font-medium capitalize text-gray-800">
                  {history.status}
                </p>

                {history.comment && (
                  <p className="mt-1 text-sm text-gray-500">
                    {history.comment}
                  </p>
                )}

                <p className="mt-1 text-xs text-gray-400">
                  {formatDate(history.createdAt)}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}