import { CheckCircle2, FlaskConical, ShieldCheck, Target } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

const qualityPoints = [
  {
    icon: ShieldCheck,
    title: "Quality Focus",
    description:
      "We maintain a strong focus on dependable product quality and consistency.",
  },
  {
    icon: FlaskConical,
    title: "Product Reliability",
    description:
      "Our approach is centered on delivering pharmaceutical products that meet customer requirements.",
  },
  {
    icon: Target,
    title: "Customer Commitment",
    description:
      "We work to understand customer and business requirements and provide reliable support.",
  },
];

export function QualitySection() {
  return (
    <section className="border-y border-border bg-muted/40 py-16 sm:py-24">
      <Container>
        <div className="grid gap-12 lg:grid-cols-[0.85fr_1.15fr] lg:items-center lg:gap-16">
          <Reveal>
            <SectionHeading
              eyebrow="Quality & Reliability"
              title="Quality is at the heart of what we do"
              description="We believe reliable healthcare starts with a consistent focus on quality, responsible processes and strong customer relationships."
            />

            <div className="mt-7 flex items-start gap-3.5 rounded-2xl border border-primary/20 bg-primary-light p-5">
              <CheckCircle2 className="mt-0.5 size-5 shrink-0 text-primary" />
              <p className="text-sm leading-6 text-foreground/80">
                Our commitment is to build long-term trust through dependable
                pharmaceutical solutions and responsive service.
              </p>
            </div>
          </Reveal>

          <div className="space-y-4">
            {qualityPoints.map((point, index) => {
              const Icon = point.icon;
              return (
                <Reveal key={point.title} delay={index * 110}>
                  <article className="group flex items-start gap-5 rounded-3xl border border-border bg-card p-6 transition-all duration-300 hover:translate-x-1.5 hover:border-primary/40 hover:shadow-[0_18px_44px_rgba(8,127,91,0.12)]">
                    <span className="flex size-14 shrink-0 items-center justify-center rounded-2xl bg-primary-light text-primary transition-all duration-300 group-hover:bg-primary group-hover:text-primary-foreground">
                      <Icon className="size-6" />
                    </span>
                    <div>
                      <h3 className="text-lg font-bold text-foreground">
                        {point.title}
                      </h3>
                      <p className="mt-1.5 text-sm leading-6 text-muted-foreground">
                        {point.description}
                      </p>
                    </div>
                  </article>
                </Reveal>
              );
            })}
          </div>
        </div>
      </Container>
    </section>
  );
}
