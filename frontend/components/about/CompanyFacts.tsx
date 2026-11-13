import { Container } from "@/components/ui/container";
import { FOUNDED_YEAR } from "@/components/about/constants";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

const facts = [
  {
    label: "Nature of business",
    value: "Manufacturers, Exporters, Wholesaler, Retailer, Trader",
  },
  { label: "Legal status", value: "Private Limited Company" },
  { label: "Year of establishment", value: FOUNDED_YEAR },
  { label: "Founder", value: "Mr. Shekappa" },
  {
    label: "Head office",
    value: "01, B Byraveshwara Nagar, Magadi Road, Sunkadakatte, Bangalore, Karnataka 560091",
  },
  { label: "Market covered", value: "Worldwide" },
  { label: "Annual turnover", value: "Rs. 50 Lakh - 1 Crore" },
  { label: "GST No", value: "29AAJCR5569D1ZY" },
];

export function CompanyFacts() {
  return (
    <section className="border-t border-border bg-muted/50 py-20 sm:py-28">
      <Container>
        <div className="grid gap-12 lg:grid-cols-12 lg:gap-16">
          <Reveal className="lg:col-span-5">
            <SectionHeading
              index="07"
              eyebrow="Company information"
              title="The business at a glance."
              description="Key information about our company profile, operations and registration."
            />
          </Reveal>

          <Reveal className="lg:col-span-7">
            <dl className="rounded-2xl border border-border bg-card px-6 sm:px-8">
              {facts.map((fact) => (
                <div
                  key={fact.label}
                  className="grid gap-1 border-b border-border py-5 last:border-b-0 sm:grid-cols-[200px_1fr] sm:gap-6"
                >
                  <dt className="font-mono text-[11px] uppercase tracking-[0.14em] text-muted-foreground sm:pt-1">
                    {fact.label}
                  </dt>
                  <dd className="break-words text-base font-medium text-foreground">
                    {fact.value}
                  </dd>
                </div>
              ))}
            </dl>
          </Reveal>
        </div>
      </Container>
    </section>
  );
}
