import Link from "next/link";
import {
  Baby,
  Bone,
  Candy,
  Droplets,
  FlaskConical,
  HeartPulse,
  Pill,
  ShieldPlus,
  Sparkles,
  Syringe,
  Stethoscope,
  Tablets,
  TestTube,
  Thermometer,
  Wheat,
  type LucideIcon,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { categoryService } from "@/services/category.service";
import type { Category } from "@/types/category";

interface CategoryMeta {
  description: string;
  icon: LucideIcon;
  className: string;
}

const categoryMeta: Record<string, CategoryMeta> = {
  "gynae-range": {
    description: "Products for women's healthcare needs.",
    icon: HeartPulse,
    className: "bg-[#3E8F96]",
  },

  "anti-inflammatory-tab-cap": {
    description: "Tablets and capsules for pain and inflammation care.",
    icon: Thermometer,
    className: "bg-[#F5821F]",
  },

  antibiotics: {
    description: "A broad range of antibiotic formulations.",
    icon: ShieldPlus,
    className: "bg-[#1B2A4A]",
  },

  "anti-ulcerant-ppi": {
    description: "PPI and digestive healthcare products.",
    icon: Pill,
    className: "bg-[#5DCAA5]",
  },

  "multivitamin-products": {
    description: "Vitamins, minerals and nutritional products.",
    icon: Wheat,
    className: "bg-[#639922]",
  },

  "other-products": {
    description: "Additional pharmaceutical healthcare products.",
    icon: FlaskConical,
    className: "bg-[#7F77DD]",
  },

  injectables: {
    description: "Injectable pharmaceutical formulations.",
    icon: Syringe,
    className: "bg-[#993C1D]",
  },

  syrups: {
    description: "Liquid pharmaceutical formulations.",
    icon: Droplets,
    className: "bg-[#0F6E56]",
  },

  "gel-oil": {
    description: "Topical gels and oil-based formulations.",
    icon: Sparkles,
    className: "bg-[#185FA5]",
  },

  "powder-sachet": {
    description: "Powder and sachet-based healthcare products.",
    icon: TestTube,
    className: "bg-[#5F5E5A]",
  },

  "derma-range": {
    description: "Dermatological and skin-care formulations.",
    icon: Droplets,
    className: "bg-[#D85A30]",
  },

  "derma-cosmetic-range": {
    description: "Cosmetic and personal-care formulations.",
    icon: Sparkles,
    className: "bg-[#7F77DD]",
  },

  "pediatric-range": {
    description: "Healthcare products for pediatric requirements.",
    icon: Baby,
    className: "bg-[#3E8F96]",
  },

  "gummies-range": {
    description: "Gummy-based nutritional products.",
    icon: Candy,
    className: "bg-[#F5821F]",
  },

  "veterinary-range": {
    description: "Products for veterinary healthcare needs.",
    icon: Stethoscope,
    className: "bg-[#1B2A4A]",
  },

  "dental-range": {
    description: "Dental and oral-care products.",
    icon: Bone,
    className: "bg-[#5DCAA5]",
  },

  "diabetic-range": {
    description: "Products for diabetic-care requirements.",
    icon: Tablets,
    className: "bg-[#639922]",
  },
};

export default async function CategoriesPage() {
  let categories: Category[] = [];

  try {
    categories = await categoryService.getAll();
  } catch {
    categories = [];
  }

  const activeCategories = categories.filter(
    (category) => category.isActive
  );

  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-14">
      <Container>
        {/* Page heading */}
        <div className="max-w-3xl">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
            Product Catalogue
          </p>

          <h1 className="mt-2 text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl">
            Browse by Category
          </h1>

          <p className="mt-3 text-sm leading-6 text-[#595959] sm:text-base">
            Explore the RashePharma product portfolio across therapeutic,
            formulation and healthcare categories.
          </p>
        </div>

        {/* Category grid */}
        {activeCategories.length > 0 ? (
          <div className="mt-10 grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6">
            {activeCategories.map((category) => {
              const meta = categoryMeta[category.slug];

              const Icon = meta?.icon ?? Pill;

              const className =
                meta?.className ?? "bg-[#1B2A4A]";

              const description =
                meta?.description ??
                "Explore products available in this category.";

              return (
                <Link
                  key={category.id}
                  href={`/products/category/${category.slug}`}
                  className="group"
                >
                  <article
                    className={[
                      "flex min-h-36 flex-col justify-between rounded-lg p-4 text-white",
                      "transition-all duration-200",
                      "hover:-translate-y-1 hover:shadow-lg",
                      className,
                    ].join(" ")}
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex size-10 items-center justify-center rounded-lg bg-white/15">
                        <Icon
                          aria-hidden="true"
                          className="size-5"
                        />
                      </div>

                      <span className="text-[10px] font-medium text-white/70">
                        Explore
                      </span>
                    </div>

                    <div className="mt-6">
                      <h2 className="text-sm font-semibold leading-5">
                        {category.name}
                      </h2>

                      <p className="mt-1.5 line-clamp-2 text-[11px] leading-4 text-white/75">
                        {description}
                      </p>
                    </div>
                  </article>
                </Link>
              );
            })}
          </div>
        ) : (
          <div className="mt-10 rounded-xl border border-dashed border-border bg-white p-10 text-center">
            <Pill className="mx-auto size-8 text-muted-foreground" />

            <h2 className="mt-3 text-lg font-semibold text-[#1B2A4A]">
              No categories available
            </h2>

            <p className="mt-1 text-sm text-muted-foreground">
              Product categories could not be loaded at this time.
            </p>
          </div>
        )}
      </Container>
    </main>
  );
}