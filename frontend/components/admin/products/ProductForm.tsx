"use client";

import { FormEvent, useEffect, useState } from "react";
import { categoryService } from "@/services/category.service";
import { productService } from "@/services/product.service";
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

        setError("Failed to load categories.");
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
    setFormData((previous) => ({
      ...previous,
      [field]: value,
    }));
  };

  const handleSubmit = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    setError(null);
    setSuccess(null);

    if (!formData.name.trim()) {
      setError("Product name is required.");
      return;
    }

    if (!formData.slug.trim()) {
      setError("Product slug is required.");
      return;
    }

    if (!formData.categoryId) {
      setError("Please select a category.");
      return;
    }

    setSubmitting(true);

    try {
      const baseData = {
        name: formData.name.trim(),
        slug: formData.slug.trim(),
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
      } else {
        const createData: ProductCreateInput =
          baseData;

        savedProduct =
          await productService.create(
            createData,
          );

        setSuccess(
          "Product created successfully.",
        );

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
      }

      onSuccess?.(savedProduct);
    } catch (error) {
      console.error(
        "Failed to save product:",
        error,
      );

      setError(
        isEditMode
          ? "Failed to update product."
          : "Failed to create product.",
      );
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
              value={formData.name}
              onChange={(event) =>
                handleChange(
                  "name",
                  event.target.value,
                )
              }
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
              Slug *
            </label>

            <input
              id="slug"
              type="text"
              value={formData.slug}
              onChange={(event) =>
                handleChange(
                  "slug",
                  event.target.value,
                )
              }
              placeholder="product-slug"
              className="w-full rounded-lg border border-gray-300 px-3 py-2.5 text-sm outline-none focus:border-[#1B2A4A]"
            />
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
        <div className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-600">
          {error}
        </div>
      )}

      {success && (
        <div className="rounded-lg bg-green-50 px-4 py-3 text-sm text-green-600">
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