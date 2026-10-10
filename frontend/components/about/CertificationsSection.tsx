import Image from "next/image";
import { Award, BadgeCheck, ShieldCheck } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";

const certifications = [
  {
    title: "WHO-GMP",
    subtitle: "Good Manufacturing Practice",
    description:
      "Certificate of Registration issued for manufacturing and trading of pharmaceutical preparations and nutraceutical products.",
    image: "/images/rashecertificate1.jpg",
  },
  {
    title: "ISO 9001:2015",
    subtitle: "Quality Management System",
    description:
      "Certificate of Registration for the quality management system applicable to pharmaceutical manufacturing and related activities.",
    image: "/images/rashecertificate2.jpg",
  },
];

export function CertificationsSection() {
  return (
    <section className="py-16 sm:py-24">
      <Container>
        <Reveal>
          <SectionHeading
            align="center"
            eyebrow="Quality Certifications"
            title="Quality-backed pharmaceutical operations"
            description="Our company profile includes certificates related to pharmaceutical manufacturing and quality management systems."
          />
        </Reveal>

        <div className="mt-12 grid gap-6 lg:grid-cols-2">
          {certifications.map((certificate, index) => (
            <Reveal key={certificate.title} delay={index * 130}>
              <article className="group h-full overflow-hidden rounded-[32px] border border-border bg-card transition-all duration-300 hover:-translate-y-1.5 hover:border-primary/40 hover:shadow-[0_28px_60px_rgba(8,127,91,0.14)]">
                <div className="grid h-full md:grid-cols-[0.82fr_1.18fr]">
                  {/* Certificate image */}
                  <div className="relative min-h-[340px] bg-gradient-to-br from-primary-light via-muted to-background">
                    <div
                      aria-hidden="true"
                      className="absolute -left-10 -top-10 size-40 rounded-full bg-primary/10 blur-2xl"
                    />
                    <Image
                      src={certificate.image}
                      alt={`${certificate.title} certificate`}
                      fill
                      sizes="(max-width: 768px) 100vw, 22vw"
                      className="object-contain p-7 drop-shadow-[0_16px_24px_rgba(7,63,50,0.18)] transition-transform duration-500 group-hover:scale-[1.04]"
                    />
                  </div>

                  {/* Content */}
                  <div className="flex flex-col justify-center p-7 sm:p-8">
                    <span className="inline-flex w-fit items-center gap-1.5 rounded-full bg-emerald-500/10 px-3 py-1 text-[11px] font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400">
                      <BadgeCheck className="size-3.5" />
                      Certified
                    </span>

                    <h3 className="mt-4 text-3xl font-extrabold tracking-tight text-foreground">
                      {certificate.title}
                    </h3>
                    <p className="mt-1 text-sm font-semibold text-primary">
                      {certificate.subtitle}
                    </p>
                    <p className="mt-4 text-sm leading-7 text-muted-foreground">
                      {certificate.description}
                    </p>

                    <div className="mt-6 flex items-center gap-3 rounded-2xl border border-border bg-muted/50 p-3.5">
                      <span className="flex size-9 shrink-0 items-center justify-center rounded-xl bg-primary-light text-primary">
                        <ShieldCheck className="size-[18px]" />
                      </span>
                      <p className="text-xs leading-5 text-muted-foreground">
                        Certificate document available as part of the company
                        profile.
                      </p>
                    </div>
                  </div>
                </div>
              </article>
            </Reveal>
          ))}
        </div>

        <p className="mx-auto mt-8 flex max-w-3xl items-center justify-center gap-2 text-center text-[11px] leading-5 text-muted-foreground">
          <Award className="size-3.5 shrink-0" />
          Certificate details and validity should be presented according to the
          latest company-issued documentation.
        </p>
      </Container>
    </section>
  );
}
