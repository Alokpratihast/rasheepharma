import {
  Baby,
  Bone,
  HeartPulse,
  Pill,
  ShieldPlus,
  Wind,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { categoryService } from "@/services/category.service";

const categoryStyles = [
  {
    icon: ShieldPlus,
    className: "bg-[#3E8F96]",
  },
  {
    icon: Wind,
    className: "bg-[#F5821F]",
  },
  {
    icon: HeartPulse,
    className: "bg-[#1B2A4A]",
  },
  {
    icon: Bone,
    className: "bg-[#5DCAA5]",
  },
  {
    icon: Baby,
    className: "bg-[#639922]",
  },
  {
    icon: Pill,
    className: "bg-[#993C1D]",
  },
];

export async function CategoriesSection() {
  const categories = await categoryService.getAll();

  const activeCategories = categories.filter(
    (category) => category.isActive,
  );

  return (
    <section className="bg-background py-12 sm:py-16">
      <Container>
        <div className="mb-8 flex items-end justify-between">
          <div>
            <p className="mb-2 text-sm font-semibold uppercase tracking-wider text-primary">
              Our Categories
            </p>

            <h2 className="text-2xl font-bold tracking-tight text-foreground sm:text-3xl">
              Explore Our Products
            </h2>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-2.5 sm:grid-cols-3 lg:grid-cols-6">
          {activeCategories.map((category, index) => {
            const style =
              categoryStyles[index % categoryStyles.length];

            const Icon = style.icon;

            return (
              <button
                key={category.id}
                type="button"
                className={`group flex min-h-[140px] flex-col items-center justify-center rounded-xl p-5 text-center text-white transition-transform hover:-translate-y-1 ${style.className}`}
              >
                <Icon
                  className="mb-3 h-8 w-8 transition-transform group-hover:scale-110"
                  strokeWidth={1.8}
                />

                <span className="text-sm font-semibold">
                  {category.name}
                </span>
              </button>
            );
          })}
        </div>
      </Container>
    </section>
  );
}