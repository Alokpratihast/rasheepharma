import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { Suspense } from "react";

import "./globals.css";

import { Header } from "@/components/layout/Header";
import { Footer } from "@/components/layout/Footer";
import { AuthProvider } from "@/components/providers/AuthProvider";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "RashePharma | Quality Healthcare, Built for Trust",
  description:
    "RashePharma provides quality pharmaceutical products and reliable healthcare solutions for customers, distributors and business partners.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} antialiased`}
    >
      <body>
        <AuthProvider>
          <Suspense fallback={null}>
            <Header />
          </Suspense>

          {children}

          <Footer />
        </AuthProvider>
      </body>
    </html>
  );
}