export interface Category {
  id: number;
  name: string;
  slug: string;
  isActive: boolean;
  description: string | null;
  parentCategoryId: number | null;
  parentCategoryName: string | null;
}

export interface CategoryDetails {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  isActive: boolean;
  parentCategoryId: number | null;
  parentCategoryName: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CategoryCreateInput {
  name: string;
  slug: string;
  description: string | null;
  isActive: boolean;
  parentCategoryId: number | null;
}

export interface CategoryUpdateInput {
  name: string;
  slug: string;
  description: string | null;
  isActive: boolean;
  parentCategoryId: number | null;
}

export interface CategoryNavigationProduct {
  id: number;
  name: string;
  slug: string;
}

export interface CategoryNavigation {
  id: number;
  name: string;
  slug: string;
  children: CategoryNavigation[];
  products: CategoryNavigationProduct[];
}