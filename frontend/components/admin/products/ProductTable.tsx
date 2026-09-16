import Link from "next/link";
import type { ProductList as Product } from "@/types/product";
import { ProductStatusBadge } from "@/components/admin/products/ProductStatusBadge";

interface ProductTableProps {
  products: Product[];
}

export function ProductTable({
  products,
}: ProductTableProps) {
  return (
    <div className="overflow-hidden rounded-xl bg-white shadow-sm">
      <div className="overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-gray-200 bg-gray-50">
            <tr>
              <th className="px-6 py-4 font-semibold text-[#1B2A4A]">
                Product
              </th>

              <th className="px-6 py-4 font-semibold text-[#1B2A4A]">
                Category
              </th>

              <th className="px-6 py-4 font-semibold text-[#1B2A4A]">
                Composition
              </th>

              <th className="px-6 py-4 font-semibold text-[#1B2A4A]">
                Starting Price
              </th>

              <th className="px-6 py-4 font-semibold text-[#1B2A4A]">
                Status
              </th>

              <th className="px-6 py-4 font-semibold text-[#1B2A4A]">
                Featured
              </th>

              <th className="px-6 py-4 font-semibold text-[#1B2A4A]">
                Action
              </th>
            </tr>
          </thead>

          <tbody>
            {products.map((product) => (
              <tr
                key={product.id}
                className="border-b border-gray-100 last:border-b-0"
              >
                <td className="px-6 py-4">
                  <div>
                    <p className="font-medium text-[#1B2A4A]">
                      {product.name}
                    </p>

                    <p className="mt-1 text-xs text-gray-500">
                      {product.slug}
                    </p>
                  </div>
                </td>

                <td className="px-6 py-4 text-gray-600">
                  {product.categoryName}
                </td>

                <td className="max-w-xs px-6 py-4 text-gray-600">
                  {product.composition || "—"}
                </td>

                <td className="px-6 py-4 text-gray-600">
                  {product.startingPrice !== null
                    ? `₹${product.startingPrice}`
                    : "—"}
                </td>

                <td className="px-6 py-4">
                  <ProductStatusBadge
                    active={product.isActive}
                  />
                </td>

                <td className="px-6 py-4">
                  <ProductStatusBadge
                    active={product.isFeatured}
                    label={
                      product.isFeatured
                        ? "Featured"
                        : "No"
                    }
                  />
                </td>

                <td className="px-6 py-4">
                  <Link
                    href={`/admin/products/${product.id}`}
                    className="font-medium text-[#1B2A4A] hover:underline"
                  >
                    Edit
                  </Link>
                </td>
              </tr>
            ))}

            {products.length === 0 && (
              <tr>
                <td
                  colSpan={7}
                  className="px-6 py-10 text-center text-sm text-gray-500"
                >
                  No products found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}