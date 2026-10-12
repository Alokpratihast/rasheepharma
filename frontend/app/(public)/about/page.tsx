import type { Metadata } from "next";

import { AboutCTA } from "@/components/about/AboutCTA";
import { AboutHero } from "@/components/about/AboutHero";
import { AboutOverview } from "@/components/about/AboutOverview";
import { AboutStats } from "@/components/about/AboutStats";
import { BusinessHighlights } from "@/components/about/BusinessHighlights";
import { BusinessInformation } from "@/components/about/BusinessInformation";
import { CertificationsSection } from "@/components/about/CertificationsSection";
import { QualitySection } from "@/components/about/QualitySection";
import { TeamSection } from "@/components/about/TeamSection";

export const metadata: Metadata = {
  title: "About Us | RashePharma",
  description:
    "Learn about RashePharma - quality-focused pharmaceutical products, certifications and long-term healthcare partnerships.",
};

export default function AboutPage() {
  return (
    <main className="bg-background">
      <AboutHero />
      <AboutStats />
      <AboutOverview />
      <QualitySection />
      <CertificationsSection />
      <TeamSection />
      <BusinessInformation />
      <BusinessHighlights />
      <AboutCTA />
    </main>
  );
}
