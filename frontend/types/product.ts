export interface ProductList {
  id: number;
  name: string;
  slug: string;
  genericName: string | null;
  dosageForm: string | null;
  manufacturer: string | null;
  categoryName: string;
  startingPrice: number | null;
  primaryImageUrl: string | null;
  isActive: boolean;
}

export interface ProductDetails {
  id: number;
  name: string;
  slug: string;
  genericName: string | null;
  composition: string | null;
  dosageForm: string | null;
  description: string | null;
  manufacturer: string | null;
  categoryId: number;
  categoryName: string;
  isActive: boolean;
  variants: ProductVariant[];
  images: ProductImage[];
}

export interface ProductVariant {
  id: number;
  strength: string | null;
  packSize: string | null;
  price: number;
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