'use client';

import React, { useState, useEffect } from 'react';
import { motion, useScroll, useMotionValueEvent } from 'framer-motion';
import { Sparkles, Menu, X } from 'lucide-react';
import Link from 'next/link';

export const Navbar: React.FC = () => {
  const [isScrolled, setIsScrolled] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const { scrollY } = useScroll();

  useMotionValueEvent(scrollY, "change", (latest) => {
    setIsScrolled(latest > 20);
  });

  return (
    <motion.nav
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
        isScrolled || mobileMenuOpen
          ? 'bg-white/80 backdrop-blur-md shadow-sm border-b border-gray-100 py-3'
          : 'bg-transparent py-5'
      }`}
      initial={{ y: -100 }}
      animate={{ y: 0 }}
      transition={{ duration: 0.5 }}
    >
      <div className="container mx-auto px-6 flex items-center justify-between">
        {/* Logo */}
        <div className="flex items-center gap-2 cursor-pointer group">
          <div className="bg-emerald-600 p-1.5 rounded-lg text-white group-hover:rotate-12 transition-transform duration-300">
            {/* <Sparkles size={20} /> */}
          </div>
          <span className={`font-heading font-bold text-xl tracking-tight ${isScrolled || mobileMenuOpen ? 'text-emerald-950' : 'text-white'}`}>
            TalentVerse
          </span>
        </div>

        {/* Desktop Links */}
        <div className="hidden md:flex items-center space-x-8">
          {['Browse', 'How it Works', 'Community'].map((item) => (
            <a
              key={item}
              href={`#${item.toLowerCase().replace(/\s/g, '-')}`}
              className={`text-sm font-medium hover:text-orange-500 transition-colors ${
                isScrolled ? 'text-gray-600' : 'text-emerald-50'
              }`}
            >
              {item}
            </a>
          ))}
        </div>

        {/* Auth Buttons */}
        <div className="hidden md:flex items-center space-x-4">
          <Link href="/login">
            <button className={`text-sm font-medium px-4 py-2 rounded-full transition-colors ${
               isScrolled ? 'text-emerald-900 hover:bg-emerald-50' : 'text-white hover:bg-white/10'
            }`}>
              Log In
            </button>
          </Link>
          <Link href="/register">
            <button className="bg-orange-600 hover:bg-orange-700 text-white text-sm font-semibold px-5 py-2.5 rounded-full transition-transform hover:scale-105 shadow-lg shadow-orange-600/20">
              Join Now
            </button>
          </Link>
        </div>

        {/* Mobile Toggle */}
        <div className="md:hidden">
          <button 
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className={`${isScrolled || mobileMenuOpen ? 'text-gray-800' : 'text-white'}`}
          >
            {mobileMenuOpen ? <X size={24} /> : <Menu size={24} />}
          </button>
        </div>
      </div>

      {/* Mobile Menu */}
      {mobileMenuOpen && (
        <motion.div 
          initial={{ opacity: 0, height: 0 }}
          animate={{ opacity: 1, height: 'auto' }}
          className="md:hidden bg-white border-t border-gray-100 absolute w-full"
        >
          <div className="flex flex-col p-6 space-y-4">
            {['Browse', 'How it Works', 'Community'].map((item) => (
              <a
                key={item}
                href="#"
                className="text-gray-800 font-medium text-lg"
                onClick={() => setMobileMenuOpen(false)}
              >
                {item}
              </a>
            ))}
            <div className="pt-4 flex flex-col space-y-3">
              <Link href="/login">
                <button className="w-full text-emerald-900 font-medium py-3 rounded-xl bg-gray-50">
                  Log In
                </button>
              </Link>
              <Link href="/register">
                <button className="w-full bg-orange-600 text-white font-bold py-3 rounded-xl">
                  Join Now
                </button>
              </Link>
            </div>
          </div>
        </motion.div>
      )}
    </motion.nav>
  );
};
