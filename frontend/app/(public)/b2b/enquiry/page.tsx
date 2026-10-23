import { EnquiryForm } from "@/components/forms/EnquiryForm";
import type { Country } from "react-phone-number-input";
import { cityLandingPageSeeder } from "@/data/seed/cityLandingPageSeeder";
import type { LocationSeoPage } from "@/types/location-seo-page";

interface EnquiryPageProps {
  searchParams: Promise<{
    countryCode?: string;
    city?: string;
    countrySlug?: string;
    citySlug?: string;
    quoteCurrency?: string;
    productId?: string;
    productName?: string;
  }>;
}

export default async function EnquiryPage({ searchParams }: EnquiryPageProps) {
  const query = await searchParams;
  let location: (typeof cityLandingPageSeeder)[number] | LocationSeoPage | undefined;
  if (query.countrySlug && query.citySlug) {
    const apiBase = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5104/api").replace(/\/+$/, "");
    try {
      const response = await fetch(
        `${apiBase}/location-pages/${encodeURIComponent(query.countrySlug)}/${encodeURIComponent(query.citySlug)}`,
        { cache: "no-store", signal: AbortSignal.timeout(4000), headers: { Accept: "application/json" } },
      );
      if (response.ok) location = await response.json() as LocationSeoPage;
    } catch {
      // Fall back to the starter city data when the API is unavailable.
    }
  }
  location ??= cityLandingPageSeeder.find(
    (page) => page.countryCode === query.countryCode?.toUpperCase() &&
      (!query.citySlug || page.citySlug === query.citySlug.toLowerCase()) &&
      (!query.countrySlug || page.countrySlug === query.countrySlug.toLowerCase()),
  );
  const initialMessage = location
    ? `B2B enquiry for ${query.city || query.citySlug ? `${location.city}, ` : ""}${location.country}. ${location.quoteCurrency === "USD" ? "Please provide a quotation in USD. " : ""}Please contact me about product availability, documentation and business requirements.`
    : undefined;
  const requestedProductId = Number(query.productId);

  return (
    <main className="min-h-screen bg-gray-50 py-10">
      <div className="mx-auto w-full max-w-4xl px-4 sm:px-6 lg:px-8">
        <EnquiryForm
          initialCountry={location?.countryCode as Country | undefined}
          initialMessage={initialMessage}
          initialProductId={Number.isInteger(requestedProductId) && requestedProductId > 0 ? requestedProductId : undefined}
          productName={query.productName}
        />
      </div>
    </main>
  );
}
