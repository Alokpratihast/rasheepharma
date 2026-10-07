"use client";

import { useEffect } from "react";
import { Check, Search, X } from "lucide-react";

import { cn } from "@/lib/utils";

export const ALL_CATEGORIES = "All Categories";
export const ALL_FORMS = "All Forms";

export interface FacetOption {
  name: string;
  count: number;
}

interface FilterPanelProps {
  search: string;
  category: string;
  form: string;
  categories: FacetOption[];
  forms: FacetOption[];
  totalCount: number;
  onSearchChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onFormChange: (value: string) => void;
  onClear: () => void;
}

const sectionTitle =
  "mb-3 text-[11px] font-bold uppercase tracking-[0.16em] text-muted-foreground";

/* ============================================================
   FILTER CONTENT (shared by desktop sidebar + mobile sheet)
============================================================ */

function FilterPanel({
  search,
  category,
  form,
  categories,
  forms,
  totalCount,
  onSearchChange,
  onCategoryChange,
  onFormChange,
  onClear,
}: FilterPanelProps) {
  const hasFilters =
    search.trim().length > 0 ||
    category !== ALL_CATEGORIES ||
    form !== ALL_FORMS;

  return (
    <div className="space-y-7">
      {/* Search */}
      <div>
        <p className={sectionTitle}>Search</p>
        <div className="group flex h-11 items-center gap-2 rounded-xl border border-border bg-muted/60 px-3 transition-all focus-within:border-primary/50 focus-within:bg-background focus-within:shadow-[0_0_0_4px_rgba(8,127,91,0.10)]">
          <Search className="size-4 shrink-0 text-muted-foreground group-focus-within:text-primary" />
          <input
            type="search"
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder="Product, salt or category"
            aria-label="Search product or salt"
            className="min-w-0 flex-1 bg-transparent text-sm text-foreground outline-none placeholder:text-muted-foreground"
          />
        </div>
      </div>

      {/* Categories */}
      <div>
        <p className={sectionTitle}>Category</p>
        <ul className="space-y-1">
          {[{ name: ALL_CATEGORIES, count: totalCount }, ...categories].map(
            (item) => {
              const active = category === item.name;
              return (
                <li key={item.name}>
                  <button
                    type="button"
                    onClick={() => onCategoryChange(item.name)}
                    aria-pressed={active}
                    className={cn(
                      "flex w-full items-center justify-between gap-3 rounded-xl px-3 py-2.5 text-left text-sm transition-colors",
                      active
                        ? "bg-primary-light font-semibold text-primary"
                        : "text-foreground/80 hover:bg-muted",
                    )}
                  >
                    <span className="flex min-w-0 items-center gap-2">
                      {active && <Check className="size-4 shrink-0" />}
                      <span className="truncate">{item.name}</span>
                    </span>
                    <span
                      className={cn(
                        "rounded-full px-2 py-0.5 text-[11px] font-semibold",
                        active
                          ? "bg-primary/10 text-primary"
                          : "bg-muted text-muted-foreground",
                      )}
                    >
                      {item.count}
                    </span>
                  </button>
                </li>
              );
            },
          )}
        </ul>
      </div>

      {/* Dosage form */}
      {forms.length > 0 && (
        <div>
          <p className={sectionTitle}>Dosage form</p>
          <div className="flex flex-wrap gap-2">
            {[{ name: ALL_FORMS, count: totalCount }, ...forms].map((item) => {
              const active = form === item.name;
              return (
                <button
                  key={item.name}
                  type="button"
                  onClick={() => onFormChange(item.name)}
                  aria-pressed={active}
                  className={cn(
                    "rounded-full border px-3.5 py-1.5 text-xs font-semibold transition-all",
                    active
                      ? "border-primary bg-primary text-primary-foreground shadow-sm"
                      : "border-border bg-background text-foreground/80 hover:border-primary/50 hover:text-primary",
                  )}
                >
                  {item.name === ALL_FORMS ? "All" : item.name}
                </button>
              );
            })}
          </div>
        </div>
      )}

      {hasFilters && (
        <button
          type="button"
          onClick={onClear}
          className="inline-flex h-10 w-full items-center justify-center gap-2 rounded-xl border border-border text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
        >
          <X className="size-4" />
          Clear all filters
        </button>
      )}
    </div>
  );
}

/* ============================================================
   DESKTOP SIDEBAR
============================================================ */

export function ProductFilters(props: FilterPanelProps) {
  return (
    <aside
      aria-label="Product filters"
      className="sticky top-28 hidden self-start rounded-3xl border border-border bg-card p-6 shadow-[0_10px_40px_rgba(7,63,50,0.05)] lg:block"
    >
      <FilterPanel {...props} />
    </aside>
  );
}

/* ============================================================
   MOBILE BOTTOM SHEET
============================================================ */

interface MobileFilterSheetProps extends FilterPanelProps {
  open: boolean;
  onClose: () => void;
  resultCount: number;
}

export function MobileFilterSheet({
  open,
  onClose,
  resultCount,
  ...panel
}: MobileFilterSheetProps) {
  useEffect(() => {
    if (!open) return;

    const previous = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    document.addEventListener("keydown", onKey);

    return () => {
      document.body.style.overflow = previous;
      document.removeEventListener("keydown", onKey);
    };
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-[90] flex items-end bg-brand-dark/60 backdrop-blur-sm animate-in fade-in duration-200 lg:hidden"
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Filters"
        className="flex max-h-[88vh] w-full flex-col rounded-t-[28px] bg-background shadow-2xl animate-in slide-in-from-bottom duration-300"
      >
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h2 className="text-base font-bold text-foreground">Filters</h2>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close filters"
            className="flex size-9 items-center justify-center rounded-full bg-muted text-foreground"
          >
            <X className="size-5" />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto px-5 py-5">
          <FilterPanel {...panel} />
        </div>

        <div className="border-t border-border bg-background px-5 py-4 pb-[max(1rem,env(safe-area-inset-bottom))]">
          <button
            type="button"
            onClick={onClose}
            className="h-12 w-full rounded-full bg-gradient-to-r from-primary to-teal-600 text-sm font-semibold text-white shadow-[0_8px_20px_rgba(8,127,91,0.3)]"
          >
            Show {resultCount} {resultCount === 1 ? "product" : "products"}
          </button>
        </div>
      </div>
    </div>
  );
}
