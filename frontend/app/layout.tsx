import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";

import "./globals.css";

import { Header } from "@/components/layout/Header";
import { Footer } from "@/components/layout/Footer";
import { categoryService } from "@/services/category.service";
import type { Category } from "@/types/category";
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

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  let categories: Category[] = [];

  try {
    categories = await categoryService.getAll();
  } catch {
    categories = [];
  }

  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} antialiased`}
    >
      <body>
        <AuthProvider>
          <Header categories={categories} />
          {children}
          <Footer />
        </AuthProvider>
      </body>
    </html>
  );
}