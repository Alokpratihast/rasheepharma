import { AdminGuard } from "@/components/auth/AdminGuard";
import { AdminSidebar } from "@/components/admin/AdminSidebar";
import { Toaster } from "sonner";

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AdminGuard>
      <div className="min-h-screen bg-[#f5f7f6]">
        <AdminSidebar />
        <Toaster position="top-right" richColors closeButton />

        <main className="min-h-screen pt-14 lg:pl-64 lg:pt-0">
          {children}
        </main>
      </div>
    </AdminGuard>
  );
}
