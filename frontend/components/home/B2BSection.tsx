import Link from "next/link";
import { ArrowRight, Boxes, Globe2, MessageSquareText } from "lucide-react";

import { Reveal } from "@/components/about/Reveal";
import { Container } from "@/components/ui/container";

import "./hero.css";

const benefits = [
  { title: "Bulk requirements", icon: Boxes },
  { title: "Global business", icon: Globe2 },
  { title: "One simple enquiry", icon: MessageSquareText },
];

/**
 * Two poster-style banners: a B2B enquiry banner (green) and an export
 * banner (orange). The footer carries the closing call to action.
 */
export function B2BSection() {
  return (
    <section className="bg-white py-14 sm:py-20">
      <Container>
        <div className="grid gap-5 lg:grid-cols-[1.3fr_1fr]">
          {/* Distributors */}
          <Reveal className="h-full">
            <div className="relative isolate h-full overflow-hidden rounded-[2rem] bg-brand-dark p-8 text-white sm:p-12">
              <div
                aria-hidden="true"
                className="pointer-events-none absolute inset-0 -z-10"
              >
                <div className="hero-drift absolute -left-24 -top-24 size-80 rounded-full bg-primary/50 blur-[90px]" />
                <div className="hero-grid absolute inset-0" />
              </div>

              <p className="font-mono text-xs font-medium uppercase tracking-[0.16em] text-orange-300">
                For distributors and importers
              </p>
              <h2 className="mt-4 max-w-xl text-3xl font-bold leading-[1.08] tracking-[-0.03em] sm:text-4xl lg:text-[44px]">
                Need bulk supply? Get a quote in one enquiry.
              </h2>

              <ul className="mt-6 flex flex-wrap gap-2.5">
                {benefits.map((benefit) => {
                  const Icon = benefit.icon;

                  return (
                    <li
                      key={benefit.title}
                      className="inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-4 py-1.5 text-sm text-white/90"
                    >
                      <Icon aria-hidden="true" className="size-4 text-teal-300" />
                      {benefit.title}
                    </li>
                  );
                })}
              </ul>

              <Link
                href="/b2b/enquiry"
                className="group mt-8 inline-flex h-12 items-center justify-center gap-2 rounded-full bg-white px-7 font-bold text-brand-dark shadow-[0_14px_34px_rgba(0,0,0,0.3)] transition-all hover:-translate-y-0.5 hover:bg-emerald-50"
              >
                Send B2B enquiry
                <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
              </Link>
            </div>
          </Reveal>

          {/* Export */}
          <Reveal delay={120} className="h-full">
            <div className="relative flex h-full flex-col overflow-hidden rounded-[2rem] bg-orange-500 p-8 text-slate-950 sm:p-12">
              <p className="font-mono text-xs font-medium uppercase tracking-[0.16em]">
                Export ready
              </p>
              <h2 className="mt-4 text-3xl font-bold leading-[1.08] tracking-[-0.03em] sm:text-4xl">
                Supplying partners across worldwide markets.
              </h2>
              <p className="mt-4 text-base leading-7 text-slate-900/80">
                Talk to us about international distribution and long-term
                partnerships.
              </p>

              <Link
                href="/contact"
                className="group mt-8 inline-flex h-12 items-center justify-center gap-2 self-start rounded-full bg-slate-950 px-7 font-bold text-white transition-all hover:-translate-y-0.5 hover:bg-slate-800 sm:mt-auto"
              >
                Contact our team
                <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
              </Link>
            </div>
          </Reveal>
        </div>
      </Container>
    </section>
  );
}
