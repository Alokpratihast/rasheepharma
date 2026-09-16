export interface Category {
  id: number;
  name: string;
  slug: string;
  isActive: boolean;
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