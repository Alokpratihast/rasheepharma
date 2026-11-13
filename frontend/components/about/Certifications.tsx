import Image from "next/image";
import { ArrowUpRight } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

const certifications = [
  {
    title: "WHO-GMP",
    subtitle: "Good Manufacturing Practice",
    description:
      "Certificate of Registration for manufacturing and trading of pharmaceutical preparations and nutraceutical products at our associated plant.",
    image: "/images/rashecertificate1.jpg",
  },
  {
    title: "ISO 9001:2015",
    subtitle: "Quality Management System",
    description:
      "Certificate of Registration for the quality management system of our associated manufacturing plant.",
    image: "/images/rashecertificate2.jpg",
  },
];

export function Certifications() {
  return (
    <section className="py-20 sm:py-28">
      <Container>
        <Reveal>
          <SectionHeading
            index="06"
            eyebrow="Certifications"
            title="Quality you can verify."
            description="Our products are manufactured at ISO and WHO-GMP certified plants we are associated with. These are the certificates of that manufacturing facility."
          />
        </Reveal>

        <div className="mt-14 grid gap-10 md:grid-cols-2 lg:gap-16">
          {certifications.map((cert, i) => (
            <Reveal key={cert.title} delay={i * 120}>
              <article className="grid items-start gap-6 sm:grid-cols-[minmax(0,220px)_1fr]">
                <a
                  href={cert.image}
                  target="_blank"
                  rel="noopener noreferrer"
                  aria-label={`View ${cert.title} certificate in full size`}
                  className="group relative block aspect-[350/489] overflow-hidden rounded-xl border border-border bg-muted shadow-sm transition-shadow hover:shadow-lg"
                >
                  <Image
                    src={cert.image}
                    alt={`${cert.title} certificate`}
                    fill
                    sizes="(max-width: 640px) 60vw, 220px"
                    className="object-cover transition-transform duration-500 group-hover:scale-[1.03]"
                  />
                  <span className="absolute inset-x-0 bottom-0 flex items-center justify-between bg-gradient-to-t from-black/70 to-transparent px-3 pb-3 pt-8 text-xs font-medium text-white opacity-0 transition-opacity group-hover:opacity-100 group-focus-visible:opacity-100">
                    View full size
                    <ArrowUpRight className="size-4" />
                  </span>
                </a>

                <div className="sm:pt-2">
                  <p className="font-mono text-[11px] uppercase tracking-[0.14em] text-primary">
                    {cert.subtitle}
                  </p>
                  <h3 className="mt-3 text-3xl font-semibold tracking-tight text-foreground">
                    {cert.title}
                  </h3>
                  <p className="mt-4 text-sm leading-7 text-muted-foreground">
                    {cert.description}
                  </p>
                </div>
              </article>
            </Reveal>
          ))}
        </div>

        <p className="mt-12 border-t border-border pt-5 text-xs leading-5 text-muted-foreground">
          Certificates belong to our associated manufacturing facility. Details
          and validity are as per the latest issued documentation.
        </p>
      </Container>
    </section>
  );
}
