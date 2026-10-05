import {
  Building2,
  Globe2,
  Handshake,
  Landmark,
} from "lucide-react";

import { Container } from "@/components/ui/container";

/**
 * Company facts strip. Sits on top of the hero's curved divider so the
 * hero flows into the page instead of ending in a hard edge.
 */
const facts = [
  {
    icon: Building2,
    label: "Established",
    value: "2019",
  },
  {
    icon: Globe2,
    label: "Market covered",
    value: "Worldwide",
  },
  {
    icon: Handshake,
    label: "Nature of business",
    value: "Manufacturer, exporter, wholesaler, retailer and trader",
  },
  {
    icon: Landmark,
    label: "GST number",
    value: "29AAJCR5569D1ZY",
  },
];

export function TrustSection() {
  return (
    <section className="relative z-10 -mt-12 pb-6 sm:-mt-16 lg:pb-10">
      <Container>
        <div className="rounded-3xl shadow-[0_24px_60px_rgba(7,63,50,0.16)]">
          {/* gap-px over a border-coloured background draws the dividers */}
          <dl className="grid gap-px overflow-hidden rounded-3xl bg-border sm:grid-cols-2 lg:grid-cols-4">
            {facts.map((fact) => {
              const Icon = fact.icon;

              return (
                <div
                  key={fact.label}
                  className="flex items-start gap-4 bg-white p-5 sm:p-6"
                >
                  <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary-light text-primary">
                    <Icon aria-hidden="true" className="size-5" />
                  </span>

                  <div className="min-w-0">
                    <dt className="text-sm text-muted-foreground">
                      {fact.label}
                    </dt>
                    <dd className="mt-1 break-words text-base font-semibold leading-6 text-brand-dark">
                      {fact.value}
                    </dd>
                  </div>
                </div>
              );
            })}
          </dl>
        </div>
      </Container>
    </section>
  );
}
