import { cn } from "@/lib/utils";

interface HomeHeadingProps {
  eyebrow: string;
  title: string;
  description?: string;
  /** Use on dark backgrounds. */
  invert?: boolean;
  className?: string;
}

/** Shared section heading: mono eyebrow with a rule, tight title. */
export function HomeHeading({
  eyebrow,
  title,
  description,
  invert = false,
  className,
}: HomeHeadingProps) {
  return (
    <div className={cn("max-w-2xl", className)}>
      <p
        className={cn(
          "flex items-center gap-3 font-mono text-xs font-medium uppercase tracking-[0.16em]",
          invert ? "text-emerald-300" : "text-primary",
        )}
      >
        <span
          aria-hidden="true"
          className={cn("h-px w-8", invert ? "bg-white/30" : "bg-primary/40")}
        />
        {eyebrow}
      </p>
      <h2
        className={cn(
          "mt-4 text-3xl font-bold leading-[1.08] tracking-[-0.03em] sm:text-4xl lg:text-[42px]",
          invert ? "text-white" : "text-brand-dark",
        )}
      >
        {title}
      </h2>
      {description && (
        <p
          className={cn(
            "mt-4 text-base leading-7 sm:text-lg",
            invert ? "text-white/70" : "text-muted-foreground",
          )}
        >
          {description}
        </p>
      )}
    </div>
  );
}
