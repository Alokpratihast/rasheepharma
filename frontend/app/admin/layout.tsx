import { AdminGuard } from "@/components/auth/AdminGuard";
import { AdminSidebar } from "@/components/admin/AdminSidebar";

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AdminGuard>
      <div className="min-h-screen bg-[#f5f7f6]">
        <AdminSidebar />

        <main className="min-h-screen pl-64">
          {children}
        </main>
      </div>
    </AdminGuard>
  );
}