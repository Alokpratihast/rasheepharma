"use client";

interface ProductFiltersProps {
  search: string;
  category: string;
  status: string;
  featured: string;
  categories: string[];
  onSearchChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onStatusChange: (value: string) => void;
  onFeaturedChange: (value: string) => void;
}

export function ProductFilters({
  search,
  category,
  status,
  featured,
  categories,
  onSearchChange,
  onCategoryChange,
  onStatusChange,
  onFeaturedChange,
}: ProductFiltersProps) {
  return (
    <div className="mt-8 rounded-xl bg-white p-5 shadow-sm">
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {/* Search */}
        <div className="lg:col-span-1">
          <label
            htmlFor="product-search"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Search
          </label>

          <input
            id="product-search"
            type="text"
            value={search}
            onChange={(event) =>
              onSearchChange(event.target.value)
            }
            placeholder="Search product..."
            className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A]"
          />
        </div>

        {/* Category */}
        <div>
          <label
            htmlFor="product-category"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Category
          </label>

          <select
            id="product-category"
            value={category}
            onChange={(event) =>
              onCategoryChange(event.target.value)
            }
            className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A]"
          >
            <option value="">All Categories</option>

            {categories.map((categoryName) => (
              <option
                key={categoryName}
                value={categoryName}
              >
                {categoryName}
              </option>
            ))}
          </select>
        </div>

        {/* Status */}
        <div>
          <label
            htmlFor="product-status"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Status
          </label>

          <select
            id="product-status"
            value={status}
            onChange={(event) =>
              onStatusChange(event.target.value)
            }
            className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A]"
          >
            <option value="">All Status</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
        </div>

        {/* Featured */}
        <div>
          <label
            htmlFor="product-featured"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Featured
          </label>

          <select
            id="product-featured"
            value={featured}
            onChange={(event) =>
              onFeaturedChange(event.target.value)
            }
            className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm outline-none transition focus:border-[#1B2A4A]"
          >
            <option value="">All Products</option>
            <option value="featured">Featured</option>
            <option value="not-featured">
              Not Featured
            </option>
          </select>
        </div>
      </div>
    </div>
  );
}