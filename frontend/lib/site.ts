/**
 * Single source of truth for company contact details.
 * Update these once - Header, Footer and Contact page can all import them.
 */
export const siteConfig = {
  name: "RashePharma",
  tagline: "Quality Healthcare, Built for Trust",
  // TODO: replace with the real numbers / addresses
  phone: "+91 80000 00000",
  phoneHref: "tel:+918000000000",
  email: "info@rasheepharma.com",
  emailHref: "mailto:info@rasheepharma.com",
  announcement: "Supplying healthcare partners worldwide",
} as const;