import { Building2, Globe2, Handshake, PackageCheck } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

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
    <section className="border-t border-border bg-muted/40 py-16 sm:py-24">
      <Container>
        <Reveal>
          <SectionHeading
            align="center"
            eyebrow="Business Highlights"
            title="Built around products, partnerships and reliability"
            description="Our business approach is centered on serving pharmaceutical requirements with dependable products, responsive communication and long-term partnerships."
          />
        </Reveal>

        <div className="mt-12 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
          {highlights.map((highlight, index) => {
            const Icon = highlight.icon;
            return (
              <Reveal key={highlight.title} delay={index * 100}>
                <article className="group relative h-full overflow-hidden rounded-3xl border border-border bg-card p-6 transition-all duration-300 hover:-translate-y-2 hover:border-primary/40 hover:shadow-[0_28px_56px_rgba(8,127,91,0.16)]">
                  <div
                    aria-hidden="true"
                    className="absolute inset-x-0 top-0 h-1 origin-left scale-x-0 bg-gradient-to-r from-primary to-teal-400 transition-transform duration-500 group-hover:scale-x-100"
                  />
                  <div className="flex items-center justify-between">
                    <span className="flex size-12 items-center justify-center rounded-2xl bg-primary-light text-primary transition-all duration-300 group-hover:bg-primary group-hover:text-primary-foreground">
                      <Icon className="size-6" />
                    </span>
                    <span className="text-3xl font-extrabold text-primary/15">
                      0{index + 1}
                    </span>
                  </div>
                  <h3 className="mt-5 text-lg font-bold leading-snug text-foreground">
                    {highlight.title}
                  </h3>
                  <p className="mt-2 text-sm leading-6 text-muted-foreground">
                    {highlight.description}
                  </p>
                </article>
              </Reveal>
            );
          })}
        </div>
      </Container>
    </section>
  );
}
