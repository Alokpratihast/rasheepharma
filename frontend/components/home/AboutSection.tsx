import Link from "next/link";
import {
  ArrowRight,
  Building2,
  Globe2,
  ShieldCheck,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";

const companyHighlights = [
  {
    icon: Building2,
    label: "Established",
    value: "2019",
  },
  {
    icon: Globe2,
    label: "Market Covered",
    value: "Worldwide",
  },
  {
    icon: ShieldCheck,
    label: "Business Focus",
    value: "Pharmaceuticals",
  },
];

export function AboutSection() {
  return (
    <section className="bg-background py-14 sm:py-16">
      <Container>
        <div className="grid items-center gap-10 lg:grid-cols-[1.05fr_0.95fr]">
          {/* Content */}
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
              About RashePharma
            </p>

            <h2 className="mt-3 max-w-xl text-2xl font-semibold tracking-tight text-[#1B2A4A] sm:text-3xl lg:text-4xl">
              Building trusted pharmaceutical partnerships
            </h2>

            <p className="mt-5 max-w-2xl text-sm leading-7 text-[#595959] sm:text-base">
              RashePharma operates in the pharmaceutical sector with a focus on
              quality products, reliable supply and long-term business
              relationships across global markets.
            </p>

            <p className="mt-4 max-w-2xl text-sm leading-7 text-[#595959] sm:text-base">
              Our product portfolio supports customers and business partners
              looking for dependable pharmaceutical solutions and responsive
              service.
            </p>

            <div className="mt-7">
              <Link href="/about">
                <Button
                  variant="outline"
                  size="lg"
                  className="h-11 rounded-lg border-[#3E8F96]/30 text-[#1B2A4A] hover:border-[#3E8F96] hover:text-[#3E8F96]"
                >
                  Learn More
                  <ArrowRight className="size-4" />
                </Button>
              </Link>
            </div>
          </div>

          {/* Highlights */}
          <div className="rounded-xl border border-[#e5e5e5] bg-[#fafafa] p-6 sm:p-8">
            <div className="grid gap-0 sm:grid-cols-3 lg:grid-cols-1">
              {companyHighlights.map((item, index) => {
                const Icon = item.icon;

                return (
                  <div
                    key={item.label}
                    className={[
                      "flex items-center gap-4 py-5",
                      index !== 0
                        ? "border-t border-[#e5e5e5]"
                        : "",
                    ].join(" ")}
                  >
                    <div className="flex size-12 shrink-0 items-center justify-center rounded-full bg-white text-[#3E8F96] shadow-sm">
                      <Icon className="size-5" />
                    </div>

                    <div>
                      <p className="text-xs font-medium uppercase tracking-[0.1em] text-[#888]">
                        {item.label}
                      </p>

                      <p className="mt-1 text-base font-semibold text-[#1B2A4A]">
                        {item.value}
                      </p>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </Container>
    </section>
  );
}