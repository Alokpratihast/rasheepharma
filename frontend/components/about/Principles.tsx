import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

/* Based on the "Why Choose Us" and About text from the company's existing website. */
const principles = [
  {
    title: "Certified manufacturing",
    text: "We are associated with ISO and WHO-GMP certified plants that manufacture essential drugs to consistent quality standards.",
  },
  {
    title: "24/7 dedicated service",
    text: "Reliable, on-time deliveries and a high order fulfilment rate, supported by customer care that stays responsive.",
  },
  {
    title: "Team of professionals",
    text: "Experienced consultation, high-calibre customer care and industry expertise from a team with 15 years in pharmacy.",
  },
  {
    title: "Larger geography coverage",
    text: "Relationships built on trust, and the result of quality and consistency in delivering our services.",
  },
];

export function Principles() {
  return (
    <section className="relative isolate overflow-hidden bg-brand-dark py-20 text-white sm:py-28">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -bottom-40 -left-32 -z-10 size-[520px] rounded-full bg-primary/30 blur-[120px]"
      />

      <Container>
        <Reveal>
          <SectionHeading
            invert
            index="05"
            eyebrow="Why choose us"
            title="Why businesses choose Rashe Lifesciences."
            description="Reliability and quality, a wide distribution network and long-term trust with our customers."
          />
        </Reveal>

        <div className="mt-16 grid gap-x-10 gap-y-12 sm:grid-cols-2 lg:grid-cols-4">
          {principles.map((item, i) => (
            <Reveal key={item.title} delay={i * 100}>
              <div className="h-full border-t border-white/20 pt-6">
                <span className="font-mono text-sm text-emerald-300">
                  {String(i + 1).padStart(2, "0")}
                </span>
                <h3 className="mt-6 text-xl font-semibold tracking-tight">
                  {item.title}
                </h3>
                <p className="mt-3 text-sm leading-7 text-white/65">
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
