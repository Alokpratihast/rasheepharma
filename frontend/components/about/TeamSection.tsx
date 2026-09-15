import {
  Handshake,
  Headset,
  ShieldCheck,
  UsersRound,
} from "lucide-react";

import { Container } from "@/components/ui/container";

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
    <section className="bg-[#fafafa] py-14 sm:py-20">
      <Container>
        <div className="grid gap-10 lg:grid-cols-[0.8fr_1.2fr] lg:items-center">
          {/* Intro */}
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
              Our Team
            </p>

            <h2 className="mt-3 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
              People focused on delivering better healthcare solutions
            </h2>

            <p className="mt-4 text-sm leading-7 text-[#595959] sm:text-base">
              A dependable pharmaceutical business is built on people,
              processes and relationships. We aim to bring these together
              to serve our customers and partners effectively.
            </p>
          </div>

          {/* Team Values */}
          <div className="grid gap-4 sm:grid-cols-2">
            {teamPoints.map((point) => {
              const Icon = point.icon;

              return (
                <article
                  key={point.title}
                  className="rounded-2xl border border-[#e4e9e7] bg-white p-5 shadow-sm transition-shadow hover:shadow-md"
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