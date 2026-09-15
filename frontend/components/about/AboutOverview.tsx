import { Container } from "@/components/ui/container";

export function AboutOverview() {
  return (
    <section className="bg-[#fafafa] py-14 sm:py-20">
      <Container>
        <div className="grid gap-10 lg:grid-cols-[0.9fr_1.1fr] lg:items-center">
          {/* Section Intro */}
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
              Who We Are
            </p>

            <h2 className="mt-3 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
              Focused on quality, reliability and long-term trust
            </h2>

            <p className="mt-4 text-sm leading-7 text-[#595959] sm:text-base">
              RashePharma is committed to building dependable healthcare
              solutions through a strong focus on product quality,
              consistency and customer needs.
            </p>
          </div>

          {/* Overview Content */}
          <div className="rounded-2xl border border-[#e4e9e7] bg-white p-6 shadow-sm sm:p-8">
            <div className="space-y-5">
              <div>
                <h3 className="text-lg font-semibold text-[#1B2A4A]">
                  Our Approach
                </h3>

                <p className="mt-2 text-sm leading-6 text-[#666]">
                  We aim to serve customers, distributors and business
                  partners with pharmaceutical products supported by
                  consistent processes and a customer-focused approach.
                </p>
              </div>

              <div className="border-t border-[#edf0ef] pt-5">
                <h3 className="text-lg font-semibold text-[#1B2A4A]">
                  Our Commitment
                </h3>

                <p className="mt-2 text-sm leading-6 text-[#666]">
                  Our focus is on maintaining dependable standards across
                  products, service and business relationships while
                  supporting healthcare requirements across markets.
                </p>
              </div>
            </div>
          </div>
        </div>
      </Container>
    </section>
  );
}