import { UnsubscribeConfirmation } from "@/components/lead-outreach/UnsubscribeConfirmation";

export default async function UnsubscribePage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string }>;
}) {
  const { token } = await searchParams;
  return <UnsubscribeConfirmation token={token ?? ""} />;
}
