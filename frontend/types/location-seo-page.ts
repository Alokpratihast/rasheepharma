export interface LocationSeoPage {
  id: number;
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
  isPublished: boolean;
  isIndexable: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export type LocationSeoPageWriteInput = Omit<LocationSeoPage, "id" | "createdAtUtc" | "updatedAtUtc">;

export type LocationSeoRouteStatus = Pick<LocationSeoPage, "countrySlug" | "citySlug" | "isPublished" | "isIndexable">;
