"use client";

import Link from "next/link";
import {
  ArrowUpRight,
  Baby,
  Bone,
  ChevronDown,
  HeartPulse,
  Loader2,
  Pill,
  ShieldPlus,
  Wind,
} from "lucide-react";
import { useCallback, useEffect, useRef, useState } from "react";
import type { KeyboardEvent as ReactKeyboardEvent } from "react";

import { Container } from "@/components/ui/container";
import { categoryService } from "@/services/category.service";
import { siteConfig } from "@/lib/site";
import { cn } from "@/lib/utils";
import type { CategoryNavigation } from "@/types/category";

interface CategoriesMegaMenuProps {
  mobile?: boolean;
  onNavigate?: () => void;
}

type Status = "idle" | "loading" | "ready" | "error";

/** Icons cycle by position, same as the home page category cards. */
const icons = [ShieldPlus, Wind, HeartPulse, Bone, Baby, Pill];

export function CategoriesMegaMenu({
  mobile = false,
  onNavigate,
}: CategoriesMegaMenuProps) {
  const [open, setOpen] = useState(false);
  const [status, setStatus] = useState<Status>("idle");
  const [categories, setCategories] = useState<CategoryNavigation[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);

  const menuRef = useRef<HTMLDivElement>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);
  const listRef = useRef<HTMLDivElement>(null);
  const productsRef = useRef<HTMLDivElement>(null);
  const startedRef = useRef(false);

  const selected =
    categories.find((category) => category.id === selectedId) ??
    categories[0] ??
    null;

  /*
   * Loads the navigation data once. Called from event handlers
   * (hover, focus, click) so the menu is usually ready by the time
   * it opens, and so a failed request can be retried.
   */
  const ensureLoaded = useCallback(async () => {
    if (startedRef.current) return;
    startedRef.current = true;
    setStatus("loading");

    try {
      const data = await categoryService.getNavigation();
      setCategories(data);
      setSelectedId(data[0]?.id ?? null);
      setStatus("ready");
    } catch {
      startedRef.current = false;
      setStatus("error");
    }
  }, []);

  /* Close on outside click / Escape (Escape returns focus to the button) */
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
        buttonRef.current?.focus();
      }
    };

    document.addEventListener("mousedown", handleOutsideClick);
    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("mousedown", handleOutsideClick);
      document.removeEventListener("keydown", handleEscape);
    };
  }, [open]);

  const toggle = () => {
    setOpen((current) => !current);
    void ensureLoaded();
  };

  const handleNavigate = () => {
    setOpen(false);
    onNavigate?.();
  };

  /* Arrow keys move through categories; Right jumps to the products */
  const handleListKeyDown = (
    event: ReactKeyboardEvent<HTMLDivElement>,
  ) => {
    const items = Array.from(
      listRef.current?.querySelectorAll<HTMLButtonElement>(
        "button[data-category]",
      ) ?? [],
    );
    const index = items.findIndex(
      (item) => item === document.activeElement,
    );

    if (index === -1) return;

    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
      event.preventDefault();
      const step = event.key === "ArrowDown" ? 1 : -1;
      items[(index + step + items.length) % items.length].focus();
    } else if (event.key === "ArrowRight") {
      const firstLink =
        productsRef.current?.querySelector<HTMLAnchorElement>("a");

      if (firstLink) {
        event.preventDefault();
        firstLink.focus();
      }
    }
  };

  /*
   * =====================================================
   * MOBILE (accordion, used inside MobileMenu)
   * =====================================================
   */

  if (mobile) {
    return (
      <div ref={menuRef} className="w-full">
        <button
          type="button"
          onClick={toggle}
          aria-expanded={open}
          aria-haspopup="true"
          className="flex w-full items-center justify-between rounded-xl px-3 py-3 text-left text-sm font-medium text-brand-dark transition-colors hover:bg-primary-light/60"
        >
          <span>Categories</span>

          <ChevronDown
            className={cn(
              "size-4 transition-transform duration-200",
              open && "rotate-180",
            )}
          />
        </button>

        {open && (
          <div className="mb-2 ml-2 border-l border-border pl-2">
            <Link
              href="/categories"
              onClick={handleNavigate}
              className="flex items-center justify-between rounded-lg px-3 py-2.5 text-sm font-semibold text-primary transition-colors hover:bg-primary-light/60"
            >
              View all categories
              <ArrowUpRight className="size-4" />
            </Link>

            {status === "loading" || status === "idle" ? (
              <div className="flex items-center gap-2 px-3 py-4 text-sm text-muted-foreground">
                <Loader2 className="size-4 animate-spin" />
                Loading categories...
              </div>
            ) : status === "error" ? (
              <div className="px-3 py-4 text-sm text-muted-foreground">
                Couldn&apos;t load categories.{" "}
                <button
                  type="button"
                  onClick={() => void ensureLoaded()}
                  className="font-semibold text-primary hover:underline"
                >
                  Try again
                </button>
              </div>
            ) : categories.length > 0 ? (
              categories.map((category) => (
                <div key={category.id}>
                  <Link
                    href={`/products/category/${category.slug}`}
                    onClick={handleNavigate}
                    className="flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm text-brand-dark transition-colors hover:bg-primary-light/60 hover:text-primary"
                  >
                    <span className="size-1.5 shrink-0 rounded-full bg-primary" />
                    <span>{category.name}</span>
                  </Link>

                  {category.products.length > 0 && (
                    <div className="ml-6 border-l border-border pl-2">
                      {category.products.map((product) => (
                        <Link
                          key={product.id}
                          href={`/products/${product.slug}`}
                          onClick={handleNavigate}
                          className="block rounded-lg px-3 py-2 text-xs text-muted-foreground transition-colors hover:bg-primary-light/60 hover:text-primary"
                        >
                          {product.name}
                        </Link>
                      ))}
                    </div>
                  )}

                  {category.children.length > 0 && (
                    <div className="ml-4 border-l border-border pl-2">
                      {category.children.map((child) => (
                        <div key={child.id}>
                          <Link
                            href={`/products/category/${child.slug}`}
                            onClick={handleNavigate}
                            className="block rounded-lg px-3 py-2 text-xs font-medium text-brand-dark transition-colors hover:bg-primary-light/60 hover:text-primary"
                          >
                            {child.name}
                          </Link>

                          {child.products.map((product) => (
                            <Link
                              key={product.id}
                              href={`/products/${product.slug}`}
                              onClick={handleNavigate}
                              className="ml-3 block rounded-lg px-3 py-1.5 text-xs text-muted-foreground transition-colors hover:bg-primary-light/60 hover:text-primary"
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
                No categories yet.
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
   *
   * The wrapper is deliberately NOT `relative`. The panel is
   * positioned against the header bar (which is `relative`), so it
   * spans the page container and can never run off-screen,
   * wherever the Categories button sits in the header.
   * =====================================================
   */

  const productCount = selected?.products.length ?? 0;

  return (
    <div ref={menuRef}>
      <button
        ref={buttonRef}
        type="button"
        onClick={toggle}
        onPointerEnter={() => void ensureLoaded()}
        onFocus={() => void ensureLoaded()}
        aria-expanded={open}
        aria-haspopup="true"
        aria-controls="categories-mega-menu"
        className={cn(
          "group relative flex items-center gap-1.5 whitespace-nowrap rounded-full px-3.5 py-2 text-sm font-medium transition-colors",
          open
            ? "text-primary"
            : "text-foreground/80 hover:text-primary",
        )}
      >
        Categories
        <ChevronDown
          className={cn(
            "size-4 transition-transform duration-200",
            open && "rotate-180",
          )}
        />
        <span
          className={cn(
            "absolute inset-x-3.5 -bottom-0.5 h-0.5 origin-left rounded-full bg-primary transition-transform duration-300",
            open
              ? "scale-x-100"
              : "scale-x-0 group-hover:scale-x-100",
          )}
        />
      </button>

      {open && (
        <>
          {/* Dims the page so the menu reads as the foreground */}
          <div
            aria-hidden="true"
            onClick={() => setOpen(false)}
            className="absolute inset-x-0 top-full z-40 h-screen bg-brand-dark/30 animate-in fade-in duration-200"
          />

          <div
            id="categories-mega-menu"
            className="absolute inset-x-0 top-full z-50"
          >
            <Container>
              <div
                aria-busy={status === "loading"}
                className="mt-2 h-[min(520px,calc(100vh-220px))] overflow-hidden rounded-3xl border border-border bg-white shadow-[0_30px_70px_rgba(7,63,50,0.24)] animate-in fade-in slide-in-from-top-2 duration-200"
              >
                {status === "idle" || status === "loading" ? (
                  <MenuSkeleton />
                ) : status === "error" ? (
                  <div className="flex h-full flex-col items-center justify-center gap-3 px-6 text-center">
                    <p className="text-sm font-medium text-brand-dark">
                      We couldn&apos;t load the categories.
                    </p>

                    <button
                      type="button"
                      onClick={() => void ensureLoaded()}
                      className="rounded-full bg-primary px-5 py-2 text-sm font-semibold text-white transition-colors hover:bg-primary-dark"
                    >
                      Try again
                    </button>
                  </div>
                ) : categories.length === 0 || !selected ? (
                  <div className="flex h-full flex-col items-center justify-center gap-3 px-6 text-center">
                    <p className="text-sm font-medium text-brand-dark">
                      No categories yet.
                    </p>

                    <Link
                      href="/products"
                      onClick={handleNavigate}
                      className="text-sm font-semibold text-primary hover:underline"
                    >
                      Browse all products
                    </Link>
                  </div>
                ) : (
                  <div className="grid h-full lg:grid-cols-[250px_1fr] xl:grid-cols-[260px_1fr_280px]">
                    {/* =============== LEFT: CATEGORIES =============== */}
                    <div className="flex min-h-0 flex-col bg-primary-light/40">
                      <p className="px-5 pb-2 pt-5 text-sm font-semibold text-brand-dark">
                        Categories
                      </p>

                      <div
                        ref={listRef}
                        onKeyDown={handleListKeyDown}
                        className="min-h-0 flex-1 space-y-1 overflow-y-auto px-3 pb-3"
                      >
                        {categories.map((category, index) => {
                          const Icon = icons[index % icons.length];
                          const isSelected =
                            selected.id === category.id;

                          return (
                            <button
                              key={category.id}
                              type="button"
                              data-category
                              tabIndex={isSelected ? 0 : -1}
                              aria-current={
                                isSelected ? "true" : undefined
                              }
                              onMouseEnter={() =>
                                setSelectedId(category.id)
                              }
                              onFocus={() =>
                                setSelectedId(category.id)
                              }
                              onClick={() =>
                                setSelectedId(category.id)
                              }
                              className={cn(
                                "flex w-full items-center gap-3 rounded-xl px-2.5 py-2 text-left text-sm font-medium transition-colors",
                                isSelected
                                  ? "bg-white text-primary shadow-sm ring-1 ring-primary/15"
                                  : "text-brand-dark hover:bg-white/70",
                              )}
                            >
                              <span
                                className={cn(
                                  "flex size-8 shrink-0 items-center justify-center rounded-lg transition-colors",
                                  isSelected
                                    ? "bg-primary text-white"
                                    : "bg-white text-primary",
                                )}
                              >
                                <Icon
                                  aria-hidden="true"
                                  className="size-4"
                                  strokeWidth={1.8}
                                />
                              </span>

                              <span className="min-w-0 flex-1 truncate">
                                {category.name}
                              </span>

                              <span className="text-xs font-normal text-muted-foreground">
                                {category.products.length}
                              </span>
                            </button>
                          );
                        })}
                      </div>

                      <Link
                        href="/categories"
                        onClick={handleNavigate}
                        className="group flex items-center justify-between border-t border-border/70 px-5 py-3.5 text-sm font-semibold text-primary transition-colors hover:bg-white"
                      >
                        View all categories
                        <ArrowUpRight className="size-4 transition-transform group-hover:-translate-y-0.5 group-hover:translate-x-0.5" />
                      </Link>
                    </div>

                    {/* =============== MIDDLE: PRODUCTS =============== */}
                    <div className="min-h-0 overflow-y-auto p-6">
                      <div ref={productsRef}>
                        <div className="flex items-start justify-between gap-4">
                          <div className="min-w-0">
                            <h3 className="truncate text-xl font-bold tracking-tight text-brand-dark">
                              {selected.name}
                            </h3>

                            <p className="mt-1 text-sm text-muted-foreground">
                              {productCount}{" "}
                              {productCount === 1
                                ? "product"
                                : "products"}
                            </p>
                          </div>

                          <Link
                            href={`/products/category/${selected.slug}`}
                            onClick={handleNavigate}
                            className="inline-flex h-9 shrink-0 items-center gap-1.5 rounded-full bg-primary-light px-4 text-sm font-semibold text-primary transition-colors hover:bg-primary hover:text-white"
                          >
                            View all
                            <ArrowUpRight className="size-3.5" />
                          </Link>
                        </div>

                        {selected.children.length > 0 && (
                          <div className="mt-5">
                            <p className="mb-2 text-sm font-semibold text-brand-dark">
                              Subcategories
                            </p>

                            <div className="flex flex-wrap gap-2">
                              {selected.children.map((child) => (
                                <Link
                                  key={child.id}
                                  href={`/products/category/${child.slug}`}
                                  onClick={handleNavigate}
                                  className="rounded-full border border-border px-3.5 py-1.5 text-sm font-medium text-brand-dark transition-colors hover:border-primary hover:text-primary"
                                >
                                  {child.name}
                                </Link>
                              ))}
                            </div>
                          </div>
                        )}

                        <div className="mt-6">
                          {selected.products.length > 0 ? (
                            <div className="grid gap-1.5 sm:grid-cols-2">
                              {selected.products.map((product) => (
                                <Link
                                  key={product.id}
                                  href={`/products/${product.slug}`}
                                  onClick={handleNavigate}
                                  className="group flex items-center gap-3 rounded-xl border border-transparent p-2.5 transition-colors hover:border-border hover:bg-primary-light/50"
                                >
                                  <span className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-primary-light text-primary">
                                    <Pill
                                      aria-hidden="true"
                                      className="size-[18px]"
                                      strokeWidth={1.8}
                                    />
                                  </span>

                                  <span className="line-clamp-2 min-w-0 flex-1 text-sm font-medium leading-5 text-brand-dark group-hover:text-primary">
                                    {product.name}
                                  </span>

                                  <ArrowUpRight
                                    aria-hidden="true"
                                    className="size-4 shrink-0 text-primary opacity-0 transition-opacity group-hover:opacity-100"
                                  />
                                </Link>
                              ))}
                            </div>
                          ) : (
                            <p className="rounded-xl bg-muted px-4 py-5 text-sm text-muted-foreground">
                              No products in this category yet.
                            </p>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* =============== RIGHT: ENQUIRY (wide screens) =============== */}
                    <aside className="relative isolate hidden overflow-hidden bg-brand-dark p-6 text-white xl:flex xl:flex-col xl:justify-end">
                      <div
                        aria-hidden="true"
                        className="pointer-events-none absolute -right-16 -top-16 -z-10 size-56 rounded-full bg-primary/60 blur-[70px]"
                      />

                      <h3 className="text-xl font-bold leading-tight tracking-tight">
                        Can&apos;t find a product?
                      </h3>

                      <p className="mt-2 text-sm leading-6 text-white/75">
                        Tell us what you need and our team will get
                        back to you.
                      </p>

                      <Link
                        href="/b2b/enquiry"
                        onClick={handleNavigate}
                        className="mt-5 inline-flex h-11 items-center justify-center gap-2 rounded-full bg-white px-5 text-sm font-bold text-brand-dark transition-colors hover:bg-emerald-50"
                      >
                        Send an enquiry
                        <ArrowUpRight className="size-4" />
                      </Link>

                      <a
                        href={siteConfig.emailHref}
                        className="mt-4 break-all text-sm text-white/65 transition-colors hover:text-white"
                      >
                        {siteConfig.email}
                      </a>
                    </aside>
                  </div>
                )}
              </div>
            </Container>
          </div>
        </>
      )}
    </div>
  );
}

/** Placeholder with the same layout, shown while the data loads. */
function MenuSkeleton() {
  return (
    <div className="grid h-full lg:grid-cols-[250px_1fr] xl:grid-cols-[260px_1fr_280px]">
      <div className="space-y-2 bg-primary-light/40 p-4">
        {Array.from({ length: 6 }).map((_, index) => (
          <div
            key={index}
            className="h-11 animate-pulse rounded-xl bg-white"
          />
        ))}
      </div>

      <div className="p-6">
        <div className="h-7 w-48 animate-pulse rounded-lg bg-muted" />

        <div className="mt-8 grid gap-2 sm:grid-cols-2">
          {Array.from({ length: 6 }).map((_, index) => (
            <div
              key={index}
              className="h-14 animate-pulse rounded-xl bg-muted"
            />
          ))}
        </div>
      </div>

      <div className="hidden bg-brand-dark xl:block" />
    </div>
  );
}
