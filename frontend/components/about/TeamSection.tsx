import { Handshake, Headset, ShieldCheck, UsersRound } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

const teamPoints = [
  {
    icon: UsersRound,
    title: "Experienced Team",
    description:
      "Our team brings a focused approach to pharmaceutical operations and customer requirements.",
  },
  {
    icon: Headset,
    title: "Customer Support",
    description:
      "We value clear communication and responsive support throughout every business interaction.",
  },
  {
    icon: Handshake,
    title: "Strong Partnerships",
    description:
      "We aim to build long-term relationships with customers, distributors and business partners.",
  },
  {
    icon: ShieldCheck,
    title: "Responsible Approach",
    description:
      "We work with a focus on consistency, reliability and responsible business practices.",
  },
];

export function TeamSection() {
  return (
    <section className="border-y border-border bg-muted/40 py-16 sm:py-24">
      <Container>
        <div className="grid gap-12 lg:grid-cols-[0.8fr_1.2fr] lg:items-center lg:gap-16">
          <Reveal>
            <SectionHeading
              eyebrow="Our Team"
              title="People focused on delivering better healthcare solutions"
              description="A dependable pharmaceutical business is built on people, processes and relationships. We aim to bring these together to serve our customers and partners effectively."
            />
          </Reveal>

          <div className="grid gap-4 sm:grid-cols-2">
            {teamPoints.map((point, index) => {
              const Icon = point.icon;
              return (
                <Reveal key={point.title} delay={(index % 2) * 100 + Math.floor(index / 2) * 100}>
                  <article className="group relative h-full overflow-hidden rounded-3xl border border-border bg-card p-6 transition-all duration-300 hover:-translate-y-1.5 hover:border-primary/40 hover:shadow-[0_24px_50px_rgba(8,127,91,0.12)]">
                    <Icon
                      aria-hidden="true"
                      className="absolute -bottom-4 -right-4 size-28 text-primary/[0.06] transition-all duration-500 group-hover:rotate-6 group-hover:scale-110 group-hover:text-primary/[0.12]"
                    />
                    <span className="relative flex size-12 items-center justify-center rounded-2xl bg-primary-light text-primary transition-all duration-300 group-hover:bg-primary group-hover:text-primary-foreground">
                      <Icon className="size-6" />
                    </span>
                    <h3 className="relative mt-5 text-lg font-bold text-foreground">
                      {point.title}
                    </h3>
                    <p className="relative mt-2 text-sm leading-6 text-muted-foreground">
                      {point.description}
                    </p>
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
