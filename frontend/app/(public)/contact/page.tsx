import type { Metadata } from "next";

import ContactPageContent from "@/components/contact/ContactPageContent";

export const metadata: Metadata = {
  title: "Contact Us | RashePharma",
  description:
    "Contact RashePharma for pharmaceutical products, distribution opportunities and business enquiries.",
};

export default function ContactPage() {
  return <ContactPageContent />;
}
