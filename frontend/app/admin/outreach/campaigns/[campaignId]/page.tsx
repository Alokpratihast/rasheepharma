import { CampaignDetail } from "@/components/admin/leads/CampaignDetail";

export default async function OutreachCampaignDetailPage({
  params,
}: {
  params: Promise<{ campaignId: string }>;
}) {
  const { campaignId } = await params;
  const id = Number(campaignId);
  if (!Number.isSafeInteger(id) || id < 1) {
    return <main className="mx-auto max-w-5xl p-6"><h1 className="text-2xl font-bold">Campaign not found</h1></main>;
  }
  return <CampaignDetail campaignId={id} />;
}
