"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent } from "react";
import { toast } from "sonner";
import { ApiError } from "@/lib/api/client";
import { blogService } from "@/services/blog.service";
import type { BlogPost, BlogPostWriteInput } from "@/types/blog";
import { ArticleBodyEditor } from "@/components/admin/blog/ArticleBodyEditor";
import { BlogCoverPreview } from "@/components/admin/blog/BlogCoverPreview";
import { BlogCoverImagePicker } from "@/components/admin/blog/BlogCoverImagePicker";

const emptyPost: BlogPostWriteInput = {
  title: "", slug: "", excerpt: "", content: "", coverImageUrl: null, originalCoverImageUrl: null,
  authorName: "", reviewerName: null, reviewedAtUtc: null, metaTitle: null,
  metaDescription: null, canonicalUrl: null, ogTitle: null, ogDescription: null,
  ogImageUrl: null, isIndexable: true,
};

function toInput(post: BlogPost): BlogPostWriteInput {
  return {
    title: post.title, slug: post.slug, excerpt: post.excerpt, content: post.content,
    coverImageUrl: post.coverImageUrl, originalCoverImageUrl: post.originalCoverImageUrl,
    authorName: post.authorName, reviewerName: post.reviewerName,
    reviewedAtUtc: post.reviewedAtUtc, metaTitle: post.metaTitle, metaDescription: post.metaDescription,
    canonicalUrl: post.canonicalUrl, ogTitle: post.ogTitle, ogDescription: post.ogDescription,
    ogImageUrl: post.ogImageUrl, isIndexable: post.isIndexable,
  };
}

export function BlogPostEditor({ postId }: { postId?: number }) {
  const router = useRouter();
  const [post, setPost] = useState<BlogPostWriteInput>(emptyPost);
  const [status, setStatus] = useState<string>("Draft");
  const [slugIsCustom, setSlugIsCustom] = useState(Boolean(postId));
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(Boolean(postId));
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [coverPreviewUrl, setCoverPreviewUrl] = useState<string | null>(null);

  useEffect(() => {
    if (!postId) return;
    blogService.getById(postId).then((loaded) => {
      const loadedPost = toInput(loaded);
      setPost(loadedPost);
      setSlugIsCustom(true);
      setHasUnsavedChanges(false);
      setStatus(loaded.status);
    }).catch((error: unknown) => {
      toast.error(error instanceof Error ? error.message : "Could not load this article.");
      router.replace("/admin/blog");
    }).finally(() => setLoading(false));
  }, [postId, router]);

  useEffect(() => {
    if (!hasUnsavedChanges) return;
    // Protect long drafts against accidental reloads; this does not send content anywhere.
    const warnBeforeLeaving = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      event.returnValue = "";
    };
    window.addEventListener("beforeunload", warnBeforeLeaving);
    return () => window.removeEventListener("beforeunload", warnBeforeLeaving);
  }, [hasUnsavedChanges]);

  useEffect(() => {
    return () => {
      if (coverPreviewUrl) URL.revokeObjectURL(coverPreviewUrl);
    };
  }, [coverPreviewUrl]);

  const setField = <K extends keyof BlogPostWriteInput>(key: K, value: BlogPostWriteInput[K]) => {
    setPost((current) => ({ ...current, [key]: value }));
    setHasUnsavedChanges(true);
  };

  const updateTitle = (value: string) => {
    setField("title", value);
    if (!slugIsCustom) {
      setPost((current) => ({ ...current, slug: slugifyBlogTitle(value) }));
    }
  };

  const updateSlug = (value: string) => {
    const isCustom = value.trim().length > 0;
    setSlugIsCustom(isCustom);
    setField("slug", isCustom ? value : slugifyBlogTitle(post.title));
  };

  const getSaveInput = (): BlogPostWriteInput => ({
    ...post,
    // Keep the generated preview out of the payload; the server resolves collisions authoritatively.
    slug: slugIsCustom ? post.slug : "",
  });

  const save = async (): Promise<BlogPost | null> => {
    if (postId) {
      const updated = await blogService.update(postId, getSaveInput());
      const normalized = toInput(updated);
      setPost(normalized);
      setHasUnsavedChanges(false);
      return updated;
    }
    const created = await blogService.create(getSaveInput());
    const normalized = toInput(created);
    setPost(normalized);
    setHasUnsavedChanges(false);
    return created;
  };

  const onSave = async (event: FormEvent) => {
    event.preventDefault();
    if (!post.content.trim() || !/[\p{L}\p{N}]/u.test(post.content)) {
      toast.error("Add article content before saving.");
      return;
    }
    setBusy(true);
    try {
      const saved = await save();
      if (!saved) throw new Error("This article could not be found.");
      setStatus(saved.status);
      toast.success(saved.status === "Published" ? "Article changes saved." : "Draft saved.");
      router.replace("/admin/blog");
    } catch (error) {
      toast.error(error instanceof ApiError ? error.message : "Could not save the article.");
    } finally { setBusy(false); }
  };

  const onPublish = async () => {
    setBusy(true);
    try {
      const saved = await save();
      if (!saved) throw new Error("This article could not be found.");
      const published = await blogService.publish(saved.id);
      setStatus(published.status);
      toast.success("Article published.");
      router.replace("/admin/blog");
    } catch (error) {
      toast.error(error instanceof ApiError ? error.message : "Could not publish the article.");
    } finally { setBusy(false); }
  };

  const onUnpublish = async () => {
    if (!postId) return;
    setBusy(true);
    try { const result = await blogService.unpublish(postId); setStatus(result.status); toast.success("Article returned to draft."); }
    catch (error) { toast.error(error instanceof Error ? error.message : "Could not unpublish the article."); }
    finally { setBusy(false); }
  };

  const onCoverUploaded = (result: { url: string; originalUrl: string }, previewUrl: string) => {
    setPost((current) => ({
      ...current,
      coverImageUrl: result.url,
      originalCoverImageUrl: result.originalUrl,
    }));
    setHasUnsavedChanges(true);
    setCoverPreviewUrl(previewUrl);
  };

  if (loading) return <div className="p-8 text-gray-600">Loading article…</div>;

  return (
    <form onSubmit={onSave} className="mx-auto max-w-5xl space-y-6 p-6 md:p-10">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div><Link href="/admin/blog" className="text-sm text-blue-700 hover:underline">← All articles</Link>
          <h1 className="mt-2 text-2xl font-bold text-[#1B2A4A]">{postId ? "Edit article" : "New article"}</h1>
          <p className="mt-1 text-sm text-gray-500">Status: {status}. New articles stay draft until you publish them.</p></div>
        <div className="flex gap-2">
          {status === "Published" && <button type="button" disabled={busy} onClick={onUnpublish} className="rounded-lg border px-4 py-2">Unpublish</button>}
          <button type="submit" disabled={busy} className="rounded-lg border px-4 py-2">{status === "Published" ? "Save changes" : "Save draft"}</button>
          {status !== "Published" && <button type="button" disabled={busy} onClick={onPublish} className="rounded-lg bg-[#1B2A4A] px-4 py-2 font-semibold text-white">Publish</button>}
        </div>
      </div>

      {hasUnsavedChanges && <p role="status" className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-2 text-sm text-amber-900">You have unsaved changes.</p>}
      <fieldset disabled={busy} className="contents">
      <section className="space-y-4 rounded-xl border bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-[#1B2A4A]">Article</h2>
        <Field label="Title" required value={post.title} onChange={updateTitle} />
        <div>
          <label htmlFor="blog-post-slug" className="block text-sm font-medium text-gray-700">
            URL slug {slugIsCustom ? "(custom)" : "(generated from title)"}
          </label>
          <input id="blog-post-slug" value={post.slug ?? ""} onChange={(event) => updateSlug(event.target.value)}
            className="mt-1 w-full rounded-lg border border-gray-300 p-3" autoComplete="off" />
          <p className="mt-1 text-xs text-gray-500">
            {slugIsCustom
              ? "Custom slug. It must be unique; published article URLs should stay stable."
              : "Preview: /blog/" + (post.slug || "your-article") + ". Duplicate titles receive a unique suffix when saved."}
          </p>
        </div>
        <Field label="Short excerpt" required value={post.excerpt} onChange={(v) => setField("excerpt", v)} textarea />
        <div className="block text-sm font-medium text-gray-700">
          <label htmlFor="article-body" className="mb-1 block">Article body <span className="text-red-600">*</span></label>
          <ArticleBodyEditor value={post.content} onChange={(value) => setField("content", value)} />
          <span className="mt-1 block text-xs text-gray-500">The preview uses the same safe renderer as the published article.</span>
        </div>
        <Field label="Author name" required value={post.authorName} onChange={(v) => setField("authorName", v)} />
        <Field label="Medical/content reviewer (optional)" value={post.reviewerName ?? ""} onChange={(v) => setField("reviewerName", v || null)} />
        <div className="grid gap-3 md:grid-cols-2">
          <Field label="16:10 cover image URL" value={post.coverImageUrl ?? ""} onChange={(v) => {
            setPost((current) => ({ ...current, coverImageUrl: v || null, originalCoverImageUrl: null }));
            setHasUnsavedChanges(true);
          }} />
          <BlogCoverImagePicker disabled={busy} postId={postId} hasOriginal={Boolean(post.originalCoverImageUrl || post.coverImageUrl)} onUploaded={onCoverUploaded} onUploadingChange={setBusy} />
        </div>
        <BlogCoverPreview postId={postId} hasCoverImage={Boolean(post.coverImageUrl)} localPreviewUrl={coverPreviewUrl} className="aspect-[16/10] w-full max-w-2xl" />
        <p className="text-xs text-gray-500">The original image is kept for future edits. The generated 16:10 WebP is used on the public blog. Stored privately and previewed through the authenticated admin API.</p>
      </section>

      <section className="space-y-4 rounded-xl border bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-[#1B2A4A]">Search and social metadata</h2>
        <Field label="Meta title" value={post.metaTitle ?? ""} onChange={(v) => setField("metaTitle", v || null)} />
        <Field label="Meta description" value={post.metaDescription ?? ""} onChange={(v) => setField("metaDescription", v || null)} textarea />
        <Field label="Canonical URL (optional)" value={post.canonicalUrl ?? ""} onChange={(v) => setField("canonicalUrl", v || null)} />
        <Field label="Social share title" value={post.ogTitle ?? ""} onChange={(v) => setField("ogTitle", v || null)} />
        <Field label="Social share description" value={post.ogDescription ?? ""} onChange={(v) => setField("ogDescription", v || null)} textarea />
        <Field label="Social share image URL" value={post.ogImageUrl ?? ""} onChange={(v) => setField("ogImageUrl", v || null)} />
        <label className="flex items-center gap-2 text-sm text-gray-700"><input type="checkbox" checked={post.isIndexable} onChange={(e) => setField("isIndexable", e.target.checked)} /> Allow search engines to index this article</label>
      </section>
      <div className="flex justify-end gap-2"><button type="submit" disabled={busy} className="rounded-lg border px-4 py-2">{status === "Published" ? "Save changes" : "Save draft"}</button>
        {status !== "Published" && <button type="button" disabled={busy} onClick={onPublish} className="rounded-lg bg-[#1B2A4A] px-4 py-2 font-semibold text-white">Publish</button>}</div>
      </fieldset>
    </form>
  );
}

function slugifyBlogTitle(value: string) {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 200)
    .replace(/-+$/g, "");
}

function Field({ label, value, onChange, required, textarea }: { label: string; value: string; onChange: (value: string) => void; required?: boolean; textarea?: boolean }) {
  return <label className="block text-sm font-medium text-gray-700">{label}{required && <span className="text-red-600"> *</span>}
    {textarea ? <textarea required={required} value={value} onChange={(e) => onChange(e.target.value)} rows={3} className="mt-1 w-full rounded-lg border border-gray-300 p-3" />
      : <input required={required} value={value} onChange={(e) => onChange(e.target.value)} className="mt-1 w-full rounded-lg border border-gray-300 p-3" />}
  </label>;
}
