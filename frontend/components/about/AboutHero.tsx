import Image from "next/image";
import Link from "next/link";
import {
  ArrowRight,
  Globe2,
  ShieldCheck,
  Handshake,
} from "lucide-react";

import { Container } from "@/components/ui/container";

export function AboutHero() {
  return (
    <section className="relative overflow-hidden border-b border-[#e7ebea] bg-[#f7faf9]">
      {/* Decorative background */}
      <div className="pointer-events-none absolute -left-32 top-20 size-72 rounded-full bg-[#3E8F96]/10 blur-3xl" />
      <div className="pointer-events-none absolute -right-32 bottom-0 size-80 rounded-full bg-[#F5821F]/10 blur-3xl" />

      <Container>
        <div className="relative grid items-center gap-10 py-12 sm:py-16 lg:grid-cols-[1.05fr_0.95fr] lg:gap-16 lg:py-20">
          {/* =================================================
              LEFT CONTENT
          ================================================== */}

          <div className="max-w-2xl">
            {/* Eyebrow */}
            <div className="inline-flex items-center gap-2 rounded-full border border-[#dcebea] bg-white px-3 py-1.5 shadow-sm">
              <span className="size-2 rounded-full bg-[#F5821F]" />

              <span className="text-[11px] font-semibold uppercase tracking-[0.16em] text-[#3E8F96]">
                About RashePharma
              </span>
            </div>

            {/* Heading */}
            <h1 className="mt-5 text-4xl font-semibold tracking-[-0.03em] text-[#1B2A4A] sm:text-5xl lg:text-6xl lg:leading-[1.05]">
              Building trust through
              <span className="block text-[#3E8F96]">
                quality healthcare.
              </span>
            </h1>

            {/* Description */}
            <p className="mt-6 max-w-xl text-sm leading-7 text-[#595959] sm:text-base">
              RashePharma is focused on delivering quality pharmaceutical
              products and dependable healthcare solutions for customers,
              distributors and business partners.
            </p>

            <p className="mt-4 max-w-xl text-sm leading-7 text-[#666]">
              Our approach combines product reliability, customer focus and
              long-term business relationships to support healthcare
              requirements across markets.
            </p>

            {/* CTA */}
            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              <Link
                href="/products"
                className="group inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-[#F5821F] px-6 text-sm font-semibold text-white shadow-sm transition-all hover:-translate-y-0.5 hover:bg-[#df7115] hover:shadow-md"
              >
                Explore Products

                <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
              </Link>

              <Link
                href="/contact"
                className="inline-flex h-11 items-center justify-center rounded-xl border border-[#d9e2df] bg-white px-6 text-sm font-semibold text-[#1B2A4A] transition-colors hover:bg-[#f1f6f4]"
              >
                Contact Us
              </Link>
            </div>

            {/* Trust points */}
            <div className="mt-9 grid max-w-xl grid-cols-1 gap-3 sm:grid-cols-3">
              <div className="rounded-xl border border-[#e3ebe9] bg-white p-4">
                <div className="flex size-9 items-center justify-center rounded-lg bg-[#EAF5F3]">
                  <ShieldCheck className="size-4 text-[#3E8F96]" />
                </div>

                <p className="mt-3 text-xs font-semibold text-[#1B2A4A]">
                  Quality Focus
                </p>

                <p className="mt-1 text-[11px] leading-4 text-[#777]">
                  Reliable pharmaceutical solutions
                </p>
              </div>

              <div className="rounded-xl border border-[#e3ebe9] bg-white p-4">
                <div className="flex size-9 items-center justify-center rounded-lg bg-[#FFF3E9]">
                  <Handshake className="size-4 text-[#F5821F]" />
                </div>

                <p className="mt-3 text-xs font-semibold text-[#1B2A4A]">
                  Partnerships
                </p>

                <p className="mt-1 text-[11px] leading-4 text-[#777]">
                  Built for long-term relationships
                </p>
              </div>

              <div className="rounded-xl border border-[#e3ebe9] bg-white p-4">
                <div className="flex size-9 items-center justify-center rounded-lg bg-[#EAF5F3]">
                  <Globe2 className="size-4 text-[#3E8F96]" />
                </div>

                <p className="mt-3 text-xs font-semibold text-[#1B2A4A]">
                  Global Vision
                </p>

                <p className="mt-1 text-[11px] leading-4 text-[#777]">
                  Serving broader healthcare markets
                </p>
              </div>
            </div>
          </div>

          {/* =================================================
              RIGHT IMAGE
          ================================================== */}

          <div className="relative mx-auto w-full max-w-lg">
            {/* Outer glow */}
            <div className="absolute -inset-4 rounded-[34px] bg-[#3E8F96]/10 blur-2xl" />

            <div className="relative overflow-hidden rounded-[28px] border border-white bg-[#1B2A4A] p-2 shadow-[0_25px_70px_rgba(27,42,74,0.18)]">
              <div className="relative aspect-square overflow-hidden rounded-[22px]">
                <Image
                  src="/images/rashepharma.jpg"
                  alt="RashePharma"
                  fill
                  priority
                  sizes="(max-width: 1024px) 100vw, 50vw"
                  className="object-cover"
                />

                {/* Image overlay */}
                <div className="absolute inset-0 bg-gradient-to-t from-[#0f1b2e]/70 via-transparent to-transparent" />

                {/* Bottom information */}
                <div className="absolute inset-x-0 bottom-0 p-5 sm:p-6">
                  <div className="rounded-2xl border border-white/15 bg-black/20 p-4 backdrop-blur-md">
                    <p className="text-[10px] font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
                      RashePharma
                    </p>

                    <p className="mt-1 text-sm font-semibold text-white sm:text-base">
                      Better Health. Global Reach.
                    </p>

                    <p className="mt-1 text-xs leading-5 text-white/70">
                      Quality-focused pharmaceutical solutions built around
                      trust and reliability.
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Floating badge */}
            <div className="absolute -bottom-5 -left-4 hidden rounded-2xl border border-[#dcebea] bg-white px-4 py-3 shadow-lg sm:flex sm:items-center sm:gap-3">
              <div className="flex size-9 items-center justify-center rounded-full bg-[#EAF5F3]">
                <ShieldCheck className="size-4 text-[#3E8F96]" />
              </div>

              <div>
                <p className="text-[10px] font-semibold uppercase tracking-wide text-[#888]">
                  Our Focus
                </p>

                <p className="text-xs font-semibold text-[#1B2A4A]">
                  Quality & Trust
                </p>
              </div>
            </div>
          </div>
        </div>
      </Container>
    </section>
  );
}