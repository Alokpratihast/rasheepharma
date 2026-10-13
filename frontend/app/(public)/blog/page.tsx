import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import type { PublicBlogPostPage } from "@/types/blog";

export const metadata: Metadata = {
  title: "Healthcare Articles | RashePharma",
  description: "Healthcare and pharmaceutical insights from RashePharma.",
  alternates: { canonical: "/blog" },
};

const apiBase = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5104/api").replace(/\/+$/, "");

async function getPosts(page: number): Promise<PublicBlogPostPage | null> {
  try {
    const response = await fetch(`${apiBase}/blog/posts?page=${page}&pageSize=12`, { cache: "no-store" });
    if (!response.ok) return null;
    return await response.json() as PublicBlogPostPage;
  } catch { return null; }
}

export default async function BlogIndexPage({ searchParams }: { searchParams: Promise<{ page?: string }> }) {
  const query = await searchParams;
  const requestedPage = Number(query.page ?? "1");
  const page = Number.isSafeInteger(requestedPage) && requestedPage > 0 ? requestedPage : 1;
  const result = await getPosts(page);
  return <main className="min-h-[60vh] bg-[#f7faf9] px-5 py-14">
    <div className="mx-auto max-w-6xl">
      <header className="mb-10 max-w-2xl"><p className="text-sm font-semibold uppercase tracking-[0.18em] text-emerald-700">RashePharma insights</p><h1 className="mt-3 text-4xl font-bold text-[#1B2A4A]">Healthcare Articles</h1><p className="mt-3 text-gray-600">Updates and educational articles from our team.</p></header>
      {!result && <div className="rounded-xl border border-amber-200 bg-amber-50 p-5 text-amber-900">Articles are temporarily unavailable. Please try again shortly.</div>}
      {result?.items.length === 0 && <p className="rounded-xl border bg-white p-8 text-gray-600">No articles have been published yet.</p>}
      {!!result?.items.length && <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">{result.items.map((post) => <article key={post.slug} className="overflow-hidden rounded-2xl border bg-white shadow-sm">
        {post.coverImageUrl && <div className="relative h-52 w-full"><Image src={`${apiBase}/blog/posts/${encodeURIComponent(post.slug)}/image`} alt="" fill unoptimized sizes="(max-width: 768px) 100vw, (max-width: 1280px) 50vw, 33vw" className="object-cover" /></div>}
        <div className="p-5"><p className="text-xs text-gray-500">{post.publishedAtUtc ? new Date(post.publishedAtUtc).toLocaleDateString("en-IN", { dateStyle: "long", timeZone: "UTC" }) : ""} · {post.authorName}</p>
          <h2 className="mt-2 text-xl font-bold text-[#1B2A4A]">{post.title}</h2><p className="mt-2 line-clamp-3 text-sm leading-6 text-gray-600">{post.excerpt}</p>
          <Link href={`/blog/${encodeURIComponent(post.slug)}`} className="mt-5 inline-flex font-semibold text-emerald-700 hover:underline">Read article <span aria-hidden="true" className="ml-1">→</span></Link>
        </div></article>)}</div>}
      {!!result && result.totalCount > result.pageSize && <nav aria-label="Blog pages" className="mt-10 flex items-center justify-center gap-4">
        {page > 1 ? <Link href={`/blog?page=${page - 1}`} className="rounded-lg border bg-white px-4 py-2 font-medium text-[#1B2A4A]">Previous</Link> : <span className="rounded-lg border px-4 py-2 text-gray-400">Previous</span>}
        <span className="text-sm text-gray-600">Page {result.page} of {Math.ceil(result.totalCount / result.pageSize)}</span>
        {page * result.pageSize < result.totalCount ? <Link href={`/blog?page=${page + 1}`} className="rounded-lg border bg-white px-4 py-2 font-medium text-[#1B2A4A]">Next</Link> : <span className="rounded-lg border px-4 py-2 text-gray-400">Next</span>}
      </nav>}
    </div>
  </main>;
}
