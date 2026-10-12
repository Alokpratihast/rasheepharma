import Link from "next/link";
import {
  ArrowRight,
  Boxes,
  Globe2,
  MessageSquareText,
} from "lucide-react";

import { Container } from "@/components/ui/container";

import "./hero.css";

const benefits = [
  {
    title: "Bulk requirements",
    description:
      "Share your product and quantity requirements with our team.",
    icon: Boxes,
  },
  {
    title: "Global business",
    description:
      "Talk to us about international distribution and partnerships.",
    icon: Globe2,
  },
  {
    title: "Quick enquiry",
    description:
      "Get product information and business support through one simple enquiry.",
    icon: MessageSquareText,
  },
];

/**
 * Closing call to action. Mirrors the hero (dark green, drifting glows,
 * grid, glass cards) so the page starts and ends in the same voice.
 */
export function B2BSection() {
  return (
    <section className="bg-white py-14 sm:py-20">
      <Container>
        <div className="relative isolate overflow-hidden rounded-[2rem] bg-brand-dark text-white">
          {/* Background */}
          <div
            aria-hidden="true"
            className="pointer-events-none absolute inset-0 -z-10"
          >
            <div className="hero-drift absolute -left-24 -top-24 size-80 rounded-full bg-primary/50 blur-[90px]" />
            <div className="hero-drift-b absolute -bottom-32 -right-20 size-80 rounded-full bg-sky-600/30 blur-[90px]" />
            <div className="hero-grid absolute inset-0" />
          </div>

          <div className="grid gap-10 p-7 sm:p-10 lg:grid-cols-[1.15fr_0.85fr] lg:items-center lg:gap-14 lg:p-14">
            {/* Content */}
            <div>
              <h2 className="max-w-xl text-3xl font-bold leading-[1.1] tracking-[-0.03em] sm:text-4xl lg:text-[44px]">
                Looking for a reliable pharmaceutical partner?
              </h2>

              <p className="mt-5 max-w-xl text-base leading-7 text-white/75">
                Contact RashePharma for bulk requirements, product
                enquiries, distribution opportunities and long-term
                partnerships.
              </p>

              <div className="mt-8 flex flex-col gap-3 sm:flex-row">
                <Link
                  href="/b2b/enquiry"
                  className="inline-flex h-12 items-center justify-center gap-2 rounded-full bg-white px-7 font-bold text-brand-dark shadow-[0_14px_34px_rgba(0,0,0,0.3)] transition-all duration-200 hover:-translate-y-0.5 hover:bg-emerald-50"
                >
                  Request a quote
                  <ArrowRight className="size-4" />
                </Link>

                <Link
                  href="/contact"
                  className="inline-flex h-12 items-center justify-center rounded-full border border-white/35 px-7 font-bold text-white transition-all duration-200 hover:-translate-y-0.5 hover:border-white hover:bg-white/10"
                >
                  Contact our team
                </Link>
              </div>
            </div>

            {/* Benefits */}
            <ul className="space-y-3">
              {benefits.map((benefit) => {
                const Icon = benefit.icon;

                return (
                  <li
                    key={benefit.title}
                    className="flex gap-4 rounded-2xl border border-white/15 bg-white/10 p-4 backdrop-blur-xl"
                  >
                    <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-emerald-400/20 text-emerald-300">
                      <Icon aria-hidden="true" className="size-5" />
                    </span>

                    <div>
                      <h3 className="text-base font-semibold">
                        {benefit.title}
                      </h3>

                      <p className="mt-1 text-sm leading-6 text-white/70">
                        {benefit.description}
                      </p>
                    </div>
                  </li>
                );
              })}
            </ul>
          </div>
        </div>
      </Container>
    </section>
  );
}
