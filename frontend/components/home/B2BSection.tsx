import Link from "next/link";
import {
  ArrowRight,
  Boxes,
  Globe2,
  MessageSquareText,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";

const benefits = [
  {
    title: "Bulk Requirements",
    description:
      "Share your pharmaceutical product and quantity requirements with our team.",
    icon: Boxes,
  },
  {
    title: "Global Business",
    description:
      "Connect with us for international distribution and business opportunities.",
    icon: Globe2,
  },
  {
    title: "Quick Enquiry",
    description:
      "Get product information and business assistance through a simple enquiry.",
    icon: MessageSquareText,
  },
];

export function B2BSection() {
  return (
    <section className="bg-background py-14 sm:py-16">
      <Container>
        <div className="overflow-hidden rounded-xl bg-[#1B2A4A]">
          <div className="grid lg:grid-cols-[1.25fr_0.75fr]">
            {/* Content */}
            <div className="p-7 sm:p-10 lg:p-12">
              <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
                B2B & Business Enquiries
              </p>

              <h2 className="mt-3 max-w-2xl text-2xl font-semibold tracking-tight text-white sm:text-3xl">
                Looking for a reliable pharmaceutical partner?
              </h2>

              <p className="mt-4 max-w-2xl text-sm leading-6 text-white/70 sm:text-base">
                Connect with RashePharma for bulk requirements, product
                enquiries, distribution opportunities and long-term business
                partnerships.
              </p>

              <div className="mt-7 flex flex-col gap-3 sm:flex-row">
                <Link href="/b2b/enquiry">
                  <Button
                    size="lg"
                    className="h-11 rounded-lg bg-[#F5821F] px-6 text-white hover:bg-[#df7115]"
                  >
                    Request a Quote
                    <ArrowRight className="size-4" />
                  </Button>
                </Link>

                <Link href="/b2b/enquiry">
                  <Button
                    variant="outline"
                    size="lg"
                    className="h-11 rounded-lg border-white/20 bg-transparent px-6 text-white hover:bg-white/10 hover:text-white"
                  >
                    Become a Partner
                  </Button>
                </Link>
              </div>
            </div>

            {/* Benefits */}
            <div className="border-t border-white/10 bg-[#223554] p-6 lg:border-l lg:border-t-0">
              <div className="space-y-3">
                {benefits.map((benefit) => {
                  const Icon = benefit.icon;

                  return (
                    <div
                      key={benefit.title}
                      className="flex gap-4 rounded-lg border border-white/10 bg-white/5 p-4"
                    >
                      <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-[#3E8F96]/20 text-[#72c5cb]">
                        <Icon className="size-5" />
                      </div>

                      <div>
                        <h3 className="text-sm font-semibold text-white">
                          {benefit.title}
                        </h3>

                        <p className="mt-1 text-xs leading-5 text-white/60">
                          {benefit.description}
                        </p>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
        </div>
      </Container>
    </section>
  );
}