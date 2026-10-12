import Image from "next/image";
import Link from "next/link";
import { ArrowRight, ChevronRight, ShieldCheck } from "lucide-react";

import { Container } from "@/components/ui/container";

// float animations (shared with the home hero)
import "@/components/home/hero.css";

const enter =
  "animate-in fade-in slide-in-from-bottom-6 duration-700 [animation-fill-mode:backwards]";

export function AboutHero() {
  return (
    <section className="relative isolate overflow-hidden bg-brand-dark text-white">
      {/* Background */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 -z-10"
      >
        <div className="absolute -left-40 -top-40 size-[520px] rounded-full bg-primary/50 blur-[110px]" />
        <div className="absolute -bottom-48 -right-32 size-[480px] rounded-full bg-sky-600/35 blur-[110px]" />
        <div
          className="absolute inset-0 opacity-60"
          style={{
            backgroundImage:
              "linear-gradient(rgba(255,255,255,.05) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,.05) 1px, transparent 1px)",
            backgroundSize: "56px 56px",
            maskImage:
              "radial-gradient(circle at 50% 35%, #000, transparent 72%)",
            WebkitMaskImage:
              "radial-gradient(circle at 50% 35%, #000, transparent 72%)",
          }}
        />
      </div>

      <Container>
        <div className="grid items-center gap-14 pb-32 pt-10 sm:pt-12 lg:grid-cols-[1.05fr_0.95fr] lg:gap-16 lg:pb-36">
          {/* LEFT */}
          <div className="max-w-[620px]">
            <nav aria-label="Breadcrumb" className="text-xs text-white/60">
              <ol className="flex items-center gap-1.5">
                <li>
                  <Link href="/" className="transition-colors hover:text-white">
                    Home
                  </Link>
                </li>
                <ChevronRight className="size-3.5" />
                <li aria-current="page" className="text-white/90">
                  About
                </li>
              </ol>
            </nav>

            <span
              className={`${enter} mt-7 inline-flex items-center gap-2.5 rounded-full border border-white/15 bg-white/10 px-4 py-1.5 text-xs font-medium text-white/90 backdrop-blur`}
            >
              <span className="size-2 rounded-full bg-emerald-400" />
              About RashePharma
            </span>

            <h1
              className={`${enter} mt-6 text-4xl font-extrabold leading-[1.05] tracking-[-0.035em] sm:text-5xl lg:text-[60px]`}
              style={{ animationDelay: "100ms" }}
            >
              Building trust through
              <span className="block bg-gradient-to-r from-teal-300 to-emerald-200 bg-clip-text text-transparent">
                quality healthcare.
              </span>
            </h1>

            <p
              className={`${enter} mt-6 text-base leading-8 text-white/75`}
              style={{ animationDelay: "200ms" }}
            >
              RashePharma is focused on delivering quality pharmaceutical
              products and dependable healthcare solutions for customers,
              distributors and business partners.
            </p>
            <p
              className={`${enter} mt-4 text-sm leading-7 text-white/60`}
              style={{ animationDelay: "280ms" }}
            >
              Our approach combines product reliability, customer focus and
              long-term business relationships to support healthcare
              requirements across markets.
            </p>

            <div
              className={`${enter} mt-9 flex flex-col gap-3 sm:flex-row`}
              style={{ animationDelay: "360ms" }}
            >
              <Link
                href="/products"
                className="group inline-flex h-12 items-center justify-center gap-2 rounded-full bg-white px-8 font-bold text-brand-dark shadow-[0_14px_34px_rgba(0,0,0,0.3)] transition-all hover:-translate-y-0.5 hover:bg-emerald-50"
              >
                Explore Products
                <ArrowRight className="size-4 transition-transform group-hover:translate-x-0.5" />
              </Link>
              <Link
                href="/contact"
                className="inline-flex h-12 items-center justify-center rounded-full border border-white/35 px-8 font-bold text-white transition-all hover:-translate-y-0.5 hover:border-white hover:bg-white/10"
              >
                Contact Us
              </Link>
            </div>
          </div>

          {/* RIGHT: IMAGE */}
          <div
            className={`${enter} relative mx-auto w-full max-w-[500px] [animation-duration:900ms]`}
            style={{ animationDelay: "250ms" }}
          >
            <div
              aria-hidden="true"
              className="absolute inset-6 rounded-full bg-teal-400/30 blur-3xl"
            />

            <div className="relative overflow-hidden rounded-[40px] border border-white/20 shadow-[0_30px_80px_rgba(0,0,0,0.45)]">
              <div className="relative aspect-square">
                <Image
                  src="/images/rashepharma.jpg"
                  alt="RashePharma"
                  fill
                  priority
                  sizes="(max-width: 1024px) 90vw, 500px"
                  className="object-cover"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-brand-dark/80 via-transparent to-transparent" />

                <div className="absolute inset-x-0 bottom-0 p-5 sm:p-6">
                  <div className="rounded-2xl border border-white/15 bg-white/10 p-4 backdrop-blur-xl">
                    <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-emerald-300">
                      RashePharma
                    </p>
                    <p className="mt-1 text-base font-bold">
                      Better Health. Global Reach.
                    </p>
                    <p className="mt-1 text-xs leading-5 text-white/70">
                      Quality-focused pharmaceutical solutions built around
                      trust and reliability.
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Floating badges */}
            <div className="hero-float absolute -left-3 top-10 hidden items-center gap-3 rounded-2xl border border-white/20 bg-white/10 px-4 py-3 shadow-[0_18px_40px_rgba(0,0,0,0.3)] backdrop-blur-xl sm:flex">
              <span className="flex size-10 items-center justify-center rounded-xl bg-emerald-400/20">
                <ShieldCheck className="size-5 text-emerald-300" />
              </span>
              <span>
                <span className="block text-[10px] uppercase tracking-wider text-white/60">
                  Our focus
                </span>
                <span className="block text-sm font-bold">Quality &amp; Trust</span>
              </span>
            </div>

            <div className="hero-float-b absolute -right-3 -top-4 flex size-24 flex-col items-center justify-center rounded-full border border-white/25 bg-gradient-to-br from-emerald-400 to-teal-500 text-brand-dark shadow-[0_18px_40px_rgba(0,0,0,0.35)]">
              <span className="text-[10px] font-bold uppercase tracking-wider">
                Since
              </span>
              <span className="text-2xl font-extrabold leading-none">2019</span>
            </div>
          </div>
        </div>
      </Container>

      <svg
        aria-hidden="true"
        viewBox="0 0 1440 80"
        preserveAspectRatio="none"
        className="absolute inset-x-0 -bottom-px h-10 w-full fill-background sm:h-16"
      >
        <path d="M0,42 C240,92 480,2 720,32 C960,62 1200,92 1440,36 L1440,80 L0,80 Z" />
      </svg>
    </section>
  );
}
