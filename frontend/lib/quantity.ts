/** Returns a line estimate only for a valid, in-stock whole-number quantity. */
export function calculateQuantityTotal(
  unitPrice: number,
  quantity: number,
  stockQuantity: number,
): number | null {
  if (
    !Number.isFinite(unitPrice) ||
    unitPrice <= 0 ||
    !Number.isSafeInteger(quantity) ||
    quantity < 1 ||
    !Number.isSafeInteger(stockQuantity) ||
    stockQuantity < 1 ||
    quantity > stockQuantity
  ) {
    return null;
  }

  return unitPrice * quantity;
}
