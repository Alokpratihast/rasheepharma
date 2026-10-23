"use client";

import { useMemo, useState } from "react";
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

import { ProductCard } from "@/components/product/ProductCard";
import { Container } from "@/components/ui/container";
import type { Category } from "@/types/category";
import type { ProductList } from "@/types/product";

const icons = [ShieldPlus, Wind, HeartPulse, Bone, Baby, Pill];
const MAX_CATEGORIES = 12;

function formatCategoryName(name: string) {
  return name.replace(/[-_]+/g, " ").replace(/\s+/g, " ").trim()
    .replace(/\b\w/g, (character) => character.toUpperCase());
}

function normalize(value: string) {
  return value.toLowerCase().replace(/[-_]+/g, " ").replace(/\s+/g, " ").trim();
}

interface CategoriesExplorerProps {
  categories: Category[];
  featuredProducts: ProductList[];
}

export function CategoriesExplorer({ categories, featuredProducts }: CategoriesExplorerProps) {
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const visible = categories.slice(0, MAX_CATEGORIES);
  const matchingProducts = useMemo(() => selectedCategory
    ? featuredProducts.filter((product) => normalize(product.categoryName) === normalize(selectedCategory.name)).slice(0, 4)
    : [], [featuredProducts, selectedCategory]);

  return (
    <section className="relative overflow-hidden bg-gradient-to-b from-[#f2faf7] via-white to-white py-16 sm:py-24">
      <Container>
        <div className="flex flex-col gap-6 sm:flex-row sm:items-end sm:justify-between">
          <div className="max-w-2xl">
            <p className="mb-3 inline-flex items-center gap-2 text-xs font-bold uppercase tracking-[0.18em] text-primary">
              <span aria-hidden="true" className="size-2 rounded-full bg-primary" />
              Browse by category
            </p>
            <h2 className="text-3xl font-bold tracking-[-0.035em] text-brand-dark sm:text-4xl">Explore our product range</h2>
            <p className="mt-3 max-w-xl text-base leading-7 text-muted-foreground sm:text-lg">
              Choose a category to preview products, then open the full catalogue when you&apos;re ready.
            </p>
          </div>
          <Link href="/categories" className="group inline-flex h-12 shrink-0 items-center justify-center gap-2 self-start rounded-full bg-brand-dark px-6 text-sm font-semibold text-white shadow-sm transition-all hover:-translate-y-0.5 hover:bg-primary hover:shadow-lg sm:self-auto">
            View all categories
            <ArrowRight className="size-4 transition-transform duration-200 group-hover:translate-x-0.5" />
          </Link>
        </div>

        {visible.length > 0 ? (
          <div className="mt-9 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:mt-12 lg:grid-cols-3 lg:gap-5 xl:grid-cols-4">
            {visible.map((category, index) => {
              const Icon = icons[index % icons.length];
              const selected = selectedCategory?.id === category.id;
              return (
                <button key={category.id} type="button" aria-pressed={selected}
                  onClick={() => setSelectedCategory(selected ? null : category)}
                  className={`group relative flex min-h-[190px] flex-col overflow-hidden rounded-2xl border p-5 text-left shadow-[0_5px_20px_rgba(15,45,38,0.035)] transition-all duration-300 hover:-translate-y-1 hover:shadow-[0_18px_38px_rgba(15,45,38,0.11)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 sm:min-h-[210px] sm:p-6 ${selected ? "border-primary bg-emerald-50/60 ring-2 ring-primary/20" : "border-slate-200/90 bg-white hover:border-emerald-200"}`}>
                  <span aria-hidden="true" className="pointer-events-none absolute -right-8 -top-10 size-28 rounded-full bg-emerald-50/80 transition-transform duration-500 group-hover:scale-125" />
                  <span className="relative flex items-start justify-between">
                    <span className={`flex size-12 items-center justify-center rounded-2xl ring-1 transition-colors duration-300 ${selected ? "bg-primary text-white ring-primary" : "bg-emerald-50 text-primary ring-emerald-100 group-hover:bg-primary group-hover:text-white group-hover:ring-primary"}`}>
                      <Icon aria-hidden="true" className="size-5" strokeWidth={1.8} />
                    </span>
                    <span className="pt-1 text-xs font-semibold tabular-nums tracking-wider text-slate-400">{String(index + 1).padStart(2, "0")}</span>
                  </span>
                  <h3 className="relative mt-5 line-clamp-2 text-lg font-bold leading-6 text-brand-dark">{formatCategoryName(category.name)}</h3>
                  <p className="relative mt-2 line-clamp-2 text-sm leading-5 text-muted-foreground">{category.description?.trim() || "Explore products available in this category."}</p>
                  <span className="relative mt-auto flex items-center justify-between pt-5 text-sm font-semibold text-primary">
                    {selected ? "Hide preview" : "Preview products"}
                    <span aria-hidden="true" className="flex size-9 items-center justify-center rounded-full border border-emerald-100 bg-white text-primary transition-all duration-300 group-hover:border-primary group-hover:bg-primary group-hover:text-white">
                      <ArrowRight className={`size-4 transition-transform duration-300 group-hover:translate-x-0.5 ${selected ? "rotate-90" : ""}`} />
                    </span>
                  </span>
                </button>
              );
            })}
          </div>
        ) : (
          <div className="mt-10 rounded-2xl border border-dashed border-border bg-muted px-6 py-14 text-center">
            <Pill aria-hidden="true" className="mx-auto size-8 text-muted-foreground" />
            <p className="mt-3 text-sm font-medium text-muted-foreground">Categories will appear here once they&apos;re added.</p>
          </div>
        )}

        {selectedCategory && (
          <div className="mt-8 scroll-mt-24" aria-live="polite">
            <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
              <div>
                <p className="text-xs font-bold uppercase tracking-wider text-primary">Category preview</p>
                <h3 className="mt-1 text-xl font-bold text-brand-dark">{formatCategoryName(selectedCategory.name)}</h3>
              </div>
              <Link href={`/products?category=${encodeURIComponent(selectedCategory.name)}`} className="inline-flex items-center gap-2 text-sm font-semibold text-primary hover:underline">
                Browse all in this category <ArrowRight className="size-4" />
              </Link>
            </div>
            {matchingProducts.length > 0 ? (
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
                {matchingProducts.map((product) => (
                  <ProductCard key={product.id} name={product.name} form={product.dosageForm ?? ""}
                    composition={product.composition ?? ""} packSize={product.packSize ?? "Contact for details"}
                    moq={product.moq} slug={product.slug} imageUrl={product.primaryImageUrl} categoryName={product.categoryName} />
                ))}
              </div>
            ) : (
              <div className="rounded-2xl border border-slate-200 bg-white p-6 text-sm text-slate-600">
                No featured products in this category yet. Use “Browse all” to view its complete catalogue.
              </div>
            )}
          </div>
        )}
      </Container>
    </section>
  );
}
