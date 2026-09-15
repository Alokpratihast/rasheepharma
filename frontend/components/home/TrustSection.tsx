import {
  Building2,
  Globe2,
  HandCoins,
  Handshake,
  Landmark,
  UserRound,
} from "lucide-react";

import { Container } from "@/components/ui/container";

const companyDetails = [
  {
    title: "Nature of Business",
    value: "Manufacturers, Exporters, Wholesaler, Retailer, Trader",
    icon: Handshake,
  },
  {
    title: "Year of Establishment",
    value: "2019",
    icon: Building2,
  },
  {
    title: "Market Covered",
    value: "Worldwide",
    icon: Globe2,
  },
  {
    title: "Name of Founder",
    value: "Mr. Shekappa",
    icon: UserRound,
  },
  {
    title: "GST No",
    value: "29AAJCR5569D1ZY",
    icon: Landmark,
  },
  {
    title: "Annual Turnover",
    value: "Rs. 50 Lakh - 1 Crore",
    icon: HandCoins,
  },
];

export function TrustSection() {
  return (
    <section className="bg-[#f7f7f7] py-14 sm:py-16">
      <Container>
        {/* =================================================
            SECTION INTRO
        ================================================== */}

        <div className="mx-auto max-w-3xl text-center">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
            About RashePharma
          </p>

          <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[#1B2A4A] sm:text-3xl">
            A Trusted Pharmaceutical Company
          </h2>

          <p className="mt-4 text-sm leading-6 text-[#595959] sm:text-base sm:leading-7">
            Rashe Lifesciences is committed to supplying quality pharmaceutical
            products with reliable service and long-term business partnerships.
          </p>
        </div>

        {/* =================================================
            COMPANY INFORMATION
        ================================================== */}

        <div className="mt-10 grid gap-x-10 gap-y-8 md:grid-cols-2 lg:grid-cols-3">
          {companyDetails.map((detail) => {
            const Icon = detail.icon;

            return (
              <div
                key={detail.title}
                className="flex items-center gap-5"
              >
                {/* Icon */}
                <div className="flex size-20 shrink-0 items-center justify-center rounded-full border border-[#d7d7d7] bg-white text-[#555] shadow-sm">
                  <Icon
                    aria-hidden="true"
                    className="size-9 stroke-[1.5]"
                  />
                </div>

                {/* Content */}
                <div className="min-w-0">
                  <h3 className="text-base font-semibold text-[#1B2A4A]">
                    {detail.title}
                  </h3>

                  <p className="mt-1 text-sm leading-6 text-[#595959]">
                    {detail.value}
                  </p>
                </div>
              </div>
            );
          })}
        </div>
      </Container>
    </section>
  );
}