import { RegisterForm } from "@/components/forms/RegisterForm";
import { Container } from "@/components/ui/container";

export default function RegisterPage() {
  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-16">
      <Container>
        <div className="mx-auto w-full max-w-2xl">
          <RegisterForm />
        </div>
      </Container>
    </main>
  );
}