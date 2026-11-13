import { Building2, Globe2, Hospital, ShoppingBag } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

const segments = [
  {
    icon: Building2,
    title: "Distributors & wholesalers",
    text: "Regular and bulk supply for distribution networks that need consistent availability and clear commercial terms.",
  },
  {
    icon: Globe2,
    title: "Importers & exporters",
    text: "Supply for international buyers and trading partners across worldwide markets.",
  },
  {
    icon: Hospital,
    title: "Hospitals, clinics & pharmacies",
    text: "Dependable access to pharmaceutical and healthcare products for institutions and healthcare providers.",
  },
  {
    icon: ShoppingBag,
    title: "Retail customers",
    text: "Products available directly through our online store, with straightforward ordering.",
  },
];

export function WhoWeServe() {
  return (
    <section className="border-t border-border bg-muted/50 py-20 sm:py-28">
      <Container>
        <Reveal>
          <SectionHeading
            index="04"
            eyebrow="Who we serve"
            title="Built for businesses that need reliable supply."
            description="Whatever the size of your requirement, we work with you as a long-term partner, not a one-time vendor."
          />
        </Reveal>

        <div className="mt-14 grid gap-px overflow-hidden rounded-2xl border border-border bg-border sm:grid-cols-2 lg:grid-cols-4">
          {segments.map((item, i) => (
            <Reveal key={item.title} delay={i * 90} className="bg-card">
              <div className="h-full p-7">
                <item.icon className="size-6 text-primary" strokeWidth={1.6} />
                <h3 className="mt-8 text-lg font-semibold tracking-tight text-foreground">
                  {item.title}
                </h3>
                <p className="mt-3 text-sm leading-7 text-muted-foreground">
                  {item.text}
                </p>
              </div>
            </Reveal>
          ))}
        </div>
      </Container>
    </section>
  );
}
