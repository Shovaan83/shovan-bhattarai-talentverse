import { ReactNode } from "react";
import Link from "next/link";

interface AuthLayoutProps {
  children: ReactNode;
  title?: string;
  subtitle?: string;
}

export default function AuthLayout({ children, title, subtitle }: AuthLayoutProps) {
  return (
    <div className="min-h-screen bg-zinc-900 flex items-center justify-center p-4 sm:p-6 lg:p-8 font-body">
      {/* Card Container */}
      <div className="w-full max-w-[1200px] grid lg:grid-cols-2 bg-white rounded-3xl overflow-hidden shadow-2xl h-[85vh] min-h-[600px]">
        
        {/* Left Side - Hero Image & Branding */}
        <div className="hidden lg:flex relative bg-zinc-900 flex-col justify-between p-12 text-white overflow-hidden">
          {/* Background Image with Overlay */}
          <div className="absolute inset-0 z-0">
            <img
              src="https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=800&q=80"
              alt="Collaboration"
              className="w-full h-full object-cover opacity-40 mix-blend-overlay"
            />
            <div className="absolute inset-0 bg-gradient-to-t from-zinc-900/90 to-zinc-800/45" />
          </div>

          {/* Content */}
          <div className="relative z-10">
            <Link href="/" className="flex items-center gap-2 w-fit group">
              <div className="bg-brand-teal-500/20 p-2 rounded-lg backdrop-blur-sm border border-brand-teal-500/40 group-hover:bg-brand-teal-500/30 transition-colors">
              </div>
              <span className="font-display font-bold text-2xl tracking-tight text-white">
                TalentVerse
              </span>
            </Link>
          </div>

          <div className="relative z-10 space-y-6 max-w-md">
            <h2 className="font-display text-4xl font-bold leading-tight">
              {title || "Master your craft. Trade your skills."}
            </h2>
            <p className="text-zinc-300 text-lg leading-relaxed">
              {subtitle || "Join thousands of professionals swapping knowledge and growing together in the new skill economy."}
            </p>
          </div>

          <div className="relative z-10 flex gap-2">
            <div className="h-1 w-12 bg-brand-teal-500 rounded-full" />
            <div className="h-1 w-2 bg-zinc-600 rounded-full" />
            <div className="h-1 w-2 bg-zinc-600 rounded-full" />
          </div>
        </div>

        {/* Right Side - Form Content */}
        <div className="h-full p-8 lg:p-12 flex flex-col justify-center relative bg-[#FAFAFA]">
           <div className="w-full max-w-md mx-auto">
            {children}
           </div>
        </div>
      </div>
    </div>
  );
}
