import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { ArrowLeft, ArrowRight, BookOpen, Search, SlidersHorizontal } from "lucide-react";
import type { PublicBlogPostPage } from "@/types/blog";

export const metadata: Metadata = {
  title: "Healthcare Articles | RashePharma",
  description: "Healthcare and pharmaceutical insights from RashePharma.",
  alternates: { canonical: "/blog" },
};

const apiBase = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5104/api").replace(/\/+$/, "");
const pageSize = 12;
const sortOptions = ["newest", "oldest", "title"] as const;
type SortOption = (typeof sortOptions)[number];

function safePage(value?: string) {
  const parsed = Number(value ?? "1");
  return Number.isSafeInteger(parsed) && parsed > 0 ? Math.min(parsed, 100_000) : 1;
}

async function getPosts(page: number, search: string, sort: SortOption): Promise<PublicBlogPostPage | null> {
  try {
    const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize), sort });
    if (search) query.set("q", search);
    const response = await fetch(apiBase + "/blog/posts?" + query, { cache: "no-store" });
    if (!response.ok) return null;
    return await response.json() as PublicBlogPostPage;
  } catch {
    return null;
  }
}

function pageHref(page: number, search: string, sort: SortOption) {
  const query = new URLSearchParams({ page: String(page), sort });
  if (search) query.set("q", search);
  return "/blog?" + query;
}

function BlogPagination({ page, totalPages, search, sort }: { page: number; totalPages: number; search: string; sort: SortOption }) {
  const start = Math.max(1, Math.min(page - 2, totalPages - 4));
  const end = Math.min(totalPages, start + 4);

  return (
    <nav aria-label="Blog pagination" className="mt-12 flex flex-wrap items-center justify-center gap-2">
      {page > 1 ? (
        <Link href={pageHref(page - 1, search, sort)} className="inline-flex h-11 items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 text-sm font-semibold text-[#1B2A4A] transition hover:border-emerald-600 hover:text-emerald-700">
          <ArrowLeft aria-hidden="true" className="size-4" /> Previous
        </Link>
      ) : (
        <span aria-disabled="true" className="inline-flex h-11 items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-4 text-sm font-semibold text-slate-400">
          <ArrowLeft aria-hidden="true" className="size-4" /> Previous
        </span>
      )}

      <div className="flex items-center gap-1" aria-label={"Page " + page + " of " + totalPages}>
        {Array.from({ length: end - start + 1 }, (_, index) => start + index).map((number) => (
          <Link key={number} href={pageHref(number, search, sort)} aria-current={number === page ? "page" : undefined}
            className={number === page
              ? "grid size-11 place-items-center rounded-xl bg-[#1B2A4A] text-sm font-semibold text-white shadow-sm"
              : "grid size-11 place-items-center rounded-xl text-sm font-semibold text-slate-600 transition hover:bg-emerald-50 hover:text-emerald-800"}>
            {number}
          </Link>
        ))}
      </div>

      {page < totalPages ? (
        <Link href={pageHref(page + 1, search, sort)} className="inline-flex h-11 items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 text-sm font-semibold text-[#1B2A4A] transition hover:border-emerald-600 hover:text-emerald-700">
          Next <ArrowRight aria-hidden="true" className="size-4" />
        </Link>
      ) : (
        <span aria-disabled="true" className="inline-flex h-11 items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-4 text-sm font-semibold text-slate-400">
          Next <ArrowRight aria-hidden="true" className="size-4" />
        </span>
      )}
    </nav>
  );
}

export default async function BlogIndexPage({
  searchParams,
}: {
  searchParams: Promise<{ page?: string; q?: string; sort?: string }>;
}) {
  const query = await searchParams;
  const page = safePage(query.page);
  const search = (query.q ?? "").trim().slice(0, 100);
  const sort: SortOption = sortOptions.includes(query.sort as SortOption) ? query.sort as SortOption : "newest";
  const result = await getPosts(page, search, sort);
  const totalPages = result ? Math.ceil(result.totalCount / result.pageSize) : 0;
  const firstResult = result && result.totalCount > 0 ? (result.page - 1) * result.pageSize + 1 : 0;
  const lastResult = result ? Math.min(page * result.pageSize, result.totalCount) : 0;

  return (
    <main className="min-h-[60vh] bg-[linear-gradient(180deg,#f1f8f6_0%,#f8faf9_420px)] px-5 py-12 md:py-16">
      <div className="mx-auto max-w-6xl">
        <header className="mb-9 flex flex-col justify-between gap-6 md:flex-row md:items-end">
          <div className="max-w-2xl">
            <p className="inline-flex items-center gap-2 text-xs font-bold uppercase tracking-[0.2em] text-emerald-700">
              <span aria-hidden="true" className="size-2 rounded-full bg-emerald-600" /> RashePharma insights
            </p>
            <h1 className="mt-3 text-4xl font-bold tracking-tight text-[#1B2A4A] md:text-5xl">Healthcare Articles</h1>
            <p className="mt-4 max-w-xl text-base leading-7 text-slate-600 md:text-lg">Practical healthcare and pharmaceutical insights from our team.</p>
          </div>
          <div className="flex w-fit items-center gap-3 rounded-2xl border border-emerald-100 bg-white/80 px-4 py-3 shadow-sm">
            <span className="grid size-10 place-items-center rounded-xl bg-emerald-50 text-emerald-700"><BookOpen aria-hidden="true" className="size-5" /></span>
            <span><strong className="block text-xl leading-6 text-[#1B2A4A]">{result?.totalCount ?? "—"}</strong><span className="text-xs text-slate-500">published articles</span></span>
          </div>
        </header>

        <section aria-label="Find articles" className="mb-8 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm md:p-5">
          <form action="/blog" method="get" className="grid gap-3 md:grid-cols-[minmax(0,1fr)_220px_auto]">
            <label className="relative block">
              <span className="sr-only">Search articles</span>
              <Search aria-hidden="true" className="pointer-events-none absolute left-4 top-1/2 size-5 -translate-y-1/2 text-slate-400" />
              <input type="search" name="q" defaultValue={search} maxLength={100} placeholder="Search by title, topic, author…"
                className="h-12 w-full rounded-xl border border-slate-200 bg-slate-50 pl-12 pr-4 text-sm text-[#1B2A4A] outline-none transition placeholder:text-slate-400 focus:border-emerald-600 focus:bg-white focus:ring-4 focus:ring-emerald-600/10" />
            </label>
            <label className="relative">
              <span className="sr-only">Sort articles</span>
              <SlidersHorizontal aria-hidden="true" className="pointer-events-none absolute left-4 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
              <select name="sort" defaultValue={sort} className="h-12 w-full appearance-none rounded-xl border border-slate-200 bg-slate-50 pl-11 pr-4 text-sm font-medium text-[#1B2A4A] outline-none transition focus:border-emerald-600 focus:bg-white focus:ring-4 focus:ring-emerald-600/10">
                <option value="newest">Newest first</option>
                <option value="oldest">Oldest first</option>
                <option value="title">Title A–Z</option>
              </select>
            </label>
            <button type="submit" className="inline-flex h-12 items-center justify-center gap-2 rounded-xl bg-emerald-700 px-6 text-sm font-semibold text-white shadow-sm transition hover:bg-emerald-800 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-emerald-700/20">
              <Search aria-hidden="true" className="size-4" /> Search
            </button>
          </form>
          {search && (
            <div className="mt-3 flex items-center justify-between gap-3 text-sm text-slate-600">
              <span>Search results for <strong className="text-[#1B2A4A]">“{search}”</strong></span>
              <Link href="/blog" className="font-semibold text-emerald-700 hover:underline">Clear search and filters</Link>
            </div>
          )}
        </section>

        {result && result.totalCount > 0 && (
          <p className="mb-5 text-sm text-slate-600" aria-live="polite">
            Showing <strong className="text-[#1B2A4A]">{firstResult}–{lastResult}</strong> of <strong className="text-[#1B2A4A]">{result.totalCount}</strong> articles
          </p>
        )}

        {!result && (
          <section role="alert" className="rounded-2xl border border-amber-200 bg-amber-50 p-8 text-center">
            <h2 className="text-lg font-bold text-amber-950">Articles couldn’t load</h2>
            <p className="mt-2 text-sm text-amber-900">Please check your connection and try again.</p>
            <Link href={pageHref(page, search, sort)} className="mt-5 inline-flex rounded-xl bg-amber-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-amber-950">Try again</Link>
          </section>
        )}

        {result?.items.length === 0 && (
          <section className="rounded-2xl border border-slate-200 bg-white px-6 py-14 text-center shadow-sm">
            <span className="mx-auto grid size-14 place-items-center rounded-2xl bg-emerald-50 text-emerald-700"><Search aria-hidden="true" className="size-6" /></span>
            <h2 className="mt-5 text-xl font-bold text-[#1B2A4A]">{search ? "No matching articles" : "No articles published yet"}</h2>
            <p className="mx-auto mt-2 max-w-md text-sm leading-6 text-slate-600">
              {search ? "Try a different title, topic, or author, or clear your search." : "New healthcare insights will appear here once they are published."}
            </p>
            {search && <Link href="/blog" className="mt-5 inline-flex rounded-xl border border-emerald-700 px-5 py-2.5 text-sm font-semibold text-emerald-800 hover:bg-emerald-50">Clear search</Link>}
          </section>
        )}

        {!!result?.items.length && (
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {result.items.map((post) => (
              <article key={post.slug} className="group flex h-full flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm transition duration-200 hover:-translate-y-1 hover:border-emerald-200 hover:shadow-lg">
                {post.coverImageUrl ? (
                  <Link href={"/blog/" + encodeURIComponent(post.slug)} tabIndex={-1} aria-hidden="true" className="relative block aspect-[16/10] overflow-hidden bg-slate-100">
                    <Image src={apiBase + "/blog/posts/" + encodeURIComponent(post.slug) + "/image"} alt="" fill unoptimized sizes="(max-width: 768px) 100vw, (max-width: 1280px) 50vw, 33vw" className="object-contain transition duration-500" />
                  </Link>
                ) : (
                  <div aria-hidden="true" className="grid aspect-[16/10] place-items-center bg-[radial-gradient(circle_at_top_right,#d7efe7,transparent_60%),linear-gradient(135deg,#eff6f4,#f8fafc)]">
                    <BookOpen className="size-10 text-emerald-700/40" />
                  </div>
                )}
                <div className="flex flex-1 flex-col p-5 md:p-6">
                  <p className="text-xs font-medium text-slate-500">
                    {post.publishedAtUtc ? new Date(post.publishedAtUtc).toLocaleDateString("en-IN", { dateStyle: "long", timeZone: "UTC" }) : ""}
                    <span aria-hidden="true"> · </span>{post.authorName}
                  </p>
                  <h2 className="mt-3 text-xl font-bold leading-snug text-[#1B2A4A] transition group-hover:text-emerald-800">
                    <Link href={"/blog/" + encodeURIComponent(post.slug)} className="focus-visible:rounded focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-700">{post.title}</Link>
                  </h2>
                  <p className="mt-3 line-clamp-3 flex-1 text-sm leading-6 text-slate-600">{post.excerpt}</p>
                  <Link href={"/blog/" + encodeURIComponent(post.slug)} className="mt-5 inline-flex w-fit items-center gap-2 text-sm font-bold text-emerald-700 transition group-hover:gap-3 hover:text-emerald-900">
                    Read article <ArrowRight aria-hidden="true" className="size-4" />
                  </Link>
                </div>
              </article>
            ))}
          </div>
        )}

        {totalPages > 1 && <BlogPagination page={page} totalPages={totalPages} search={search} sort={sort} />}
      </div>
    </main>
  );
}
