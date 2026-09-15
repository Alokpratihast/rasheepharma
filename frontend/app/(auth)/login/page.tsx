import { LoginForm } from "@/components/forms/LoginForm";
import { Container } from "@/components/ui/container";

export default function LoginPage() {
  return (
    <main className="min-h-screen bg-[#fafafa] py-10 sm:py-16">
      <Container>
        <div className="mx-auto w-full max-w-md">
          <LoginForm />
        </div>
      </Container>
    </main>
  );
}