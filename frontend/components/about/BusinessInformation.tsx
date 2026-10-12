import {
  Building2,
  CalendarDays,
  FileText,
  Globe2,
  Scale,
  TrendingUp,
  UserRound,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";
import { cn } from "@/lib/utils";

const businessDetails = [
  {
    icon: Building2,
    label: "Nature of Business",
    value: "Manufacturers, Exporters, Wholesaler, Retailer, Trader",
    wide: true,
  },
  { icon: CalendarDays, label: "Year of Establishment", value: "2019" },
  { icon: Globe2, label: "Market Covered", value: "Worldwide" },
  { icon: UserRound, label: "Name of Founder", value: "Mr. Shekappa" },
  { icon: FileText, label: "GST No", value: "29AAJCR5569D1ZY" },
  { icon: TrendingUp, label: "Annual Turnover", value: "Rs. 50 Lakh - 1 Crore" },
  { icon: Scale, label: "Legal Status of Firm", value: "Private Limited Company" },
];

export function BusinessInformation() {
  return (
    <section className="py-16 sm:py-24">
      <Container>
        <Reveal>
          <SectionHeading
            eyebrow="Company Information"
            title="Business Details"
            description="Key information about our business, operations and company profile."
          />
        </Reveal>

        <div className="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {businessDetails.map((detail, index) => {
            const Icon = detail.icon;
            return (
              <Reveal
                key={detail.label}
                delay={(index % 4) * 80}
                className={cn(detail.wide && "sm:col-span-2")}
              >
                <article className="group h-full rounded-3xl border border-border bg-card p-5 transition-all duration-300 hover:-translate-y-1 hover:border-primary/40 hover:shadow-[0_20px_44px_rgba(8,127,91,0.12)] sm:p-6">
                  <span className="flex size-11 items-center justify-center rounded-xl bg-primary-light text-primary transition-all duration-300 group-hover:bg-primary group-hover:text-primary-foreground">
                    <Icon className="size-5" />
                  </span>
                  <p className="mt-4 text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground">
                    {detail.label}
                  </p>
                  <p className="mt-1.5 break-words text-base font-bold leading-snug text-foreground">
                    {detail.value}
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
