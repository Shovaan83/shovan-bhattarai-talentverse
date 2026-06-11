'use client';

import { useState } from 'react';
import { motion } from 'framer-motion';
import { Search, Users, ArrowLeft } from 'lucide-react';
import Link from 'next/link';
import { useSearchUsers, useFeaturedUsers, useBrowseSkills } from '@/lib/hooks/useMarketplace';
import {
  MarketplaceHoverCard,
  MarketplaceHoverCardSkeleton,
} from '@/app/components/marketplace/MarketplaceHoverCard';
import { SearchFilters } from './components';
import type { UserSearchParams } from '@/lib/types/marketplace';

export default function MarketplacePage() {
  const [searchParams, setSearchParams] = useState<UserSearchParams>({
    page: 1,
    pageSize: 12,
  });

  const { data: searchResults, isLoading: isSearching } = useSearchUsers(searchParams);
  const { data: featuredUsers, isLoading: isFeaturedLoading } = useFeaturedUsers();
  const { data: popularSkills } = useBrowseSkills('Offered');

  const handleSearch = (params: Partial<UserSearchParams>) => {
    setSearchParams(prev => ({ ...prev, ...params, page: 1 }));
  };

  const handlePageChange = (page: number) => {
    setSearchParams(prev => ({ ...prev, page }));
  };

  const isShowingSearchResults = searchParams.query || searchParams.skillName || searchParams.category;
  const users = isShowingSearchResults ? searchResults?.users : featuredUsers;
  const isLoading = isShowingSearchResults ? isSearching : isFeaturedLoading;

  return (
    <div className="min-h-screen bg-[#FAFAFA] text-zinc-900">
      {/* Header */}
      <div className="border-b border-zinc-200 bg-white/95 backdrop-blur-sm sticky top-16 z-10">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link
                href="/profile"
                className="p-2 rounded-xl bg-zinc-100 hover:bg-zinc-200 text-zinc-600 transition-colors"
              >
                <ArrowLeft className="w-5 h-5" />
              </Link>
              <div>
                <h1 className="text-2xl font-display font-bold text-zinc-900">Skill Marketplace</h1>
                <p className="text-zinc-500 text-sm">Discover talents, create connections</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <Link
                href="/proposals"
                className="px-4 py-2 rounded-xl bg-zinc-900 hover:bg-zinc-800 text-white transition-colors flex items-center gap-2 text-sm font-medium"
              >
                <Users className="w-4 h-4" />
                My Proposals
              </Link>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-6 py-8">
        {/* Search & Filters */}
        <SearchFilters onSearch={handleSearch} currentParams={searchParams} />

        {/* Stats Strip */}
        <div className="flex items-center gap-8 py-4 mb-8">
          <div>
            <p className="text-2xl font-semibold text-zinc-900">{searchResults?.totalCount || featuredUsers?.length || 0}</p>
            <p className="text-xs text-zinc-500 uppercase tracking-wide">{isShowingSearchResults ? 'Results' : 'Active Users'}</p>
          </div>
          <div className="w-px h-8 bg-zinc-200" />
          <div>
            <p className="text-2xl font-semibold text-zinc-900">{popularSkills?.length || 0}</p>
            <p className="text-xs text-zinc-500 uppercase tracking-wide">Skills Available</p>
          </div>
          <div className="w-px h-8 bg-zinc-200" />
          <div>
            <p className="text-2xl font-semibold text-zinc-900">{popularSkills?.[0]?.skillName || 'N/A'}</p>
            <p className="text-xs text-zinc-500 uppercase tracking-wide">Most Popular</p>
          </div>
        </div>

        {/* Popular Skills Quick Filters */}
        {popularSkills && popularSkills.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="mb-8"
          >
            <h3 className="text-sm font-medium text-zinc-600 mb-3">Popular Skills</h3>
            <div className="flex flex-wrap gap-2">
              {popularSkills.slice(0, 8).map((skill) => (
                <button
                  key={skill.skillName}
                  onClick={() => handleSearch({ skillName: skill.skillName, skillType: 'Offered' })}
                  className={`px-3 py-1.5 rounded-full text-sm transition-colors ${
                    searchParams.skillName === skill.skillName
                      ? 'bg-[#1D9E75] text-white'
                      : 'bg-white text-zinc-900 hover:bg-zinc-100 border border-zinc-200'
                  }`}
                >
                  {skill.skillName}
                  <span className="ml-1.5 text-[#1D9E75]">({skill.userCount})</span>
                </button>
              ))}
            </div>
          </motion.div>
        )}

        {/* Section Title */}
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-display font-semibold text-zinc-900">
            {isShowingSearchResults ? (
              <>
                Search Results
                {searchResults && (
                  <span className="text-zinc-500 text-base font-normal ml-2">
                    ({searchResults.totalCount} users)
                  </span>
                )}
              </>
            ) : (
              'Featured Users'
            )}
          </h2>
          {isShowingSearchResults && (
            <button
              onClick={() => setSearchParams({ page: 1, pageSize: 12 })}
              className="text-sm text-zinc-500 hover:text-zinc-900 transition-colors"
            >
              Clear filters
            </button>
          )}
        </div>

        {/* Users Grid */}
        {isLoading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {[...Array(6)].map((_, i) => (
              <MarketplaceHoverCardSkeleton key={i} />
            ))}
          </div>
        ) : users && users.length > 0 ? (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.3 }}
            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5"
          >
            {users.map((user) => (
              <MarketplaceHoverCard key={user.id} user={user} />
            ))}
          </motion.div>
        ) : (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center py-16"
          >
            <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-zinc-100 flex items-center justify-center">
              <Search className="w-10 h-10 text-zinc-400" />
            </div>
            <h3 className="text-xl font-display font-semibold text-zinc-900 mb-2">No users found</h3>
            <p className="text-gray-500 max-w-md mx-auto">
              {isShowingSearchResults
                ? 'Try adjusting your search criteria or browse popular skills above.'
                : 'Be the first to add skills and appear in the marketplace!'}
            </p>
          </motion.div>
        )}

        {/* Pagination */}
        {searchResults && searchResults.totalPages > 1 && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="flex justify-center gap-2 mt-8"
          >
            {[...Array(searchResults.totalPages)].map((_, i) => (
              <button
                key={i}
                onClick={() => handlePageChange(i + 1)}
                className={`w-10 h-10 rounded-xl transition-colors ${
                  searchParams.page === i + 1
                    ? 'bg-[#1D9E75] text-white'
                    : 'bg-white text-zinc-900 hover:bg-zinc-100 border border-zinc-200'
                }`}
              >
                {i + 1}
              </button>
            ))}
          </motion.div>
        )}
      </div>
    </div>
  );
}
