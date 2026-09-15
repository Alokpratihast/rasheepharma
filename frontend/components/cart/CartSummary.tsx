import { ArrowRight, ShieldCheck } from "lucide-react";

interface CartSummaryProps {
  totalAmount: number;
  itemCount: number;
  onCheckout?: () => void;
}

export function CartSummary({
  totalAmount,
  itemCount,
  onCheckout,
}: CartSummaryProps) {
  return (
    <aside className="rounded-2xl border border-[#e5e8e7] bg-white p-5 shadow-sm sm:p-6">
      {/* Header */}
      <div>
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
          Order Summary
        </p>

        <h2 className="mt-1 text-xl font-semibold tracking-tight text-[#1B2A4A]">
          Cart total
        </h2>
      </div>

      {/* Summary */}
      <div className="mt-6 space-y-3">
        <div className="flex items-center justify-between text-sm">
          <span className="text-[#666]">
            Items
          </span>

          <span className="font-medium text-[#1B2A4A]">
            {itemCount}
          </span>
        </div>

        <div className="flex items-center justify-between border-t border-[#edf0ef] pt-3">
          <span className="text-sm font-medium text-[#1B2A4A]">
            Total Amount
          </span>

          <span className="text-lg font-bold text-[#F5821F]">
            ₹{totalAmount.toFixed(2)}
          </span>
        </div>
      </div>

      {/* CTA */}
      <button
        type="button"
        onClick={onCheckout}
        className="mt-6 flex h-11 w-full items-center justify-center gap-2 rounded-lg bg-[#F5821F] px-5 text-sm font-semibold text-white transition-colors hover:bg-[#df7115]"
      >
        Proceed to Checkout
        <ArrowRight className="size-4" />
      </button>

      {/* Trust */}
      <div className="mt-4 flex items-start gap-2 rounded-lg bg-[#F4FAF8] p-3">
        <ShieldCheck className="mt-0.5 size-4 shrink-0 text-[#3E8F96]" />

        <p className="text-xs leading-5 text-[#666]">
          Your cart is securely linked to your account.
        </p>
      </div>
    </aside>
  );
}