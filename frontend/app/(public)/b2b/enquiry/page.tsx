import { EnquiryForm } from "@/components/forms/EnquiryForm";
import type { Country } from "react-phone-number-input";
import { cityLandingPageSeeder } from "@/data/seed/cityLandingPageSeeder";

interface EnquiryPageProps {
  searchParams: Promise<{
    countryCode?: string;
    city?: string;
    quoteCurrency?: string;
  }>;
}

export default async function EnquiryPage({ searchParams }: EnquiryPageProps) {
  const query = await searchParams;
  const location = cityLandingPageSeeder.find(
    (page) =>
      page.countryCode === query.countryCode?.toUpperCase() &&
      (!query.city || page.citySlug === query.city.toLowerCase().replaceAll(" ", "-")),
  );
  const initialMessage = location
    ? `B2B enquiry for ${query.city ? `${location.city}, ` : ""}${location.country}. ${location.quoteCurrency === "USD" ? "Please provide a quotation in USD. " : ""}Please contact me about product availability, documentation and business requirements.`
    : undefined;

  return (
    <main className="min-h-screen bg-gray-50 py-10">
      <div className="mx-auto w-full max-w-4xl px-4 sm:px-6 lg:px-8">
        <EnquiryForm
          initialCountry={location?.countryCode as Country | undefined}
          initialMessage={initialMessage}
        />
      </div>
    </main>
  );
}
