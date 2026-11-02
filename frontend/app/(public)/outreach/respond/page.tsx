import OutreachResponseForm from "@/components/outreach/OutreachResponseForm";

export const metadata = { title: "Reply to Rashe Lifesciences", robots: { index: false, follow: false }, referrer: "no-referrer" };

export default async function OutreachResponsePage({ searchParams }: { searchParams: Promise<{ token?: string }> }) {
  const { token } = await searchParams;
  return <OutreachResponseForm token={token ?? ""} />;
}
