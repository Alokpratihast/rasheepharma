import Link from "next/link";
import { ArrowRight, MessageCircle } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";

export function AboutCTA() {
  return (
    <section className="py-16 sm:py-24">
      <Container>
        <Reveal>
          <div className="relative isolate overflow-hidden rounded-[36px] bg-brand-dark px-6 py-12 text-white shadow-[0_28px_70px_rgba(7,63,50,0.28)] sm:px-12 sm:py-16">
            <div
              aria-hidden="true"
              className="pointer-events-none absolute inset-0 -z-10"
            >
              <div className="absolute -left-24 -top-24 size-96 rounded-full bg-primary/50 blur-[100px]" />
              <div className="absolute -bottom-32 right-0 size-80 rounded-full bg-sky-600/35 blur-[100px]" />
            </div>

            <div className="flex flex-col gap-8 lg:flex-row lg:items-center lg:justify-between">
              <div className="max-w-2xl">
                <span className="inline-flex rounded-full border border-white/15 bg-white/10 px-3.5 py-1.5 text-[10px] font-bold uppercase tracking-[0.16em] text-white/85 backdrop-blur">
                  Let&apos;s work together
                </span>
                <h2 className="mt-5 text-3xl font-extrabold leading-[1.1] tracking-[-0.03em] sm:text-4xl lg:text-5xl">
                  Looking for a reliable{" "}
                  <span className="bg-gradient-to-r from-teal-300 to-emerald-200 bg-clip-text text-transparent">
                    pharmaceutical partner?
                  </span>
                </h2>
                <p className="mt-4 text-sm leading-7 text-white/70 sm:text-base">
                  Explore our product portfolio or share your business
                  requirement with the RashePharma team.
                </p>
              </div>

              <div className="flex shrink-0 flex-col gap-3 sm:flex-row">
                <Link
                  href="/products"
                  className="group inline-flex h-12 items-center justify-center gap-2 rounded-full bg-white px-8 font-bold text-brand-dark shadow-[0_14px_34px_rgba(0,0,0,0.3)] transition-all hover:-translate-y-0.5 hover:bg-emerald-50"
                >
                  Explore Products
                  <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
                </Link>
                <Link
                  href="/contact"
                  className="inline-flex h-12 items-center justify-center gap-2 rounded-full border border-white/35 px-8 font-bold text-white transition-all hover:-translate-y-0.5 hover:border-white hover:bg-white/10"
                >
                  Contact Us
                  <MessageCircle className="size-4" />
                </Link>
              </div>
            </div>
          </div>
        </Reveal>
      </Container>
    </section>
  );
}
