import { describe, expect, it } from "vitest";

import { calculateQuantityTotal } from "./quantity";

describe("calculateQuantityTotal", () => {
  it("multiplies unit price by the entered quantity", () => {
    expect(calculateQuantityTotal(1.04, 100, 100)).toBe(104);
    expect(calculateQuantityTotal(0.1, 200, 200)).toBe(20);
  });

  it("does not return a total for an invalid or out-of-stock quantity", () => {
    expect(calculateQuantityTotal(0.1, 200, 100)).toBeNull();
    expect(calculateQuantityTotal(0.1, 0, 100)).toBeNull();
    expect(calculateQuantityTotal(0.1, 1.5, 100)).toBeNull();
  });
});
