import Link from "next/link";
import {
  ArrowRight,
  Baby,
  Bone,
  HeartPulse,
  Pill,
  ShieldPlus,
  Wind,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { categoryService } from "@/services/category.service";
import type { Category } from "@/types/category";

/** Icons cycle by position; every card shares one colour treatment. */
const icons = [ShieldPlus, Wind, HeartPulse, Bone, Baby, Pill];

const MAX_CATEGORIES = 12;

function formatCategoryName(name: string) {
  return name
    .replace(/[-_]+/g, " ")
    .replace(/\s+/g, " ")
    .trim()
    .replace(/\b\w/g, (character) => character.toUpperCase());
}

export async function CategoriesSection() {
  let categories: Category[] = [];

  try {
    categories = (await categoryService.getAll()).filter(
      (category) => category.isActive,
    );
  } catch {
    // API unavailable: render the empty state instead of crashing the page.
    categories = [];
  }

  const visible = categories.slice(0, MAX_CATEGORIES);

  return (
    <section className="relative overflow-hidden bg-gradient-to-b from-[#f2faf7] via-white to-white py-16 sm:py-24">
      <Container>
        {/* Heading */}
        <div className="flex flex-col gap-6 sm:flex-row sm:items-end sm:justify-between">
          <div className="max-w-2xl">
            <p className="mb-3 inline-flex items-center gap-2 text-xs font-bold uppercase tracking-[0.18em] text-primary">
              <span aria-hidden="true" className="size-2 rounded-full bg-primary" />
              Browse by category
            </p>

            <h2 className="text-3xl font-bold tracking-[-0.035em] text-brand-dark sm:text-4xl">
              Explore our product range
            </h2>

            <p className="mt-3 max-w-xl text-base leading-7 text-muted-foreground sm:text-lg">
              Pharmaceutical products across therapeutic categories, for
              healthcare and B2B buyers.
            </p>
          </div>

          <Link
            href="/categories"
            className="group inline-flex h-12 shrink-0 items-center justify-center gap-2 self-start rounded-full bg-brand-dark px-6 text-sm font-semibold text-white shadow-sm transition-all hover:-translate-y-0.5 hover:bg-primary hover:shadow-lg sm:self-auto"
          >
            View all categories
            <ArrowRight className="size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
          </Link>
        </div>

        {/* Grid */}
        {visible.length > 0 ? (
          <div className="mt-9 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:mt-12 lg:grid-cols-3 lg:gap-5 xl:grid-cols-4">
            {visible.map((category, index) => {
              const Icon = icons[index % icons.length];
              const description = category.description?.trim();

              return (
                <Link
                  key={category.id}
                  href={`/products?category=${encodeURIComponent(
                    category.name,
                  )}`}
                  className="group relative flex min-h-[190px] flex-col overflow-hidden rounded-2xl border border-slate-200/90 bg-white p-5 shadow-[0_5px_20px_rgba(15,45,38,0.035)] transition-all duration-300 hover:-translate-y-1 hover:border-emerald-200 hover:shadow-[0_18px_38px_rgba(15,45,38,0.11)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 sm:min-h-[210px] sm:p-6"
                >
                  <span aria-hidden="true" className="pointer-events-none absolute -right-8 -top-10 size-28 rounded-full bg-emerald-50/80 transition-transform duration-500 group-hover:scale-125" />

                  <span className="relative flex items-start justify-between">
                    <span className="flex size-12 items-center justify-center rounded-2xl bg-emerald-50 text-primary ring-1 ring-emerald-100 transition-colors duration-300 group-hover:bg-primary group-hover:text-white group-hover:ring-primary">
                      <Icon
                        aria-hidden="true"
                        className="size-5"
                        strokeWidth={1.8}
                      />
                    </span>
                    <span className="pt-1 text-xs font-semibold tabular-nums tracking-wider text-slate-400">
                      {String(index + 1).padStart(2, "0")}
                    </span>
                  </span>

                  <h3 className="relative mt-5 line-clamp-2 text-lg font-bold leading-6 text-brand-dark">
                    {formatCategoryName(category.name)}
                  </h3>

                  <p className="relative mt-2 line-clamp-2 text-sm leading-5 text-muted-foreground">
                    {description || "Explore products available in this category."}
                  </p>

                  <span className="relative mt-auto flex items-center justify-between pt-5 text-sm font-semibold text-primary">
                    Explore category
                    <span
                      aria-hidden="true"
                      className="flex size-9 items-center justify-center rounded-full border border-emerald-100 bg-white text-primary transition-all duration-300 group-hover:border-primary group-hover:bg-primary group-hover:text-white"
                    >
                      <ArrowRight className="size-4 transition-transform duration-300 group-hover:translate-x-0.5" />
                    </span>
                  </span>
                </Link>
              );
            })}
          </div>
        ) : (
          <div className="mt-10 rounded-2xl border border-dashed border-border bg-muted px-6 py-14 text-center">
            <Pill
              aria-hidden="true"
              className="mx-auto size-8 text-muted-foreground"
            />

            <p className="mt-3 text-sm font-medium text-muted-foreground">
              Categories will appear here once they&apos;re added.
            </p>
          </div>
        )}
      </Container>
    </section>
  );
}
