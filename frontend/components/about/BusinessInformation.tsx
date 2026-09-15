import { Container } from "@/components/ui/container";

const businessDetails = [
  {
    label: "Nature of Business",
    value: "Manufacturers, Exporters, Wholesaler, Retailer, Trader",
  },
  {
    label: "Year of Establishment",
    value: "2019",
  },
  {
    label: "Market Covered",
    value: "Worldwide",
  },
  {
    label: "Name of Founder",
    value: "Mr. Shekappa",
  },
  {
    label: "GST No",
    value: "29AAJCR5569D1ZY",
  },
  {
    label: "Annual Turnover",
    value: "Rs. 50 Lakh - 1 Crore",
  },
  {
    label: "Legal Status of Firm",
    value: "Private Limited Company",
  },
];

export function BusinessInformation() {
  return (
    <section className="bg-[#fafafa] py-14 sm:py-20">
      <Container>
        <div className="mb-8">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
            Company Information
          </p>

          <h2 className="mt-3 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
            Business Details
          </h2>

          <p className="mt-3 max-w-2xl text-sm leading-7 text-[#595959] sm:text-base">
            Key information about our business, operations and company
            profile.
          </p>
        </div>

        <div className="overflow-hidden rounded-2xl border border-[#dfe4e3] bg-white">
          <div className="divide-y divide-[#dfe4e3]">
            {businessDetails.map((detail) => (
              <div
                key={detail.label}
                className="grid grid-cols-1 sm:grid-cols-[32%_68%]"
              >
                <div className="border-b border-[#dfe4e3] bg-[#fafafa] px-4 py-4 sm:border-b-0 sm:border-r">
                  <p className="text-sm font-semibold text-[#1B2A4A]">
                    {detail.label}
                  </p>
                </div>

                <div className="px-4 py-4">
                  <p className="text-sm leading-6 text-[#333]">
                    {detail.value}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </Container>
    </section>
  );
}