import { apiClient } from "@/lib/api/client";
import type { LocationSeoPage, LocationSeoPageWriteInput, LocationSeoRouteStatus } from "@/types/location-seo-page";

const PUBLIC = "/location-pages";
const ADMIN = "/admin/location-pages";

export const locationSeoService = {
  getPublished() {
    return apiClient<LocationSeoPage[]>(PUBLIC);
  },
  getPublishedBySlug(countrySlug: string, citySlug: string) {
    return apiClient<LocationSeoPage>(`${PUBLIC}/${encodeURIComponent(countrySlug)}/${encodeURIComponent(citySlug)}`);
  },
  getRouteStatuses() {
    return apiClient<LocationSeoRouteStatus[]>(`${PUBLIC}/status`);
  },
  getAdminPages() {
    return apiClient<LocationSeoPage[]>(ADMIN);
  },
  create(input: LocationSeoPageWriteInput) {
    return apiClient<LocationSeoPage>(ADMIN, { method: "POST", body: JSON.stringify(input) });
  },
  update(id: number, input: LocationSeoPageWriteInput) {
    return apiClient<LocationSeoPage>(`${ADMIN}/${id}`, { method: "PUT", body: JSON.stringify(input) });
  },
  delete(id: number) {
    return apiClient<void>(`${ADMIN}/${id}`, { method: "DELETE" });
  },
};
