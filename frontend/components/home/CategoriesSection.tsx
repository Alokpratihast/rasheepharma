import {
  Baby,
  Bone,
  HeartPulse,
  Pill,
  ShieldPlus,
  Wind,
} from "lucide-react";

import { Container } from "@/components/ui/container";

const categories = [
  {
    title: "Anti-Infectives",
    icon: ShieldPlus,
    className: "bg-[#3E8F96]",
  },
  {
    title: "Respiratory & Cough Care",
    icon: Wind,
    className: "bg-[#F5821F]",
  },
  {
    title: "Vitamins & Nutrition",
    icon: HeartPulse,
    className: "bg-[#1B2A4A]",
  },
  {
    title: "Bone & Joint Health",
    icon: Bone,
    className: "bg-[#5DCAA5]",
  },
  {
    title: "Pediatric Care",
    icon: Baby,
    className: "bg-[#639922]",
  },
  {
    title: "Pain & Fever Care",
    icon: Pill,
    className: "bg-[#993C1D]",
  },
];

export function CategoriesSection() {
  return (
    <section className="bg-background py-12 sm:py-16">
      <Container>
        {/* Section heading */}
        <div className="mb-5 flex items-end justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold tracking-tight text-[#1B2A4A] sm:text-2xl">
              Browse by category
            </h2>

            <p className="mt-1 text-sm text-[#595959]">
              Explore our pharmaceutical product range by therapeutic area.
            </p>
          </div>
        </div>

        {/* Category tiles */}
        <div className="grid grid-cols-2 gap-2.5 sm:grid-cols-3 lg:grid-cols-6">
          {categories.map((category) => {
            const Icon = category.icon;

            return (
              <button
                key={category.title}
                type="button"
                className={[
                  "group flex min-h-28 flex-col items-center justify-center",
                  "rounded-lg px-3 py-5 text-center text-white",
                  "transition-all duration-200",
                  "hover:-translate-y-0.5 hover:shadow-md",
                  "focus-visible:outline-none focus-visible:ring-2",
                  "focus-visible:ring-[#3E8F96] focus-visible:ring-offset-2",
                  category.className,
                ].join(" ")}
              >
                <Icon
                  aria-hidden="true"
                  className="size-7 transition-transform duration-200 group-hover:scale-105"
                />

                <span className="mt-3 text-xs font-semibold leading-4 sm:text-sm">
                  {category.title}
                </span>
              </button>
            );
          })}
        </div>
      </Container>
    </section>
  );
}