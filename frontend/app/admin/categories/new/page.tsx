import { CategoryForm } from "@/components/admin/categories/CategoryForm";

export default function NewCategoryPage() {
  return (
    <main className="min-h-screen bg-[#f5f7f6] p-6">
      <div className="mx-auto max-w-5xl">
        <div className="mb-8">
          <h1 className="text-3xl font-semibold text-[#1B2A4A]">
            Add Category
          </h1>

          <p className="mt-2 text-sm text-[#595959]">
            Create a new product category or subcategory.
          </p>
        </div>

        <CategoryForm />
      </div>
    </main>
  );
}