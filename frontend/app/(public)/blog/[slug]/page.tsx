import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import type { PublicBlogPost } from "@/types/blog";
import { MarkdownArticle } from "@/components/blog/MarkdownArticle";

const apiBase = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5104/api").replace(/\/+$/, "");

async function getPost(slug: string): Promise<PublicBlogPost | null> {
  try {
    const response = await fetch(`${apiBase}/blog/posts/${encodeURIComponent(slug)}`, { cache: "no-store" });
    if (!response.ok) return null;
    return await response.json() as PublicBlogPost;
  } catch { return null; }
}

type PageProps = { params: Promise<{ slug: string }> };

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const post = await getPost(slug);
  if (!post) return { title: "Article not found | RashePharma", robots: { index: false, follow: false } };
  const title = post.metaTitle || post.title;
  const description = post.metaDescription || post.excerpt;
  const image = post.ogImageUrl || (post.coverImageUrl ? `${apiBase}/blog/posts/${encodeURIComponent(post.slug)}/image` : undefined);
  return {
    title,
    description,
    alternates: { canonical: post.canonicalUrl || `/blog/${encodeURIComponent(post.slug)}` },
    robots: { index: post.isIndexable, follow: post.isIndexable },
    openGraph: { type: "article", title: post.ogTitle || title, description: post.ogDescription || description, images: image ? [image] : [], publishedTime: post.publishedAtUtc || undefined, authors: [post.authorName] },
  };
}

export default async function BlogArticlePage({ params }: PageProps) {
  const { slug } = await params;
  const post = await getPost(slug);
  if (!post) notFound();

  const structuredData = {
    "@context": "https://schema.org", "@type": "BlogPosting", headline: post.title,
    description: post.excerpt, datePublished: post.publishedAtUtc, dateModified: post.updatedAtUtc,
    author: { "@type": "Person", name: post.authorName },
    ...(post.coverImageUrl ? { image: [post.coverImageUrl] } : {}),
  };

  return <main className="min-h-[60vh] bg-white px-5 py-12">
    <article className="mx-auto max-w-3xl">
      <Link href="/blog" className="text-sm font-semibold text-emerald-700 hover:underline">← All articles</Link>
      <header className="mb-8 mt-6"><p className="text-sm text-gray-500">{post.publishedAtUtc ? new Date(post.publishedAtUtc).toLocaleDateString("en-IN", { dateStyle: "long", timeZone: "UTC" }) : ""} · {post.authorName}</p>
        <h1 className="mt-3 text-3xl font-bold leading-tight text-[#1B2A4A] md:text-5xl">{post.title}</h1><p className="mt-4 text-lg leading-8 text-gray-600">{post.excerpt}</p>
        {post.reviewerName && <p className="mt-3 text-sm text-gray-500">Reviewed by {post.reviewerName}{post.reviewedAtUtc ? ` · ${new Date(post.reviewedAtUtc).toLocaleDateString("en-IN", { dateStyle: "long", timeZone: "UTC" })}` : ""}</p>}
      </header>
      {post.coverImageUrl && <Image src={`${apiBase}/blog/posts/${encodeURIComponent(post.slug)}/image`} alt="" width={1200} height={700} unoptimized className="mb-8 max-h-[480px] w-full rounded-2xl object-cover" />}
      <MarkdownArticle content={post.content} />
      <p className="mt-10 border-t pt-5 text-sm text-gray-500">This article is for general information and does not replace advice from a qualified healthcare professional.</p>
    </article>
    <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(structuredData).replace(/</g, "\\u003c") }} />
  </main>;
}
