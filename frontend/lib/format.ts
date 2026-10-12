/** Formats a price in US dollars, e.g. 2.7 -> "$2.70", 1250 -> "$1,250.00". */
export function formatPrice(value: number): string {
  return `$${value.toLocaleString("en-US", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
}