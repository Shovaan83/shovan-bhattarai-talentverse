import type { Metadata } from "next";
import { Poppins, Inter } from "next/font/google";
import "./globals.css";
import { Providers } from "@/lib/providers";
import { GlobalEnforcement } from "./components/GlobalEnforcement";
import { GlobalNavbar } from "./components/GlobalNavbar";

const poppins = Poppins({
  variable: "--font-heading",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

const inter = Inter({
  variable: "--font-sans",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "TalentVerse",
  description: "Connect, Collaborate, and Showcase Your Talents",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={`${poppins.variable} ${inter.variable} antialiased`}
      >
        <Providers>
          <GlobalEnforcement />
          <GlobalNavbar />
          {children}
        </Providers>
      </body>
    </html>
  );
}
