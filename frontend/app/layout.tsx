import type { Metadata } from "next";
import { Sora, DM_Sans } from "next/font/google";
import "./globals.css";
import { Providers } from "@/lib/providers";
import { GlobalEnforcement } from "./components/GlobalEnforcement";
import { GlobalNavbar } from "./components/GlobalNavbar";

const sora = Sora({
  variable: "--font-display",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

const dmSans = DM_Sans({
  variable: "--font-body",
  subsets: ["latin"],
  weight: ["400", "500"],
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
      <body className={`${sora.variable} ${dmSans.variable} bg-[#FAFAFA] antialiased`}>
        <Providers>
          <GlobalEnforcement />
          <GlobalNavbar />
          {children}
        </Providers>
      </body>
    </html>
  );
}
