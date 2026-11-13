import Image from "next/image";
import Link from "next/link";
import { ArrowRight, ChevronRight } from "lucide-react";

import { Container } from "@/components/ui/container";
import { FOUNDED_YEAR } from "@/components/about/constants";

const enter =
  "animate-in fade-in slide-in-from-bottom-4 duration-700 [animation-fill-mode:backwards]";

const facts = [
  { label: "Established", value: FOUNDED_YEAR },
  { label: "Team experience", value: "15 years" },
  { label: "Markets covered", value: "Worldwide" },
  { label: "Certified plants", value: "ISO · WHO-GMP" },
];

/* Dividers: 2x2 grid on mobile, single row with vertical rules on desktop. */
const factBorders = [
  "pr-4",
  "border-l pl-5 lg:pl-8",
  "border-t pr-4 lg:border-l lg:border-t-0 lg:pl-8",
  "border-l border-t pl-5 lg:border-t-0 lg:pl-8",
];

/* Company story shown below the fact strip. */
const story = [
  "Rashe Lifesciences Pvt Ltd is an emerging pharmaceutical company based in Bangalore. It was started in 2017 by an expert team with 15 years of experience in the field of pharmacy. We are committed to supplying high quality products that serve healthcare, and to providing excellent customer service. Our reliability, quality and wide distribution network ensure timely delivery of goods, and have helped us earn many satisfied clients.",
  "With the experience gained working in the field, we are associated with ISO and WHO-GMP certified plants that manufacture essential drugs to help improve patients' health. Our long-term bond and trust with customers reflect our product quality and consistent service. We pursue excellence by adopting superior processes that add value to what we deliver.",
  "Rashe Lifesciences Pvt Ltd is a renowned trademark for the marketing and supply of medicines used in treating serious illnesses. We ensure our products are consistently produced to meet quality standards.",
];

export function AboutHero() {
  return (
    <section className="relative overflow-hidden bg-background">
      {/* soft brand wash, top right */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute -right-40 -top-40 size-[560px] rounded-full bg-primary-light blur-3xl dark:bg-primary/10"
      />

      <Container className="relative">
        <div className="grid items-center gap-14 pb-20 pt-8 sm:pt-10 lg:grid-cols-12 lg:gap-12 lg:pb-28">
          {/* LEFT */}
          <div className="lg:col-span-7">
            <nav aria-label="Breadcrumb" className="text-xs text-muted-foreground">
              <ol className="flex items-center gap-1.5">
                <li>
                  <Link href="/" className="transition-colors hover:text-foreground">
                    Home
                  </Link>
                </li>
                <ChevronRight className="size-3.5" />
                <li aria-current="page" className="text-foreground">
                  About
                </li>
              </ol>
            </nav>

            <p
              className={`${enter} mt-10 flex items-center gap-3 font-mono text-xs font-medium uppercase tracking-[0.16em] text-primary`}
            >
              <span aria-hidden="true" className="h-px w-8 bg-primary/40" />
              About RashePharma
            </p>

            <h1
              className={`${enter} mt-6 text-[40px] font-semibold leading-[1.02] tracking-[-0.045em] text-foreground sm:text-6xl lg:text-[76px]`}
              style={{ animationDelay: "100ms" }}
            >
              Quality healthcare,{" "}
              <span className="text-primary">built on trust.</span>
            </h1>

            <p
              className={`${enter} mt-7 max-w-xl text-lg leading-8 text-muted-foreground`}
              style={{ animationDelay: "200ms" }}
            >
              Rashe Lifesciences Pvt Ltd is an emerging pharmaceutical company
              based in Bangalore, started in {FOUNDED_YEAR} by a team with 15
              years of experience in pharmacy and committed to supplying high
              quality healthcare products with excellent customer service.
            </p>

            <div
              className={`${enter} mt-9 flex flex-col gap-3 sm:flex-row`}
              style={{ animationDelay: "300ms" }}
            >
              <Link
                href="/products"
                className="group inline-flex h-12 items-center justify-center gap-2 rounded-lg bg-primary px-7 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary-dark"
              >
                Explore products
                <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
              </Link>
              <Link
                href="/contact"
                className="inline-flex h-12 items-center justify-center rounded-lg border border-foreground/20 px-7 text-sm font-semibold text-foreground transition-colors hover:border-foreground hover:bg-foreground/5"
              >
                Contact us
              </Link>
            </div>
          </div>

          {/* RIGHT */}
          <figure
            className={`${enter} relative mx-auto w-full max-w-[440px] lg:col-span-5 lg:max-w-none`}
            style={{ animationDelay: "200ms" }}
          >
            <div
              aria-hidden="true"
              className="absolute inset-0 translate-x-3 translate-y-3 rounded-2xl border border-primary/30 sm:translate-x-4 sm:translate-y-4"
            />
            <div className="relative aspect-square overflow-hidden rounded-2xl bg-muted">
              <Image
                src="/images/rashepharma.jpg"
                alt="RashePharma"
                fill
                priority
                sizes="(max-width: 1024px) 90vw, 40vw"
                className="object-cover"
              />
            </div>
            <figcaption className="absolute -bottom-1 left-4 right-4 translate-y-1/2 rounded-xl bg-brand-dark px-5 py-4 text-white shadow-xl sm:left-6 sm:right-auto sm:max-w-[300px]">
              <span className="font-mono text-[10px] uppercase tracking-[0.16em] text-emerald-300">
                RashePharma
              </span>
              <span className="mt-1 block text-base font-semibold">
                Better health. Global reach.
              </span>
            </figcaption>
          </figure>
        </div>

        {/* Fact strip (replaces the old stats section) */}
        <dl className="grid grid-cols-2 border-t border-border lg:grid-cols-4">
          {facts.map((fact, i) => (
            <div
              key={fact.label}
              className={`border-border py-6 sm:py-8 ${factBorders[i]}`}
            >
              <dt className="font-mono text-[11px] uppercase tracking-[0.14em] text-muted-foreground">
                {fact.label}
              </dt>
              <dd className="mt-2 text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">
                {fact.value}
              </dd>
            </div>
          ))}
        </dl>

        {/* Company story */}
        <div className="grid gap-6 border-t border-border py-12 lg:grid-cols-12 lg:gap-12 lg:py-16">
          <h2 className="text-2xl font-semibold tracking-tight text-foreground sm:text-3xl lg:col-span-4">
            Who we are
          </h2>
          <div className="space-y-5 text-base leading-7 text-muted-foreground lg:col-span-8 lg:max-w-2xl">
            {story.map((paragraph, i) => (
              <p key={i}>{paragraph}</p>
            ))}
          </div>
        </div>
      </Container>
    </section>
  );
}
