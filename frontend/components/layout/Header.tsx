"use client";

import Image from "next/image";
import Link from "next/link";

import {
  usePathname,
  useRouter,
  useSearchParams,
} from "next/navigation";

import {
  Search,
  ShoppingCart,
  UserRound,
  Menu,
  X,
  Pill,
  FolderTree,
} from "lucide-react";

import {
  useEffect,
  useMemo,
  useState,
} from "react";

import { UserProfile } from "@/components/account/UserProfile";
import { CategoriesMegaMenu } from "@/components/navigation/CategoriesMegaMenu";
import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";
import { EnquiryForm } from "@/components/forms/EnquiryForm";

import { productService } from "@/services/product.service";
import { categoryService } from "@/services/category.service";
import { useAuth } from "@/components/providers/AuthProvider";

import type { ProductList } from "@/types/product";
import type { Category } from "@/types/category";

import { MobileMenu } from "@/components/navigation/MobileMenu";

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

export function Header() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const pathname = usePathname();

  const {
    isAuthenticated,
    isLoading: authLoading,
  } = useAuth();

  const [mounted, setMounted] = useState(false);

  const [mobileMenuOpen, setMobileMenuOpen] =
    useState(false);

  const [enquiryOpen, setEnquiryOpen] =
    useState(false);

  const [products, setProducts] =
    useState<ProductList[]>([]);

  const [categories, setCategories] =
    useState<Category[]>([]);

  const [desktopSearch, setDesktopSearch] =
    useState("");

  const [mobileSearch, setMobileSearch] =
    useState("");

  const [desktopSearchFocused, setDesktopSearchFocused] =
    useState(false);

  const [mobileSearchFocused, setMobileSearchFocused] =
    useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  /*
   * =================================================
   * ENQUIRY BUTTON
   * ==================================================
   */

  const handleEnquiryClick = () => {
    if (authLoading) return;

    if (!isAuthenticated) {
      router.push("/login?redirect=enquiry");
      return;
    }

    setEnquiryOpen(true);
  };

  /*
   * =================================================
   * OPEN ENQUIRY FROM QUERY PARAM
   * ==================================================
   */

  useEffect(() => {
    const openEnquiry =
      searchParams.get("openEnquiry");

    if (
      openEnquiry === "1" &&
      isAuthenticated &&
      !authLoading
    ) {
      setEnquiryOpen(true);

      router.replace("/", {
        scroll: false,
      });
    }
  }, [
    searchParams,
    isAuthenticated,
    authLoading,
    router,
  ]);

  /*
   * =================================================
   * LOAD PRODUCTS + CATEGORIES
   * ==================================================
   */

  useEffect(() => {
    async function loadSearchData() {
      try {
        const [
          productData,
          categoryData,
        ] = await Promise.all([
          productService.getAll(),
          categoryService.getAll(),
        ]);

        setProducts(productData);
        setCategories(categoryData);
      } catch {
        setProducts([]);
        setCategories([]);
      }
    }

    loadSearchData();
  }, []);

  /*
   * =================================================
   * DESKTOP SEARCH RESULTS
   * ==================================================
   */

  const desktopSearchResults = useMemo(() => {
    const query =
      desktopSearch.trim().toLowerCase();

    if (!query) {
      return {
        products: [],
        categories: [],
      };
    }

    const matchingProducts = products
      .filter((product) => {
        return (
          product.name
            .toLowerCase()
            .includes(query) ||
          (product.genericName ?? "")
            .toLowerCase()
            .includes(query) ||
          (product.composition ?? "")
            .toLowerCase()
            .includes(query) ||
          product.categoryName
            .toLowerCase()
            .includes(query)
        );
      })
      .slice(0, 5);

    const matchingCategories = categories
      .filter((category) => {
        return (
          category.isActive &&
          (
            category.name
              .toLowerCase()
              .includes(query) ||
            category.slug
              .toLowerCase()
              .includes(query)
          )
        );
      })
      .slice(0, 4);

    return {
      products: matchingProducts,
      categories: matchingCategories,
    };
  }, [
    desktopSearch,
    products,
    categories,
  ]);

  /*
   * =================================================
   * MOBILE SEARCH RESULTS
   * ==================================================
   */

  const mobileSearchResults = useMemo(() => {
    const query =
      mobileSearch.trim().toLowerCase();

    if (!query) {
      return {
        products: [],
        categories: [],
      };
    }

    const matchingProducts = products
      .filter((product) => {
        return (
          product.name
            .toLowerCase()
            .includes(query) ||
          (product.genericName ?? "")
            .toLowerCase()
            .includes(query) ||
          (product.composition ?? "")
            .toLowerCase()
            .includes(query) ||
          product.categoryName
            .toLowerCase()
            .includes(query)
        );
      })
      .slice(0, 5);

    const matchingCategories = categories
      .filter((category) => {
        return (
          category.isActive &&
          (
            category.name
              .toLowerCase()
              .includes(query) ||
            category.slug
              .toLowerCase()
              .includes(query)
          )
        );
      })
      .slice(0, 4);

    return {
      products: matchingProducts,
      categories: matchingCategories,
    };
  }, [
    mobileSearch,
    products,
    categories,
  ]);

  /*
   * =================================================
   * SEARCH SUBMIT
   * ==================================================
   */

  const handleSearch = (value: string) => {
    const query = value.trim();

    if (!query) {
      return;
    }

    router.push(
      `/products?search=${encodeURIComponent(query)}`
    );

    setDesktopSearchFocused(false);
    setMobileSearchFocused(false);
    setMobileMenuOpen(false);
  };

  /*
   * =================================================
   * CLOSE MENUS / MODAL WITH ESC
   * ==================================================
   */

  useEffect(() => {
    function handleEscape(event: KeyboardEvent) {
      if (event.key !== "Escape") return;

      setEnquiryOpen(false);
      setMobileMenuOpen(false);
      setDesktopSearchFocused(false);
      setMobileSearchFocused(false);
    }

    document.addEventListener(
      "keydown",
      handleEscape
    );

    return () => {
      document.removeEventListener(
        "keydown",
        handleEscape
      );
    };
  }, []);

  /*
   * =================================================
   * BODY SCROLL LOCK WHEN ENQUIRY MODAL IS OPEN
   * ==================================================
   */

  useEffect(() => {
    if (!enquiryOpen) return;

    const previousOverflow =
      document.body.style.overflow;

    document.body.style.overflow = "hidden";

    return () => {
      document.body.style.overflow =
        previousOverflow;
    };
  }, [enquiryOpen]);

  /*
   * IMPORTANT:
   * All hooks are above this point.
   * Admin pages simply hide the public Header.
   */

  if (pathname.startsWith("/admin")) {
    return null;
  }

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
              className="group flex shrink-0 items-center"
              aria-label="Rashe Lifesciences Home"
            >
              <div className="relative h-14 w-auto shrink-0 transition-transform duration-200 group-hover:scale-[1.03]">
                <Image
                  src="/images/Rashelifescience.png"
                  alt="Rashe Lifesciences Pvt Ltd."
                  width={240}
                  height={62}
                  priority
                  className="h-full w-auto object-contain"
                />
              </div>
            </Link>

            {/* =================================================
                DESKTOP SEARCH
            ================================================== */}

            <div className="relative mx-auto hidden w-full max-w-xl md:block">
              <form
                onSubmit={(event) => {
                  event.preventDefault();
                  handleSearch(desktopSearch);
                }}
              >
                <div className="flex h-11 items-center rounded-xl border border-transparent bg-[#F3F4F4] px-3 transition-colors focus-within:border-[#cbded9] focus-within:bg-white">
                  <Search className="mr-2 size-4 shrink-0 text-[#617083]" />

                  <input
                    type="search"
                    value={desktopSearch}
                    onChange={(event) =>
                      setDesktopSearch(
                        event.target.value
                      )
                    }
                    onFocus={() =>
                      setDesktopSearchFocused(true)
                    }
                    placeholder="Search product or salt (e.g. cefixime)"
                    aria-label="Search products"
                    className="w-full border-0 bg-transparent text-sm text-foreground outline-none placeholder:text-[#718096]"
                  />
                </div>
              </form>

              {/* Desktop suggestions */}

              {desktopSearchFocused &&
                desktopSearch.trim() && (
                  <div className="absolute left-0 right-0 top-12 z-[70] overflow-hidden rounded-xl border border-[#e5e5e5] bg-white shadow-xl">

                    {desktopSearchResults.products
                      .length > 0 && (
                      <div className="p-2">
                        <p className="px-3 py-2 text-[10px] font-semibold uppercase tracking-[0.12em] text-[#999]">
                          Products
                        </p>

                        {desktopSearchResults.products.map(
                          (product) => (
                            <Link
                              key={product.id}
                              href={`/products/${product.slug}`}
                              onClick={() =>
                                setDesktopSearchFocused(
                                  false
                                )
                              }
                              className="flex items-center gap-3 rounded-lg px-3 py-2.5 transition-colors hover:bg-[#F4F7F6]"
                            >
                              <div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-[#E8F4F4]">
                                <Pill className="size-4 text-[#3E8F96]" />
                              </div>

                              <div className="min-w-0">
                                <p className="truncate text-sm font-medium text-[#1B2A4A]">
                                  {product.name}
                                </p>

                                <p className="truncate text-[11px] text-[#888]">
                                  {product.categoryName}
                                </p>
                              </div>
                            </Link>
                          )
                        )}
                      </div>
                    )}

                    {desktopSearchResults.categories
                      .length > 0 && (
                      <div className="border-t border-[#eeeeee] p-2">
                        <p className="px-3 py-2 text-[10px] font-semibold uppercase tracking-[0.12em] text-[#999]">
                          Categories
                        </p>

                        {desktopSearchResults.categories.map(
                          (category) => (
                            <Link
                              key={category.id}
                              href={`/products/category/${category.slug}`}
                              onClick={() =>
                                setDesktopSearchFocused(
                                  false
                                )
                              }
                              className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-[#1B2A4A] transition-colors hover:bg-[#F4F7F6]"
                            >
                              <FolderTree className="size-4 text-[#3E8F96]" />

                              {category.name}
                            </Link>
                          )
                        )}
                      </div>
                    )}

                    {desktopSearchResults.products
                      .length === 0 &&
                      desktopSearchResults.categories
                        .length === 0 && (
                        <div className="px-4 py-6 text-center">
                          <p className="text-sm font-medium text-[#1B2A4A]">
                            No results found
                          </p>

                          <p className="mt-1 text-xs text-[#888]">
                            Try another product, salt or
                            category.
                          </p>
                        </div>
                      )}
                  </div>
                )}
            </div>

            {/* =================================================
                DESKTOP NAVIGATION
            ================================================== */}

            <nav
              aria-label="Main navigation"
              className="hidden items-center gap-5 lg:flex"
            >
              <CategoriesMegaMenu />

              {navigation.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className="whitespace-nowrap text-sm font-medium text-[#1B2A4A] transition-colors hover:text-primary"
                >
                  {item.label}
                </Link>
              ))}

              <Button
                type="button"
                size="sm"
                onClick={handleEnquiryClick}
                disabled={!mounted || authLoading}
                className="h-10 rounded-xl bg-[#F5821F] px-5 text-white shadow-sm transition-all hover:bg-[#df7115] hover:shadow-md disabled:cursor-not-allowed disabled:opacity-70"
              >
                Enquire
              </Button>
            </nav>

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
                onClick={() =>
                  document
                    .getElementById(
                      "mobile-product-search"
                    )
                    ?.focus()
                }
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
                  setMobileMenuOpen(
                    (current) => !current
                  )
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

          <div className="relative pb-3 md:hidden">
            <form
              onSubmit={(event) => {
                event.preventDefault();
                handleSearch(mobileSearch);
              }}
            >
              <div className="flex h-10 items-center rounded-xl border border-transparent bg-[#F3F4F4] px-3 focus-within:border-[#cbded9] focus-within:bg-white">
                <Search className="mr-2 size-4 shrink-0 text-[#617083]" />

                <input
                  id="mobile-product-search"
                  type="search"
                  value={mobileSearch}
                  onChange={(event) =>
                    setMobileSearch(
                      event.target.value
                    )
                  }
                  onFocus={() =>
                    setMobileSearchFocused(true)
                  }
                  placeholder="Search products or salt..."
                  aria-label="Search products"
                  className="w-full border-0 bg-transparent text-sm text-foreground outline-none placeholder:text-muted-foreground"
                />
              </div>
            </form>

            {/* Mobile suggestions */}

            {mobileSearchFocused &&
              mobileSearch.trim() && (
                <div className="absolute left-0 right-0 top-12 z-[70] max-h-[70vh] overflow-y-auto rounded-xl border border-[#e5e5e5] bg-white shadow-xl">

                  {mobileSearchResults.products
                    .length > 0 && (
                    <div className="p-2">
                      <p className="px-3 py-2 text-[10px] font-semibold uppercase tracking-[0.12em] text-[#999]">
                        Products
                      </p>

                      {mobileSearchResults.products.map(
                        (product) => (
                          <Link
                            key={product.id}
                            href={`/products/${product.slug}`}
                            onClick={() =>
                              setMobileSearchFocused(
                                false
                              )
                            }
                            className="flex items-center gap-3 rounded-lg px-3 py-2.5 hover:bg-[#F4F7F6]"
                          >
                            <div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-[#E8F4F4]">
                              <Pill className="size-4 text-[#3E8F96]" />
                            </div>

                            <div className="min-w-0">
                              <p className="truncate text-sm font-medium text-[#1B2A4A]">
                                {product.name}
                              </p>

                              <p className="truncate text-[11px] text-[#888]">
                                {product.categoryName}
                              </p>
                            </div>
                          </Link>
                        )
                      )}
                    </div>
                  )}

                  {mobileSearchResults.categories
                    .length > 0 && (
                    <div className="border-t border-[#eeeeee] p-2">
                      <p className="px-3 py-2 text-[10px] font-semibold uppercase tracking-[0.12em] text-[#999]">
                        Categories
                      </p>

                      {mobileSearchResults.categories.map(
                        (category) => (
                          <Link
                            key={category.id}
                            href={`/products/category/${category.slug}`}
                            onClick={() =>
                              setMobileSearchFocused(
                                false
                              )
                            }
                            className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-[#1B2A4A] hover:bg-[#F4F7F6]"
                          >
                            <FolderTree className="size-4 text-[#3E8F96]" />

                            {category.name}
                          </Link>
                        )
                      )}
                    </div>
                  )}

                  {mobileSearchResults.products
                    .length === 0 &&
                    mobileSearchResults.categories
                      .length === 0 && (
                      <div className="px-4 py-6 text-center">
                        <p className="text-sm font-medium text-[#1B2A4A]">
                          No results found
                        </p>

                        <p className="mt-1 text-xs text-[#888]">
                          Try another product, salt or
                          category.
                        </p>
                      </div>
                    )}
                </div>
              )}
          </div>

          {/* ===================================================
              MOBILE MENU
          ==================================================== */}

          {mobileMenuOpen && (
            <MobileMenu
              onClose={() =>
                setMobileMenuOpen(false)
              }
              onEnquire={handleEnquiryClick}
            />
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
            if (
              event.target === event.currentTarget
            ) {
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
                onClick={() =>
                  setEnquiryOpen(false)
                }
                aria-label="Close enquiry form"
                className="absolute right-3 top-3 z-20 flex size-9 items-center justify-center rounded-full border border-[#e5e8e7] bg-white text-[#1B2A4A] shadow-sm transition-colors hover:bg-[#F2F2F2]"
              >
                <X className="size-5" />
              </button>

              {/* Form */}

              <EnquiryForm />
            </div>
          </div>
        </div>
      )}
    </>
  );
}