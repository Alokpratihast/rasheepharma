import {
  Globe2,
  Handshake,
  PackageCheck,
  Building2,
} from "lucide-react";

import { Container } from "@/components/ui/container";

const highlights = [
  {
    icon: PackageCheck,
    title: "Pharmaceutical Portfolio",
    description:
      "A growing portfolio of pharmaceutical products across therapeutic and formulation categories.",
  },
  {
    icon: Handshake,
    title: "Business Partnerships",
    description:
      "Focused on building reliable, long-term relationships with customers, distributors and business partners.",
  },
  {
    icon: Globe2,
    title: "Market Focus",
    description:
      "Built with a broader market perspective and an ambition to serve healthcare requirements across markets.",
  },
  {
    icon: Building2,
    title: "Business Approach",
    description:
      "A customer-focused approach that combines product reliability, communication and dependable service.",
  },
];

export function BusinessHighlights() {
  return (
    <section className="bg-white py-14 sm:py-20">
      <Container>
        {/* Header */}
        <div className="mx-auto max-w-3xl text-center">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
            Business Highlights
          </p>

          <h2 className="mt-3 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
            Built around products, partnerships and reliability
          </h2>

          <p className="mt-4 text-sm leading-7 text-[#595959] sm:text-base">
            Our business approach is centered on serving pharmaceutical
            requirements with dependable products, responsive communication
            and long-term partnerships.
          </p>
        </div>

        {/* Highlights */}
        <div className="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {highlights.map((highlight) => {
            const Icon = highlight.icon;

            return (
              <article
                key={highlight.title}
                className="rounded-2xl border border-[#e4e9e7] bg-[#fafcfc] p-5 transition-all duration-200 hover:-translate-y-0.5 hover:shadow-sm"
              >
                <div className="flex size-11 items-center justify-center rounded-xl bg-[#EAF5F3]">
                  <Icon className="size-5 text-[#3E8F96]" />
                </div>

                <h3 className="mt-4 text-sm font-semibold text-[#1B2A4A]">
                  {highlight.title}
                </h3>

                <p className="mt-2 text-xs leading-5 text-[#666]">
                  {highlight.description}
                </p>
              </article>
            );
          })}
        </div>
      </Container>
    </section>
  );
}