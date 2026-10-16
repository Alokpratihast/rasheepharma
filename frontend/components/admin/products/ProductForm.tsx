"use client";

import { FormEvent, useEffect, useState } from "react";
import { LoaderCircle } from "lucide-react";
import { toast } from "sonner";
import { categoryService } from "@/services/category.service";
import { productService } from "@/services/product.service";
import { getAdminErrorMessage } from "@/lib/admin-error-message";
import { slugify } from "@/lib/slugify";
import type { Category } from "@/types/category";
import type {
  ProductCreateInput,
  ProductDetails,
  ProductUpdateInput,
} from "@/types/product";


interface ProductFormProps {
  product?: ProductDetails;
  onSuccess?: (product: ProductDetails) => void;
}

interface FormData {
  name: string;
  slug: string;
  genericName: string;
  composition: string;
  dosageForm: string;
  description: string;
  brandName: string;
  manufacturer: string;
  categoryId: string;
  isActive: boolean;
  isFeatured: boolean;
}

export function ProductForm({
  product,
  onSuccess,
}: ProductFormProps) {
  const isEditMode = Boolean(product);
  const [slugIsCustom, setSlugIsCustom] = useState(Boolean(product));

  const [categories, setCategories] = useState<Category[]>(
    [],
  );

  const [loadingCategories, setLoadingCategories] =
    useState(true);

  const [submitting, setSubmitting] = useState(false);

  const [error, setError] = useState<string | null>(
    null,
  );

  const [success, setSuccess] = useState<string | null>(
    null,
  );

  const [formData, setFormData] = useState<FormData>({
    name: product?.name ?? "",
    slug: product?.slug ?? "",
    genericName: product?.genericName ?? "",
    composition: product?.composition ?? "",
    dosageForm: product?.dosageForm ?? "",
    description: product?.description ?? "",
    brandName: product?.brandName ?? "",
    manufacturer: product?.manufacturer ?? "",
    categoryId: product?.categoryId
      ? String(product.categoryId)
      : "",
    isActive: product?.isActive ?? true,
    isFeatured: product?.isFeatured ?? false,
  });

  useEffect(() => {
    const loadCategories = async () => {
      try {
        setLoadingCategories(true);

        const data =
          await categoryService.getAll();

        setCategories(data);
      } catch (error) {
        console.error(
          "Failed to load categories:",
          error,
        );

        setError(getAdminErrorMessage(error, "We could not load categories. Refresh the page before adding a product."));
      } finally {
        setLoadingCategories(false);
      }
    };

    loadCategories();
  }, []);

  const handleChange = (
    field: keyof FormData,
    value: string | boolean,
  ) => {
    setError(null);
    setSuccess(null);
    setFormData((previous) => ({
      ...previous,
      [field]: value,
    }));
  };

  const handleNameChange = (name: string) => {
    setError(null);
    setSuccess(null);
    setFormData((previous) => ({
      ...previous,
      name,
      ...(!slugIsCustom ? { slug: slugify(name) } : {}),
    }));
  };

  const handleSlugChange = (slug: string) => {
    setSlugIsCustom(true);
    handleChange("slug", slug);
  };

  const handleSubmit = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    setError(null);
    setSuccess(null);

    if (!formData.name.trim()) {
      const message = "Enter a product name before saving.";
      setError(message);
      toast.error(message);
      return;
    }

    if (!slugify(formData.slug)) {
      const message = "Enter a URL slug or generate one from the product name.";
      setError(message);
      toast.error(message);
      return;
    }

    if (!formData.categoryId) {
      const message = "Choose a category for this product before saving.";
      setError(message);
      toast.error(message);
      return;
    }

    setSubmitting(true);

    try {
      const baseData = {
        name: formData.name.trim(),
        slug: slugify(formData.slug),
        genericName:
          formData.genericName.trim() || null,
        composition:
          formData.composition.trim() || null,
        dosageForm:
          formData.dosageForm.trim() || null,
        description:
          formData.description.trim() || null,
        brandName:
          formData.brandName.trim() || null,
        manufacturer:
          formData.manufacturer.trim() || null,
        categoryId: Number(formData.categoryId),
        isActive: formData.isActive,
        isFeatured: formData.isFeatured,
      };

      let savedProduct: ProductDetails;

      if (isEditMode && product) {
        const updateData: ProductUpdateInput =
          baseData;

        savedProduct =
          await productService.update(
            product.id,
            updateData,
          );

        setSuccess(
          "Product updated successfully.",
        );
        toast.success("Product changes saved.");
      } else {
        const createData: ProductCreateInput =
          baseData;

        savedProduct =
          await productService.create(
            createData,
          );

        setSuccess(
          "Product added. You can add another product here.",
        );
        toast.success("Product added successfully. Add the next product when you’re ready.");

        setFormData({
          name: "",
          slug: "",
          genericName: "",
          composition: "",
          dosageForm: "",
          description: "",
          brandName: "",
          manufacturer: "",
          categoryId: "",
          isActive: true,
          isFeatured: false,
        });
        setSlugIsCustom(false);
      }

      onSuccess?.(savedProduct);
    } catch (error) {
      console.error(
        "Failed to save product:",
        error,
      );

      const message = getAdminErrorMessage(
        error,
        isEditMode
          ? "Could not update this product. Check the fields and try again."
          : "Could not add this product. Check the fields and try again.",
      );
      setError(message);
      toast.error(message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-6"
    >
      {/* Basic Information */}
      <section className="rounded-xl bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-[#1B2A4A]">
          Basic Information
        </h2>

        <div className="mt-6 grid gap-5 md:grid-cols-2">
          {/* Name */}
          <div>
            <label
              htmlFor="name"
              className="mb-2 block text-sm font-medium text-[#1B2A4A]"
            >
              Product Name *
            </label>

            <input
              id="name"
              type="text"
              required
              value={formData.name}
              onChange={(event) => handleNameChange(event.target.value)}
              autoComplete="off"
              placeholder="Enter product name"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
            />
          </div>

          {/* Slug */}
          <div>
            <label
              htmlFor="slug"
              className="mb-2 block text-sm font-medium text-[#1B2A4A]"
            >
              URL slug *
            </label>

            <input
              id="slug"
              type="text"
              required
              value={formData.slug}
              onChange={(event) => handleSlugChange(event.target.value)}
              onBlur={() => handleChange("slug", slugify(formData.slug))}
              autoComplete="off"
              placeholder="product-slug"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
            />
            <div className="mt-2 flex flex-wrap items-center justify-between gap-2">
              <p className="text-xs text-gray-500">
                {slugIsCustom
                  ? "Custom URL. You can regenerate it from the product name."
                  : `Auto-generated: /products/${formData.slug || "product-name"}`}
              </p>
              {slugIsCustom && (
                <button
                  type="button"
                  onClick={() => {
                    setSlugIsCustom(false);
                    handleChange("slug", slugify(formData.name));
                  }}
                  className="text-xs font-semibold text-[#007f68] hover:underline"
                >
                  Generate from name
                </button>
              )}
            </div>
          </div>

          {/* Generic Name */}
          <div>
            <label
              htmlFor="genericName"
              className="mb-2 block text-sm font-medium text-[#1B2A4A]"
            >
              Generic Name
            </label>

            <input
              id="genericName"
              type="text"
              value={formData.genericName}
              onChange={(event) =>
                handleChange(
                  "genericName",
                  event.target.value,
                )
              }
              placeholder="Enter generic name"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
            />
          </div>

          {/* Brand Name */}
          <div>
            <label
              htmlFor="brandName"
              className="mb-2 block text-sm font-medium text-[#1B2A4A]"
            >
              Brand Name
            </label>

            <input
              id="brandName"
              type="text"
              value={formData.brandName}
              onChange={(event) =>
                handleChange(
                  "brandName",
                  event.target.value,
                )
              }
              placeholder="Enter brand name"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
            />
          </div>

          {/* Composition */}
          <div>
            <label
              htmlFor="composition"
              className="mb-2 block text-sm font-medium text-[#1B2A4A]"
            >
              Composition
            </label>

            <input
              id="composition"
              type="text"
              value={formData.composition}
              onChange={(event) =>
                handleChange(
                  "composition",
                  event.target.value,
                )
              }
              placeholder="e.g. Paracetamol 500mg"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
            />
          </div>

          {/* Dosage Form */}
          <div>
            <label
              htmlFor="dosageForm"
              className="mb-2 block text-sm font-medium text-[#1B2A4A]"
            >
              Dosage Form
            </label>

            <input
              id="dosageForm"
              type="text"
              value={formData.dosageForm}
              onChange={(event) =>
                handleChange(
                  "dosageForm",
                  event.target.value,
                )
              }
              placeholder="e.g. Tablet"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
            />
          </div>

          {/* Manufacturer */}
          <div>
            <label
              htmlFor="manufacturer"
              className="mb-2 block text-sm font-medium text-[#1B2A4A]"
            >
              Manufacturer
            </label>

            <input
              id="manufacturer"
              type="text"
              value={formData.manufacturer}
              onChange={(event) =>
                handleChange(
                  "manufacturer",
                  event.target.value,
                )
              }
              placeholder="Enter manufacturer"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
            />
          </div>

          {/* Category */}
          <div>
            <label
              htmlFor="categoryId"
              className="mb-2 block text-sm font-medium text-[#1B2A4A]"
            >
              Category *
            </label>

            <select
              id="categoryId"
              value={formData.categoryId}
              onChange={(event) =>
                handleChange(
                  "categoryId",
                  event.target.value,
                )
              }
              disabled={loadingCategories}
              className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A] disabled:bg-gray-100"
            >
              <option value="">
                {loadingCategories
                  ? "Loading categories..."
                  : "Select category"}
              </option>

              {categories
                .filter(
                  (category) =>
                    category.isActive,
                )
                .map((category) => (
                  <option
                    key={category.id}
                    value={category.id}
                  >
                    {category.parentCategoryName
                      ? `${category.parentCategoryName} / ${category.name}`
                      : category.name}
                  </option>
                ))}
            </select>
          </div>
        </div>

        {/* Description */}
        <div className="mt-5">
          <label
            htmlFor="description"
            className="mb-2 block text-sm font-medium text-[#1B2A4A]"
          >
            Description
          </label>

          <textarea
            id="description"
            value={formData.description}
            onChange={(event) =>
              handleChange(
                "description",
                event.target.value,
              )
            }
            placeholder="Enter product description"
            rows={5}
            className="w-full resize-none rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
          />
        </div>
      </section>

      {/* Product Settings */}
      <section className="rounded-xl bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-[#1B2A4A]">
          Product Settings
        </h2>

        <div className="mt-5 space-y-4">
          <label className="flex cursor-pointer items-center gap-3">
            <input
              type="checkbox"
              checked={formData.isActive}
              onChange={(event) =>
                handleChange(
                  "isActive",
                  event.target.checked,
                )
              }
              className="h-4 w-4 rounded border-gray-300"
            />

            <span className="text-sm text-gray-700">
              Product is active
            </span>
          </label>

          <label className="flex cursor-pointer items-center gap-3">
            <input
              type="checkbox"
              checked={formData.isFeatured}
              onChange={(event) =>
                handleChange(
                  "isFeatured",
                  event.target.checked,
                )
              }
              className="h-4 w-4 rounded border-gray-300"
            />

            <span className="text-sm text-gray-700">
              Show as featured product
            </span>
          </label>
        </div>
      </section>

      {/* Messages */}
      {error && (
        <div role="alert" className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {success && (
        <div role="status" className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
          {success}
        </div>
      )}

      {/* Submit */}
      <div className="flex justify-end">
        <button
          type="submit"
          disabled={submitting || loadingCategories}
          className="rounded-lg bg-[#1B2A4A] px-6 py-2.5 text-sm font-medium text-white transition hover:bg-[#142039] disabled:cursor-not-allowed disabled:opacity-60"
        >
          {submitting && <LoaderCircle aria-hidden="true" className="mr-2 inline size-4 animate-spin" />}
          {submitting
            ? isEditMode
              ? "Updating..."
              : "Creating..."
            : isEditMode
              ? "Update Product"
              : "Create Product"}
        </button>
      </div>
    </form>
  );
}
