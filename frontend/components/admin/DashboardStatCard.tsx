interface DashboardStatCardProps {
  title: string;
  value: number;
}

export function DashboardStatCard({
  title,
  value,
}: DashboardStatCardProps) {
  return (
    <div className="rounded-xl bg-white p-6 shadow-sm">
      <p className="text-sm text-[#777]">
        {title}
      </p>

      <p className="mt-2 text-3xl font-semibold text-[#1B2A4A]">
        {value}
      </p>
    </div>
  );
}