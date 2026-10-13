import { notFound } from "next/navigation";
import { BlogPostEditor } from "@/components/admin/blog/BlogPostEditor";

export default async function EditBlogPostPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const postId = Number(id);
  if (!Number.isSafeInteger(postId) || postId < 1) notFound();
  return <BlogPostEditor postId={postId} />;
}
