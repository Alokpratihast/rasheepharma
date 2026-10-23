import { HeroSection } from "@/components/home/HeroSection";
import { TrustSection } from "@/components/home/TrustSection";
import { CategoriesSection } from "@/components/home/CategoriesSection";
import { FeaturedProducts } from "@/components/home/FeaturedProducts";
import { B2BSection } from "@/components/home/B2BSection";
import { AboutSection } from "@/components/home/AboutSection";
import { HomeFaq } from "@/components/home/HomeFaq";

export default function HomePage() {
  return (
    <main>
      <HeroSection />
      <TrustSection />
      <CategoriesSection />
      <FeaturedProducts />
      <AboutSection />
      <HomeFaq />
      <B2BSection />
    </main>
  );
}
