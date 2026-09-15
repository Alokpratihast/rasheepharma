import Link from "next/link";
import { ArrowRight, MessageCircle } from "lucide-react";

import { Container } from "@/components/ui/container";

export function AboutCTA() {
  return (
    <section className="bg-[#1B2A4A] py-14 sm:py-20">
      <Container>
        <div className="flex flex-col gap-8 rounded-3xl border border-white/10 bg-white/5 p-7 sm:p-10 lg:flex-row lg:items-center lg:justify-between">
          {/* Content */}
          <div className="max-w-2xl">
            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
              Let&apos;s Work Together
            </p>

            <h2 className="mt-3 text-3xl font-semibold tracking-tight text-white sm:text-4xl">
              Looking for a reliable pharmaceutical partner?
            </h2>

            <p className="mt-4 text-sm leading-7 text-white/70 sm:text-base">
              Explore our product portfolio or share your business
              requirement with the RashePharma team.
            </p>
          </div>

          {/* Actions */}
          <div className="flex shrink-0 flex-col gap-3 sm:flex-row">
            <Link
              href="/products"
              className="inline-flex h-11 items-center justify-center gap-2 rounded-lg bg-[#F5821F] px-6 text-sm font-semibold text-white transition-colors hover:bg-[#df7115]"
            >
              Explore Products
              <ArrowRight className="size-4" />
            </Link>

            <Link
              href="/contact"
              className="inline-flex h-11 items-center justify-center gap-2 rounded-lg border border-white/20 bg-white/10 px-6 text-sm font-semibold text-white transition-colors hover:bg-white/15"
            >
              Contact Us
              <MessageCircle className="size-4" />
            </Link>
          </div>
        </div>
      </Container>
    </section>
  );
}