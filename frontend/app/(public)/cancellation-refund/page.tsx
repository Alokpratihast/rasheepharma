import type { Metadata } from "next";
import Link from "next/link";

import { Container } from "@/components/ui/container";
import { siteConfig } from "@/lib/site";

export const metadata: Metadata = {
  title: "Cancellation & Refund Policy | RashePharma",
  description:
    "Read how to contact RashePharma about order cancellation, returns and refunds.",
};

// This public page uses RashePharma contact details and avoids carrying over the source site's unrelated brand names or unverified deadlines.
export default function CancellationRefundPage() {
  return (
    <main className="bg-background py-12 sm:py-16">
      <Container>
        <nav aria-label="Breadcrumb" className="mb-8 text-sm text-muted-foreground">
          <Link href="/" className="transition-colors hover:text-primary">Home</Link>
          <span className="px-2" aria-hidden="true">/</span>
          <span aria-current="page">Cancellation &amp; Refunds</span>
        </nav>

        <article className="mx-auto max-w-4xl rounded-3xl border border-border bg-card px-6 py-8 shadow-sm sm:px-10 sm:py-12">
          <p className="text-xs font-bold uppercase tracking-[0.16em] text-primary">Order support</p>
          <h1 className="mt-3 text-3xl font-extrabold tracking-tight text-foreground sm:text-4xl">Cancellation &amp; Refund Policy</h1>
          <p className="mt-4 text-sm text-muted-foreground">Last updated: October 10, 2026</p>
          <p className="mt-6 text-base leading-8 text-muted-foreground">
            We want order issues to be handled clearly. Because availability and return eligibility can differ by product and order terms, contact our team as soon as possible with your order number. We will review the request and confirm the available options.
          </p>

          <div className="mt-10 space-y-9 text-sm leading-7 text-muted-foreground sm:text-base">
            <section>
              <h2 className="text-xl font-bold text-foreground">1. Requesting a cancellation</h2>
              <p className="mt-3">If you need to cancel an order, contact us promptly using the details below. Whether it can be cancelled depends on its fulfilment status and the applicable product or order terms. We will confirm the result of your request; submitting a request does not by itself cancel an order.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">2. Return eligibility</h2>
              <p className="mt-3">Return eligibility and any applicable time window are product- and order-specific. Please check the product information and the terms shared with your quotation or order confirmation. Contact us before sending anything back so that we can confirm eligibility and provide return instructions.</p>
              <p className="mt-3">For medicines and other healthcare products, do not use or return a product if its packaging appears damaged or tampered with. Contact us promptly if an item is damaged, incorrect, defective or otherwise inconsistent with the confirmed order. We will review the issue under the applicable product terms and law.</p>
            </section>

            <section>
              <h2 className="text-xl font-bold text-foreground">3. Refunds</h2>
              <p className="mt-3">A refund is considered after we approve the cancellation or return request. If approved, we will arrange it through the original payment method where available and confirm any processing details. The time for funds to appear can depend on the payment provider or bank.</p>
            </section>

            <section className="rounded-2xl bg-primary-light/50 p-5 sm:p-6">
              <h2 className="text-xl font-bold text-foreground">4. Contact us about an order</h2>
              <p className="mt-3">Please include your order number, the product name and a short description of the issue. For a damaged or incorrect delivery, include clear photos where possible.</p>
              <address className="mt-3 space-y-1 not-italic">
                <p>{siteConfig.address.join(" ")}</p>
                <p><a className="font-semibold text-primary hover:underline" href={siteConfig.emailHref}>{siteConfig.email}</a></p>
                <p><a className="font-semibold text-primary hover:underline" href={siteConfig.phoneHref}>{siteConfig.phone}</a></p>
              </address>
            </section>
          </div>
        </article>
      </Container>
    </main>
  );
}
