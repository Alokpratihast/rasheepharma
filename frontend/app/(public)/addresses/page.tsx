import { AddressesPage } from "@/components/Accounts/AddressesPage";

export default function AddressesRoute() {
  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-14">
      <div className="mx-auto w-full max-w-6xl px-5 sm:px-6 lg:px-8">
        <AddressesPage />
      </div>
    </main>
  );
}