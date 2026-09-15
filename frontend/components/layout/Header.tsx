"use client";

import Link from "next/link";
import {
  Search,
  ShoppingCart,
  UserRound,
  Menu,
  X,
  ChevronDown,
  ArrowUpRight,
} from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { UserProfile } from "@/components/account/UserProfile";

import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";
import type { Category } from "@/types/category";
import { EnquiryForm } from "@/components/forms/EnquiryForm";

interface HeaderProps {
  categories: Category[];
}

const navigation = [
  {
    label: "About",
    href: "/about",
  },
  {
    label: "Contact",
    href: "/contact",
  },
];

export function Header({ categories }: HeaderProps) {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [categoriesOpen, setCategoriesOpen] = useState(false);
  const [mobileCategoriesOpen, setMobileCategoriesOpen] = useState(false);
  const [enquiryOpen, setEnquiryOpen] = useState(false);

  const categoriesRef = useRef<HTMLDivElement>(null);

  const activeCategories = categories.filter(
    (category) => category.isActive
  );

  /* =================================================
     CLOSE CATEGORY DROPDOWN ON OUTSIDE CLICK
  ================================================== */

  useEffect(() => {
    function handleOutsideClick(event: MouseEvent) {
      if (
        categoriesRef.current &&
        !categoriesRef.current.contains(event.target as Node)
      ) {
        setCategoriesOpen(false);
      }
    }

    document.addEventListener("mousedown", handleOutsideClick);

    return () => {
      document.removeEventListener(
        "mousedown",
        handleOutsideClick
      );
    };
  }, []);

  /* =================================================
     CLOSE MENUS / MODAL WITH ESC
  ================================================== */

  useEffect(() => {
    function handleEscape(event: KeyboardEvent) {
      if (event.key !== "Escape") return;

      setCategoriesOpen(false);
      setMobileCategoriesOpen(false);
      setEnquiryOpen(false);
    }

    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("keydown", handleEscape);
    };
  }, []);

  /* =================================================
     BODY SCROLL LOCK WHEN ENQUIRY MODAL IS OPEN
  ================================================== */

  useEffect(() => {
    if (!enquiryOpen) return;

    const previousOverflow = document.body.style.overflow;

    document.body.style.overflow = "hidden";

    return () => {
      document.body.style.overflow = previousOverflow;
    };
  }, [enquiryOpen]);

  return (
    <>
      <header className="sticky top-0 z-50 border-b border-[#e7ebea] bg-white/95 backdrop-blur">
        <Container>
          <div className="flex min-h-16 items-center gap-4 py-2">
            {/* =================================================
                LOGO
            ================================================== */}

            <Link
              href="/"
              className="group flex shrink-0 items-center gap-2"
              aria-label="RashePharma Home"
            >
              <div className="flex size-10 items-center justify-center rounded-xl bg-brand-dark text-lg font-bold text-white transition-transform duration-200 group-hover:scale-[1.03]">
                R
              </div>

              <div className="hidden sm:block">
                <div className="text-lg font-bold tracking-tight text-[#1B2A4A]">
                  Rashe<span className="text-primary">Pharma</span>
                </div>

                <p className="text-[8px] font-medium uppercase tracking-[0.16em] text-muted-foreground">
                  Better Health. Global Reach.
                </p>
              </div>
            </Link>

            {/* =================================================
                DESKTOP SEARCH
            ================================================== */}

            <div className="mx-auto hidden w-full max-w-xl md:block">
              <div className="flex h-11 items-center rounded-xl border border-transparent bg-[#F3F4F4] px-3 transition-colors focus-within:border-[#cbded9] focus-within:bg-white">
                <Search className="mr-2 size-4 shrink-0 text-[#617083]" />

                <input
                  type="search"
                  placeholder="Search product or salt (e.g. cefixime)"
                  aria-label="Search products"
                  className="w-full border-0 bg-transparent text-sm text-foreground outline-none placeholder:text-[#718096]"
                />
              </div>
            </div>

            {/* =================================================
                DESKTOP NAVIGATION
            ================================================== */}

            <nav
              aria-label="Main navigation"
              className="hidden items-center gap-5 lg:flex"
            >
              {/* ================= CATEGORIES ================= */}

              <div
                ref={categoriesRef}
                className="relative"
              >
                <button
                  type="button"
                  onClick={() =>
                    setCategoriesOpen((current) => !current)
                  }
                  aria-expanded={categoriesOpen}
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
                      categoriesOpen ? "rotate-180" : "",
                    ].join(" ")}
                  />
                </button>

                {categoriesOpen && (
                  <div className="absolute right-0 top-full z-50 mt-3 w-[600px] overflow-hidden rounded-2xl border border-[#e4e8e7] bg-white shadow-[0_20px_50px_rgba(27,42,74,0.14)]">
                    {/* Dropdown header */}
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
                        onClick={() =>
                          setCategoriesOpen(false)
                        }
                        className="group inline-flex items-center gap-1 rounded-lg px-2.5 py-2 text-xs font-semibold text-primary transition-colors hover:bg-[#EAF6F1]"
                      >
                        View all
                        <ArrowUpRight className="size-3.5 transition-transform group-hover:translate-x-0.5 group-hover:-translate-y-0.5" />
                      </Link>
                    </div>

                    {/* Categories */}
                    {activeCategories.length > 0 ? (
                      <div className="grid max-h-[420px] grid-cols-3 gap-1 overflow-y-auto p-3">
                        {activeCategories.map((category) => (
                          <Link
                            key={category.id}
                            href={`/products/category/${category.slug}`}
                            onClick={() =>
                              setCategoriesOpen(false)
                            }
                            className="group flex items-center gap-2 rounded-lg px-3 py-2.5 transition-colors hover:bg-[#F5F9F7]"
                          >
                            <span className="size-1.5 shrink-0 rounded-full bg-[#3E8F96]" />

                            <p className="min-w-0 text-[13px] font-medium leading-5 text-[#1B2A4A] group-hover:text-primary">
                              {category.name}
                            </p>
                          </Link>
                        ))}
                      </div>
                    ) : (
                      <div className="px-5 py-10 text-center">
                        <p className="text-sm font-medium text-[#1B2A4A]">
                          No categories available
                        </p>

                        <p className="mt-1 text-xs text-muted-foreground">
                          Please check the product catalogue later.
                        </p>
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* ================= OTHER NAV ================= */}

              {navigation.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className="whitespace-nowrap text-sm font-medium text-[#1B2A4A] transition-colors hover:text-primary"
                >
                  {item.label}
                </Link>
              ))}

              {/* ================= ENQUIRE ================= */}

              <Button
                type="button"
                size="sm"
                onClick={() => setEnquiryOpen(true)}
                className="h-10 rounded-xl bg-[#F5821F] px-5 text-white shadow-sm transition-all hover:bg-[#df7115] hover:shadow-md"
              >
                Enquire
              </Button>
            </nav>

            {/* =================================================
                DESKTOP ACCOUNT / CART
            ================================================== */}

            {/* =================================================
    DESKTOP ACCOUNT / CART
================================================== */}

<div className="hidden items-center gap-1 lg:flex">
  <UserProfile />

  <Link
    href="/cart"
    aria-label="Shopping cart"
  >
    <Button
      variant="ghost"
      size="icon"
      className="rounded-lg text-[#1B2A4A] hover:bg-[#F3F6F5] hover:text-primary"
    >
      <ShoppingCart className="size-4" />
    </Button>
  </Link>
</div>
            

            {/* =================================================
                MOBILE ACTIONS
            ================================================== */}

            <div className="ml-auto flex items-center gap-1 md:hidden">
              <Button
                variant="ghost"
                size="icon"
                aria-label="Search products"
                className="rounded-lg text-[#1B2A4A]"
              >
                <Search className="size-5" />
              </Button>

              <Link
                href="/cart"
                aria-label="Shopping cart"
              >
                <Button
                  variant="ghost"
                  size="icon"
                  className="rounded-lg text-[#1B2A4A]"
                >
                  <ShoppingCart className="size-5" />
                </Button>
              </Link>

              <Button
                variant="ghost"
                size="icon"
                aria-label={
                  mobileMenuOpen
                    ? "Close navigation menu"
                    : "Open navigation menu"
                }
                aria-expanded={mobileMenuOpen}
                onClick={() =>
                  setMobileMenuOpen((current) => !current)
                }
                className="rounded-lg text-[#1B2A4A]"
              >
                {mobileMenuOpen ? (
                  <X className="size-5" />
                ) : (
                  <Menu className="size-5" />
                )}
              </Button>
            </div>
          </div>

          {/* ===================================================
              MOBILE SEARCH
          ==================================================== */}

          <div className="pb-3 md:hidden">
            <div className="flex h-10 items-center rounded-xl border border-transparent bg-[#F3F4F4] px-3 focus-within:border-[#cbded9] focus-within:bg-white">
              <Search className="mr-2 size-4 shrink-0 text-[#617083]" />

              <input
                type="search"
                placeholder="Search products or salt..."
                aria-label="Search products"
                className="w-full border-0 bg-transparent text-sm text-foreground outline-none placeholder:text-muted-foreground"
              />
            </div>
          </div>

          {/* ===================================================
              MOBILE NAVIGATION
          ==================================================== */}

          {mobileMenuOpen && (
            <div className="border-t border-[#edf0ef] py-3 md:hidden">
              <nav
                aria-label="Mobile navigation"
                className="flex flex-col"
              >
                {/* ================= CATEGORIES ================= */}

                <button
                  type="button"
                  onClick={() =>
                    setMobileCategoriesOpen(
                      (current) => !current
                    )
                  }
                  aria-expanded={mobileCategoriesOpen}
                  className="flex w-full items-center justify-between rounded-xl px-3 py-3 text-left text-sm font-medium text-[#1B2A4A] transition-colors hover:bg-[#F4F7F6]"
                >
                  <span>Categories</span>

                  <ChevronDown
                    className={[
                      "size-4 transition-transform duration-200",
                      mobileCategoriesOpen
                        ? "rotate-180"
                        : "",
                    ].join(" ")}
                  />
                </button>

                {mobileCategoriesOpen && (
                  <div className="mb-2 ml-2 border-l border-[#dfe8e5] pl-2">
                    <Link
                      href="/categories"
                      onClick={() =>
                        setMobileMenuOpen(false)
                      }
                      className="flex items-center justify-between rounded-lg px-3 py-2.5 text-sm font-semibold text-primary hover:bg-[#F4F7F6]"
                    >
                      View All Categories
                      <ArrowUpRight className="size-4" />
                    </Link>

                    {activeCategories.map((category) => (
                      <Link
                        key={category.id}
                        href={`/products/category/${category.slug}`}
                        onClick={() =>
                          setMobileMenuOpen(false)
                        }
                        className="flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm text-[#1B2A4A] hover:bg-[#F4F7F6] hover:text-primary"
                      >
                        <span className="size-1.5 rounded-full bg-[#3E8F96]" />

                        {category.name}
                      </Link>
                    ))}
                  </div>
                )}

                {/* ================= OTHER NAV ================= */}

                {navigation.map((item) => (
                  <Link
                    key={item.href}
                    href={item.href}
                    onClick={() =>
                      setMobileMenuOpen(false)
                    }
                    className="rounded-xl px-3 py-3 text-sm font-medium text-[#1B2A4A] transition-colors hover:bg-[#F4F7F6] hover:text-primary"
                  >
                    {item.label}
                  </Link>
                ))}

                {/* ================= SIGN IN ================= */}

                <Link
                  href="/login"
                  onClick={() =>
                    setMobileMenuOpen(false)
                  }
                  className="rounded-xl px-3 py-3 text-sm font-medium text-[#1B2A4A] transition-colors hover:bg-[#F4F7F6] hover:text-primary"
                >
                  Sign In
                </Link>

                {/* ================= ENQUIRE ================= */}

                <Button
                  type="button"
                  onClick={() => {
                    setMobileMenuOpen(false);
                    setMobileCategoriesOpen(false);
                    setEnquiryOpen(true);
                  }}
                  className="mt-2 h-11 w-full rounded-xl bg-[#F5821F] text-white hover:bg-[#df7115]"
                >
                  Enquire
                </Button>
              </nav>
            </div>
          )}
        </Container>
      </header>

      {/* =====================================================
          ENQUIRY MODAL
      ====================================================== */}

      {enquiryOpen && (
        <div
          className="fixed inset-0 z-[100] overflow-y-auto bg-black/45 px-4 py-6 backdrop-blur-sm sm:py-10"
          role="presentation"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) {
              setEnquiryOpen(false);
            }
          }}
        >
          <div
            className="mx-auto w-full max-w-2xl"
            role="dialog"
            aria-modal="true"
            aria-labelledby="enquiry-modal-title"
          >
            <div className="relative">
              {/* Close button */}
              <button
                type="button"
                onClick={() => setEnquiryOpen(false)}
                aria-label="Close enquiry form"
                className="absolute right-3 top-3 z-20 flex size-9 items-center justify-center rounded-full border border-[#e5e8e7] bg-white text-[#1B2A4A] shadow-sm transition-colors hover:bg-[#F2F2F2]"
              >
                <X className="size-5" />
              </button>

              {/* Form */}
              <EnquiryForm
                onSuccess={() => setEnquiryOpen(false)}
              />
            </div>
          </div>
        </div>
      )}
    </>
  );
}