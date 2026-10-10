import { Award, CalendarDays, Globe2, Scale } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";

/* Values come from the existing Business Details / Certifications data. */
const stats = [
  { Icon: CalendarDays, value: "2019", label: "Year established" },
  { Icon: Globe2, value: "Worldwide", label: "Market covered" },
  { Icon: Scale, value: "Pvt. Ltd.", label: "Legal status" },
  { Icon: Award, value: "2", label: "Quality certifications" },
];

export function AboutStats() {
  return (
    <section className="relative z-10 -mt-14 pb-4 sm:-mt-16">
      <Container>
        <Reveal>
          <div className="grid grid-cols-2 gap-3 rounded-[28px] border border-border bg-card/90 p-3 shadow-[0_24px_60px_rgba(7,63,50,0.12)] backdrop-blur-xl lg:grid-cols-4">
            {stats.map(({ Icon, value, label }) => (
              <div
                key={label}
                className="flex items-center gap-3.5 rounded-2xl p-4 transition-colors hover:bg-primary-light/60 sm:p-5"
              >
                <span className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-primary-light text-primary">
                  <Icon className="size-6" />
                </span>
                <div className="min-w-0">
                  <p className="truncate text-xl font-extrabold tracking-tight text-foreground sm:text-2xl">
                    {value}
                  </p>
                  <p className="truncate text-xs text-muted-foreground">
                    {label}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </Reveal>
      </Container>
    </section>
  );
}
