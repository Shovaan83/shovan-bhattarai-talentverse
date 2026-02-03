'use client';

import { useState } from 'react';
import { motion } from 'framer-motion';
import { Search, Users, TrendingUp, Sparkles, ArrowLeft } from 'lucide-react';
import Link from 'next/link';
import { useSearchUsers, useFeaturedUsers, useBrowseSkills } from '@/lib/hooks/useMarketplace';
import { UserCard as NewUserCard, UserCardSkeleton } from '@/app/components/marketplace/UserCard';
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

  const isShowingSearchResults = searchParams.query || searchParams.skillName;
  const users = isShowingSearchResults ? searchResults?.users : featuredUsers;
  const isLoading = isShowingSearchResults ? isSearching : isFeaturedLoading;

  return (
    <div className="min-h-screen bg-emerald-950 text-white">
      {/* Header */}
      <div className="border-b border-emerald-900/50 bg-emerald-950/80 backdrop-blur-sm sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link 
                href="/profile" 
                className="p-2 rounded-xl bg-emerald-900/50 hover:bg-emerald-900 transition-colors"
              >
                <ArrowLeft className="w-5 h-5" />
              </Link>
              <div>
                <h1 className="text-2xl font-bold">Skill Marketplace</h1>
                <p className="text-emerald-400 text-sm">Discover talents, create connections</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <Link
                href="/proposals"
                className="px-4 py-2 rounded-xl bg-purple-600 hover:bg-purple-500 transition-colors flex items-center gap-2"
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
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8"
        >
          <div className="bg-emerald-900/30 rounded-2xl p-4 border border-emerald-800/50">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-emerald-800/50">
                <Users className="w-5 h-5 text-emerald-400" />
              </div>
              <div>
                <p className="text-2xl font-bold">{searchResults?.totalCount || featuredUsers?.length || 0}</p>
                <p className="text-emerald-400 text-sm">
                  {isShowingSearchResults ? 'Results Found' : 'Active Users'}
                </p>
              </div>
            </div>
          </div>

          <div className="bg-orange-900/30 rounded-2xl p-4 border border-orange-800/50">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-orange-800/50">
                <TrendingUp className="w-5 h-5 text-orange-400" />
              </div>
              <div>
                <p className="text-2xl font-bold">{popularSkills?.length || 0}</p>
                <p className="text-orange-400 text-sm">Skills Available</p>
              </div>
            </div>
          </div>

          <div className="bg-purple-900/30 rounded-2xl p-4 border border-purple-800/50">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-xl bg-purple-800/50">
                <Sparkles className="w-5 h-5 text-purple-400" />
              </div>
              <div>
                <p className="text-2xl font-bold">
                  {popularSkills?.[0]?.skillName || 'N/A'}
                </p>
                <p className="text-purple-400 text-sm">Most Popular Skill</p>
              </div>
            </div>
          </div>
        </motion.div>

        {/* Popular Skills Quick Filters */}
        {popularSkills && popularSkills.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="mb-8"
          >
            <h3 className="text-sm font-medium text-emerald-400 mb-3">Popular Skills</h3>
            <div className="flex flex-wrap gap-2">
              {popularSkills.slice(0, 8).map((skill) => (
                <button
                  key={skill.skillName}
                  onClick={() => handleSearch({ skillName: skill.skillName, skillType: 'Offered' })}
                  className={`px-3 py-1.5 rounded-full text-sm transition-colors ${
                    searchParams.skillName === skill.skillName
                      ? 'bg-emerald-600 text-white'
                      : 'bg-emerald-900/50 text-emerald-300 hover:bg-emerald-900'
                  }`}
                >
                  {skill.skillName}
                  <span className="ml-1.5 text-emerald-500">({skill.userCount})</span>
                </button>
              ))}
            </div>
          </motion.div>
        )}

        {/* Section Title */}
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-semibold">
            {isShowingSearchResults ? (
              <>
                Search Results
                {searchResults && (
                  <span className="text-emerald-400 text-base font-normal ml-2">
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
              className="text-sm text-emerald-400 hover:text-emerald-300 transition-colors"
            >
              Clear filters
            </button>
          )}
        </div>

        {/* Users Grid */}
        {isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 justify-items-center">
            {[...Array(6)].map((_, i) => (
              <UserCardSkeleton key={i} />
            ))}
          </div>
        ) : users && users.length > 0 ? (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.3 }}
            className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 justify-items-center"
          >
            {users.map((user) => (
              <NewUserCard key={user.id} user={user} />
            ))}
          </motion.div>
        ) : (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center py-16"
          >
            <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-emerald-900/30 flex items-center justify-center">
              <Search className="w-10 h-10 text-emerald-600" />
            </div>
            <h3 className="text-xl font-semibold mb-2">No users found</h3>
            <p className="text-emerald-400 max-w-md mx-auto">
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
                    ? 'bg-emerald-600 text-white'
                    : 'bg-emerald-900/50 text-emerald-300 hover:bg-emerald-900'
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
