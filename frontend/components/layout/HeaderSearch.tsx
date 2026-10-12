"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  useEffect,
  useId,
  useMemo,
  useRef,
  useState,
} from "react";
import {
  ArrowRight,
  FolderTree,
  Pill,
  Search,
  X,
} from "lucide-react";

import { cn } from "@/lib/utils";
import type { ProductList } from "@/types/product";
import type { Category } from "@/types/category";

interface HeaderSearchProps {
  products: ProductList[];
  categories: Category[];
  placeholder?: string;
  /** Ctrl/Cmd + K focuses this input. Enable on ONE instance only. */
  enableShortcut?: boolean;
  className?: string;
  onNavigate?: () => void;
}

interface ResultItem {
  key: string;
  href: string;
  title: string;
  subtitle?: string;
  type: "product" | "category";
}

export function HeaderSearch({
  products,
  categories,
  placeholder = "Search product or salt (e.g. cefixime)",
  enableShortcut = false,
  className,
  onNavigate,
}: HeaderSearchProps) {
  const router = useRouter();
  const listId = useId();
  const wrapperRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const [query, setQuery] = useState("");
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);

  const trimmed = query.trim();

  /* ---------- results ---------- */
  const items = useMemo<ResultItem[]>(() => {
    const q = trimmed.toLowerCase();
    if (!q) return [];

    const productItems: ResultItem[] = products
      .filter(
        (p) =>
          p.name.toLowerCase().includes(q) ||
          (p.genericName ?? "").toLowerCase().includes(q) ||
          (p.composition ?? "").toLowerCase().includes(q) ||
          p.categoryName.toLowerCase().includes(q),
      )
      .slice(0, 5)
      .map((p) => ({
        key: `p-${p.id}`,
        href: `/products/${p.slug}`,
        title: p.name,
        subtitle: p.categoryName,
        type: "product",
      }));

    const categoryItems: ResultItem[] = categories
      .filter(
        (c) =>
          c.isActive &&
          (c.name.toLowerCase().includes(q) ||
            c.slug.toLowerCase().includes(q)),
      )
      .slice(0, 4)
      .map((c) => ({
        key: `c-${c.id}`,
        href: `/products/category/${c.slug}`,
        title: c.name,
        type: "category",
      }));

    return [...productItems, ...categoryItems];
  }, [trimmed, products, categories]);

  const allResultsHref = `/products?search=${encodeURIComponent(trimmed)}`;
  // last row = "view all results"
  const totalRows = items.length + (trimmed ? 1 : 0);

  function close() {
    setOpen(false);
    setActive(-1);
  }

  function go(href: string) {
    router.push(href);
    close();
    onNavigate?.();
  }

  /* ---------- keyboard ---------- */
  function onKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === "ArrowDown") {
      e.preventDefault();
      setOpen(true);
      setActive((i) => (i + 1) % Math.max(totalRows, 1));
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setActive((i) => (i <= 0 ? totalRows - 1 : i - 1));
    } else if (e.key === "Escape") {
      close();
      inputRef.current?.blur();
    } else if (e.key === "Enter") {
      e.preventDefault();
      if (!trimmed) return;
      if (active >= 0 && active < items.length) {
        go(items[active].href);
      } else {
        go(allResultsHref);
      }
    }
  }

  /* ---------- click outside ---------- */
  useEffect(() => {
    function onPointerDown(e: MouseEvent) {
      if (
        wrapperRef.current &&
        !wrapperRef.current.contains(e.target as Node)
      ) {
        setOpen(false);
        setActive(-1);
      }
    }
    document.addEventListener("mousedown", onPointerDown);
    return () =>
      document.removeEventListener("mousedown", onPointerDown);
  }, []);

  /* ---------- Ctrl/Cmd + K ---------- */
  useEffect(() => {
    if (!enableShortcut) return;
    function onShortcut(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        inputRef.current?.focus();
        setOpen(true);
      }
    }
    document.addEventListener("keydown", onShortcut);
    return () => document.removeEventListener("keydown", onShortcut);
  }, [enableShortcut]);

  const products_ = items.filter((i) => i.type === "product");
  const categories_ = items.filter((i) => i.type === "category");

  const renderRow = (item: ResultItem) => {
    const index = items.indexOf(item);
    const isActive = index === active;
    return (
      <li key={item.key} role="option" aria-selected={isActive}>
        <Link
          href={item.href}
          onClick={() => {
            close();
            onNavigate?.();
          }}
          onMouseEnter={() => setActive(index)}
          className={cn(
            "flex items-center gap-3 rounded-xl px-3 py-2.5 transition-colors",
            isActive ? "bg-primary-light" : "hover:bg-muted",
          )}
        >
          <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary-light text-primary">
            {item.type === "product" ? (
              <Pill className="size-4" />
            ) : (
              <FolderTree className="size-4" />
            )}
          </span>
          <span className="min-w-0">
            <span className="block truncate text-sm font-medium text-foreground">
              {item.title}
            </span>
            {item.subtitle && (
              <span className="block truncate text-xs text-muted-foreground">
                {item.subtitle}
              </span>
            )}
          </span>
        </Link>
      </li>
    );
  };

  const groupLabel =
    "px-3 pb-1 pt-2 text-[10px] font-semibold uppercase tracking-[0.14em] text-muted-foreground";

  return (
    <div ref={wrapperRef} className={cn("relative", className)}>
      <form
        role="search"
        onSubmit={(e) => {
          e.preventDefault();
          if (trimmed) go(allResultsHref);
        }}
      >
        <div className="group flex h-11 items-center gap-2 rounded-full border border-border bg-muted/70 px-4 transition-all duration-200 focus-within:border-primary/50 focus-within:bg-background focus-within:shadow-[0_0_0_4px_rgba(8,127,91,0.12)]">
          <Search className="size-4 shrink-0 text-muted-foreground transition-colors group-focus-within:text-primary" />
          <input
            ref={inputRef}
            type="text"
            role="combobox"
            aria-expanded={open && !!trimmed}
            aria-controls={listId}
            aria-autocomplete="list"
            autoComplete="off"
            value={query}
            onChange={(e) => {
              setQuery(e.target.value);
              setOpen(true);
              setActive(-1);
            }}
            onFocus={() => setOpen(true)}
            onKeyDown={onKeyDown}
            placeholder={placeholder}
            aria-label="Search products"
            className="min-w-0 flex-1 bg-transparent text-sm text-foreground outline-none placeholder:text-muted-foreground"
          />
          {query ? (
            <button
              type="button"
              aria-label="Clear search"
              onClick={() => {
                setQuery("");
                inputRef.current?.focus();
              }}
              className="flex size-6 items-center justify-center rounded-full text-muted-foreground transition-colors hover:bg-border hover:text-foreground"
            >
              <X className="size-3.5" />
            </button>
          ) : (
            enableShortcut && (
              <kbd className="hidden rounded-md border border-border bg-background px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground xl:block">
                Ctrl K
              </kbd>
            )
          )}
        </div>
      </form>

      {open && trimmed && (
        <div className="absolute inset-x-0 top-[calc(100%+8px)] z-[70] max-h-[70vh] overflow-y-auto rounded-2xl border border-border bg-background p-2 shadow-[0_24px_60px_rgba(7,63,50,0.18)] animate-in fade-in slide-in-from-top-2 duration-150">
          <ul id={listId} role="listbox">
            {products_.length > 0 && (
              <>
                <li role="presentation" className={groupLabel}>
                  Products
                </li>
                {products_.map(renderRow)}
              </>
            )}
            {categories_.length > 0 && (
              <>
                <li
                  role="presentation"
                  className={cn(
                    groupLabel,
                    products_.length > 0 && "mt-1 border-t border-border pt-3",
                  )}
                >
                  Categories
                </li>
                {categories_.map(renderRow)}
              </>
            )}
            {items.length === 0 && (
              <li
                role="presentation"
                className="px-4 py-6 text-center"
              >
                <p className="text-sm font-medium text-foreground">
                  No results found
                </p>
                <p className="mt-1 text-xs text-muted-foreground">
                  Try another product, salt or category.
                </p>
              </li>
            )}
          </ul>

          <Link
            href={allResultsHref}
            onClick={() => {
              close();
              onNavigate?.();
            }}
            onMouseEnter={() => setActive(items.length)}
            className={cn(
              "mt-1 flex items-center justify-between rounded-xl px-3 py-2.5 text-sm font-semibold text-primary transition-colors",
              active === items.length
                ? "bg-primary-light"
                : "hover:bg-muted",
            )}
          >
            <span className="truncate">
              See all results for &ldquo;{trimmed}&rdquo;
            </span>
            <ArrowRight className="size-4 shrink-0" />
          </Link>
        </div>
      )}
    </div>
  );
}
