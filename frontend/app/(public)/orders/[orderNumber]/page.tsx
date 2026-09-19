"use client";

import { useEffect, useState } from "react";
import { Loader2 } from "lucide-react";
import { useParams, useRouter } from "next/navigation";
import { toast } from "sonner";

import { OrderDetails } from "@/components/order/OrderDetails";
import { orderService } from "@/services/order.service";
import type { Order } from "@/types/order";

export default function OrderPage() {
  const params = useParams();
  const router = useRouter();

  const orderNumber = params.orderNumber as string;

  const [order, setOrder] = useState<Order | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!orderNumber) return;

    const loadOrder = async () => {
      try {
        setIsLoading(true);

        const data =
          await orderService.getOrderByNumber(orderNumber);

        setOrder(data);
      } catch (error) {
        console.error("Failed to load order:", error);

        toast.error(
          "Unable to load order details."
        );

        router.push("/orders");
      } finally {
        setIsLoading(false);
      }
    };

    loadOrder();
  }, [orderNumber, router]);

  if (isLoading) {
    return (
      <main className="min-h-screen bg-[#fafafa]">
        <div className="flex min-h-[500px] items-center justify-center">
          <div className="flex items-center gap-2 text-sm text-[#666]">
            <Loader2 className="size-5 animate-spin" />
            Loading order details...
          </div>
        </div>
      </main>
    );
  }

  if (!order) {
    return null;
  }

  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-14">
      <div className="mx-auto w-full max-w-5xl px-5 sm:px-6 lg:px-8">
        <OrderDetails order={order} />
      </div>
    </main>
  );
}