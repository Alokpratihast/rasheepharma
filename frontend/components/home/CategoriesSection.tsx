import { CategoriesExplorer } from "@/components/home/CategoriesExplorer";
import { categoryService } from "@/services/category.service";
import { productService } from "@/services/product.service";
import type { Category } from "@/types/category";
import type { ProductList } from "@/types/product";

export async function CategoriesSection() {
  let categories: Category[] = [];
  let featuredProducts: ProductList[] = [];

  const [categoryResult, productResult] = await Promise.allSettled([
    categoryService.getAll(),
    productService.getFeatured(),
  ]);
  if (categoryResult.status === "fulfilled") {
    categories = categoryResult.value.filter((category) => category.isActive);
  }
  if (productResult.status === "fulfilled") {
    featuredProducts = productResult.value;
  }

  return <CategoriesExplorer categories={categories} featuredProducts={featuredProducts} />;
}
