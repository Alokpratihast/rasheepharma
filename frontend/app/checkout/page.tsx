import { Checkout } from "@/components/checkout/Checkout";

export default function CheckoutPage() {
  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-14">
      <div className="mx-auto w-full max-w-7xl px-5 sm:px-6 lg:px-8">
        <Checkout />
      </div>
    </main>
  );
}