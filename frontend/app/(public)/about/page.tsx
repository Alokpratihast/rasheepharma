import { AboutCTA } from "@/components/about/AboutCTA";
import { AboutHero } from "@/components/about/AboutHero";
import { AboutOverview } from "@/components/about/AboutOverview";
import { BusinessHighlights } from "@/components/about/BusinessHighlights";
import { QualitySection } from "@/components/about/QualitySection";
import { TeamSection } from "@/components/about/TeamSection";
import{BusinessInformation} from "@/components/about/BusinessInformation";
import {CertificationsSection} from "@/components/about/CertificationsSection";

export default function AboutPage() {
  return (
    <main>
      <AboutHero />

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