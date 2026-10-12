/**
 * Single source of truth for company contact details.
 * Update these once - Header, Footer and Contact page all read from here.
 */
export const siteConfig = {
  name: "RashePharma",
  tagline: "Quality Healthcare, Built for Trust",
  phone: "+91 91104 56656",
  phoneHref: "tel:+919110456656",
  email: "hr.rashelifescience@gmail.com",
  emailHref: "mailto:hr.rashelifescience@gmail.com",
  announcement: "Supplying healthcare partners worldwide",
  addressLabel: "Head office",
  address: [
    "01, B Byraveshwara Nagar,",
    "Magadi Road, Sunkadakatte,",
    "Bangalore, Karnataka 560091",
  ],
} as const;