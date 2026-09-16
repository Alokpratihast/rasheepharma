import Image from "next/image";
import Link from "next/link";
import { ArrowUpRight, Pill } from "lucide-react";

import { getApiAssetUrl } from "@/lib/api/client";

interface ProductCardProps {
  name: string;
  form: string;
  composition: string;
  packSize: string;
  slug: string;
  imageUrl?: string | null;
}

export function ProductCard({
  name,
  form,
  composition,
  packSize,
  slug,
  imageUrl,
}: ProductCardProps) {
  const productImageUrl = getApiAssetUrl(imageUrl);

  return (
    <Link
      href={`/products/${slug}`}
      className="group block h-full"
    >
      <article className="h-full overflow-hidden rounded-lg border border-[#e5e5e5] bg-white transition-all duration-200 hover:-translate-y-0.5 hover:border-[#3E8F96]/40 hover:shadow-md">
        {/* Product image */}
        <div className="relative flex h-48 items-center justify-center bg-[#F2F2F2]">
          {productImageUrl ? (
            <Image
              src={productImageUrl}
              alt={name}
              fill
              className="object-contain p-6 transition-transform duration-200 group-hover:scale-105"
            />
          ) : (
            <div className="flex size-20 items-center justify-center rounded-xl bg-white shadow-sm transition-transform duration-200 group-hover:scale-105">
              <Pill
                aria-hidden="true"
                className="size-9 text-[#3E8F96]"
              />
            </div>
          )}

          <span className="absolute right-3 top-3 rounded-full bg-white px-2.5 py-1 text-[10px] font-medium uppercase tracking-wide text-[#3E8F96] shadow-sm">
            {form}
          </span>
        </div>

        {/* Product details */}
        <div className="p-4">
          <div className="flex items-start justify-between gap-3">
            <h2 className="line-clamp-2 text-sm font-semibold leading-5 text-[#1B2A4A]">
              {name}
            </h2>

            <ArrowUpRight className="mt-0.5 size-4 shrink-0 text-[#999] transition-colors group-hover:text-[#F5821F]" />
          </div>

          <div className="mt-2 h-10 overflow-hidden text-xs leading-5 text-[#595959]">
            {composition}
          </div>

          <div className="mt-4 border-t border-dashed border-[#e2e2e2] pt-3">
            <p className="text-[11px] font-medium uppercase tracking-[0.08em] text-[#888]">
              Pack Size
            </p>

            <p className="mt-1 text-xs text-[#595959]">
              {packSize}
            </p>
          </div>
        </div>
      </article>
    </Link>
  );
}