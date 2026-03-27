'use client';

import React from 'react';
import { ArrowRight } from 'lucide-react';
import Link from 'next/link';

export const CTA: React.FC = () => {
  return (
    <section className="py-20 px-6">
      <div className="container mx-auto">
        <div className="relative rounded-[2.5rem] bg-gradient-to-r from-zinc-800 to-zinc-900 overflow-hidden px-8 py-16 md:px-20 md:py-24 text-center md:text-left flex flex-col md:flex-row items-center justify-between">
          
          {/* Content */}
          <div className="relative z-10 max-w-2xl mb-8 md:mb-0">
            <h2 className="font-heading text-4xl md:text-5xl font-bold text-white mb-6">
              Join the Cashless Revolution.
            </h2>
            <p className="text-zinc-300 text-lg md:text-xl leading-relaxed opacity-90">
              Stop paying for skills you can earn. Start building your portfolio and your network today.
            </p>
          </div>

          {/* Button */}
          <div className="relative z-10">
            <Link href="/register">
              <button className="group bg-white text-zinc-900 font-bold text-lg px-8 py-4 rounded-full transition-all hover:shadow-[0_0_40px_rgba(255,255,255,0.3)] hover:scale-105 flex items-center gap-3">
                Get Started for Free
                <ArrowRight className="group-hover:translate-x-1 transition-transform" />
              </button>
            </Link>
          </div>

          {/* Abstract Shapes */}
          <div className="absolute top-0 right-0 -translate-y-1/2 translate-x-1/3 w-96 h-96 bg-[#1D9E75]/20 rounded-full blur-3xl" />
          <div className="absolute bottom-0 left-0 translate-y-1/3 -translate-x-1/3 w-96 h-96 bg-orange-500/20 rounded-full blur-3xl" />
          
          {/* Pattern Overlay */}
          <div className="absolute inset-0 opacity-10" style={{ backgroundImage: 'radial-gradient(circle at 2px 2px, white 1px, transparent 0)', backgroundSize: '40px 40px' }}></div>
        </div>
      </div>
    </section>
  );
};
