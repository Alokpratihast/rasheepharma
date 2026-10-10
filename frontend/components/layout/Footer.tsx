"use client";

import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { ArrowUpRight, Mail, MapPin, Phone } from "lucide-react";

import { Container } from "@/components/ui/container";
import { siteConfig } from "@/lib/site";

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

  if (pathname.startsWith("/admin")) {
    return null;
  }

  return (
    <footer className="relative isolate overflow-hidden bg-brand-dark text-white">
      {/* Soft glow, same family as the hero */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -right-32 -top-32 -z-10 size-96 rounded-full bg-primary/30 blur-[110px]"
      />

      <Container>
        <div className="grid gap-10 py-14 sm:py-16 sm:grid-cols-2 xl:grid-cols-[1.5fr_repeat(4,minmax(0,1fr))]">
          {/* Company */}
          <div className="max-w-md">
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

            <p className="mt-6 max-w-sm text-sm leading-7 text-white/70">
              Quality pharmaceutical products and trusted healthcare
              partnerships across global markets.
            </p>

            <ul className="mt-7 space-y-4 text-sm text-white/75">
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

          <FooterColumn title="Explore" links={footerLinks.Explore} />
          <FooterColumn title="Business" links={footerLinks.Business} />
          <FooterColumn title="Account" links={footerLinks.Account} />
          {/* Public policy links make order and data-handling terms easy to find from every customer page. */}
          <FooterColumn title="Policies" links={footerLinks.Policies} />
        </div>

        {/* Bottom bar */}
        <div className="flex flex-col gap-4 border-t border-white/10 py-6 text-sm text-white/55 sm:flex-row sm:items-center sm:justify-between">
          <p>
            &copy; {new Date().getFullYear()} {siteConfig.name}. All
            rights reserved.
          </p>

          <Link
            href="/b2b/enquiry"
            className="inline-flex items-center gap-1 font-medium text-white/80 transition-colors hover:text-emerald-300"
          >
            Request a quote
            <ArrowUpRight aria-hidden="true" className="size-4" />
          </Link>
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

      <ul className="mt-5 flex flex-col gap-3">
        {links.map((link) => (
          <li key={link.href + link.label}>
            <Link
              href={link.href}
              className="text-sm text-white/65 transition-colors hover:text-emerald-300"
            >
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </nav>
  );
}
