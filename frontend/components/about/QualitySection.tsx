import {
  CheckCircle2,
  FlaskConical,
  ShieldCheck,
  Target,
} from "lucide-react";

import { Container } from "@/components/ui/container";

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
    <section className="bg-white py-14 sm:py-20">
      <Container>
        <div className="grid gap-10 lg:grid-cols-[0.8fr_1.2fr] lg:items-center">
          {/* Left Content */}
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
              Quality & Reliability
            </p>

            <h2 className="mt-3 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
              Quality is at the heart of what we do
            </h2>

            <p className="mt-4 text-sm leading-7 text-[#595959] sm:text-base">
              We believe reliable healthcare starts with a consistent focus
              on quality, responsible processes and strong customer
              relationships.
            </p>

            <div className="mt-6 flex items-start gap-3 rounded-xl border border-[#dcebea] bg-[#F4FAF8] p-4">
              <CheckCircle2 className="mt-0.5 size-5 shrink-0 text-[#3E8F96]" />

              <p className="text-sm leading-6 text-[#595959]">
                Our commitment is to build long-term trust through dependable
                pharmaceutical solutions and responsive service.
              </p>
            </div>
          </div>

          {/* Quality Cards */}
          <div className="grid gap-4 sm:grid-cols-3">
            {qualityPoints.map((point) => {
              const Icon = point.icon;

              return (
                <article
                  key={point.title}
                  className="rounded-2xl border border-[#e4e9e7] bg-[#fafcfc] p-5 transition-shadow hover:shadow-sm"
                >
                  <div className="flex size-11 items-center justify-center rounded-xl bg-[#EAF5F3]">
                    <Icon className="size-5 text-[#3E8F96]" />
                  </div>

                  <h3 className="mt-4 text-sm font-semibold text-[#1B2A4A]">
                    {point.title}
                  </h3>

                  <p className="mt-2 text-xs leading-5 text-[#666]">
                    {point.description}
                  </p>
                </article>
              );
            })}
          </div>
        </div>
      </Container>
    </section>
  );
}