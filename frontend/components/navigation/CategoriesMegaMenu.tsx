"use client";

import Link from "next/link";
import {
  ArrowUpRight,
  ChevronDown,
  Loader2,
} from "lucide-react";
import { useEffect, useRef, useState } from "react";

import { categoryService } from "@/services/category.service";
import type { CategoryNavigation } from "@/types/category";

interface CategoriesMegaMenuProps {
  mobile?: boolean;
  onNavigate?: () => void;
}

export function CategoriesMegaMenu({
  mobile = false,
  onNavigate,
}: CategoriesMegaMenuProps) {
  const [open, setOpen] = useState(false);
  const [categories, setCategories] = useState<CategoryNavigation[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedCategory, setSelectedCategory] =
    useState<CategoryNavigation | null>(null);

  const menuRef = useRef<HTMLDivElement>(null);

  /*
   * =====================================================
   * CLOSE MENU ON OUTSIDE CLICK / ESCAPE
   * =====================================================
   */

  useEffect(() => {
    if (!open) return;

    const handleOutsideClick = (event: MouseEvent) => {
      if (
        menuRef.current &&
        !menuRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    };

    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setOpen(false);
      }
    };

    document.addEventListener("mousedown", handleOutsideClick);
    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("mousedown", handleOutsideClick);
      document.removeEventListener("keydown", handleEscape);
    };
  }, [open]);

  /*
   * =====================================================
   * LOAD CATEGORY NAVIGATION
   * =====================================================
   */

  useEffect(() => {
    if (!open) return;

    if (categories.length > 0) return;

    let cancelled = false;

    async function loadNavigation() {
      try {
        setLoading(true);

        const data = await categoryService.getNavigation();

        if (cancelled) return;

        setCategories(data);

        if (data.length > 0) {
          setSelectedCategory(data[0]);
        }
      } catch {
        if (!cancelled) {
          setCategories([]);
          setSelectedCategory(null);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    loadNavigation();

    return () => {
      cancelled = true;
    };
  }, [open, categories.length]);

  /*
   * =====================================================
   * CLOSE + NAVIGATE
   * =====================================================
   */

  const handleNavigate = () => {
    setOpen(false);
    onNavigate?.();
  };

  /*
   * =====================================================
   * MOBILE
   * =====================================================
   */

  if (mobile) {
    return (
      <div ref={menuRef} className="w-full">
        <button
          type="button"
          onClick={() => setOpen((current) => !current)}
          aria-expanded={open}
          aria-haspopup="true"
          className={[
            "flex w-full items-center justify-between rounded-xl px-3 py-3",
            "text-left text-sm font-medium text-[#1B2A4A]",
            "transition-colors hover:bg-[#F4F7F6]",
          ].join(" ")}
        >
          <span>Categories</span>

          <ChevronDown
            className={[
              "size-4 transition-transform duration-200",
              open ? "rotate-180" : "",
            ].join(" ")}
          />
        </button>

        {open && (
          <div className="mb-2 ml-2 border-l border-[#dfe8e5] pl-2">
            <Link
              href="/categories"
              onClick={handleNavigate}
              className="flex items-center justify-between rounded-lg px-3 py-2.5 text-sm font-semibold text-primary transition-colors hover:bg-[#F4F7F6]"
            >
              View All Categories

              <ArrowUpRight className="size-4" />
            </Link>

            {loading ? (
              <div className="flex items-center gap-2 px-3 py-4 text-sm text-muted-foreground">
                <Loader2 className="size-4 animate-spin" />

                Loading categories...
              </div>
            ) : categories.length > 0 ? (
              categories.map((category) => (
                <div key={category.id}>
                  <Link
                    href={`/products/category/${category.slug}`}
                    onClick={handleNavigate}
                    className="flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm text-[#1B2A4A] transition-colors hover:bg-[#F4F7F6] hover:text-primary"
                  >
                    <span className="size-1.5 shrink-0 rounded-full bg-[#3E8F96]" />

                    <span>{category.name}</span>
                  </Link>

                  {category.products.length > 0 && (
                    <div className="ml-6 border-l border-[#edf0ef] pl-2">
                      {category.products.map((product) => (
                        <Link
                          key={product.id}
                          href={`/products/${product.slug}`}
                          onClick={handleNavigate}
                          className="block rounded-lg px-3 py-2 text-xs text-[#617083] transition-colors hover:bg-[#F4F7F6] hover:text-primary"
                        >
                          {product.name}
                        </Link>
                      ))}
                    </div>
                  )}

                  {category.children.length > 0 && (
                    <div className="ml-4 border-l border-[#edf0ef] pl-2">
                      {category.children.map((child) => (
                        <div key={child.id}>
                          <Link
                            href={`/products/category/${child.slug}`}
                            onClick={handleNavigate}
                            className="block rounded-lg px-3 py-2 text-xs font-medium text-[#1B2A4A] transition-colors hover:bg-[#F4F7F6] hover:text-primary"
                          >
                            {child.name}
                          </Link>

                          {child.products.map((product) => (
                            <Link
                              key={product.id}
                              href={`/products/${product.slug}`}
                              onClick={handleNavigate}
                              className="ml-3 block rounded-lg px-3 py-1.5 text-xs text-[#617083] transition-colors hover:bg-[#F4F7F6] hover:text-primary"
                            >
                              {product.name}
                            </Link>
                          ))}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              ))
            ) : (
              <p className="px-3 py-4 text-xs text-muted-foreground">
                No categories available.
              </p>
            )}
          </div>
        )}
      </div>
    );
  }

  /*
   * =====================================================
   * DESKTOP
   * =====================================================
   */

  return (
    <div ref={menuRef} className="relative">
      <button
        type="button"
        onClick={() => setOpen((current) => !current)}
        aria-expanded={open}
        aria-haspopup="true"
        className={[
          "flex items-center gap-1.5 rounded-lg px-1 py-2",
          "text-sm font-medium text-[#1B2A4A]",
          "transition-colors hover:text-primary",
        ].join(" ")}
      >
        Categories

        <ChevronDown
          className={[
            "size-4 transition-transform duration-200",
            open ? "rotate-180" : "",
          ].join(" ")}
        />
      </button>

      {open && (
        <div className="absolute right-0 top-full z-50 mt-3 w-[760px] overflow-hidden rounded-2xl border border-[#e4e8e7] bg-white shadow-[0_20px_50px_rgba(27,42,74,0.14)]">
          {/* Header */}
          <div className="flex items-center justify-between border-b border-[#edf0ef] bg-[#fafbfb] px-5 py-4">
            <div>
              <p className="text-sm font-semibold text-[#1B2A4A]">
                Product Categories
              </p>

              <p className="mt-0.5 text-xs text-[#6b7280]">
                Explore our pharmaceutical product portfolio
              </p>
            </div>

            <Link
              href="/categories"
              onClick={handleNavigate}
              className="group inline-flex items-center gap-1 rounded-lg px-2.5 py-2 text-xs font-semibold text-primary transition-colors hover:bg-[#EAF6F1]"
            >
              View all

              <ArrowUpRight className="size-3.5 transition-transform group-hover:translate-x-0.5 group-hover:-translate-y-0.5" />
            </Link>
          </div>

          {/* Loading */}
          {loading ? (
            <div className="flex min-h-[280px] items-center justify-center gap-2 text-sm text-muted-foreground">
              <Loader2 className="size-4 animate-spin" />

              Loading categories...
            </div>
          ) : categories.length === 0 ? (
            /* Empty */
            <div className="px-5 py-12 text-center">
              <p className="text-sm font-medium text-[#1B2A4A]">
                No categories available
              </p>

              <p className="mt-1 text-xs text-muted-foreground">
                Please check the product catalogue later.
              </p>
            </div>
          ) : (
            <div className="grid min-h-[360px] grid-cols-[240px_1fr]">
              {/* =================================================
                  LEFT — CATEGORIES
              ================================================== */}

              <div className="border-r border-[#edf0ef] bg-[#fafbfb] p-3">
                <p className="px-3 pb-2 text-[10px] font-semibold uppercase tracking-[0.14em] text-[#8a9694]">
                  Categories
                </p>

                <div className="space-y-1">
                  {categories.map((category) => {
                    const isSelected =
                      selectedCategory?.id === category.id;

                    return (
                      <button
                        key={category.id}
                        type="button"
                        onMouseEnter={() =>
                          setSelectedCategory(category)
                        }
                        onFocus={() =>
                          setSelectedCategory(category)
                        }
                        onClick={() =>
                          setSelectedCategory(category)
                        }
                        className={[
                          "flex w-full items-center justify-between rounded-lg px-3 py-2.5",
                          "text-left text-[13px] font-medium transition-colors",
                          isSelected
                            ? "bg-white text-primary shadow-sm"
                            : "text-[#1B2A4A] hover:bg-white hover:text-primary",
                        ].join(" ")}
                      >
                        <span className="flex min-w-0 items-center gap-2">
                          <span
                            className={[
                              "size-1.5 shrink-0 rounded-full",
                              isSelected
                                ? "bg-primary"
                                : "bg-[#3E8F96]",
                            ].join(" ")}
                          />

                          <span className="truncate">
                            {category.name}
                          </span>
                        </span>

                        <ChevronDown className="size-3 -rotate-90 shrink-0" />
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* =================================================
                  RIGHT — PRODUCTS
              ================================================== */}

              <div className="p-5">
                {selectedCategory && (
                  <>
                    <div className="mb-5 flex items-start justify-between gap-4">
                      <div>
                        <p className="text-base font-semibold text-[#1B2A4A]">
                          {selectedCategory.name}
                        </p>

                        <p className="mt-1 text-xs text-[#6b7280]">
                          Products available in this category
                        </p>
                      </div>

                      <Link
                        href={`/products/category/${selectedCategory.slug}`}
                        onClick={handleNavigate}
                        className="inline-flex shrink-0 items-center gap-1 text-xs font-semibold text-primary transition-colors hover:underline"
                      >
                        View all

                        <ArrowUpRight className="size-3.5" />
                      </Link>
                    </div>

                    {/* Child categories */}
                    {selectedCategory.children.length > 0 && (
                      <div className="mb-5">
                        <p className="mb-2 text-[10px] font-semibold uppercase tracking-[0.14em] text-[#8a9694]">
                          Subcategories
                        </p>

                        <div className="grid grid-cols-2 gap-2">
                          {selectedCategory.children.map((child) => (
                            <Link
                              key={child.id}
                              href={`/products/category/${child.slug}`}
                              onClick={handleNavigate}
                              className="rounded-lg border border-[#edf0ef] px-3 py-2 text-xs font-medium text-[#1B2A4A] transition-colors hover:border-[#cbded9] hover:bg-[#F5F9F7] hover:text-primary"
                            >
                              {child.name}
                            </Link>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Products */}
                    <div>
                      <p className="mb-2 text-[10px] font-semibold uppercase tracking-[0.14em] text-[#8a9694]">
                        Products
                      </p>

                      {selectedCategory.products.length > 0 ? (
                        <div className="grid max-h-[230px] grid-cols-2 gap-1 overflow-y-auto pr-1">
                          {selectedCategory.products.map((product) => (
                            <Link
                              key={product.id}
                              href={`/products/${product.slug}`}
                              onClick={handleNavigate}
                              className="group rounded-lg px-3 py-2.5 transition-colors hover:bg-[#F5F9F7]"
                            >
                              <p className="text-xs font-medium leading-5 text-[#1B2A4A] group-hover:text-primary">
                                {product.name}
                              </p>
                            </Link>
                          ))}
                        </div>
                      ) : (
                        <p className="rounded-lg bg-[#fafbfb] px-3 py-4 text-xs text-muted-foreground">
                          No products available in this category.
                        </p>
                      )}
                    </div>
                  </>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}