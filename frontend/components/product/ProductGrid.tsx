import { ProductCard } from "@/components/product/ProductCard";

export interface ProductGridItem {
  id: number;
  name: string;
  form: string;
  composition: string;
  packSize: string;
  slug: string;
}

interface ProductGridProps {
  products: ProductGridItem[];
}

export function ProductGrid({
  products,
}: ProductGridProps) {
  if (products.length === 0) {
    return (
      <div className="rounded-xl border border-dashed border-[#d9d9d9] bg-white py-16 text-center">
        <p className="text-sm font-medium text-[#1B2A4A]">
          No products found
        </p>

        <p className="mt-1 text-xs text-[#777]">
          Try changing your search or filter.
        </p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
      {products.map((product) => (
        <ProductCard
          key={product.id}
          name={product.name}
          form={product.form}
          composition={product.composition}
          packSize={product.packSize}
          slug={product.slug}
        />
      ))}
    </div>
  );
}