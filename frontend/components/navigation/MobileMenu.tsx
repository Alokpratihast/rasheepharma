"use client";

import Link from "next/link";
import { ChevronRight } from "lucide-react";

import { CategoriesMegaMenu } from "@/components/navigation/CategoriesMegaMenu";
import { Button } from "@/components/ui/button";

interface MobileMenuProps {
  onClose: () => void;
  onEnquire: () => void;
}

const navigation = [
  {
    label: "About",
    href: "/about",
  },
  {
    label: "Contact",
    href: "/contact",
  },
];

export function MobileMenu({
  onClose,
  onEnquire,
}: MobileMenuProps) {
  return (
    <div className="border-t border-[#edf0ef] bg-white md:hidden">
      <nav
        aria-label="Mobile navigation"
        className="px-3 py-3"
      >
        {/* Categories */}
        <div className="rounded-xl">
          <CategoriesMegaMenu
            mobile
            onNavigate={onClose}
          />
        </div>

        {/* Main navigation */}
        <div className="mt-1 space-y-0.5">
          {navigation.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              onClick={onClose}
              className="group flex items-center justify-between rounded-xl px-3 py-3 text-sm font-medium text-[#1B2A4A] transition-all duration-200 hover:bg-[#F4F7F6] hover:text-primary"
            >
              <span>{item.label}</span>

              <ChevronRight className="size-4 text-[#8A9694] transition-transform duration-200 group-hover:translate-x-0.5 group-hover:text-primary" />
            </Link>
          ))}

          {/* Sign In */}
          <Link
            href="/login"
            onClick={onClose}
            className="group flex items-center justify-between rounded-xl px-3 py-3 text-sm font-medium text-[#1B2A4A] transition-all duration-200 hover:bg-[#F4F7F6] hover:text-primary"
          >
            <span>Sign In</span>

            <ChevronRight className="size-4 text-[#8A9694] transition-transform duration-200 group-hover:translate-x-0.5 group-hover:text-primary" />
          </Link>
        </div>

        {/* Enquire CTA */}
        <Button
          type="button"
          onClick={() => {
            onClose();
            onEnquire();
          }}
          className="mt-3 h-11 w-full rounded-xl bg-[#F5821F] font-semibold text-white shadow-[0_6px_16px_rgba(245,130,31,0.20)] transition-all duration-200 hover:-translate-y-0.5 hover:bg-[#df7115] hover:shadow-[0_9px_20px_rgba(245,130,31,0.25)] active:translate-y-0"
        >
          Enquiry
        </Button>
      </nav>
    </div>
  );
}