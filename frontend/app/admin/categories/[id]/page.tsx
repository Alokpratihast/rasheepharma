"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { categoryService } from "@/services/category.service";
import type { CategoryDetails } from "@/types/category";
import { CategoryForm } from "@/components/admin/categories/CategoryForm";

export default function EditCategoryPage() {
  const params = useParams();
  const router = useRouter();

  const categoryId = Number(params.id);

  const [category, setCategory] =
    useState<CategoryDetails | null>(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadCategory = async () => {
      if (!categoryId || Number.isNaN(categoryId)) {
        setError("Invalid category ID.");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const data =
          await categoryService.getById(categoryId);

        setCategory(data);
      } catch (error) {
        console.error(
          "Failed to load category:",
          error,
        );

        setError("Failed to load category.");
      } finally {
        setLoading(false);
      }
    };

    loadCategory();
  }, [categoryId]);

  if (loading) {
    return (
      <main className="min-h-screen bg-[#f5f7f6] p-6">
        <div className="mx-auto max-w-5xl">
          <div className="rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-gray-500">
              Loading category...
            </p>
          </div>
        </div>
      </main>
    );
  }

  if (error || !category) {
    return (
      <main className="min-h-screen bg-[#f5f7f6] p-6">
        <div className="mx-auto max-w-5xl">
          <div className="rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-red-600">
              {error ?? "Category not found."}
            </p>

            <button
              type="button"
              onClick={() =>
                router.push("/admin/categories")
              }
              className="mt-4 rounded-lg bg-[#1B2A4A] px-4 py-2 text-sm font-medium text-white hover:bg-[#142039]"
            >
              Back to Categories
            </button>
          </div>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-[#f5f7f6] p-6">
      <div className="mx-auto max-w-5xl">
        <div className="mb-8">
          <h1 className="text-3xl font-semibold text-[#1B2A4A]">
            Edit Category
          </h1>

          <p className="mt-2 text-sm text-[#595959]">
            Update category information and hierarchy.
          </p>
        </div>

        <CategoryForm
          category={category}
          onSuccess={(updatedCategory) => {
            setCategory(updatedCategory);
          }}
          onCancel={() =>
            router.push("/admin/categories")
          }
        />
      </div>
    </main>
  );
}