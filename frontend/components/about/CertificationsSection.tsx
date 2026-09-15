import Image from "next/image";
import {
  Award,
  CheckCircle2,
  ShieldCheck,
} from "lucide-react";

import { Container } from "@/components/ui/container";

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
    <section className="bg-[#fafafa] py-14 sm:py-20">
      <Container>
        {/* =================================================
            HEADER
        ================================================== */}

        <div className="mx-auto max-w-3xl text-center">
          <div className="mx-auto flex size-12 items-center justify-center rounded-xl bg-[#EAF5F3]">
            <Award className="size-5 text-[#3E8F96]" />
          </div>

          <p className="mt-4 text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
            Quality Certifications
          </p>

          <h2 className="mt-3 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
            Quality-backed pharmaceutical operations
          </h2>

          <p className="mt-4 text-sm leading-7 text-[#595959] sm:text-base">
            Our company profile includes certificates related to
            pharmaceutical manufacturing and quality management systems.
          </p>
        </div>

        {/* =================================================
            CERTIFICATE CARDS
        ================================================== */}

        <div className="mt-10 grid gap-6 lg:grid-cols-2">
          {certifications.map((certificate) => (
            <article
              key={certificate.title}
              className="overflow-hidden rounded-2xl border border-[#e2e8e6] bg-white shadow-sm transition-all duration-200 hover:-translate-y-1 hover:shadow-lg"
            >
              <div className="grid md:grid-cols-[0.8fr_1.2fr]">
                {/* Certificate Image */}
                <div className="relative min-h-[320px] bg-[#f3f6f5]">
                  <Image
                    src={certificate.image}
                    alt={`${certificate.title} certificate`}
                    fill
                    sizes="(max-width: 768px) 100vw, 40vw"
                    className="object-contain p-5"
                  />
                </div>

                {/* Certificate Content */}
                <div className="flex flex-col justify-center p-6 sm:p-7">
                  <div className="flex size-11 items-center justify-center rounded-xl bg-[#EAF5F3]">
                    <ShieldCheck className="size-5 text-[#3E8F96]" />
                  </div>

                  <p className="mt-5 text-xs font-semibold uppercase tracking-[0.14em] text-[#F5821F]">
                    Certificate
                  </p>

                  <h3 className="mt-2 text-2xl font-semibold tracking-tight text-[#1B2A4A]">
                    {certificate.title}
                  </h3>

                  <p className="mt-1 text-sm font-medium text-[#3E8F96]">
                    {certificate.subtitle}
                  </p>

                  <p className="mt-4 text-sm leading-6 text-[#666]">
                    {certificate.description}
                  </p>

                  <div className="mt-5 flex items-start gap-2 rounded-xl border border-[#e0ebe8] bg-[#F4FAF8] p-3">
                    <CheckCircle2 className="mt-0.5 size-4 shrink-0 text-[#3E8F96]" />

                    <p className="text-xs leading-5 text-[#666]">
                      Certificate document available as part of the company
                      profile.
                    </p>
                  </div>
                </div>
              </div>
            </article>
          ))}
        </div>

        {/* =================================================
            NOTE
        ================================================== */}

        <p className="mx-auto mt-6 max-w-3xl text-center text-[11px] leading-5 text-[#888]">
          Certificate details and validity should be presented according to
          the latest company-issued documentation.
        </p>
      </Container>
    </section>
  );
}