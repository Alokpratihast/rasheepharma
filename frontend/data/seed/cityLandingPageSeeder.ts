export interface CityLandingPageSeed {
  country: string;
  countryCode: string;
  countrySlug: string;
  quoteCurrency: "INR" | "USD";
  city: string;
  region: string;
  citySlug: string;
  metaTitle: string;
  metaDescription: string;
  heading: string;
  intro: string;
  buyerFocus: string;
}

/**
 * Starter city landing-page seed data. Keep pages focused on B2B enquiries;
 * confirm actual product and destination availability before making promises.
 */
export const cityLandingPageSeeder: CityLandingPageSeed[] = [
  {
    country: "India",
    countryCode: "IN",
    countrySlug: "india",
    quoteCurrency: "INR",
    city: "Mumbai",
    region: "Maharashtra",
    citySlug: "mumbai",
    metaTitle: "B2B Pharma Enquiries in Mumbai | RashePharma",
    metaDescription:
      "Discuss pharmaceutical product sourcing, business requirements and partnership enquiries for your Mumbai-based healthcare business with RashePharma.",
    heading: "Pharmaceutical B2B Enquiries in Mumbai",
    intro:
      "Planning a pharmaceutical product enquiry for a Mumbai-based business? Share your requirements with RashePharma and our team can review product availability, documentation needs and the right next steps.",
    buyerFocus:
      "Use one clear enquiry to outline the product range, expected volumes and market requirements your procurement or distribution team needs reviewed.",
  },
  {
    country: "India",
    countryCode: "IN",
    countrySlug: "india",
    quoteCurrency: "INR",
    city: "Delhi",
    region: "Delhi",
    citySlug: "delhi",
    metaTitle: "B2B Pharma Enquiries in Delhi | RashePharma",
    metaDescription:
      "Connect with RashePharma about pharmaceutical product requirements, documentation and potential B2B partnerships for businesses in Delhi.",
    heading: "Pharmaceutical B2B Enquiries in Delhi",
    intro:
      "For healthcare and pharmaceutical businesses in Delhi, start a conversation about the products and commercial requirements you are evaluating. RashePharma will review the enquiry and confirm what can be supported.",
    buyerFocus:
      "Tell us whether your enquiry is for distribution, institutional procurement or another business requirement so the team can route it appropriately.",
  },
  {
    country: "India",
    countryCode: "IN",
    countrySlug: "india",
    quoteCurrency: "INR",
    city: "Ahmedabad",
    region: "Gujarat",
    citySlug: "ahmedabad",
    metaTitle: "B2B Pharma Enquiries in Ahmedabad | RashePharma",
    metaDescription:
      "Submit an Ahmedabad business enquiry to discuss pharmaceutical products, documentation and possible supply or partnership requirements with RashePharma.",
    heading: "Pharmaceutical B2B Enquiries in Ahmedabad",
    intro:
      "Looking into pharmaceutical products for a business in Ahmedabad? Send your product and market requirements to RashePharma for an availability and documentation review.",
    buyerFocus:
      "A useful first enquiry includes the product or category, intended market, expected quantity and the business documents you need to evaluate.",
  },
  {
    country: "India",
    countryCode: "IN",
    countrySlug: "india",
    quoteCurrency: "INR",
    city: "Hyderabad",
    region: "Telangana",
    citySlug: "hyderabad",
    metaTitle: "B2B Pharma Enquiries in Hyderabad | RashePharma",
    metaDescription:
      "Talk to RashePharma about pharmaceutical product sourcing, business documentation and B2B enquiry requirements for Hyderabad-based organisations.",
    heading: "Pharmaceutical B2B Enquiries in Hyderabad",
    intro:
      "RashePharma welcomes product and partnership enquiries from healthcare businesses in Hyderabad. Tell us what your organisation is evaluating and the team will respond with the relevant next steps.",
    buyerFocus:
      "For a focused review, include the product category, intended use in the business, target market and any documentation questions.",
  },
  {
    country: "India",
    countryCode: "IN",
    countrySlug: "india",
    quoteCurrency: "INR",
    city: "Bengaluru",
    region: "Karnataka",
    citySlug: "bengaluru",
    metaTitle: "B2B Pharma Enquiries in Bengaluru | RashePharma",
    metaDescription:
      "Explore pharmaceutical product and B2B partnership enquiries for Bengaluru businesses. Share requirements with RashePharma for review.",
    heading: "Pharmaceutical B2B Enquiries in Bengaluru",
    intro:
      "If your healthcare or pharmaceutical business is based in Bengaluru, use this page to start a product or partnership enquiry with RashePharma. Availability and requirements are confirmed individually.",
    buyerFocus:
      "Share your organisation type, product interests, expected volumes and target market to help the team understand your business enquiry.",
  },
  {
    country: "India",
    countryCode: "IN",
    countrySlug: "india",
    quoteCurrency: "INR",
    city: "Chennai",
    region: "Tamil Nadu",
    citySlug: "chennai",
    metaTitle: "B2B Pharma Enquiries in Chennai | RashePharma",
    metaDescription:
      "Contact RashePharma with Chennai-based business requirements for pharmaceutical products, documentation review and potential B2B partnerships.",
    heading: "Pharmaceutical B2B Enquiries in Chennai",
    intro:
      "For businesses in Chennai evaluating pharmaceutical products or a potential partnership, RashePharma can review the details you share and confirm suitable next steps.",
    buyerFocus:
      "Keep your enquiry specific: name the product or category, intended market, expected quantity and any supply or documentation questions.",
  },
  {
    country: "United Arab Emirates",
    countryCode: "AE",
    countrySlug: "united-arab-emirates",
    quoteCurrency: "USD",
    city: "Dubai",
    region: "Dubai",
    citySlug: "dubai",
    metaTitle: "B2B Pharma Enquiries in Dubai, UAE | RashePharma",
    metaDescription:
      "Discuss pharmaceutical product requirements and B2B partnership enquiries for your Dubai-based organisation with RashePharma. International quotes can be requested in USD.",
    heading: "Pharmaceutical B2B Enquiries in Dubai",
    intro:
      "For healthcare and pharmaceutical businesses in Dubai evaluating a product or partnership opportunity, share your market and documentation requirements with RashePharma for review.",
    buyerFocus:
      "Include the product category, intended UAE market, expected volumes and any documentation requirements. Quote currency can be requested in USD.",
  },
  {
    country: "Saudi Arabia",
    countryCode: "SA",
    countrySlug: "saudi-arabia",
    quoteCurrency: "USD",
    city: "Riyadh",
    region: "Riyadh Province",
    citySlug: "riyadh",
    metaTitle: "B2B Pharma Enquiries in Riyadh, Saudi Arabia | RashePharma",
    metaDescription:
      "Send RashePharma a pharmaceutical product or partnership enquiry for a Riyadh-based business. Share Saudi market and documentation requirements; request a USD quote.",
    heading: "Pharmaceutical B2B Enquiries in Riyadh",
    intro:
      "If your organisation in Riyadh is assessing pharmaceutical products or a business partnership, send the product and destination requirements for an initial review.",
    buyerFocus:
      "Mention your intended Saudi market, product category, expected volumes and documentation questions. International quotations may be requested in USD.",
  },
  {
    country: "Qatar",
    countryCode: "QA",
    countrySlug: "qatar",
    quoteCurrency: "USD",
    city: "Doha",
    region: "Doha",
    citySlug: "doha",
    metaTitle: "B2B Pharma Enquiries in Doha, Qatar | RashePharma",
    metaDescription:
      "Connect with RashePharma about pharmaceutical products and business partnership requirements for a Doha-based organisation. Request international quotations in USD.",
    heading: "Pharmaceutical B2B Enquiries in Doha",
    intro:
      "For healthcare businesses in Doha considering a product enquiry, share your target market, product interests and documentation needs so RashePharma can review the request.",
    buyerFocus:
      "A clear enquiry for Qatar should include product or category, expected quantities and any local documentation questions. USD quote requests are supported for international enquiries.",
  },
  {
    country: "United Kingdom",
    countryCode: "GB",
    countrySlug: "united-kingdom",
    quoteCurrency: "USD",
    city: "London",
    region: "England",
    citySlug: "london",
    metaTitle: "B2B Pharma Enquiries in London, UK | RashePharma",
    metaDescription:
      "Discuss pharmaceutical product and partnership enquiries for a London-based business with RashePharma. Share target-market and documentation needs; request a USD quote.",
    heading: "Pharmaceutical B2B Enquiries in London",
    intro:
      "RashePharma can review product and partnership enquiries from businesses in London. Share the product scope, intended market and documentation questions to begin the conversation.",
    buyerFocus:
      "For UK enquiries, identify your organisation type, product requirements and intended market. Any availability and destination requirements are reviewed before next steps.",
  },
  {
    country: "United States",
    countryCode: "US",
    countrySlug: "united-states",
    quoteCurrency: "USD",
    city: "New York",
    region: "New York",
    citySlug: "new-york",
    metaTitle: "B2B Pharma Enquiries in New York, USA | RashePharma",
    metaDescription:
      "Submit a pharmaceutical product or B2B partnership enquiry for a New York-based organisation. RashePharma can review requirements and discuss USD quotations.",
    heading: "Pharmaceutical B2B Enquiries in New York",
    intro:
      "For organisations in New York evaluating pharmaceutical products or a potential partnership, provide your product scope and intended market for an initial review by RashePharma.",
    buyerFocus:
      "Share the relevant product category, expected volumes and business requirements. USD is the requested quotation currency for this international enquiry page.",
  },
  {
    country: "Canada",
    countryCode: "CA",
    countrySlug: "canada",
    quoteCurrency: "USD",
    city: "Toronto",
    region: "Ontario",
    citySlug: "toronto",
    metaTitle: "B2B Pharma Enquiries in Toronto, Canada | RashePharma",
    metaDescription:
      "Contact RashePharma about pharmaceutical product requirements and potential B2B partnerships for your Toronto-based organisation. Request a USD quotation.",
    heading: "Pharmaceutical B2B Enquiries in Toronto",
    intro:
      "Toronto-based healthcare and pharmaceutical businesses can use this page to share product requirements and partnership enquiries for review by RashePharma.",
    buyerFocus:
      "Include the product or category, target market, expected quantity and documentation questions. Destination and availability checks are part of the enquiry review.",
  },
  {
    country: "Australia",
    countryCode: "AU",
    countrySlug: "australia",
    quoteCurrency: "USD",
    city: "Sydney",
    region: "New South Wales",
    citySlug: "sydney",
    metaTitle: "B2B Pharma Enquiries in Sydney, Australia | RashePharma",
    metaDescription:
      "Share pharmaceutical product and partnership requirements for a Sydney-based business with RashePharma. Destination requirements are reviewed; USD quotes can be requested.",
    heading: "Pharmaceutical B2B Enquiries in Sydney",
    intro:
      "If your business in Sydney is assessing a pharmaceutical product or partnership opportunity, share the request with RashePharma for an initial review.",
    buyerFocus:
      "Describe your product scope, intended Australian market and documentation needs. Product availability and destination requirements are confirmed individually.",
  },
  {
    country: "Germany",
    countryCode: "DE",
    countrySlug: "germany",
    quoteCurrency: "USD",
    city: "Frankfurt",
    region: "Hesse",
    citySlug: "frankfurt",
    metaTitle: "B2B Pharma Enquiries in Frankfurt, Germany | RashePharma",
    metaDescription:
      "Discuss pharmaceutical product and B2B partnership enquiries for a Frankfurt-based business with RashePharma. Share destination requirements and request a USD quotation.",
    heading: "Pharmaceutical B2B Enquiries in Frankfurt",
    intro:
      "Businesses in Frankfurt can share pharmaceutical product enquiries and partnership requirements with RashePharma for an initial review of the request.",
    buyerFocus:
      "Include the product or category, intended market and any documentation questions. Availability and regulatory requirements are assessed before any commercial next steps.",
  },
  {
    country: "Singapore",
    countryCode: "SG",
    countrySlug: "singapore",
    quoteCurrency: "USD",
    city: "Singapore",
    region: "Singapore",
    citySlug: "singapore",
    metaTitle: "B2B Pharma Enquiries in Singapore | RashePharma",
    metaDescription:
      "Send RashePharma a pharmaceutical product or partnership enquiry for a Singapore-based business. Share product and destination requirements; request a USD quote.",
    heading: "Pharmaceutical B2B Enquiries in Singapore",
    intro:
      "For a Singapore-based healthcare or pharmaceutical organisation evaluating a product or partnership, RashePharma can review your requirements and discuss next steps.",
    buyerFocus:
      "Outline your product scope, target market, expected volume and documentation requirements. International quotations can be requested in USD.",
  },
];

export function getCityLandingPage(countrySlug: string, citySlug: string) {
  return cityLandingPageSeeder.find(
    (page) =>
      page.countrySlug === countrySlug.toLowerCase() &&
      page.citySlug === citySlug.toLowerCase(),
  );
}
