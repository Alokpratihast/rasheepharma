import {
  Mail,
  MapPin,
  Phone,
  Clock3,
} from "lucide-react";

import { Container } from "@/components/ui/container";
import { EnquiryForm } from "@/components/forms/EnquiryForm";

export default function ContactPage() {
  return (
    <main className="min-h-screen bg-[#fafafa]">
      {/* =================================================
          HERO
      ================================================== */}

      <section className="border-b border-border bg-white">
        <Container>
          <div className="py-12 sm:py-16">
            <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#F5821F]">
              Contact Us
            </p>

            <h1 className="mt-2 max-w-3xl text-3xl font-semibold tracking-tight text-[#1B2A4A] sm:text-4xl lg:text-5xl">
              Let&apos;s discuss your requirement
            </h1>

            <p className="mt-4 max-w-2xl text-sm leading-7 text-[#595959] sm:text-base">
              Whether you are looking for pharmaceutical products,
              distribution opportunities or business enquiries, our team is
              ready to assist.
            </p>
          </div>
        </Container>
      </section>

      {/* =================================================
          CONTACT + FORM
      ================================================== */}

      <section className="py-10 sm:py-14">
        <Container>
          <div className="grid gap-8 lg:grid-cols-[0.8fr_1.2fr]">
            {/* =================================================
                CONTACT INFORMATION
            ================================================== */}

            <div>
              <div>
                <p className="text-sm font-semibold text-[#1B2A4A]">
                  Get in touch
                </p>

                <p className="mt-2 text-sm leading-6 text-[#595959]">
                  Send us your requirement and our team will get back to you
                  with the relevant information.
                </p>
              </div>

              <div className="mt-7 space-y-3">
                {/* Address */}
                <div className="rounded-xl border border-border bg-white p-4">
                  <div className="flex gap-3">
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-[#EAF5F3]">
                      <MapPin className="size-5 text-[#3E8F96]" />
                    </div>

                    <div>
                      <p className="text-xs font-semibold uppercase tracking-wide text-[#888]">
                        Head Office
                      </p>

                      <p className="mt-1 text-sm leading-6 text-[#1B2A4A]">
                        15th Main Rd, 3rd Stage, 4th Block,
                        <br />
                        Sahakar Nagar, Byatarayanapura,
                        <br />
                        Bengaluru, Karnataka 560092
                      </p>
                    </div>
                  </div>
                </div>

                {/* Email */}
                <div className="rounded-xl border border-border bg-white p-4">
                  <div className="flex gap-3">
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-[#FFF3E9]">
                      <Mail className="size-5 text-[#F5821F]" />
                    </div>

                    <div>
                      <p className="text-xs font-semibold uppercase tracking-wide text-[#888]">
                        Email
                      </p>

                      <a
                        href="mailto:info@rasheepharma.com"
                        className="mt-1 block text-sm text-[#1B2A4A] hover:text-[#3E8F96]"
                      >
                        info@rasheepharma.com
                      </a>
                    </div>
                  </div>
                </div>

                {/* Phone */}
                <div className="rounded-xl border border-border bg-white p-4">
                  <div className="flex gap-3">
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-[#EAF5F3]">
                      <Phone className="size-5 text-[#3E8F96]" />
                    </div>

                    <div>
                      <p className="text-xs font-semibold uppercase tracking-wide text-[#888]">
                        Phone
                      </p>

                      <a
                        href="tel:+919876543210"
                        className="mt-1 block text-sm text-[#1B2A4A] hover:text-[#3E8F96]"
                      >
                        +91 98765 43210
                      </a>
                    </div>
                  </div>
                </div>

                {/* Business Hours */}
                <div className="rounded-xl border border-border bg-white p-4">
                  <div className="flex gap-3">
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-[#F2F2F2]">
                      <Clock3 className="size-5 text-[#1B2A4A]" />
                    </div>

                    <div>
                      <p className="text-xs font-semibold uppercase tracking-wide text-[#888]">
                        Business Hours
                      </p>

                      <p className="mt-1 text-sm leading-6 text-[#1B2A4A]">
                        Monday – Saturday
                        <br />
                        9:30 AM – 6:30 PM
                      </p>
                    </div>
                  </div>
                </div>
              </div>

              {/* Quick note */}
              <div className="mt-6 rounded-xl border border-[#d9ebe8] bg-[#F2F9F7] p-5">
                <p className="text-sm font-semibold text-[#1B2A4A]">
                  Looking for a specific product?
                </p>

                <p className="mt-1.5 text-xs leading-5 text-[#595959]">
                  Mention the product name, required quantity and your
                  business details in the enquiry form.
                </p>
              </div>
            </div>

            {/* =================================================
                ENQUIRY FORM
            ================================================== */}

            <div>
              <EnquiryForm />
            </div>
          </div>
        </Container>
      </section>
    </main>
  );
}