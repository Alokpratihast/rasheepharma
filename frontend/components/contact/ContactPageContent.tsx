import Link from "next/link";
import {
  ArrowRight,
  ArrowUpRight,
  ChevronRight,
  Clock3,
  Hash,
  Mail,
  MapPin,
  Package,
  Phone,
  Building2,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { EnquiryForm } from "@/components/forms/EnquiryForm";
import { OpenStatus } from "@/components/contact/OpenStatus";
import { siteConfig } from "@/lib/site";

const ADDRESS_LINES = siteConfig.address;

const addressQuery = encodeURIComponent(ADDRESS_LINES.join(" "));

// last address line, e.g. "Bangalore, Karnataka 560091"
const cityLine = ADDRESS_LINES[ADDRESS_LINES.length - 1];

const enter =
  "animate-in fade-in slide-in-from-bottom-6 duration-700 [animation-fill-mode:backwards]";

const cardBase =
  "group rounded-3xl border border-border bg-card p-5 transition-all duration-300 hover:-translate-y-1 hover:border-primary/40 hover:shadow-[0_20px_44px_rgba(8,127,91,0.12)]";

const iconBox =
  "flex size-12 shrink-0 items-center justify-center rounded-2xl bg-primary-light text-primary transition-all duration-300 group-hover:bg-primary group-hover:text-primary-foreground";

const label =
  "text-[11px] font-bold uppercase tracking-[0.14em] text-muted-foreground";

export default function ContactPageContent() {
  return (
    <main className="min-h-screen bg-background">
      {/* ================= BANNER ================= */}
      <section className="relative isolate overflow-hidden bg-brand-dark text-white">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute inset-0 -z-10"
        >
          <div className="absolute -left-32 -top-32 size-[440px] rounded-full bg-primary/45 blur-[100px]" />
          <div className="absolute -bottom-40 right-0 size-[400px] rounded-full bg-sky-600/30 blur-[100px]" />
          <div
            className="absolute inset-0 opacity-60"
            style={{
              backgroundImage:
                "linear-gradient(rgba(255,255,255,.05) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,.05) 1px, transparent 1px)",
              backgroundSize: "56px 56px",
              maskImage:
                "radial-gradient(circle at 50% 30%, #000, transparent 75%)",
              WebkitMaskImage:
                "radial-gradient(circle at 50% 30%, #000, transparent 75%)",
            }}
          />
        </div>

        <Container>
          <div className="pb-24 pt-10 sm:pb-28 sm:pt-12">
            <nav aria-label="Breadcrumb" className="text-xs text-white/60">
              <ol className="flex items-center gap-1.5">
                <li>
                  <Link href="/" className="transition-colors hover:text-white">
                    Home
                  </Link>
                </li>
                <ChevronRight className="size-3.5" />
                <li aria-current="page" className="text-white/90">
                  Contact
                </li>
              </ol>
            </nav>

            <div className="mt-7 flex flex-col gap-8 lg:flex-row lg:items-end lg:justify-between">
              <div className={`${enter} max-w-2xl`}>
                <p className="text-xs font-bold uppercase tracking-[0.18em] text-emerald-300">
                  Contact Us
                </p>
                <h1 className="mt-3 text-4xl font-extrabold leading-[1.05] tracking-[-0.035em] sm:text-5xl lg:text-[58px]">
                  Let&apos;s discuss your{" "}
                  <span className="bg-gradient-to-r from-teal-300 to-emerald-200 bg-clip-text text-transparent">
                    requirement
                  </span>
                </h1>
                <p className="mt-5 text-base leading-8 text-white/70">
                  Whether you are looking for pharmaceutical products,
                  distribution opportunities or business enquiries, our team is
                  ready to assist.
                </p>
              </div>

              {/* Quick actions */}
              <div
                className={`${enter} flex flex-col gap-3 sm:flex-row`}
                style={{ animationDelay: "150ms" }}
              >
                <a
                  href={siteConfig.phoneHref}
                  className="group inline-flex h-12 items-center justify-center gap-2.5 rounded-full bg-white px-7 font-bold text-brand-dark shadow-[0_14px_34px_rgba(0,0,0,0.3)] transition-all hover:-translate-y-0.5 hover:bg-emerald-50"
                >
                  <Phone className="size-4" />
                  Call us
                </a>
                <a
                  href={siteConfig.emailHref}
                  className="inline-flex h-12 items-center justify-center gap-2.5 rounded-full border border-white/35 px-7 font-bold text-white transition-all hover:-translate-y-0.5 hover:border-white hover:bg-white/10"
                >
                  <Mail className="size-4" />
                  Email us
                </a>
              </div>
            </div>
          </div>
        </Container>

        <svg
          aria-hidden="true"
          viewBox="0 0 1440 60"
          preserveAspectRatio="none"
          className="absolute inset-x-0 -bottom-px h-8 w-full fill-background sm:h-12"
        >
          <path d="M0,30 C240,70 480,0 720,24 C960,48 1200,70 1440,26 L1440,60 L0,60 Z" />
        </svg>
      </section>

      {/* ================= INFO + FORM ================= */}
      <section className="pb-12 pt-4 sm:pb-16 sm:pt-6">
        <Container>
          <div className="grid gap-10 lg:grid-cols-[0.82fr_1.18fr] lg:gap-12">
            {/* -------- LEFT: CONTACT INFO -------- */}
            <div
              className={`${enter}`}
              style={{ animationDelay: "200ms" }}
            >
              <p className="text-xs font-bold uppercase tracking-[0.18em] text-primary">
                Get in touch
              </p>
              <h2 className="mt-3 text-2xl font-extrabold tracking-[-0.02em] text-foreground sm:text-3xl">
                We&apos;d love to hear from you
              </h2>
              <p className="mt-3 text-sm leading-7 text-muted-foreground">
                Send us your requirement and our team will get back to you with
                the relevant information.
              </p>

              <div className="mt-7 space-y-3.5">
                {/* Address */}
                <article className={cardBase}>
                  <div className="flex gap-4">
                    <span className={iconBox}>
                      <MapPin className="size-5" />
                    </span>
                    <div className="min-w-0 flex-1">
                      <p className={label}>{siteConfig.addressLabel}</p>
                      <address className="mt-1.5 text-sm not-italic leading-6 text-foreground">
                        {ADDRESS_LINES.map((line) => (
                          <span key={line} className="block">
                            {line}
                          </span>
                        ))}
                      </address>
                      <a
                        href={`https://www.google.com/maps/search/?api=1&query=${addressQuery}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="mt-3 inline-flex items-center gap-1.5 text-sm font-semibold text-primary hover:underline"
                      >
                        Get directions
                        <ArrowUpRight className="size-4" />
                      </a>
                    </div>
                  </div>
                </article>

                {/* Email */}
                <article className={cardBase}>
                  <div className="flex gap-4">
                    <span className={iconBox}>
                      <Mail className="size-5" />
                    </span>
                    <div className="min-w-0">
                      <p className={label}>Email</p>
                      <a
                        href={siteConfig.emailHref}
                        className="mt-1.5 block break-all text-sm font-semibold text-foreground transition-colors hover:text-primary"
                      >
                        {siteConfig.email}
                      </a>
                    </div>
                  </div>
                </article>

                {/* Phone */}
                <article className={cardBase}>
                  <div className="flex gap-4">
                    <span className={iconBox}>
                      <Phone className="size-5" />
                    </span>
                    <div className="min-w-0">
                      <p className={label}>Phone</p>
                      <a
                        href={siteConfig.phoneHref}
                        className="mt-1.5 block text-sm font-semibold text-foreground transition-colors hover:text-primary"
                      >
                        {siteConfig.phone}
                      </a>
                    </div>
                  </div>
                </article>

                {/* Hours */}
                <article className={cardBase}>
                  <div className="flex gap-4">
                    <span className={iconBox}>
                      <Clock3 className="size-5" />
                    </span>
                    <div className="min-w-0 flex-1">
                      <div className="flex flex-wrap items-center justify-between gap-2">
                        <p className={label}>Business Hours</p>
                        <OpenStatus />
                      </div>
                      <p className="mt-1.5 text-sm leading-6 text-foreground">
                        Monday – Saturday
                        <br />
                        9:30 AM – 6:30 PM
                      </p>
                    </div>
                  </div>
                </article>
              </div>

              {/* What to include */}
              <div className="mt-6 rounded-3xl border border-primary/20 bg-primary-light p-6">
                <p className="text-sm font-bold text-foreground">
                  Looking for a specific product?
                </p>
                <p className="mt-1.5 text-xs leading-5 text-muted-foreground">
                  Mention these details in the enquiry form so we can respond
                  faster:
                </p>

                <ul className="mt-4 grid gap-2.5 sm:grid-cols-3 lg:grid-cols-1 xl:grid-cols-3">
                  {[
                    { Icon: Package, text: "Product name" },
                    { Icon: Hash, text: "Required quantity" },
                    { Icon: Building2, text: "Business details" },
                  ].map(({ Icon, text }) => (
                    <li
                      key={text}
                      className="flex items-center gap-2.5 rounded-xl bg-background/80 px-3 py-2.5 text-xs font-semibold text-foreground"
                    >
                      <Icon className="size-4 shrink-0 text-primary" />
                      {text}
                    </li>
                  ))}
                </ul>

                <Link
                  href="/products"
                  className="mt-5 inline-flex items-center gap-1.5 text-sm font-semibold text-primary hover:underline"
                >
                  Browse our products
                  <ArrowRight className="size-4" />
                </Link>
              </div>
            </div>

            {/* -------- RIGHT: ENQUIRY FORM -------- */}
            <div
              className={`${enter} relative`}
              style={{ animationDelay: "300ms" }}
            >
              <div
                aria-hidden="true"
                className="pointer-events-none absolute -inset-4 -z-10 rounded-[40px] bg-gradient-to-br from-primary/10 via-transparent to-sky-500/10 blur-2xl"
              />
              <EnquiryForm />
            </div>
          </div>
        </Container>
      </section>

      {/* ================= MAP ================= */}
      <section className="pb-16 sm:pb-24">
        <Container>
          <div className="overflow-hidden rounded-[32px] border border-border bg-card shadow-[0_20px_60px_rgba(7,63,50,0.08)]">
            <div className="flex flex-col gap-3 border-b border-border p-6 sm:flex-row sm:items-center sm:justify-between">
              <div className="flex items-center gap-4">
                <span className="flex size-12 items-center justify-center rounded-2xl bg-primary-light text-primary">
                  <MapPin className="size-5" />
                </span>
                <div>
                  <p className="font-bold text-foreground">Find us on the map</p>
                  <p className="text-xs text-muted-foreground">{cityLine}</p>
                </div>
              </div>
              <a
                href={`https://www.google.com/maps/search/?api=1&query=${addressQuery}`}
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex h-11 items-center justify-center gap-2 rounded-full bg-gradient-to-r from-primary to-teal-600 px-6 text-sm font-semibold text-white shadow-[0_8px_20px_rgba(8,127,91,0.3)] transition-all hover:-translate-y-0.5"
              >
                Open in Google Maps
                <ArrowUpRight className="size-4" />
              </a>
            </div>

            <iframe
              title={`${siteConfig.name} head office location`}
              src={`https://www.google.com/maps?q=${addressQuery}&output=embed`}
              loading="lazy"
              referrerPolicy="no-referrer-when-downgrade"
              className="h-[340px] w-full border-0 bg-muted sm:h-[420px]"
            />
          </div>
        </Container>
      </section>
    </main>
  );
}
