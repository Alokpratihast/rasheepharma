import { HeroSection } from "@/components/home/HeroSection";
import { TrustSection } from "@/components/home/TrustSection";
import { CategoriesSection } from "@/components/home/CategoriesSection";
import { FeaturedProducts } from "@/components/home/FeaturedProducts";
import { B2BSection } from "@/components/home/B2BSection";
import { AboutSection } from "@/components/home/AboutSection";

export default function HomePage() {
  return (
    <main>
      <HeroSection />
      <TrustSection />
      <CategoriesSection />
      <FeaturedProducts />
      <AboutSection />
      <B2BSection />
    </main>
  );
}