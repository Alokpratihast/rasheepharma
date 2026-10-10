"use client";

import Image from "next/image";
import Link from "next/link";
import {
  usePathname,
  useRouter,
  useSearchParams,
} from "next/navigation";
import {
  Mail,
  Menu,
  Phone,
  ShoppingCart,
  X,
} from "lucide-react";
import {
  useEffect,
  useRef,
  useState,
  useSyncExternalStore,
} from "react";

import { UserProfile } from "@/components/account/UserProfile";
import { CategoriesMegaMenu } from "@/components/navigation/CategoriesMegaMenu";
import { MobileMenu } from "@/components/navigation/MobileMenu";
import { EnquiryForm } from "@/components/forms/EnquiryForm";
import { HeaderSearch } from "@/components/layout/HeaderSearch";
import { Button } from "@/components/ui/button";
import { Container } from "@/components/ui/container";
import { useAuth } from "@/components/providers/AuthProvider";

import { productService } from "@/services/product.service";
import { categoryService } from "@/services/category.service";
import { siteConfig } from "@/lib/site";
import { cn } from "@/lib/utils";

import type { ProductList } from "@/types/product";
import type { Category } from "@/types/category";

const navigation = [
  { label: "Products", href: "/products" },
  { label: "About", href: "/about" },
  { label: "Contact", href: "/contact" },
];

export function Header() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const { isAuthenticated, isLoading: authLoading } = useAuth();

  // true only after hydration (avoids an effect + setState)
  const mounted = useSyncExternalStore(
    () => () => {},
    () => true,
    () => false,
  );
  const [scrolled, setScrolled] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [enquiryOpen, setEnquiryOpen] = useState(false);
  const [products, setProducts] = useState<ProductList[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);

  const progressRef = useRef<HTMLDivElement>(null);

  /* ---------- scroll: compact header + progress bar ---------- */
  useEffect(() => {
    function onScroll() {
      const y = window.scrollY;
      setScrolled(y > 16);

      const max =
        document.documentElement.scrollHeight - window.innerHeight;
      const progress = max > 0 ? Math.min(y / max, 1) : 0;

      if (progressRef.current) {
        progressRef.current.style.transform = `scaleX(${progress})`;
      }
    }

    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    window.addEventListener("resize", onScroll);
    return () => {
      window.removeEventListener("scroll", onScroll);
      window.removeEventListener("resize", onScroll);
    };
  }, [pathname]);

  /* ---------- enquiry ---------- */
  const handleEnquiryClick = () => {
    if (authLoading) return;

    if (!isAuthenticated) {
      router.push("/login?redirect=enquiry");
      return;
    }

    setEnquiryOpen(true);
  };

  useEffect(() => {
    if (
      searchParams.get("openEnquiry") === "1" &&
      isAuthenticated &&
      !authLoading
    ) {
      // Opens the modal from ?openEnquiry=1 (same behaviour as before)
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setEnquiryOpen(true);
      router.replace("/", { scroll: false });
    }
  }, [searchParams, isAuthenticated, authLoading, router]);

  /* ---------- search data ---------- */
  useEffect(() => {
    async function loadSearchData() {
      try {
        const [productData, categoryData] = await Promise.all([
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

  /* ---------- ESC closes menu / modal ---------- */
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key !== "Escape") return;
      setEnquiryOpen(false);
      setMobileMenuOpen(false);
    }
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, []);

  /* ---------- lock body scroll while modal / menu is open ---------- */
  useEffect(() => {
    if (!enquiryOpen) return;
    const previous = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = previous;
    };
  }, [enquiryOpen]);

  /* All hooks are above this point. Admin pages hide the public header. */
  if (pathname.startsWith("/admin")) {
    return null;
  }

  const isActive = (href: string) =>
    pathname === href || pathname.startsWith(`${href}/`);

  return (
    <>
      <header
        className={cn(
          "sticky top-0 z-50 w-full transition-shadow duration-300",
          scrolled && "shadow-[0_8px_30px_rgba(7,63,50,0.08)]",
        )}
      >
        {/* =============== ANNOUNCEMENT BAR =============== */}
        <div
          className={cn(
            "hidden overflow-hidden bg-brand-dark text-white transition-all duration-300 md:block",
            scrolled ? "max-h-0 opacity-0" : "max-h-10 opacity-100",
          )}
        >
          <Container className="flex h-10 items-center justify-between text-xs">
            <p className="flex items-center gap-2 text-white/80">
              <span className="relative flex size-2">
                <span className="absolute inline-flex size-full animate-ping rounded-full bg-emerald-400 opacity-60" />
                <span className="relative inline-flex size-2 rounded-full bg-emerald-400" />
              </span>
              {siteConfig.announcement}
            </p>

            <div className="flex items-center gap-5">
              <a
                href={siteConfig.phoneHref}
                className="flex items-center gap-1.5 text-white/80 transition-colors hover:text-white"
              >
                <Phone className="size-3.5" />
                {siteConfig.phone}
              </a>
              <a
                href={siteConfig.emailHref}
                className="flex items-center gap-1.5 text-white/80 transition-colors hover:text-white"
              >
                <Mail className="size-3.5" />
                {siteConfig.email}
              </a>
            </div>
          </Container>
        </div>

        {/* =============== MAIN BAR =============== */}
        <div className="relative border-b border-border/70 bg-background/80 backdrop-blur-xl supports-[backdrop-filter]:bg-background/70">
          <Container>
            <div
              className={cn(
                "flex items-center gap-4 transition-all duration-300 lg:gap-6",
                scrolled ? "h-16" : "h-[76px]",
              )}
            >
              {/* Logo */}
              <Link
                href="/"
                aria-label="Rashe Lifesciences Home"
                className="group flex shrink-0 items-center"
              >
                <Image
                  src="/images/Rashelifescience.png"
                  alt="Rashe Lifesciences Pvt Ltd."
                  width={240}
                  height={62}
                  priority
                  className={cn(
                    "w-auto object-contain transition-all duration-300 group-hover:scale-[1.03]",
                    scrolled ? "h-10" : "h-12",
                  )}
                />
              </Link>

              {/* Desktop search */}
              <HeaderSearch
                products={products}
                categories={categories}
                enableShortcut
                className="mx-auto hidden w-full max-w-xl md:block"
              />

              {/* Desktop navigation */}
              <nav
                aria-label="Main navigation"
                className="hidden items-center gap-1 lg:flex"
              >
                <CategoriesMegaMenu />

                {navigation.map((item) => (
                  <Link
                    key={item.href}
                    href={item.href}
                    aria-current={isActive(item.href) ? "page" : undefined}
                    className={cn(
                      "group relative whitespace-nowrap rounded-full px-3.5 py-2 text-sm font-medium transition-colors",
                      isActive(item.href)
                        ? "text-primary"
                        : "text-foreground/80 hover:text-primary",
                    )}
                  >
                    {item.label}
                    <span
                      className={cn(
                        "absolute inset-x-3.5 -bottom-0.5 h-0.5 origin-left rounded-full bg-primary transition-transform duration-300",
                        isActive(item.href)
                          ? "scale-x-100"
                          : "scale-x-0 group-hover:scale-x-100",
                      )}
                    />
                  </Link>
                ))}
              </nav>

              {/* Desktop actions */}
              <div className="hidden items-center gap-1.5 lg:flex">
                <UserProfile />

                <Link
                  href="/cart"
                  aria-label="Shopping cart"
                  className="flex size-9 items-center justify-center rounded-full text-foreground/80 transition-colors hover:bg-primary-light hover:text-primary"
                >
                  <ShoppingCart className="size-[18px]" />
                </Link>

                <Button
                  type="button"
                  onClick={handleEnquiryClick}
                  disabled={!mounted || authLoading}
                  className="ml-1 h-10 whitespace-nowrap rounded-full bg-gradient-to-r from-primary to-teal-600 px-5 text-sm font-semibold text-white shadow-[0_8px_20px_rgba(8,127,91,0.30)] transition-all hover:-translate-y-0.5 hover:shadow-[0_12px_28px_rgba(8,127,91,0.42)] disabled:translate-y-0 disabled:opacity-60"
                >
                  Request Quote
                </Button>
              </div>

              {/* Mobile actions */}
              <div className="ml-auto flex items-center gap-1 md:hidden">
                <Link
                  href="/cart"
                  aria-label="Shopping cart"
                  className="flex size-9 items-center justify-center rounded-full text-foreground/80"
                >
                  <ShoppingCart className="size-5" />
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
                  onClick={() => setMobileMenuOpen((c) => !c)}
                  className="rounded-full text-foreground/80"
                >
                  {mobileMenuOpen ? (
                    <X className="size-5" />
                  ) : (
                    <Menu className="size-5" />
                  )}
                </Button>
              </div>

              {/* Tablet: menu button when md..lg (desktop nav hidden) */}
              <div className="hidden items-center gap-1 md:flex lg:hidden">
                <Link
                  href="/cart"
                  aria-label="Shopping cart"
                  className="flex size-9 items-center justify-center rounded-full text-foreground/80"
                >
                  <ShoppingCart className="size-5" />
                </Link>
                <Button
                  variant="ghost"
                  size="icon"
                  aria-label="Toggle navigation menu"
                  aria-expanded={mobileMenuOpen}
                  onClick={() => setMobileMenuOpen((c) => !c)}
                  className="rounded-full text-foreground/80"
                >
                  {mobileMenuOpen ? (
                    <X className="size-5" />
                  ) : (
                    <Menu className="size-5" />
                  )}
                </Button>
              </div>
            </div>

            {/* Mobile search row */}
            <div className="pb-3 md:hidden">
              <HeaderSearch
                products={products}
                categories={categories}
                placeholder="Search products or salt..."
                onNavigate={() => setMobileMenuOpen(false)}
              />
            </div>

            {/* Mobile / tablet menu */}
            {mobileMenuOpen && (
              <MobileMenu
                onClose={() => setMobileMenuOpen(false)}
                onEnquire={handleEnquiryClick}
              />
            )}
          </Container>

          {/* Scroll progress */}
          <div
            aria-hidden="true"
            className="pointer-events-none absolute inset-x-0 bottom-0 h-[2px]"
          >
            <div
              ref={progressRef}
              className="h-full origin-left scale-x-0 bg-gradient-to-r from-primary via-teal-400 to-emerald-300"
            />
          </div>
        </div>
      </header>

      {/* =============== ENQUIRY MODAL =============== */}
      {enquiryOpen && (
        <div
          className="fixed inset-0 z-[100] overflow-y-auto bg-brand-dark/60 px-4 py-6 backdrop-blur-sm animate-in fade-in duration-200 sm:py-10"
          role="presentation"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) {
              setEnquiryOpen(false);
            }
          }}
        >
          <div
            className="mx-auto w-full max-w-2xl animate-in zoom-in-95 slide-in-from-bottom-4 duration-300"
            role="dialog"
            aria-modal="true"
            aria-labelledby="enquiry-modal-title"
          >
            <div className="relative">
              <button
                type="button"
                onClick={() => setEnquiryOpen(false)}
                aria-label="Close enquiry form"
                className="absolute right-3 top-3 z-20 flex size-9 items-center justify-center rounded-full border border-border bg-background text-foreground shadow-sm transition-colors hover:bg-muted"
              >
                <X className="size-5" />
              </button>

              <EnquiryForm />
            </div>
          </div>
        </div>
      )}
    </>
  );
}
