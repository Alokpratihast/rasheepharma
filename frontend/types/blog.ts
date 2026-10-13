export type BlogPostStatus = "Draft" | "Published" | "Archived";

export interface BlogPost {
  id: number;
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  coverImageUrl: string | null;
  authorName: string;
  reviewerName: string | null;
  reviewedAtUtc: string | null;
  status: BlogPostStatus;
  publishedAtUtc: string | null;
  metaTitle: string | null;
  metaDescription: string | null;
  canonicalUrl: string | null;
  ogTitle: string | null;
  ogDescription: string | null;
  ogImageUrl: string | null;
  isIndexable: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export type BlogPostWriteInput = Omit<BlogPost, "id" | "status" | "publishedAtUtc" | "createdAtUtc" | "updatedAtUtc">;

export type BlogPostListItem = Pick<BlogPost, "id" | "title" | "slug" | "status" | "publishedAtUtc" | "updatedAtUtc">;

export interface BlogPostPage {
  items: BlogPostListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export type PublicBlogPost = Pick<BlogPost,
  "title" | "slug" | "excerpt" | "content" | "coverImageUrl" | "authorName" |
  "reviewerName" | "reviewedAtUtc" | "publishedAtUtc" | "updatedAtUtc" |
  "metaTitle" | "metaDescription" | "canonicalUrl" | "ogTitle" |
  "ogDescription" | "ogImageUrl" | "isIndexable">;

export type PublicBlogPostListItem = Pick<BlogPost,
  "title" | "slug" | "excerpt" | "coverImageUrl" | "authorName" |
  "publishedAtUtc" | "updatedAtUtc" | "isIndexable">;

export interface PublicBlogPostPage {
  items: PublicBlogPostListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
}
