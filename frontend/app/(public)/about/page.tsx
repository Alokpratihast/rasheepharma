// import type { Metadata } from "next";

// import { AboutCTA } from "@/components/about/AboutCTA";
// import { AboutHero } from "@/components/about/AboutHero";
// import { AboutOverview } from "@/components/about/AboutOverview";
// import { AboutStats } from "@/components/about/AboutStats";
// import { BusinessHighlights } from "@/components/about/BusinessHighlights";
// import { BusinessInformation } from "@/components/about/BusinessInformation";
// import { CertificationsSection } from "@/components/about/CertificationsSection";
// import { QualitySection } from "@/components/about/QualitySection";
// import { TeamSection } from "@/components/about/TeamSection";

// export const metadata: Metadata = {
//   title: "About Us | RashePharma",
//   description:
//     "Learn about RashePharma - quality-focused pharmaceutical products, certifications and long-term healthcare partnerships.",
// };

// export default function AboutPage() {
//   return (
//     <main className="bg-background">
//       <AboutHero />
//       <AboutStats />
//       <AboutOverview />
//       <QualitySection />
//       <CertificationsSection />
//       <TeamSection />
//       <BusinessInformation />
//       <BusinessHighlights />
//       <AboutCTA />
//     </main>
//   );
// }


import type { Metadata } from "next";

import { AboutFAQ } from "@/components/about/AboutFAQ";
import { AboutHero } from "@/components/about/AboutHero";
import { AboutStory } from "@/components/about/AboutStory";
import { Certifications } from "@/components/about/Certifications";
import { CompanyFacts } from "@/components/about/CompanyFacts";
import { Principles } from "@/components/about/Principles";
import { ProductPortfolio } from "@/components/about/ProductPortfolio";
import { WhatWeDo } from "@/components/about/WhatWeDo";
import { WhoWeServe } from "@/components/about/WhoWeServe";

export const metadata: Metadata = {
  title: "About Us | Rashe Lifesciences",
  description:
    "Rashe Lifesciences is a Bangalore-based pharmaceutical company with 15 years of team experience, supplying quality medicines worldwide through ISO and WHO-GMP certified plants.",
};

export default function AboutPage() {
  return (
    <main className="bg-background">
      <AboutHero />
      <AboutStory />
      <WhatWeDo />
      <ProductPortfolio />
      <WhoWeServe />
      <Principles />
      <Certifications />
      <CompanyFacts />
      <AboutFAQ />
    </main>
  );
}
