import { Cart } from "@/components/cart/Cart";
import { Container } from "@/components/ui/container";

export default function CartPage() {
  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-14">
      <Container>
        {/* Page Header */}
        <div className="mb-8">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
            Shopping Cart
          </p>

          <h1 className="mt-2 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
            Your Cart
          </h1>

          <p className="mt-2 max-w-2xl text-sm leading-6 text-[#595959]">
            Review your selected products and update quantities before
            proceeding.
          </p>
        </div>

        {/* Cart Component */}
        <Cart />
      </Container>
    </main>
  );
}