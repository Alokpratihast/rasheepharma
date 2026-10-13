"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { blogService } from "@/services/blog.service";
import type { BlogPostPage } from "@/types/blog";

export function BlogPostList() {
  const [result, setResult] = useState<BlogPostPage | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;
    // Resolve the request asynchronously and ignore stale page responses after navigation.
    blogService.getAdminPage(page)
      .then((posts) => { if (active) setResult(posts); })
      .catch((error: unknown) => { if (active) toast.error(error instanceof Error ? error.message : "Could not load blog posts."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [page]);

  const load = async () => {
    try { setResult(await blogService.getAdminPage(page)); }
    catch (error) { toast.error(error instanceof Error ? error.message : "Could not load blog posts."); }
    finally { setLoading(false); }
  };

  const remove = async (id: number) => {
    if (!window.confirm("Delete this blog post? This cannot be undone.")) return;
    setLoading(true);
    try { await blogService.delete(id); toast.success("Blog post deleted."); await load(); }
    catch (error) { setLoading(false); toast.error(error instanceof Error ? error.message : "Could not delete blog post."); }
  };

  const togglePublish = async (id: number, published: boolean) => {
    setLoading(true);
    try {
      if (published) await blogService.unpublish(id); else await blogService.publish(id);
      toast.success(published ? "Post returned to draft." : "Post published.");
      await load();
    } catch (error) { setLoading(false); toast.error(error instanceof Error ? error.message : "Could not update publication status."); }
  };

  return <section className="mx-auto max-w-7xl p-6 md:p-10">
    <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
      <div><h1 className="text-2xl font-bold text-[#1B2A4A]">Blog articles</h1><p className="mt-1 text-sm text-gray-500">Create, review, and publish articles independently of Website Content.</p></div>
      <Link href="/admin/blog/new" className="rounded-lg bg-[#1B2A4A] px-4 py-2.5 font-semibold text-white">New article</Link>
    </div>
    <div className="overflow-x-auto rounded-xl border bg-white shadow-sm">
      <table className="w-full min-w-[760px] text-left text-sm">
        <thead className="bg-gray-50 text-gray-600"><tr><th className="p-4">Article</th><th className="p-4">Status</th><th className="p-4">Updated</th><th className="p-4">Actions</th></tr></thead>
        <tbody className="divide-y">
          {loading && <tr><td colSpan={4} className="p-8 text-center text-gray-500">Loading articles…</td></tr>}
          {!loading && !result?.items.length && <tr><td colSpan={4} className="p-8 text-center text-gray-500">No articles yet. Create a draft to get started.</td></tr>}
          {!loading && result?.items.map((post) => <tr key={post.id}>
            <td className="p-4"><p className="font-semibold text-[#1B2A4A]">{post.title}</p><p className="mt-1 text-xs text-gray-500">/blog/{post.slug}</p></td>
            <td className="p-4"><span className={`rounded-full px-2.5 py-1 text-xs font-semibold ${post.status === "Published" ? "bg-green-100 text-green-800" : "bg-gray-100 text-gray-700"}`}>{post.status}</span></td>
            <td className="p-4 text-gray-600">{new Date(post.updatedAtUtc).toLocaleDateString()}</td>
            <td className="p-4"><div className="flex items-center gap-3">
              <Link className="font-medium text-blue-700 hover:underline" href={`/admin/blog/${post.id}`}>Edit</Link>
              <button type="button" onClick={() => void togglePublish(post.id, post.status === "Published")} className="font-medium text-blue-700 hover:underline">{post.status === "Published" ? "Unpublish" : "Publish"}</button>
              <button type="button" onClick={() => void remove(post.id)} className="font-medium text-red-700 hover:underline">Delete</button>
            </div></td>
          </tr>)}
        </tbody>
      </table>
    </div>
    <div className="mt-4 flex items-center justify-between text-sm text-gray-600">
      <span>{result ? `${result.totalCount} article${result.totalCount === 1 ? "" : "s"}` : ""}</span>
      <div className="flex gap-2"><button disabled={page <= 1 || loading} onClick={() => { setLoading(true); setPage((current) => current - 1); }} className="rounded border px-3 py-1.5 disabled:opacity-40">Previous</button>
        <button disabled={!result || page * result.pageSize >= result.totalCount || loading} onClick={() => { setLoading(true); setPage((current) => current + 1); }} className="rounded border px-3 py-1.5 disabled:opacity-40">Next</button></div>
    </div>
  </section>;
}
