"use client";

import { useRef, useState } from "react";
import { MarkdownArticle } from "@/components/blog/MarkdownArticle";

type Tool = "bold" | "italic" | "heading" | "bullets" | "numbers" | "quote" | "code" | "link";

/** A lightweight, keyboard-friendly Markdown editor that persists in the existing Content field. */
export function ArticleBodyEditor({ value, onChange }: { value: string; onChange: (value: string) => void }) {
  const inputRef = useRef<HTMLTextAreaElement>(null);
  const [preview, setPreview] = useState(false);

  const applyTool = (tool: Tool) => {
    const input = inputRef.current;
    if (!input) return;
    const { selectionStart: start, selectionEnd: end } = input;
    const selected = value.slice(start, end);
    let next = value;
    let caretStart = start;
    let caretEnd = end;

    if (tool === "bold" || tool === "italic" || tool === "code" || tool === "link") {
      const marker = tool === "bold" ? "**" : tool === "italic" ? "*" : tool === "code" ? "`" : "";
      if (tool === "link") {
        const url = window.prompt("Paste an https:// link");
        if (!url) return;
        if (!/^https?:\/\//i.test(url)) {
          window.alert("Use a link that starts with https:// or http://.");
          return;
        }
        const label = selected || "link text";
        const inserted = `[${label}](${url.trim()})`;
        next = value.slice(0, start) + inserted + value.slice(end);
        caretStart = start + 1;
        caretEnd = caretStart + label.length;
      } else {
        const text = selected || "text";
        const inserted = `${marker}${text}${marker}`;
        next = value.slice(0, start) + inserted + value.slice(end);
        caretStart = start + marker.length;
        caretEnd = caretStart + text.length;
      }
    } else {
      const lineStart = value.lastIndexOf("\n", Math.max(0, start - 1)) + 1;
      const nextBreak = value.indexOf("\n", end);
      const lineEnd = nextBreak === -1 ? value.length : nextBreak;
      const selectedLines = value.slice(lineStart, lineEnd);
      const prefix = tool === "heading" ? "# " : tool === "bullets" ? "- " : tool === "numbers" ? "1. " : "> ";
      const prefixPattern = tool === "heading" ? /^#\s/ : tool === "bullets" ? /^[-*+]\s/ : tool === "numbers" ? /^\d+[.)]\s/ : /^>\s?/;
      const lines = selectedLines.split("\n");
      const removePrefix = lines.every((line) => prefixPattern.test(line));
      const changedLines = lines.map((line) => removePrefix ? line.replace(prefixPattern, "") : `${prefix}${line}`);
      const changed = changedLines.join("\n");
      next = value.slice(0, lineStart) + changed + value.slice(lineEnd);
      caretStart = lineStart;
      caretEnd = lineStart + changed.length;
    }

    onChange(next);
    // Restore the selection after React commits the updated controlled value.
    requestAnimationFrame(() => {
      input.focus();
      input.setSelectionRange(caretStart, caretEnd);
    });
  };

  const tools: Array<{ key: Tool; label: string; description: string }> = [
    { key: "bold", label: "B", description: "Bold" },
    { key: "italic", label: "I", description: "Italic" },
    { key: "heading", label: "H2", description: "Heading" },
    { key: "bullets", label: "• List", description: "Bulleted list" },
    { key: "numbers", label: "1. List", description: "Numbered list" },
    { key: "quote", label: "❝ Quote", description: "Quote" },
    { key: "code", label: "Code", description: "Inline code" },
    { key: "link", label: "Link", description: "Insert link" },
  ];

  return <div className="mt-1 overflow-hidden rounded-xl border border-gray-300 focus-within:border-emerald-700 focus-within:ring-2 focus-within:ring-emerald-700/20">
    <div className="flex flex-wrap items-center gap-1 border-b bg-gray-50 p-2" role="toolbar" aria-label="Article formatting">
      {tools.map((tool) => <button key={tool.key} type="button" title={tool.description} aria-label={tool.description}
        onMouseDown={(event) => event.preventDefault()} onClick={() => applyTool(tool.key)}
        className="min-h-9 rounded-md border border-transparent px-2.5 text-sm font-medium text-gray-700 hover:border-gray-300 hover:bg-white focus-visible:outline-2 focus-visible:outline-emerald-700">
        {tool.label}
      </button>)}
      <span className="mx-1 h-6 border-l" aria-hidden="true" />
      <button type="button" aria-pressed={preview} onClick={() => setPreview((current) => !current)}
        className="min-h-9 rounded-md px-3 text-sm font-medium text-emerald-800 hover:bg-emerald-50">
        {preview ? "Edit" : "Preview"}
      </button>
    </div>
    {preview
      ? <div className="min-h-80 px-5 py-3" aria-label="Article body preview"><MarkdownArticle content={value || "Nothing to preview yet."} /></div>
      : <textarea ref={inputRef} required maxLength={100000} rows={16} value={value} onChange={(event) => onChange(event.target.value)}
          id="article-body" className="block min-h-80 w-full resize-y border-0 p-4 text-base leading-7 text-gray-900 outline-none focus:ring-0"
          aria-label="Article body" placeholder="Write your article. Use the toolbar to add headings, lists, quotes, emphasis, and links." />}
    <div className="flex flex-wrap items-center justify-between gap-2 border-t bg-gray-50 px-3 py-2 text-xs text-gray-500">
      <span>Formatting is stored as Markdown. Pasted HTML and scripts are displayed as text.</span>
      <span aria-live="polite">{value.length.toLocaleString()} / 100,000 characters</span>
    </div>
  </div>;
}
