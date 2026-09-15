export default function AdminPage() {
  return (
    <main className="min-h-screen bg-[#f5f7f6] p-6">
      <div className="mx-auto max-w-7xl">
        <h1 className="text-3xl font-semibold text-[#1B2A4A]">
          Admin Dashboard
        </h1>

        <p className="mt-2 text-sm text-[#595959]">
          Welcome to the RashePharma admin panel.
        </p>

        <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
          <div className="rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-[#777]">Total Users</p>
            <p className="mt-2 text-3xl font-semibold text-[#1B2A4A]">
              0
            </p>
          </div>

          <div className="rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-[#777]">Products</p>
            <p className="mt-2 text-3xl font-semibold text-[#1B2A4A]">
              0
            </p>
          </div>

          <div className="rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-[#777]">Enquiries</p>
            <p className="mt-2 text-3xl font-semibold text-[#1B2A4A]">
              0
            </p>
          </div>

          <div className="rounded-xl bg-white p-6 shadow-sm">
            <p className="text-sm text-[#777]">Active Users</p>
            <p className="mt-2 text-3xl font-semibold text-[#1B2A4A]">
              0
            </p>
          </div>
        </div>
      </div>
    </main>
  );
}