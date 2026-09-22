"use client";

import { XCircle } from "lucide-react";
import Link from "next/link";

export default function PaymentCancelPage() {
  return (
    <main className="min-h-[70vh] bg-[#FAFAFA] px-4 py-16">
      <div className="mx-auto max-w-xl">
        <div className="rounded-2xl border border-[#e5e8e7] bg-white p-8 text-center shadow-sm">
          <div className="mx-auto flex size-16 items-center justify-center rounded-full bg-red-50">
            <XCircle className="size-9 text-red-500" />
          </div>

          <h1 className="mt-6 text-2xl font-bold text-[#1B2A4A]">
            Payment Cancelled
          </h1>

          <p className="mt-3 text-sm leading-6 text-[#666]">
            Your payment was cancelled. Your order is still
            pending and you can try again.
          </p>

          <div className="mt-7 flex flex-col gap-3 sm:flex-row sm:justify-center">
            <Link
              href="/orders"
              className="rounded-xl bg-[#F5821F] px-5 py-3 text-sm font-semibold text-white transition hover:bg-[#e67512]"
            >
              View My Orders
            </Link>

            <Link
              href="/"
              className="rounded-xl border border-[#dfe3e2] px-5 py-3 text-sm font-semibold text-[#1B2A4A] transition hover:bg-[#FAFAFA]"
            >
              Continue Shopping
            </Link>
          </div>
        </div>
      </div>
    </main>
  );
}