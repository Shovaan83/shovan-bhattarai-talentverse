import type { Metadata } from "next";
import { Sora, DM_Sans } from "next/font/google";
import Script from "next/script";
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

const stripInjectedAttrsScript = `(function () {
  var shouldRemove = function (attrName) {
    if (!attrName) return false;
    if (attrName === "bis_skin_checked" || attrName === "bis_register") return true;
    return /^__processed_[a-f0-9-]+__$/.test(attrName);
  };

  var cleanNode = function (node) {
    if (!node || node.nodeType !== 1 || !node.attributes) return;
    for (var i = node.attributes.length - 1; i >= 0; i--) {
      var attrName = node.attributes[i].name;
      if (shouldRemove(attrName)) {
        node.removeAttribute(attrName);
      }
    }
  };

  var cleanTree = function () {
    cleanNode(document.documentElement);
    cleanNode(document.body);
    var nodes = document.getElementsByTagName("*");
    for (var i = 0; i < nodes.length; i++) {
      cleanNode(nodes[i]);
    }
  };

  cleanTree();
})();`;

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body
        suppressHydrationWarning
        className={`${sora.variable} ${dmSans.variable} bg-[#FAFAFA] antialiased`}
      >
        <Script id="strip-extension-attrs" strategy="beforeInteractive">
          {stripInjectedAttrsScript}
        </Script>
        <Providers>
          <GlobalEnforcement />
          <GlobalNavbar />
          {children}
        </Providers>
      </body>
    </html>
  );
}
