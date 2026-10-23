import type { MetadataRoute } from "next";

import type { Category } from "@/types/category";
import type { PublicBlogPostPage } from "@/types/blog";
import type { ProductList } from "@/types/product";
import { cityLandingPageSeeder } from "@/data/seed/cityLandingPageSeeder";
import type { LocationSeoPage, LocationSeoRouteStatus } from "@/types/location-seo-page";

const apiBase = (
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5104/api"
).replace(/\/+$/, "");

// Keep canonical sitemap URLs independent of the API host and Azure preview URLs.
const siteUrl = (
  process.env.NEXT_PUBLIC_SITE_URL ?? "https://www.rashelifesciences.in"
).replace(/\/+$/, "");

const staticRoutes = [
  { path: "", changeFrequency: "weekly", priority: 1 },
  { path: "/about", changeFrequency: "monthly", priority: 0.7 },
  { path: "/contact", changeFrequency: "monthly", priority: 0.7 },
  { path: "/products", changeFrequency: "daily", priority: 0.9 },
  { path: "/categories", changeFrequency: "weekly", priority: 0.8 },
  { path: "/blog", changeFrequency: "weekly", priority: 0.7 },
  { path: "/privacy-policy", changeFrequency: "yearly", priority: 0.3 },
  { path: "/cancellation-refund", changeFrequency: "yearly", priority: 0.3 },
  { path: "/b2b/enquiry", changeFrequency: "monthly", priority: 0.5 },
] as const;

async function fetchPublicJson<T>(path: string): Promise<T | null> {
  try {
    const response = await fetch(`${apiBase}${path}`, {
      next: { revalidate: 3600 },
      signal: AbortSignal.timeout(10_000),
      headers: { Accept: "application/json" },
    });

    if (!response.ok) {
      console.error(`Sitemap data request failed (${response.status}): ${path}`);
      return null;
    }

    return (await response.json()) as T;
  } catch (error) {
    console.error(`Sitemap data request failed: ${path}`, error);
    return null;
  }
}

async function getPublishedPosts(): Promise<NonNullable<PublicBlogPostPage["items"]>> {
  const firstPage = await fetchPublicJson<PublicBlogPostPage>(
    "/blog/posts?page=1&pageSize=100",
  );

  if (!firstPage) return [];

  const pageCount = Math.min(Math.ceil(firstPage.totalCount / 100), 500);
  const remainingPages: Array<PublicBlogPostPage | null> = [];

  // Limit concurrency so a large blog cannot overwhelm the API during crawls.
  for (let startPage = 2; startPage <= pageCount; startPage += 10) {
    const batch = Array.from(
      { length: Math.min(10, pageCount - startPage + 1) },
      (_, index) => startPage + index,
    );
    remainingPages.push(
      ...(await Promise.all(
        batch.map((page) =>
          fetchPublicJson<PublicBlogPostPage>(
            `/blog/posts?page=${page}&pageSize=100`,
          ),
        ),
      )),
    );
  }

  return [
    ...firstPage.items,
    ...remainingPages.flatMap((page) => page?.items ?? []),
  ].slice(0, 50_000);
}

type SitemapLocation = (typeof cityLandingPageSeeder)[number] & { isPublished?: boolean; isIndexable?: boolean };

async function getLocationPages(): Promise<SitemapLocation[]> {
  try {
    const [pagesResponse, statusResponse] = await Promise.all([
      fetch(`${apiBase}/location-pages`, { next: { revalidate: 3600 }, signal: AbortSignal.timeout(10_000) }),
      fetch(`${apiBase}/location-pages/status`, { next: { revalidate: 3600 }, signal: AbortSignal.timeout(10_000) }),
    ]);
    const saved = pagesResponse.ok ? await pagesResponse.json() as LocationSeoPage[] : [];
    const statuses = statusResponse.ok ? await statusResponse.json() as LocationSeoRouteStatus[] : [];
    const savedByRoute = new Map(saved.map(page => [`${page.countrySlug}/${page.citySlug}`, page]));
    const statusByRoute = new Map(statuses.map(page => [`${page.countrySlug}/${page.citySlug}`, page]));
    const initial = cityLandingPageSeeder.flatMap(seed => {
      const route = `${seed.countrySlug}/${seed.citySlug}`;
      if (statusByRoute.get(route)?.isPublished === false) return [];
      return [savedByRoute.get(route) ?? { ...seed, isIndexable: true }];
    });
    const initialRoutes = new Set(cityLandingPageSeeder.map(page => `${page.countrySlug}/${page.citySlug}`));
    return [...initial, ...saved.filter(page => !initialRoutes.has(`${page.countrySlug}/${page.citySlug}`))];
  } catch {
    return cityLandingPageSeeder.map(page => ({ ...page, isPublished: true, isIndexable: true }));
  }
}
// Generate at request time so a frontend build does not need a reachable API.
// The API responses themselves remain cached for an hour by fetchPublicJson.
export const dynamic = "force-dynamic";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const [products, categories, posts, locations] = await Promise.all([
    fetchPublicJson<ProductList[]>("/Products"),
    fetchPublicJson<Category[]>("/Categories"),
    getPublishedPosts(),
    getLocationPages(),
  ]);

  return [
    ...staticRoutes.map((route) => ({
      url: `${siteUrl}${route.path}`,
      changeFrequency: route.changeFrequency,
      priority: route.priority,
    })),
    ...(products ?? [])
      .filter((product) => product.isActive && product.slug)
      .map((product) => ({
        url: `${siteUrl}/products/${encodeURIComponent(product.slug)}`,
        changeFrequency: "weekly" as const,
        priority: 0.7,
      })),
    ...(categories ?? [])
      .filter((category) => category.isActive && category.slug)
      .map((category) => ({
        url: `${siteUrl}/products/category/${encodeURIComponent(category.slug)}`,
        changeFrequency: "weekly" as const,
        priority: 0.6,
      })),
    ...posts
      .filter((post) => post.isIndexable && post.slug)
      .map((post) => ({
        url: `${siteUrl}/blog/${encodeURIComponent(post.slug)}`,
        lastModified: post.updatedAtUtc || post.publishedAtUtc || undefined,
        changeFrequency: "monthly" as const,
        priority: 0.6,
      })),
    ...Array.from(new Map(locations.filter(page => page.isPublished !== false).map(page => [page.countrySlug, page] as const)).values())
      .map(page => ({ url: `${siteUrl}/countries/${page.countrySlug}`, changeFrequency: "monthly" as const, priority: 0.6 })),
    ...locations.filter(page => page.isPublished !== false && page.isIndexable !== false).map(page => ({
      url: `${siteUrl}/countries/${page.countrySlug}/${page.citySlug}`,
      changeFrequency: "monthly" as const,
      priority: 0.6,
    })),
  ];
}

