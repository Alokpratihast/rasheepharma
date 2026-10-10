import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

const points = [
  {
    no: "01",
    title: "Our Approach",
    text: "We aim to serve customers, distributors and business partners with pharmaceutical products supported by consistent processes and a customer-focused approach.",
  },
  {
    no: "02",
    title: "Our Commitment",
    text: "Our focus is on maintaining dependable standards across products, service and business relationships while supporting healthcare requirements across markets.",
  },
];

export function AboutOverview() {
  return (
    <section className="py-16 sm:py-24">
      <Container>
        <div className="grid gap-12 lg:grid-cols-[0.9fr_1.1fr] lg:items-center lg:gap-16">
          <Reveal>
            <SectionHeading
              eyebrow="Who We Are"
              title="Focused on quality, reliability and long-term trust"
              description="RashePharma is committed to building dependable healthcare solutions through a strong focus on product quality, consistency and customer needs."
            />
          </Reveal>

          <div className="space-y-4">
            {points.map((point, index) => (
              <Reveal key={point.no} delay={index * 120}>
                <article className="group relative overflow-hidden rounded-3xl border border-border bg-card p-6 transition-all duration-300 hover:-translate-y-1 hover:border-primary/40 hover:shadow-[0_24px_50px_rgba(8,127,91,0.12)] sm:p-8">
                  <span
                    aria-hidden="true"
                    className="absolute -right-2 -top-6 select-none text-[110px] font-extrabold leading-none text-primary/[0.06] transition-colors group-hover:text-primary/[0.12]"
                  >
                    {point.no}
                  </span>
                  <div className="relative flex gap-5">
                    <span className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-primary to-teal-600 text-sm font-extrabold text-white shadow-[0_10px_24px_rgba(8,127,91,0.3)]">
                      {point.no}
                    </span>
                    <div>
                      <h3 className="text-xl font-bold text-foreground">
                        {point.title}
                      </h3>
                      <p className="mt-2 text-sm leading-7 text-muted-foreground">
                        {point.text}
                      </p>
                    </div>
                  </div>
                </article>
              </Reveal>
            ))}
          </div>
        </div>
      </Container>
    </section>
  );
}
