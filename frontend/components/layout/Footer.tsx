"use client";

import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";
import { ArrowRight, ArrowUpRight, Mail, MapPin, Phone } from "lucide-react";

import { Container } from "@/components/ui/container";
import { siteConfig } from "@/lib/site";
import { cityLandingPageSeeder } from "@/data/seed/cityLandingPageSeeder";
import { locationSeoService } from "@/services/location-seo.service";
import type { LocationSeoPage, LocationSeoRouteStatus } from "@/types/location-seo-page";

/** Only routes that exist in the app. */
const footerLinks = {
  Explore: [
    { label: "Products", href: "/products" },
    { label: "Categories", href: "/categories" },
    { label: "About us", href: "/about" },
  ],
  Business: [
    { label: "B2B enquiry", href: "/b2b/enquiry" },
    { label: "Contact us", href: "/contact" },
  ],
  Account: [
    { label: "Cart", href: "/cart" },
    { label: "My orders", href: "/orders" },
    { label: "Profile", href: "/profile" },
  ],
  Policies: [
    { label: "Privacy Policy", href: "/privacy-policy" },
    { label: "Cancellation & Refunds", href: "/cancellation-refund" },
  ],
};

export function Footer() {
  const pathname = usePathname();
  const [locationPages, setLocationPages] = useState<Array<LocationSeoPage | (typeof cityLandingPageSeeder)[number]>>(
    cityLandingPageSeeder,
  );

  useEffect(() => {
    let active = true;
    void Promise.all([
      locationSeoService.getPublished(),
      locationSeoService.getRouteStatuses(),
    ]).then(([saved, statuses]: [LocationSeoPage[], LocationSeoRouteStatus[]]) => {
      if (!active) return;
      const savedByRoute = new Map(saved.map((page) => [`${page.countrySlug}/${page.citySlug}`, page]));
      const statusByRoute = new Map(statuses.map((page) => [`${page.countrySlug}/${page.citySlug}`, page]));
      const seedRoutes = new Set(cityLandingPageSeeder.map((page) => `${page.countrySlug}/${page.citySlug}`));
      const seededPages = cityLandingPageSeeder.flatMap((page) => {
        const route = `${page.countrySlug}/${page.citySlug}`;
        if (statusByRoute.get(route)?.isPublished === false) return [];
        return [savedByRoute.get(route) ?? page];
      });
      setLocationPages([
        ...seededPages,
        ...saved.filter((page) => !seedRoutes.has(`${page.countrySlug}/${page.citySlug}`)),
      ]);
    }).catch(() => {
      // Keep starter links visible if the API is temporarily unavailable.
    });
    return () => { active = false; };
  }, []);

  const indiaLocationLinks = locationPages
    .filter((page) => page.countrySlug === "india")
    .map((page) => ({ label: page.city, href: `/countries/${page.countrySlug}/${page.citySlug}` }));
  const internationalLocationLinks = locationPages
    .filter((page) => page.countrySlug !== "india")
    .map((page) => ({ label: page.city, href: `/countries/${page.countrySlug}/${page.citySlug}` }));

  if (pathname.startsWith("/admin")) {
    return null;
  }

  return (
    <footer className="relative isolate overflow-hidden bg-brand-dark text-white">
      {/* Subtle ambient color keeps the footer within the existing brand palette. */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -right-28 top-0 -z-10 size-[28rem] rounded-full bg-primary/20 blur-[120px]"
      />

      <Container>
        <section
          aria-labelledby="footer-cta-title"
          className="relative -translate-y-8 overflow-hidden rounded-3xl border border-white/10 bg-white/[0.07] p-6 shadow-2xl shadow-black/10 backdrop-blur-sm sm:-translate-y-10 sm:p-9 lg:flex lg:items-center lg:justify-between lg:gap-8"
        >
          <div className="max-w-2xl">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-300">
              Rashe Lifesciences · B2B partnerships
            </p>
            <h2 id="footer-cta-title" className="mt-3 text-2xl font-semibold tracking-tight sm:text-3xl">
              Looking for a dependable pharmaceutical partner?
            </h2>
            <p className="mt-2 max-w-xl text-sm leading-6 text-white/65 sm:text-base">
              Share your requirements and our team will help you find the right next step.
            </p>
          </div>
          <Link
            href="/b2b/enquiry"
            className="mt-6 inline-flex min-h-12 shrink-0 items-center justify-center gap-2 rounded-full bg-emerald-400 px-6 text-sm font-semibold text-brand-dark shadow-lg shadow-emerald-950/20 transition hover:bg-emerald-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white focus-visible:ring-offset-2 focus-visible:ring-offset-brand-dark lg:mt-0"
          >
            Discuss your requirements
            <ArrowRight aria-hidden="true" className="size-4" />
          </Link>
        </section>

        <div className="grid gap-12 pb-10 pt-1 sm:grid-cols-2 lg:grid-cols-[1.15fr_1.3fr_1.25fr] lg:gap-10 lg:pb-14">
          {/* Company and contact information */}
          <div className="max-w-sm">
            <Link
              href="/"
              aria-label="Rashe Lifesciences home"
              className="inline-flex rounded-xl bg-white px-4 py-2.5"
            >
              <Image
                src="/images/Rashelifescience.png"
                alt="Rashe Lifesciences Pvt Ltd."
                width={180}
                height={60}
                className="h-10 w-auto object-contain"
              />
            </Link>

            <p className="mt-5 text-sm leading-7 text-white/65">
              Quality pharmaceutical products and trusted healthcare partnerships across global markets.
            </p>

            <ul className="mt-6 space-y-3 text-sm text-white/70">
              <li className="flex items-start gap-3">
                <MapPin
                  aria-hidden="true"
                  className="mt-0.5 size-4 shrink-0 text-emerald-300"
                />
                <address className="not-italic leading-6">
                  {siteConfig.addressLabel}
                  {siteConfig.address.map((line) => (
                    <span key={line} className="block">
                      {line}
                    </span>
                  ))}
                </address>
              </li>

              <li>
                <a
                  href={siteConfig.emailHref}
                  className="flex items-center gap-3 transition-colors hover:text-white"
                >
                  <Mail
                    aria-hidden="true"
                    className="size-4 shrink-0 text-emerald-300"
                  />
                  {siteConfig.email}
                </a>
              </li>

              <li>
                <a
                  href={siteConfig.phoneHref}
                  className="flex items-center gap-3 transition-colors hover:text-white"
                >
                  <Phone
                    aria-hidden="true"
                    className="size-4 shrink-0 text-emerald-300"
                  />
                  {siteConfig.phone}
                </a>
              </li>
            </ul>
          </div>

          <div className="grid grid-cols-2 gap-x-6 gap-y-10">
            <FooterColumn title="Explore" links={footerLinks.Explore} />
            <FooterColumn title="For business" links={footerLinks.Business} />
            <FooterColumn title="Your account" links={footerLinks.Account} />
            <FooterColumn title="Policies" links={footerLinks.Policies} />
          </div>

          <nav aria-label="Find us by location">
            <h3 className="text-sm font-semibold text-white">Find us by location</h3>
            <div className="mt-5 space-y-5">
              <LocationGroup title="India" links={indiaLocationLinks} />
              <LocationGroup title="International markets" links={internationalLocationLinks} />
            </div>
          </nav>
        </div>

        <div className="flex flex-col gap-4 border-t border-white/10 py-6 text-xs text-white/50 sm:flex-row sm:items-center sm:justify-between sm:text-sm">
          <p>&copy; {new Date().getFullYear()} {siteConfig.name}. All rights reserved.</p>
          <div className="flex flex-wrap items-center gap-x-5 gap-y-2">
            <Link
              href="/b2b/enquiry"
              className="inline-flex items-center gap-1 font-medium text-white/80 transition-colors hover:text-emerald-300"
            >
              Request a quote
              <ArrowUpRight aria-hidden="true" className="size-4" />
            </Link>
          </div>
        </div>
      </Container>
    </footer>
  );
}

interface FooterColumnProps {
  title: string;
  links: { label: string; href: string }[];
}

function FooterColumn({ title, links }: FooterColumnProps) {
  return (
    <nav aria-label={title}>
      <h3 className="text-sm font-semibold text-white">{title}</h3>

      <ul className="mt-4 flex flex-col gap-3">
        {links.map((link) => (
          <li key={link.href + link.label}>
            <Link
              href={link.href}
              className="text-sm text-white/60 transition-colors hover:text-emerald-300"
            >
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </nav>
  );
}

function LocationGroup({ title, links }: FooterColumnProps) {
  return (
    <div>
      <h4 className="text-xs font-medium uppercase tracking-wider text-white/45">{title}</h4>
      <ul className="mt-2 flex flex-wrap gap-2">
        {links.map((link) => (
          <li key={link.href}>
            <Link
              href={link.href}
              className="inline-flex rounded-full border border-white/10 bg-white/[0.04] px-3 py-1.5 text-xs text-white/70 transition hover:border-emerald-300/40 hover:bg-emerald-300/10 hover:text-emerald-200"
            >
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
