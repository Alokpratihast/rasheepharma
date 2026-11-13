import { cn } from "@/lib/utils";

interface SectionHeadingProps {
  /** Two-digit section index, e.g. "01". */
  index?: string;
  eyebrow: string;
  title: string;
  description?: string;
  /** Use on dark backgrounds. */
  invert?: boolean;
  align?: "left" | "center";
  className?: string;
}

export function SectionHeading({
  index,
  eyebrow,
  title,
  description,
  invert = false,
  align = "left",
  className,
}: SectionHeadingProps) {
  return (
    <div className={cn(align === "center" && "text-center", className)}>
      <p
        className={cn(
          "flex items-center gap-3 font-mono text-xs font-medium uppercase tracking-[0.16em]",
          align === "center" && "justify-center",
          invert ? "text-emerald-300" : "text-primary",
        )}
      >
        {index && <span>{index}</span>}
        <span
          aria-hidden="true"
          className={cn("h-px w-8", invert ? "bg-white/30" : "bg-primary/40")}
        />
        <span>{eyebrow}</span>
      </p>
      <h2
        className={cn(
          "mt-5 text-3xl font-semibold leading-[1.08] tracking-[-0.03em] sm:text-4xl lg:text-[44px]",
          invert ? "text-white" : "text-foreground",
        )}
      >
        {title}
      </h2>
      {description && (
        <p
          className={cn(
            "mt-5 max-w-xl text-base leading-7",
            invert ? "text-white/65" : "text-muted-foreground",
          )}
        >
          {description}
        </p>
      )}
    </div>
  );
}
