import type { ReactNode } from "react";

/**
 * Render the CMS's small Markdown subset as React nodes. Raw HTML is never
 * interpreted, which keeps author-entered content safe without an HTML sink.
 */
export function MarkdownArticle({ content }: { content: string }) {
  const lines = content.replace(/\r\n?/g, "\n").split("\n");
  const blocks: ReactNode[] = [];
  let index = 0;

  while (index < lines.length) {
    const line = lines[index];
    if (!line.trim()) {
      index += 1;
      continue;
    }

    const heading = /^(#{1,3})\s+(.+)$/.exec(line);
    if (heading) {
      const level = heading[1].length;
      const classNames = level === 1
        ? "mt-8 text-3xl font-bold leading-tight text-[#1B2A4A]"
        : level === 2
          ? "mt-8 text-2xl font-bold leading-tight text-[#1B2A4A]"
          : "mt-6 text-xl font-semibold leading-tight text-[#1B2A4A]";
      const children = renderInline(heading[2]);
      blocks.push(level === 1
        ? <h2 key={index} className={classNames}>{children}</h2>
        : level === 2
          ? <h3 key={index} className={classNames}>{children}</h3>
          : <h4 key={index} className={classNames}>{children}</h4>);
      index += 1;
      continue;
    }

    if (/^\s*>\s?/.test(line)) {
      const quote: string[] = [];
      while (index < lines.length && /^\s*>\s?/.test(lines[index])) {
        quote.push(lines[index].replace(/^\s*>\s?/, ""));
        index += 1;
      }
      blocks.push(<blockquote key={`quote-${index}`} className="my-6 border-l-4 border-emerald-600 pl-4 italic text-gray-700">
        {quote.map((part, partIndex) => <span key={partIndex}>{partIndex > 0 && <br />}{renderInline(part)}</span>)}
      </blockquote>);
      continue;
    }

    const unordered = /^\s*[-*+]\s+(.+)$/.exec(line);
    const ordered = /^\s*\d+[.)]\s+(.+)$/.exec(line);
    if (unordered || ordered) {
      const items: string[] = [];
      const isOrdered = Boolean(ordered);
      while (index < lines.length) {
        const item = isOrdered ? /^\s*\d+[.)]\s+(.+)$/.exec(lines[index]) : /^\s*[-*+]\s+(.+)$/.exec(lines[index]);
        if (!item) break;
        items.push(item[1]);
        index += 1;
      }
      const listItems = items.map((item, itemIndex) => <li key={itemIndex}>{renderInline(item)}</li>);
      blocks.push(isOrdered
        ? <ol key={`list-${index}`} className="my-5 list-decimal space-y-2 pl-6">{listItems}</ol>
        : <ul key={`list-${index}`} className="my-5 list-disc space-y-2 pl-6">{listItems}</ul>);
      continue;
    }

    // Consecutive plain lines stay in one paragraph; explicit line breaks survive edits.
    const paragraph: string[] = [];
    while (index < lines.length && lines[index].trim() &&
      !/^(#{1,3})\s+/.test(lines[index]) && !/^\s*>\s?/.test(lines[index]) &&
      !/^\s*[-*+]\s+/.test(lines[index]) && !/^\s*\d+[.)]\s+/.test(lines[index])) {
      paragraph.push(lines[index]);
      index += 1;
    }
    blocks.push(<p key={`paragraph-${index}`} className="my-5 leading-8 text-gray-800">
      {paragraph.map((part, partIndex) => <span key={partIndex}>{partIndex > 0 && <br />}{renderInline(part)}</span>)}
    </p>);
  }

  return <div className="break-words">{blocks}</div>;
}

function renderInline(text: string): ReactNode[] {
  const token = /(\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)|\*\*([^*]+)\*\*|\*([^*]+)\*|`([^`]+)`)/g;
  const nodes: ReactNode[] = [];
  let lastIndex = 0;

  for (const match of text.matchAll(token)) {
    const start = match.index ?? 0;
    if (start > lastIndex) nodes.push(text.slice(lastIndex, start));
    if (match[2] && match[3]) {
      // Only absolute HTTP(S) links are recognized; unsafe schemes stay plain text.
      nodes.push(<a key={`link-${start}`} href={match[3]} target="_blank" rel="noopener noreferrer" className="text-emerald-800 underline underline-offset-2">{match[2]}</a>);
    } else if (match[4]) nodes.push(<strong key={`bold-${start}`}>{match[4]}</strong>);
    else if (match[5]) nodes.push(<em key={`italic-${start}`}>{match[5]}</em>);
    else if (match[6]) nodes.push(<code key={`code-${start}`} className="rounded bg-gray-100 px-1 py-0.5 text-[0.9em]">{match[6]}</code>);
    lastIndex = start + match[0].length;
  }

  if (lastIndex < text.length) nodes.push(text.slice(lastIndex));
  return nodes;
}
