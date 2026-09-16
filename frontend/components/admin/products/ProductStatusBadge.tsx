interface ProductStatusBadgeProps {
  active: boolean;
  label?: string;
}

export function ProductStatusBadge({
  active,
  label,
}: ProductStatusBadgeProps) {
  return (
    <span
      className={`rounded-full px-2.5 py-1 text-xs font-medium ${
        active
          ? "bg-green-100 text-green-700"
          : "bg-gray-100 text-gray-600"
      }`}
    >
      {label ?? (active ? "Active" : "Inactive")}
    </span>
  );
}