import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

const pillars = [
  {
    title: "Our approach",
    text: "We commit ourselves to supplying high quality products that serve healthcare, backed by excellent customer service and superior processes that add value to every delivery.",
  },
  {
    title: "Our commitment",
    text: "Reliability, quality and a wide distribution network assure timely delivery. Our long-term bond and trust with customers reflects our product quality and consistent service.",
  },
];

const direction = [
  {
    label: "Why we started",
    text: "To reach quality medications to all people and ease the illness of patients, whatever their economic conditions.",
  },
  {
    label: "How we are different",
    text: "We follow an efficient business model, unlike big pharma, that keeps treatments for serious illness available and accessible.",
  },
];

export function AboutStory() {
  return (
    <section className="border-t border-border py-20 sm:py-28">
      <Container>
        <div className="grid gap-12 lg:grid-cols-12 lg:gap-16">
          <Reveal className="lg:col-span-5">
            <SectionHeading
              index="01"
              eyebrow="Who we are"
              title="An emerging Bangalore company with deep pharmacy experience."
            />
          </Reveal>

          <div className="lg:col-span-7">
            <Reveal>
              <p className="text-xl leading-9 text-foreground sm:text-2xl sm:leading-10">
                Rashe Lifesciences is a renowned name in the marketing and
                supply of medicines that help treat serious illness, and we
                ensure our products are consistently produced to meet quality
                standards.
              </p>
            </Reveal>

            <div className="mt-12 grid gap-10 sm:grid-cols-2">
              {pillars.map((pillar, i) => (
                <Reveal key={pillar.title} delay={i * 120}>
                  <div className="border-t-2 border-primary pt-5">
                    <h3 className="text-lg font-semibold text-foreground">
                      {pillar.title}
                    </h3>
                    <p className="mt-3 text-sm leading-7 text-muted-foreground">
                      {pillar.text}
                    </p>
                  </div>
                </Reveal>
              ))}
            </div>
          </div>
        </div>

        <div className="mt-16 grid gap-px overflow-hidden rounded-2xl border border-border bg-border md:grid-cols-2">
          {direction.map((item, i) => (
            <Reveal key={item.label} delay={i * 120} className="bg-card">
              <div className="h-full p-8 sm:p-10">
                <p className="font-mono text-[11px] uppercase tracking-[0.16em] text-primary">
                  {item.label}
                </p>
                <p className="mt-5 text-xl font-medium leading-8 tracking-tight text-foreground sm:text-2xl sm:leading-9">
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
