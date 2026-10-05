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
    <section className="bg-white py-14 sm:py-20">
      <Container>
        {/* Heading */}
        <div className="flex flex-col gap-5 sm:flex-row sm:items-end sm:justify-between">
          <div className="max-w-2xl">
            <h2 className="text-3xl font-bold tracking-[-0.025em] text-brand-dark sm:text-4xl">
              Explore our product range
            </h2>

            <p className="mt-3 max-w-xl text-base leading-7 text-muted-foreground">
              Pharmaceutical products across therapeutic categories, for
              healthcare and B2B buyers.
            </p>
          </div>

          <Link
            href="/categories"
            className="group inline-flex h-11 shrink-0 items-center justify-center gap-2 self-start rounded-full border border-border px-5 text-sm font-semibold text-brand-dark transition-colors hover:border-primary hover:text-primary sm:self-auto"
          >
            View all categories
            <ArrowRight className="size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
          </Link>
        </div>

        {/* Grid */}
        {visible.length > 0 ? (
          <div className="mt-10 grid grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-3 xl:grid-cols-4">
            {visible.map((category, index) => {
              const Icon = icons[index % icons.length];

              return (
                <Link
                  key={category.id}
                  href={`/products?category=${encodeURIComponent(
                    category.name,
                  )}`}
                  className="group flex min-h-[160px] flex-col rounded-2xl border border-border bg-white p-4 transition-colors duration-300 hover:border-brand-dark hover:bg-brand-dark sm:min-h-[184px] sm:p-5"
                >
                  <span className="flex size-11 items-center justify-center rounded-xl bg-primary-light text-primary transition-colors duration-300 group-hover:bg-white/10 group-hover:text-emerald-300">
                    <Icon
                      aria-hidden="true"
                      className="size-5"
                      strokeWidth={1.8}
                    />
                  </span>

                  <h3 className="mt-4 line-clamp-2 text-base font-semibold leading-6 text-brand-dark transition-colors duration-300 group-hover:text-white">
                    {category.name}
                  </h3>

                  {category.description && (
                    <p className="mt-1 hidden line-clamp-2 text-sm leading-6 text-muted-foreground transition-colors duration-300 group-hover:text-white/70 sm:block">
                      {category.description}
                    </p>
                  )}

                  <span
                    aria-hidden="true"
                    className="mt-auto flex size-9 items-center justify-center self-start rounded-full border border-border text-brand-dark transition-all duration-300 group-hover:border-white/30 group-hover:text-white"
                  >
                    <ArrowRight className="size-4 transition-transform duration-300 group-hover:translate-x-0.5" />
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
