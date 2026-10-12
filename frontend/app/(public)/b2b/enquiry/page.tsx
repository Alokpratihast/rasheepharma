import { EnquiryForm } from "@/components/forms/EnquiryForm";

export default function EnquiryPage() {
  return (
    <main className="min-h-screen bg-gray-50 py-10">
      <div className="mx-auto w-full max-w-4xl px-4 sm:px-6 lg:px-8">
        <EnquiryForm />
      </div>
    </main>
  );
}