"use client";

import { Search, SlidersHorizontal, X } from "lucide-react";
import { useState } from "react";

interface ProductFiltersProps {
  search?: string;
  category?: string;
  form?: string;
  onSearchChange?: (value: string) => void;
  onCategoryChange?: (value: string) => void;
  onFormChange?: (value: string) => void;
  onClear?: () => void;
}

const categories = [
  "All Categories",
  "Gynae Range",
  "Anti-Inflammatory",
  "Antibiotics",
  "Anti-Ulcerant",
  "Multivitamin Products",
  "Other Products",
  "Injectables",
  "Syrups",
  "Gel/Oil",
  "Powder & Sachet",
  "Derma Range",
  "Derma Cosmetic Range",
  "Pediatric Range",
  "Gummies Range",
  "Veterinary Range",
  "Dental Range",
  "Diabetic Range",
];

const dosageForms = [
  "All Forms",
  "Tablets",
  "Capsules",
  "Syrup",
  "Injection",
  "Cream",
  "Lotion",
  "Gel",
  "Powder",
  "Sachet",
  "Drops",
  "Suspension",
];

export function ProductFilters({
  search = "",
  category = "All Categories",
  form = "All Forms",
  onSearchChange,
  onCategoryChange,
  onFormChange,
  onClear,
}: ProductFiltersProps) {
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  const hasFilters =
    search.trim().length > 0 ||
    category !== "All Categories" ||
    form !== "All Forms";

  const handleClear = () => {
    onClear?.();
  };

  return (
    <div className="mb-6">
      {/* Desktop / Tablet */}
      <div className="hidden gap-3 rounded-xl border border-[#e5e5e5] bg-white p-3 md:flex">
        {/* Search */}
        <div className="flex min-w-0 flex-1 items-center rounded-lg bg-[#F2F2F2] px-3">
          <Search className="mr-2 size-4 shrink-0 text-[#888]" />

          <input
            type="search"
            value={search}
            onChange={(event) =>
              onSearchChange?.(event.target.value)
            }
            placeholder="Search product or salt..."
            aria-label="Search product or salt"
            className="w-full bg-transparent py-2 text-sm text-[#1B2A4A] outline-none placeholder:text-[#999]"
          />
        </div>

        {/* Category */}
        <select
          value={category}
          onChange={(event) =>
            onCategoryChange?.(event.target.value)
          }
          aria-label="Filter by category"
          className="h-10 min-w-44 rounded-lg border border-[#e5e5e5] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96]"
        >
          {categories.map((item) => (
            <option key={item} value={item}>
              {item}
            </option>
          ))}
        </select>

        {/* Dosage form */}
        <select
          value={form}
          onChange={(event) =>
            onFormChange?.(event.target.value)
          }
          aria-label="Filter by dosage form"
          className="h-10 min-w-36 rounded-lg border border-[#e5e5e5] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96]"
        >
          {dosageForms.map((item) => (
            <option key={item} value={item}>
              {item}
            </option>
          ))}
        </select>

        {/* Clear */}
        {hasFilters && (
          <button
            type="button"
            onClick={handleClear}
            className="inline-flex h-10 shrink-0 items-center gap-1.5 rounded-lg px-3 text-sm font-medium text-[#888] transition-colors hover:bg-[#F2F2F2] hover:text-[#1B2A4A]"
          >
            <X className="size-4" />
            Clear
          </button>
        )}
      </div>

      {/* Mobile */}
      <div className="space-y-3 md:hidden">
        <div className="flex h-11 items-center rounded-lg bg-[#F2F2F2] px-3">
          <Search className="mr-2 size-4 shrink-0 text-[#888]" />

          <input
            type="search"
            value={search}
            onChange={(event) =>
              onSearchChange?.(event.target.value)
            }
            placeholder="Search product or salt..."
            aria-label="Search product or salt"
            className="w-full bg-transparent text-sm text-[#1B2A4A] outline-none placeholder:text-[#999]"
          />
        </div>

        <button
          type="button"
          onClick={() =>
            setMobileFiltersOpen((current) => !current)
          }
          className="flex h-10 w-full items-center justify-center gap-2 rounded-lg border border-[#e5e5e5] bg-white text-sm font-medium text-[#1B2A4A]"
          aria-expanded={mobileFiltersOpen}
        >
          <SlidersHorizontal className="size-4" />
          Filters

          {hasFilters && (
            <span className="rounded-full bg-[#3E8F96] px-2 py-0.5 text-[10px] font-semibold text-white">
              Active
            </span>
          )}
        </button>

        {mobileFiltersOpen && (
          <div className="space-y-3 rounded-xl border border-[#e5e5e5] bg-white p-4">
            <div>
              <label
                htmlFor="mobile-category"
                className="mb-1.5 block text-xs font-semibold text-[#1B2A4A]"
              >
                Category
              </label>

              <select
                id="mobile-category"
                value={category}
                onChange={(event) =>
                  onCategoryChange?.(event.target.value)
                }
                className="h-10 w-full rounded-lg border border-[#e5e5e5] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96]"
              >
                {categories.map((item) => (
                  <option key={item} value={item}>
                    {item}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label
                htmlFor="mobile-form"
                className="mb-1.5 block text-xs font-semibold text-[#1B2A4A]"
              >
                Dosage Form
              </label>

              <select
                id="mobile-form"
                value={form}
                onChange={(event) =>
                  onFormChange?.(event.target.value)
                }
                className="h-10 w-full rounded-lg border border-[#e5e5e5] bg-white px-3 text-sm text-[#1B2A4A] outline-none focus:border-[#3E8F96]"
              >
                {dosageForms.map((item) => (
                  <option key={item} value={item}>
                    {item}
                  </option>
                ))}
              </select>
            </div>

            {hasFilters && (
              <button
                type="button"
                onClick={handleClear}
                className="inline-flex h-10 w-full items-center justify-center gap-2 rounded-lg border border-[#e5e5e5] text-sm font-medium text-[#555] hover:bg-[#F2F2F2]"
              >
                <X className="size-4" />
                Clear Filters
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}