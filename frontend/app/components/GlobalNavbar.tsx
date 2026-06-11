'use client';

import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Menu, X, Search, ArrowRightLeft, MessageSquare, Coins, Trophy, Users } from 'lucide-react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/lib/hooks/useAuth';
import { useProposalNotifications } from '@/lib/hooks/useProposalNotifications';
import { ProfileDropdown } from './ProfileDropdown';
import { NotificationDropdown } from './NotificationDropdown';
import { useUnreadCount } from '@/lib/hooks/useMessages';
import { useWallet } from '@/lib/hooks/useCredits';
import { BrandLogo } from './BrandLogo';

export function GlobalNavbar() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [mounted, setMounted] = useState(false);
  const pathname = usePathname();
  const { user, isAuthenticated, isLoading } = useAuth();
  const { count: notificationCount } = useProposalNotifications(user?.id);
  const { data: unreadMessageCount = 0 } = useUnreadCount(isAuthenticated);
  const { data: wallet } = useWallet(isAuthenticated);

  useEffect(() => {
    setMounted(true);
  }, []);

  // Don't show navbar on landing page and auth pages
  const hideNavbar = pathname === '/' ||
                      pathname?.startsWith('/login') ||
                      pathname?.startsWith('/register') ||
                      pathname?.startsWith('/onboarding') ||
                      pathname?.startsWith('/setup-2fa') ||
                      pathname?.startsWith('/oauth-callback') ||
                      pathname?.startsWith('/forgot-password');

  if (!mounted || hideNavbar) {
    return hideNavbar ? null : <div className="h-16" />;
  }

  if (isLoading) {
    return <div className="h-16" />;
  }

  const isActive = (path: string) => {
    if (path === '/marketplace') {
      return pathname === '/marketplace' || pathname === '/dashboard';
    }
    return pathname === path || pathname?.startsWith(`${path}/`);
  };

  const authenticatedLinks = [
    { href: '/marketplace', label: 'Marketplace', icon: Search },
    { href: '/proposals', label: 'Proposals', icon: ArrowRightLeft, badge: notificationCount },
    { href: '/messages', label: 'Messages', icon: MessageSquare, badge: unreadMessageCount },
    { href: '/credits', label: 'Credits', icon: Coins, badge: wallet?.balance !== undefined && wallet.balance > 0 ? Math.floor(wallet.balance) : undefined, isGold: true },
    { href: '/leaderboard', label: 'Leaderboard', icon: Trophy },
    { href: '/#community', label: 'Community', icon: Users },
  ];

  const unauthenticatedLinks = [
    { href: '/#how-it-works', label: 'How it Works' },
    { href: '/#community', label: 'Community' },
  ];

  return (
    <>
      <nav
        className="fixed top-0 left-0 right-0 z-50 h-16
                   bg-white/95 backdrop-blur-md border-b border-zinc-200"
      >
        <div className="container mx-auto px-6 h-full">
          <div className="flex items-center justify-between h-full">
            {/* Logo */}
            <Link href={isAuthenticated ? '/marketplace' : '/'}>
              <div className="flex items-center gap-2 cursor-pointer group">
                <BrandLogo iconClassName="h-8 w-8" textClassName="text-xl text-zinc-900" />
              </div>
            </Link>

            {/* Desktop Navigation */}
            <div className="hidden md:flex items-center gap-1">
              {isAuthenticated ? (
                <>
                  {authenticatedLinks.map((link) => (
                    <Link key={link.href} href={link.href}>
                      <button
                        className={`relative flex items-center gap-2 px-3 py-2 text-sm font-medium rounded-lg transition-all duration-150 ${
                          isActive(link.href)
                            ? 'text-[#1D9E75] bg-zinc-100'
                            : 'text-zinc-600 hover:text-zinc-900 hover:bg-zinc-50'
                        }`}
                      >
                        <link.icon className="w-4 h-4" />
                        {link.label}
                        {link.badge !== undefined && link.badge > 0 && (
                            <span className={`ml-1 px-1.5 py-0.5 text-xs font-bold rounded-full flex items-center justify-center min-w-5 ${
                            link.isGold
                              ? 'bg-[#EF9F27] text-white'
                              : 'bg-[#1D9E75] text-white'
                          }`}>
                            {link.badge > 99 ? '99+' : link.badge}
                          </span>
                        )}
                      </button>
                    </Link>
                  ))}

                  {/* Notification Bell */}
                  <NotificationDropdown count={notificationCount} />

                  {/* Profile Dropdown */}
                  {user && (
                    <ProfileDropdown
                      username={user.username}
                      profilePictureUrl={user.profilePictureUrl}
                    />
                  )}
                </>
              ) : (
                <>
                  {unauthenticatedLinks.map((link) => (
                    <a
                      key={link.href}
                      href={link.href}
                      className="text-sm font-medium text-zinc-600 hover:text-zinc-900 transition-colors px-3 py-2"
                    >
                      {link.label}
                    </a>
                  ))}

                  <Link href="/login">
                    <button className="text-sm font-medium text-zinc-600 hover:text-zinc-900 px-4 py-2 transition-colors">
                      Log In
                    </button>
                  </Link>
                  <Link href="/register">
                    <button className="bg-[#1D9E75] hover:bg-[#0F6E56] text-white text-sm font-semibold px-5 py-2.5 rounded-lg transition-colors active:scale-[0.98]">
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
                className="p-2 text-zinc-600 hover:text-zinc-900 transition-colors"
              >
                {mobileMenuOpen ? <X size={24} /> : <Menu size={24} />}
              </button>
            </div>
          </div>

          {/* Mobile Menu */}
          <AnimatePresence>
            {mobileMenuOpen && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                exit={{ opacity: 0, height: 0 }}
                className="md:hidden border-t border-zinc-200 mt-0 pt-4 pb-4"
              >
                <div className="flex flex-col space-y-1">
                  {isAuthenticated ? (
                    <>
                      {authenticatedLinks.map((link) => (
                        <Link key={link.href} href={link.href}>
                          <button
                            onClick={() => setMobileMenuOpen(false)}
                            className={`w-full flex items-center justify-between px-4 py-3 text-left text-sm font-medium rounded-lg transition-colors ${
                              isActive(link.href)
                                ? 'text-[#1D9E75] bg-zinc-100'
                                : 'text-zinc-600 hover:text-zinc-900 hover:bg-zinc-50'
                            }`}
                          >
                            <div className="flex items-center gap-3">
                              <link.icon className="w-5 h-5" />
                              {link.label}
                            </div>
                            {link.badge !== undefined && link.badge > 0 && (
                              <span className={`px-2 py-0.5 text-xs font-bold rounded-full ${
                                link.isGold
                                  ? 'bg-[#EF9F27] text-white'
                                  : 'bg-[#1D9E75] text-white'
                              }`}>
                                {link.badge > 99 ? '99+' : link.badge}
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
                              ? 'text-[#1D9E75] bg-zinc-100'
                              : 'text-zinc-600 hover:text-zinc-900 hover:bg-zinc-50'
                          }`}
                        >
                          <div className="w-6 h-6 rounded-full overflow-hidden bg-zinc-100 flex items-center justify-center">
                            {user?.profilePictureUrl ? (
                              <img
                                src={user.profilePictureUrl}
                                alt={user.username}
                                className="w-full h-full object-cover"
                              />
                            ) : (
                              <span className="text-xs font-medium text-zinc-600">
                                {user?.username?.charAt(0).toUpperCase()}
                              </span>
                            )}
                          </div>
                          {user?.username || 'Profile'}
                        </button>
                      </Link>

                      {/* Mobile Logout */}
                      <button
                        onClick={() => {
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-3 text-left text-sm font-medium text-red-400 hover:bg-red-500/10 rounded-lg transition-colors"
                      >
                        Log Out
                      </button>
                    </>
                  ) : (
                    <>
                      {unauthenticatedLinks.map((link) => (
                        <a
                          key={link.href}
                          href={link.href}
                          onClick={() => setMobileMenuOpen(false)}
                          className="px-4 py-3 text-zinc-600 hover:text-zinc-900 font-medium text-base rounded-lg transition-colors"
                        >
                          {link.label}
                        </a>
                      ))}

                      <div className="pt-4 flex flex-col space-y-3">
                        <Link href="/login">
                          <button className="w-full text-zinc-900 font-medium py-3 rounded-xl border border-zinc-200 hover:bg-zinc-50 transition-colors">
                            Log In
                          </button>
                        </Link>
                        <Link href="/register">
                          <button className="w-full bg-[#1D9E75] hover:bg-[#0F6E56] text-white font-bold py-3 rounded-xl transition-colors">
                            Join Now
                          </button>
                        </Link>
                      </div>
                    </>
                  )}
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </nav>

      {/* Spacer for fixed navbar */}
      <div className="h-16" />
    </>
  );
}
