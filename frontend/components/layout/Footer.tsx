import Link from "next/link";
import {
  ArrowUpRight,
  Mail,
  MapPin,
  Phone,
} from "lucide-react";

import { Container } from "@/components/ui/container";

const footerLinks = {
  Explore: [
    {
      label: "Products",
      href: "/products",
    },
    {
      label: "Categories",
      href: "/categories",
    },
    {
      label: "About Us",
      href: "/about",
    },
  ],
  Business: [
    {
      label: "B2B Enquiry",
      href: "/b2b/enquiries",
    },
    {
      label: "Become a Partner",
      href: "/b2b/partner",
    },
    {
      label: "Contact Us",
      href: "/contact",
    },
  ],
  Company: [
    {
      label: "Privacy Policy",
      href: "/privacy",
    },
    {
      label: "Terms & Conditions",
      href: "/terms",
    },
  ],
};

export function Footer() {
  return (
    <footer className="bg-[#1B2A4A] text-white">
      <Container>
        <div className="grid gap-10 py-12 sm:py-14 lg:grid-cols-[1.5fr_1fr_1fr_1fr]">
          {/* =================================================
              COMPANY
          ================================================== */}

          <div className="max-w-md">
            <Link
              href="/"
              className="inline-flex items-center gap-3"
            >
              <div className="flex size-11 items-center justify-center rounded-lg bg-[#3E8F96] text-xl font-bold text-white">
                R
              </div>

              <div>
                <div className="text-xl font-bold tracking-tight">
                  Rashe<span className="text-[#5DCAA5]">Pharma</span>
                </div>

                <p className="mt-0.5 text-[9px] font-medium uppercase tracking-[0.16em] text-white/50">
                  Healthcare Solutions
                </p>
              </div>
            </Link>

            <p className="mt-5 max-w-sm text-sm leading-6 text-white/60">
              Delivering quality pharmaceutical products and building
              trusted healthcare partnerships across global markets.
            </p>

            {/* Contact details */}
            <div className="mt-6 space-y-3">
              <div className="flex items-start gap-3">
                <MapPin className="mt-0.5 size-4 shrink-0 text-[#5DCAA5]" />

                <p className="text-sm leading-6 text-white/70">
                  Head Office
                  <br />
                  15th Main Rd, 3rd Stage, 4th Block,
                  <br />
                  Sahakar Nagar, Byatarayanapura,
                  <br />
                  Bengaluru, Karnataka 560092
                </p>
              </div>

              <a
                href="mailto:info@rasheepharma.com"
                className="flex items-center gap-3 text-sm text-white/70 transition-colors hover:text-white"
              >
                <Mail className="size-4 shrink-0 text-[#5DCAA5]" />
                info@rasheepharma.com
              </a>

              <a
                href="tel:+918000000000"
                className="flex items-center gap-3 text-sm text-white/70 transition-colors hover:text-white"
              >
                <Phone className="size-4 shrink-0 text-[#5DCAA5]" />
                Contact our team
              </a>
            </div>
          </div>

          {/* =================================================
              FOOTER LINKS
          ================================================== */}

          <FooterColumn
            title="Explore"
            links={footerLinks.Explore}
          />

          <FooterColumn
            title="Business"
            links={footerLinks.Business}
          />

          <FooterColumn
            title="Company"
            links={footerLinks.Company}
          />
        </div>

        {/* ===================================================
            BOTTOM BAR
        ==================================================== */}

        <div className="flex flex-col gap-4 border-t border-white/10 py-5 text-xs text-white/45 sm:flex-row sm:items-center sm:justify-between">
          <p>
            © {new Date().getFullYear()} RashePharma. All rights
            reserved.
          </p>

          <div className="flex items-center gap-5">
            <Link
              href="/contact"
              className="transition-colors hover:text-white"
            >
              Contact
            </Link>

            <Link
              href="/b2b/enquiries"
              className="inline-flex items-center gap-1 transition-colors hover:text-white"
            >
              Request a Quote
              <ArrowUpRight className="size-3" />
            </Link>
          </div>
        </div>
      </Container>
    </footer>
  );
}

interface FooterColumnProps {
  title: string;
  links: {
    label: string;
    href: string;
  }[];
}

function FooterColumn({
  title,
  links,
}: FooterColumnProps) {
  return (
    <div>
      <h3 className="text-sm font-semibold text-white">
        {title}
      </h3>

      <nav className="mt-4 flex flex-col gap-3">
        {links.map((link) => (
          <Link
            key={link.href}
            href={link.href}
            className="text-sm text-white/55 transition-colors hover:text-[#5DCAA5]"
          >
            {link.label}
          </Link>
        ))}
      </nav>
    </div>
  );
}