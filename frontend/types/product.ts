export interface ProductFacet {
  name: string;
  count: number;
}

export interface ProductPage {
  items: ProductList[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  categoryFacets: ProductFacet[];
  formFacets: ProductFacet[];
  categoryFacetTotalCount: number;
  formFacetTotalCount: number;
}

export interface ProductPageQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  category?: string;
  form?: string;
  sortBy?: "featured" | "name" | "price";
  sortOrder?: "asc" | "desc";
}

export interface ProductList {
  id: number;
  name: string;
  slug: string;
  genericName: string | null;
  composition: string | null;
  dosageForm: string | null;
  packSize: string | null;
  moq: number | null;
  manufacturer: string | null;
  brandName: string | null;
  categoryName: string;
  startingPrice: number | null;
  primaryImageUrl: string | null;
  isActive: boolean;
  isFeatured: boolean;
}

export interface ProductDetails {
  id: number;
  name: string;
  slug: string;
  genericName: string | null;
  composition: string | null;
  dosageForm: string | null;
  description: string | null;
  brandName: string | null;
  manufacturer: string | null;
  categoryId: number;
  categoryName: string;
  isActive: boolean;
  isFeatured: boolean;
  variants: ProductVariant[];
  images: ProductImage[];
}

export interface ProductVariant {
  id: number;
  strength: string | null;
  packSize: string | null;
  price: number;
  currency: string | null;
  moq: number | null;
  unitType: string | null;
  sku: string | null;
  stockQuantity: number;
  isActive: boolean;
}

export interface ProductImage {
  id: number;
  imageUrl: string;
  altText: string | null;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ProductCreateInput {
  name: string;
  slug: string;
  genericName: string | null;
  composition: string | null;
  dosageForm: string | null;
  description: string | null;
  brandName: string | null;
  manufacturer: string | null;
  categoryId: number;
  isActive: boolean;
  isFeatured: boolean;
}

export interface ProductUpdateInput {
  name: string;
  slug: string;
  genericName: string | null;
  composition: string | null;
  dosageForm: string | null;
  description: string | null;
  brandName: string | null;
  manufacturer: string | null;
  categoryId: number;
  isActive: boolean;
  isFeatured: boolean;
}

export interface ProductVariantCreateInput {
  productId: number;
  strength: string | null;
  packSize: string | null;
  price: number;
  moq: number | null;
  unitType: string | null;
  sku: string | null;
  stockQuantity: number;
  isActive: boolean;
}

export interface ProductVariantUpdateInput {
  strength: string | null;
  packSize: string | null;
  price: number;
  moq: number | null;
  unitType: string | null;
  sku: string | null;
  stockQuantity: number;
  isActive: boolean;
}