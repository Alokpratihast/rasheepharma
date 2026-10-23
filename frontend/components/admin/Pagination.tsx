"use client";

interface PaginationProps {
  page: number;
  pageSize: number;
  totalItems: number;
  itemLabel?: string;
  disabled?: boolean;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

export function Pagination({
  page,
  pageSize,
  totalItems,
  itemLabel = "items",
  disabled = false,
  onPageChange,
  onPageSizeChange,
}: PaginationProps) {
  const pageCount = Math.max(1, Math.ceil(totalItems / pageSize));
  const safePage = Math.min(page, pageCount);
  const firstItem = totalItems === 0 ? 0 : (safePage - 1) * pageSize + 1;
  const lastItem = Math.min(safePage * pageSize, totalItems);
  const pageNumbers = getPageNumbers(safePage, pageCount);

  return (
    <nav
      aria-label={`${itemLabel} pagination`}
      className="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm sm:flex-row sm:items-center sm:justify-between"
    >
      <div className="flex flex-wrap items-center gap-x-4 gap-y-2 text-slate-600">
        <p aria-live="polite">
          Showing <span className="font-semibold text-slate-900">{firstItem}–{lastItem}</span> of{" "}
          <span className="font-semibold text-slate-900">{totalItems}</span> {itemLabel}
        </p>
        <label className="flex items-center gap-2">
          <span>Rows</span>
          <select
            aria-label={`Rows per page for ${itemLabel}`}
            value={pageSize}
            disabled={disabled}
            onChange={(event) => onPageSizeChange(Number(event.target.value))}
            className="rounded-lg border border-slate-300 bg-white px-2 py-1.5 text-slate-800 disabled:opacity-60"
          >
            {[10, 25, 50, 100].map((size) => <option key={size} value={size}>{size}</option>)}
          </select>
          <span>per page</span>
        </label>
      </div>

      <div className="flex items-center gap-1">
        <button
          type="button"
          aria-label="Previous page"
          disabled={disabled || safePage <= 1}
          onClick={() => onPageChange(safePage - 1)}
          className="rounded-lg border border-slate-300 px-3 py-2 font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
        >
          Previous
        </button>
        {pageNumbers.map((item, index) => item === "ellipsis" ? (
          <span key={`ellipsis-${index}`} aria-hidden="true" className="px-1 text-slate-400">…</span>
        ) : (
          <button
            key={item}
            type="button"
            aria-label={`Page ${item}`}
            aria-current={item === safePage ? "page" : undefined}
            disabled={disabled}
            onClick={() => onPageChange(item)}
            className={`min-w-9 rounded-lg border px-2.5 py-2 font-semibold disabled:opacity-60 ${item === safePage ? "border-[#1B2A4A] bg-[#1B2A4A] text-white" : "border-slate-300 text-slate-700 hover:bg-slate-50"}`}
          >
            {item}
          </button>
        ))}
        <button
          type="button"
          aria-label="Next page"
          disabled={disabled || safePage >= pageCount}
          onClick={() => onPageChange(safePage + 1)}
          className="rounded-lg border border-slate-300 px-3 py-2 font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
        >
          Next
        </button>
      </div>
    </nav>
  );
}

function getPageNumbers(page: number, pageCount: number): Array<number | "ellipsis"> {
  if (pageCount <= 5) return Array.from({ length: pageCount }, (_, index) => index + 1);

  const visible = [...new Set([1, page - 1, page, page + 1, pageCount])]
    .filter((number) => number >= 1 && number <= pageCount)
    .sort((left, right) => left - right);

  return visible.flatMap((number, index) => {
    const previous = visible[index - 1];
    return previous && number - previous > 1 ? ["ellipsis", number] : [number];
  });
}
