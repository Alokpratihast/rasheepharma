import Link from "next/link";
import { ArrowRight } from "lucide-react";

import { Container } from "@/components/ui/container";
import { Reveal } from "@/components/about/Reveal";
import { SectionHeading } from "@/components/about/SectionHeading";
import { categoryService } from "@/services/category.service";
import type { Category } from "@/types/category";

/* Categories are loaded from the database, so names, descriptions and slugs
   always match what /products/category/[slug] can resolve. */
async function getActiveCategories(): Promise<Category[]> {
  try {
    const all = await categoryService.getAll();
    return all
      .filter((category) => category.isActive)
      .sort((a, b) => a.name.localeCompare(b.name));
  } catch {
    return [];
  }
}

const FALLBACK_TEXT = "Explore products in this category.";


const forms = [
  "Tablets",
  "Capsules",
  "Injections",
  "Nasal spray",
  "Respules",
  "Cream & gel",
  "Oral jelly",
];

const brands = [
  "Farovib Tablets",
  "Goldaris Super Power Capsules",
  "Evaparin PFS Injection",
  "Troycon Capsules",
  "Iptrovib Respules",
  "Acnetroy Gel",
  "Favitru Tablets",
  "AZR Tablets",
];

export async function ProductPortfolio() {
  const categories = await getActiveCategories();
  const count = categories.length;

  return (
    <section className="border-t border-border py-20 sm:py-28">
      <Container>
        <div className="flex flex-col justify-between gap-8 lg:flex-row lg:items-end">
          <Reveal>
            <SectionHeading
              index="03"
              eyebrow="Product portfolio"
              title={
                count > 0
                  ? `${count} ${count === 1 ? "category" : "categories"}, one dependable supplier.`
                  : "A complete range, one dependable supplier."
              }
              description="Our range spans tablets, capsules, injections, nasal spray, respules, creams and gels, and oral jelly, across therapeutic areas from everyday care to specialised segments."
            />
          </Reveal>
          <Reveal delay={100}>
            <Link
              href="/categories"
              className="group inline-flex items-center gap-2 text-sm font-semibold text-primary"
            >
              View all categories
              <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
            </Link>
          </Reveal>
        </div>

        {count > 0 ? (
          <Reveal delay={120}>
            <ul className="mt-14 grid border-l border-t border-border sm:grid-cols-2 lg:grid-cols-4">
              {categories.map((cat, i) => (
                <li key={cat.id} className="border-b border-r border-border">
                  <Link
                    href={`/products/category/${cat.slug}`}
                    className="group flex h-full flex-col p-6 transition-colors hover:bg-primary-light dark:hover:bg-primary/10"
                  >
                    <span className="font-mono text-[11px] text-muted-foreground">
                      {String(i + 1).padStart(2, "0")}
                    </span>
                    <span className="mt-5 text-lg font-semibold tracking-tight text-foreground group-hover:text-primary">
                      {cat.name}
                    </span>
                    <span className="mt-1.5 line-clamp-2 text-sm leading-6 text-muted-foreground">
                      {cat.description?.trim() || FALLBACK_TEXT}
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          </Reveal>
        ) : (
          <Reveal delay={120}>
            <div className="mt-14 rounded-2xl border border-dashed border-border p-10 text-center">
              <p className="text-sm text-muted-foreground">
                Our product categories are being updated. Browse the full
                catalogue to see what we offer.
              </p>
              <Link
                href="/products"
                className="mt-4 inline-flex items-center gap-2 text-sm font-semibold text-primary"
              >
                Browse all products
                <ArrowRight className="size-4" />
              </Link>
            </div>
          </Reveal>
        )}

        <Reveal delay={120}>
          <div className="mt-12 grid gap-8 border-t border-border pt-10 lg:grid-cols-12">
            <div className="lg:col-span-4">
              <p className="font-mono text-[11px] uppercase tracking-[0.16em] text-primary">
                Dosage forms
              </p>
              <ul className="mt-4 flex flex-wrap gap-2">
                {forms.map((f) => (
                  <li
                    key={f}
                    className="rounded-full border border-border bg-card px-3.5 py-1.5 text-sm text-foreground"
                  >
                    {f}
                  </li>
                ))}
              </ul>
            </div>
            <div className="lg:col-span-8">
              <p className="font-mono text-[11px] uppercase tracking-[0.16em] text-primary">
                From our range
              </p>
              <ul className="mt-4 flex flex-wrap gap-2">
                {brands.map((b) => (
                  <li
                    key={b}
                    className="rounded-full bg-primary-light px-3.5 py-1.5 text-sm font-medium text-primary-dark dark:bg-primary/15 dark:text-emerald-300"
                  >
                    {b}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </Reveal>
      </Container>
    </section>
  );
}
