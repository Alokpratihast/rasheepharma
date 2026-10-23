import Link from "next/link";
import {
  ArrowRight,
  Globe2,
  Handshake,
  ShieldCheck,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { CertificateGallery } from "@/components/home/CertificateGallery";

const points = [
  {
    icon: ShieldCheck,
    title: "Quality products",
    description:
      "Pharmaceuticals backed by WHO-GMP and ISO 9001:2015 certificates.",
  },
  {
    icon: Globe2,
    title: "Reliable supply",
    description:
      "Dependable supply to customers and partners across global markets.",
  },
  {
    icon: Handshake,
    title: "Responsive service",
    description:
      "Product information and business support when you need it.",
  },
];

export function AboutSection() {
  return (
    <section className="bg-white pb-2 pt-14 sm:pt-20">
      <Container>
        <div className="grid items-center gap-12 lg:grid-cols-[0.9fr_1.1fr] lg:gap-16">
          {/* Certificates: the proof is the visual */}
          <CertificateGallery />

          {/* Content */}
          <div>
            <h2 className="max-w-xl text-3xl font-bold leading-[1.1] tracking-[-0.025em] text-brand-dark sm:text-4xl">
              Building trusted pharmaceutical partnerships
            </h2>

            <p className="mt-5 max-w-xl text-base leading-7 text-muted-foreground">
              RashePharma operates in the pharmaceutical sector with a
              focus on quality products, reliable supply and long-term
              business relationships across global markets.
            </p>

            <p className="mt-4 max-w-xl text-base leading-7 text-muted-foreground">
              Our product portfolio supports customers and business
              partners looking for dependable pharmaceutical solutions
              and responsive service.
            </p>

            <ul className="mt-8 grid gap-5">
              {points.map((point) => {
                const Icon = point.icon;

                return (
                  <li key={point.title} className="flex gap-4">
                    <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary-light text-primary">
                      <Icon aria-hidden="true" className="size-5" />
                    </span>

                    <div>
                      <h3 className="text-base font-semibold text-brand-dark">
                        {point.title}
                      </h3>

                      <p className="mt-0.5 text-sm leading-6 text-muted-foreground">
                        {point.description}
                      </p>
                    </div>
                  </li>
                );
              })}
            </ul>

            <Link
              href="/about"
              className="group mt-9 inline-flex h-12 items-center justify-center gap-2 rounded-full bg-brand-dark px-7 text-sm font-semibold text-white shadow-[0_10px_24px_rgba(7,63,50,0.22)] transition-all hover:-translate-y-0.5 hover:bg-primary"
            >
              Learn more about us
              <ArrowRight className="size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
            </Link>
          </div>
        </div>
      </Container>
    </section>
  );
}
