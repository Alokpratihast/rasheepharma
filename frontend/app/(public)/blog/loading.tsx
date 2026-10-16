import { BookOpen } from "lucide-react";

export default function BlogLoading() {
  return (
    <main aria-busy="true" aria-label="Loading articles" className="min-h-[60vh] bg-[linear-gradient(180deg,#f1f8f6_0%,#f8faf9_420px)] px-5 py-12 md:py-16">
      <div className="mx-auto max-w-6xl animate-pulse">
        <div className="mb-9 flex flex-col justify-between gap-6 md:flex-row md:items-end">
          <div className="w-full max-w-2xl">
            <div className="h-4 w-44 rounded bg-emerald-100" />
            <div className="mt-4 h-11 w-3/4 max-w-md rounded-lg bg-slate-200" />
            <div className="mt-4 h-5 w-full max-w-lg rounded bg-slate-100" />
          </div>
          <div className="h-16 w-48 rounded-2xl bg-white shadow-sm" />
        </div>
        <div className="mb-8 h-[76px] rounded-2xl border border-slate-200 bg-white" />
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, index) => (
            <div key={index} className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
              <div className="grid aspect-[16/10] place-items-center bg-slate-100"><BookOpen className="size-8 text-slate-300" /></div>
              <div className="space-y-4 p-6">
                <div className="h-3 w-1/2 rounded bg-slate-100" />
                <div className="h-6 w-4/5 rounded bg-slate-200" />
                <div className="h-4 w-full rounded bg-slate-100" />
                <div className="h-4 w-2/3 rounded bg-slate-100" />
              </div>
            </div>
          ))}
        </div>
      </div>
    </main>
  );
}