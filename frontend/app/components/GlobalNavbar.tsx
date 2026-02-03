'use client';

import { useState, useEffect } from 'react';
import { motion, useScroll, useMotionValueEvent } from 'framer-motion';
import { Menu, X, Search, ArrowRightLeft, Users } from 'lucide-react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/lib/hooks/useAuth';
import { useProposalNotifications } from '@/lib/hooks/useProposalNotifications';
import { ProfileDropdown } from './ProfileDropdown';
import { NotificationDropdown } from './NotificationDropdown';

export function GlobalNavbar() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [mounted, setMounted] = useState(false);
  const [isScrolled, setIsScrolled] = useState(false);
  const pathname = usePathname();
  const { user, isAuthenticated, isLoading } = useAuth();
  const { count: notificationCount } = useProposalNotifications();
  const { scrollY } = useScroll();

  // Prevent hydration mismatch by only rendering after mount
  useEffect(() => {
    setMounted(true);
  }, []);

  // Scroll detection - changes navbar appearance after 20px scroll
  useMotionValueEvent(scrollY, "change", (latest) => {
    setIsScrolled(latest > 20);
  });

  // Don't show navbar on landing page and auth pages
  const hideNavbar = pathname === '/' || 
                      pathname?.startsWith('/login') || 
                      pathname?.startsWith('/register') ||
                      pathname?.startsWith('/onboarding') ||
                      pathname?.startsWith('/setup-2fa') ||
                      pathname?.startsWith('/oauth-callback') ||
                      pathname?.startsWith('/forgot-password');

  // Return placeholder during SSR and initial mount to prevent hydration mismatch
  if (!mounted || hideNavbar) {
    return hideNavbar ? null : <div className="h-16" />;
  }

  // Show loading placeholder while auth is loading
  if (isLoading) {
    return <div className="h-16" />;
  }

  // Check if route is active
  const isActive = (path: string) => {
    if (path === '/marketplace') {
      return pathname === '/marketplace' || pathname === '/dashboard';
    }
    return pathname === path || pathname?.startsWith(`${path}/`);
  };

  // Navigation links for authenticated users
  const authenticatedLinks = [
    { href: '/marketplace', label: 'Marketplace', icon: Search },
    { href: '/proposals', label: 'Proposals', icon: ArrowRightLeft, badge: notificationCount },
    { href: '/#community', label: 'Community', icon: Users },
  ];

  // Navigation links for unauthenticated users
  const unauthenticatedLinks = [
    { href: '/#how-it-works', label: 'How it Works' },
    { href: '/#community', label: 'Community' },
  ];

  return (
    <>
      <motion.nav
        className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
          isScrolled || mobileMenuOpen
            ? 'bg-white/80 backdrop-blur-md shadow-sm border-b border-gray-100'
            : 'bg-gradient-to-r bg-emerald-950'
        }`}
        initial={{ y: -100 }}
        animate={{ y: 0 }}
        transition={{ duration: 0.3 }}
      >
        <div className={`container mx-auto px-6 ${
          isScrolled || mobileMenuOpen ? 'py-3' : 'py-2'
        }`}>
          <div className="flex items-center justify-between">
            {/* Logo */}
            <Link href={isAuthenticated ? '/marketplace' : '/'}>
              <div className="flex items-center gap-2 cursor-pointer group">
                <div className="bg-emerald-600 p-1.5 rounded-lg text-white group-hover:rotate-12 transition-transform duration-300">
                  {/* Icon placeholder */}
                </div>
                <span className={`font-heading font-bold text-xl tracking-tight transition-colors ${
                  isScrolled || mobileMenuOpen ? 'text-emerald-950' : 'text-white'
                }`}>
                  TalentVerse
                </span>
              </div>
            </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center gap-6">
            {isAuthenticated ? (
              <>
                {/* Authenticated Links */}
                {authenticatedLinks.map((link) => (
                  <Link key={link.href} href={link.href}>
                    <button
                      className={`relative flex items-center gap-2 px-3 py-2 text-sm font-medium rounded-lg transition-colors ${
                        isActive(link.href) && (isScrolled || mobileMenuOpen)
                          ? 'text-emerald-600 bg-emerald-50'
                          : isActive(link.href) && !(isScrolled || mobileMenuOpen)
                          ? 'text-white bg-white/10'
                          : isScrolled || mobileMenuOpen
                          ? 'text-gray-700 hover:text-emerald-600 hover:bg-gray-50'
                          : 'text-emerald-50 hover:text-white hover:bg-white/10'
                      }`}
                    >
                      <link.icon className="w-4 h-4" />
                      {link.label}
                      {link.badge !== undefined && link.badge > 0 && (
                        <span className="ml-1 px-2 py-0.5 bg-red-500 text-white text-xs font-bold rounded-full">
                          {link.badge > 9 ? '9+' : link.badge}
                        </span>
                      )}
                    </button>
                  </Link>
                ))}

                {/* Notification Bell */}
                <NotificationDropdown count={notificationCount} isScrolled={isScrolled || mobileMenuOpen} />

                {/* Profile Dropdown */}
                {user && (
                  <ProfileDropdown
                    username={user.username}
                    profilePictureUrl={user.profilePictureUrl}
                    isScrolled={isScrolled || mobileMenuOpen}
                  />
                )}
              </>
            ) : (
              <>
                {/* Unauthenticated Links */}
                {unauthenticatedLinks.map((link) => (
                  <a
                    key={link.href}
                    href={link.href}
                    className={`text-sm font-medium transition-colors ${
                      isScrolled || mobileMenuOpen
                        ? 'text-gray-600 hover:text-emerald-600'
                        : 'text-emerald-50 hover:text-white'
                    }`}
                  >
                    {link.label}
                  </a>
                ))}

                {/* Auth Buttons */}
                <Link href="/login">
                  <button className={`text-sm font-medium px-4 py-2 rounded-full transition-colors ${
                    isScrolled || mobileMenuOpen
                      ? 'text-emerald-900 hover:bg-emerald-50'
                      : 'text-white hover:bg-white/10'
                  }`}>
                    Log In
                  </button>
                </Link>
                <Link href="/register">
                  <button className="bg-orange-600 hover:bg-orange-700 text-white text-sm font-semibold px-5 py-2.5 rounded-full transition-transform hover:scale-105 shadow-lg shadow-orange-600/20">
                    Join Now
                  </button>
                </Link>
              </>
            )}
          </div>

          {/* Mobile Menu Toggle */}
          <div className="md:hidden">
            <button
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className={`p-2 transition-colors ${
                isScrolled || mobileMenuOpen ? 'text-gray-800' : 'text-white'
              }`}
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
            exit={{ opacity: 0, height: 0 }}
            className="md:hidden border-t border-gray-100 mt-3 pt-4 pb-4 px-6"
          >
            <div className="flex flex-col space-y-2">
              {isAuthenticated ? (
                <>
                  {/* Mobile Authenticated Links */}
                  {authenticatedLinks.map((link) => (
                    <Link key={link.href} href={link.href}>
                      <button
                        onClick={() => setMobileMenuOpen(false)}
                        className={`w-full flex items-center justify-between px-4 py-3 text-left text-sm font-medium rounded-lg transition-colors ${
                          isActive(link.href)
                            ? 'text-emerald-600 bg-emerald-50'
                            : 'text-gray-700 hover:bg-gray-50'
                        }`}
                      >
                        <div className="flex items-center gap-3">
                          <link.icon className="w-5 h-5" />
                          {link.label}
                        </div>
                        {link.badge !== undefined && link.badge > 0 && (
                          <span className="px-2 py-0.5 bg-red-500 text-white text-xs font-bold rounded-full">
                            {link.badge > 9 ? '9+' : link.badge}
                          </span>
                        )}
                      </button>
                    </Link>
                  ))}

                  {/* Mobile Profile Link */}
                  <Link href="/profile">
                    <button
                      onClick={() => setMobileMenuOpen(false)}
                      className={`w-full flex items-center gap-3 px-4 py-3 text-left text-sm font-medium rounded-lg transition-colors ${
                        isActive('/profile')
                          ? 'text-emerald-600 bg-emerald-50'
                          : 'text-gray-700 hover:bg-gray-50'
                      }`}
                    >
                      <div className="w-6 h-6 rounded-full overflow-hidden bg-gray-200 flex items-center justify-center">
                        {user?.profilePictureUrl ? (
                          <img
                            src={user.profilePictureUrl}
                            alt={user.username}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          <Users className="w-4 h-4 text-gray-400" />
                        )}
                      </div>
                      {user?.username || 'Profile'}
                    </button>
                  </Link>

                  {/* Mobile Logout */}
                  <button
                    onClick={() => {
                      setMobileMenuOpen(false);
                      if (user) {
                        // Trigger logout from useAuth hook
                      }
                    }}
                    className="w-full px-4 py-3 text-left text-sm font-medium text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                  >
                    Log Out
                  </button>
                </>
              ) : (
                <>
                  {/* Mobile Unauthenticated Links */}
                  {unauthenticatedLinks.map((link) => (
                    <a
                      key={link.href}
                      href={link.href}
                      onClick={() => setMobileMenuOpen(false)}
                      className="px-4 py-3 text-gray-800 font-medium text-base hover:bg-gray-50 rounded-lg"
                    >
                      {link.label}
                    </a>
                  ))}

                  {/* Mobile Auth Buttons */}
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
                </>
              )}
            </div>
          </motion.div>
        )}
        </div>
      </motion.nav>
      
      {/* Spacer for fixed navbar */}
      <div className="h-16" />
    </>
  );
}
