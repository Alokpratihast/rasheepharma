import type { LucideIcon } from "lucide-react";

interface DashboardStatCardProps {
  title: string;
  value: number;
  detail: string;
  icon: LucideIcon;
  highlighted?: boolean;
}

export function DashboardStatCard({
  title,
  value,
  detail,
  icon: Icon,
  highlighted = false,
}: DashboardStatCardProps) {
  return (
    <div className={`rounded-2xl border p-5 shadow-sm transition hover:-translate-y-0.5 hover:shadow-md ${highlighted ? "border-amber-200 bg-amber-50/70" : "border-slate-200 bg-white"}`}>
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-slate-600">{title}</p>
          <p className="mt-3 text-3xl font-bold tracking-tight text-[#1B2A4A]">
            {new Intl.NumberFormat("en-IN").format(value)}
          </p>
        </div>
        <span className={`flex size-11 items-center justify-center rounded-xl ${highlighted ? "bg-amber-100 text-amber-700" : "bg-emerald-50 text-[#00856b]"}`}>
          <Icon aria-hidden="true" className="size-5" />
        </span>
      </div>
      <p className="mt-3 text-xs text-slate-500">{detail}</p>
    </div>
  );
}
