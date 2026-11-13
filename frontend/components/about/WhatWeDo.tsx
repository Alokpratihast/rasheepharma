import { ArrowUpRight } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

/* Roles come from the existing "Nature of Business" detail.
   Descriptions are short, general copy - adjust to match the company profile. */
const roles = [
  {
    name: "Manufacturer",
    text: "Essential drugs made at the ISO and WHO-GMP certified plants we are associated with.",
  },
  {
    name: "Exporter",
    text: "Serving healthcare requirements across worldwide markets.",
  },
  {
    name: "Wholesaler",
    text: "A wide distribution network that supports bulk supply and timely delivery.",
  },
  {
    name: "Retailer",
    text: "Healthcare products available directly to customers.",
  },
  {
    name: "Trader",
    text: "Trading support for business requirements, with reliable order fulfilment.",
  },
];

export function WhatWeDo() {
  return (
    <section className="border-t border-border bg-muted/50 py-20 sm:py-28">
      <Container>
        <div className="grid gap-12 lg:grid-cols-12 lg:gap-16">
          <Reveal className="lg:col-span-5">
            <div className="lg:sticky lg:top-28">
              <SectionHeading
                index="02"
                eyebrow="What we do"
                title="One company, five ways we serve the market."
                description="Our experienced team markets and supplies medicines through certified manufacturing partners, serving customers, distributors and partners at every stage of the supply chain."
              />
            </div>
          </Reveal>

          <ul className="lg:col-span-7">
            {roles.map((role, i) => (
              <Reveal key={role.name} delay={i * 70}>
                <li className="group grid items-baseline gap-x-6 gap-y-1 border-t border-border py-6 transition-colors last:border-b hover:bg-card sm:grid-cols-[3rem_1fr_auto] sm:px-3">
                  <span className="font-mono text-xs text-muted-foreground">
                    {String(i + 1).padStart(2, "0")}
                  </span>
                  <div>
                    <h3 className="text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">
                      {role.name}
                    </h3>
                    <p className="mt-1.5 text-sm leading-6 text-muted-foreground">
                      {role.text}
                    </p>
                  </div>
                  <ArrowUpRight
                    aria-hidden="true"
                    className="hidden size-5 text-muted-foreground/40 transition-all group-hover:-translate-y-0.5 group-hover:translate-x-0.5 group-hover:text-primary sm:block"
                  />
                </li>
              </Reveal>
            ))}
          </ul>
        </div>
      </Container>
    </section>
  );
}
