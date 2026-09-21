import { UserProfilePage } from "@/components/Accounts/UserProfilePage";

export default function ProfilePage() {
  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-14">
      <div className="mx-auto w-full max-w-5xl px-5 sm:px-6 lg:px-8">
        <UserProfilePage />
      </div>
    </main>
  );
}