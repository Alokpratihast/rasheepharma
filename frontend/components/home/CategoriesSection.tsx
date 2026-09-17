import Link from "next/link";
import {
  Baby,
  Bone,
  HeartPulse,
  Pill,
  ShieldPlus,
  Wind,
  ArrowRight,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { categoryService } from "@/services/category.service";

const categoryStyles = [
  {
    icon: ShieldPlus,
    iconBg: "bg-[#E8F6F6]",
    iconColor: "text-[#168999]",
  },
  {
    icon: Wind,
    iconBg: "bg-[#FFF1E5]",
    iconColor: "text-[#F5821F]",
  },
  {
    icon: HeartPulse,
    iconBg: "bg-[#EEF1F7]",
    iconColor: "text-[#1B2A4A]",
  },
  {
    icon: Bone,
    iconBg: "bg-[#EAF8F3]",
    iconColor: "text-[#299B78]",
  },
  {
    icon: Baby,
    iconBg: "bg-[#F0F7E8]",
    iconColor: "text-[#639922]",
  },
  {
    icon: Pill,
    iconBg: "bg-[#FBEFEA]",
    iconColor: "text-[#993C1D]",
  },
];

export async function CategoriesSection() {
  const categories = await categoryService.getAll();

  const activeCategories = categories.filter(
    (category) => category.isActive,
  );

  return (
    <section className="relative overflow-hidden bg-white py-14 sm:py-18 lg:py-20">
      {/* Soft background decoration */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -left-32 top-20 size-72 rounded-full bg-[#3E8F96]/5 blur-3xl"
      />

      <div
        aria-hidden="true"
        className="pointer-events-none absolute -right-32 bottom-10 size-72 rounded-full bg-[#F5821F]/5 blur-3xl"
      />

      <Container className="relative">
        {/* Section Header */}
        <div className="mb-9 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div className="max-w-2xl">
            <div className="mb-3 flex items-center gap-2">
              <span className="h-px w-8 bg-[#F5821F]" />

              <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#F5821F]">
                Our Categories
              </p>
            </div>

            <h2 className="text-3xl font-bold tracking-tight text-[#1B2A4A] sm:text-4xl">
              Explore Our{" "}
              <span className="text-[#3E8F96]">Product Range</span>
            </h2>

            <p className="mt-3 max-w-xl text-sm leading-6 text-[#71839A] sm:text-base">
              Discover pharmaceutical products across a wide range of
              therapeutic categories for healthcare and B2B requirements.
            </p>
          </div>

          {/* View all */}
          <Link
            href="/categories"
            className="group inline-flex shrink-0 items-center gap-2 text-sm font-semibold text-[#1B2A4A] transition-colors hover:text-[#3E8F96]"
          >
            View all categories
            <ArrowRight
              className="size-4 transition-transform duration-200 group-hover:translate-x-1"
            />
          </Link>
        </div>

        {/* Category Grid */}
        {activeCategories.length > 0 ? (
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
            {activeCategories.map((category, index) => {
              const style =
                categoryStyles[index % categoryStyles.length];

              const Icon = style.icon;

              return (
                <Link
                  key={category.id}
                  href={`/products?category=${encodeURIComponent(
                    category.name,
                  )}`}
                  className="group relative min-h-[155px] overflow-hidden rounded-2xl border border-[#E5ECEC] bg-white p-5 shadow-[0_4px_20px_rgba(27,42,74,0.04)] transition-all duration-300 hover:-translate-y-1.5 hover:border-[#3E8F96]/30 hover:shadow-[0_14px_30px_rgba(27,42,74,0.10)]"
                >
                  {/* Top accent */}
                  <div className="absolute left-0 top-0 h-1 w-0 bg-[#3E8F96] transition-all duration-300 group-hover:w-full" />

                  {/* Icon */}
                  <div
                    className={`flex size-12 items-center justify-center rounded-xl ${style.iconBg} transition-transform duration-300 group-hover:scale-110`}
                  >
                    <Icon
                      className={`size-6 ${style.iconColor}`}
                      strokeWidth={1.8}
                    />
                  </div>

                  {/* Category Name */}
                  <h3 className="mt-5 line-clamp-2 text-sm font-bold leading-5 text-[#1B2A4A] transition-colors group-hover:text-[#168999]">
                    {category.name}
                  </h3>

                  {/* Explore */}
                  <div className="mt-3 flex items-center gap-1 text-[11px] font-semibold text-[#8A9694] transition-colors group-hover:text-[#3E8F96]">
                    Explore
                    <ArrowRight className="size-3 transition-transform duration-200 group-hover:translate-x-0.5" />
                  </div>

                  {/* Decorative circle */}
                  <div
                    aria-hidden="true"
                    className="absolute -bottom-8 -right-8 size-20 rounded-full bg-[#3E8F96]/[0.035] transition-transform duration-300 group-hover:scale-125"
                  />
                </Link>
              );
            })}
          </div>
        ) : (
          <div className="rounded-2xl border border-dashed border-[#DCE6E6] bg-[#F8FAFA] px-6 py-12 text-center">
            <Pill className="mx-auto size-8 text-[#9AA8A7]" />

            <p className="mt-3 text-sm font-medium text-[#71839A]">
              No product categories available yet.
            </p>
          </div>
        )}
      </Container>
    </section>
  );
}