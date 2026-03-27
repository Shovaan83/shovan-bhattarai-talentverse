'use client';

import { useState } from 'react';
import { Search, SlidersHorizontal, X, ChevronDown } from 'lucide-react';
import { useCategories } from '@/lib/hooks/useMarketplace';
import type { UserSearchParams } from '@/lib/types/marketplace';

interface SearchFiltersProps {
  onSearch: (params: Partial<UserSearchParams>) => void;
  currentParams: UserSearchParams;
}

export function SearchFilters({ onSearch, currentParams }: SearchFiltersProps) {
  const [showFilters, setShowFilters] = useState(false);
  const [searchQuery, setSearchQuery] = useState(currentParams.query || '');
  const { data: categories } = useCategories();

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSearch({ query: searchQuery });
  };

  const handleSkillTypeChange = (type: 'Offered' | 'Wanted' | undefined) => {
    onSearch({ skillType: type });
  };

  const handleCategoryChange = (category: string | undefined) => {
    onSearch({ category });
  };

  const handleProficiencyChange = (min: number, max: number) => {
    onSearch({ minProficiency: min, maxProficiency: max });
  };

  const hasActiveFilters = currentParams.skillType || currentParams.minProficiency || currentParams.category;

  return (
    <div className="mb-8">
      {/* Search Bar */}
      <form onSubmit={handleSearchSubmit} className="flex gap-3 mb-4">
        <div className="flex-1 relative">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-zinc-400" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search by name or skill..."
            className="w-full pl-12 pr-4 py-3 rounded-xl bg-white border border-zinc-200 text-zinc-900 placeholder-gray-400 focus:outline-none focus:border-[#1D9E75] focus:ring-1 focus:ring-[#1D9E75]/20 transition-colors"
          />
          {searchQuery && (
            <button
              type="button"
              onClick={() => {
                setSearchQuery('');
                onSearch({ query: undefined });
              }}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-zinc-900"
            >
              <X className="w-4 h-4" />
            </button>
          )}
        </div>
        <button
          type="submit"
          className="px-6 py-3 rounded-xl bg-[#1D9E75] hover:bg-[#0F6E56] text-white transition-colors font-medium"
        >
          Search
        </button>
        <button
          type="button"
          onClick={() => setShowFilters(!showFilters)}
          className={`px-4 py-3 rounded-xl border transition-colors ${
            showFilters
              ? 'bg-zinc-900 border-zinc-900 text-white'
              : 'bg-white border-zinc-200 text-zinc-600 hover:border-zinc-400'
          }`}
        >
          <SlidersHorizontal className="w-5 h-5" />
        </button>
      </form>

      {/* Filter Panel */}
      {showFilters && (
        <div className="bg-white rounded-xl p-4 border border-zinc-200 space-y-4">
          <div className="flex flex-wrap gap-4">
            {/* Skill Type Filter */}
            <div>
              <label className="block text-sm font-medium text-zinc-600 mb-2">
                Skill Type
              </label>
              <div className="flex gap-2">
                <button
                  onClick={() => handleSkillTypeChange(undefined)}
                  className={`px-3 py-1.5 rounded-lg text-sm transition-colors ${
                    !currentParams.skillType
                      ? 'bg-[#1D9E75] text-white'
                      : 'bg-zinc-50 text-zinc-900 hover:bg-zinc-100 border border-zinc-200'
                  }`}
                >
                  All
                </button>
                <button
                  onClick={() => handleSkillTypeChange('Offered')}
                  className={`px-3 py-1.5 rounded-lg text-sm transition-colors ${
                    currentParams.skillType === 'Offered'
                      ? 'bg-[#1D9E75] text-white'
                      : 'bg-zinc-50 text-zinc-900 hover:bg-zinc-100 border border-zinc-200'
                  }`}
                >
                  Offering
                </button>
                <button
                  onClick={() => handleSkillTypeChange('Wanted')}
                  className={`px-3 py-1.5 rounded-lg text-sm transition-colors ${
                    currentParams.skillType === 'Wanted'
                      ? 'bg-[#3C2A8A] text-white'
                      : 'bg-zinc-50 text-zinc-900 hover:bg-zinc-100 border border-zinc-200'
                  }`}
                >
                  Seeking
                </button>
              </div>
            </div>

            {/* Category Filter */}
            <div>
              <label className="block text-sm font-medium text-zinc-600 mb-2">
                Category
              </label>
              <div className="relative">
                <select
                  value={currentParams.category || ''}
                  onChange={(e) => handleCategoryChange(e.target.value || undefined)}
                  className="appearance-none pl-3 pr-8 py-1.5 rounded-lg text-sm bg-zinc-50 text-zinc-900 border border-zinc-200 hover:border-zinc-400 focus:outline-none focus:border-[#1D9E75] transition-colors cursor-pointer min-w-[160px]"
                >
                  <option value="">All Categories</option>
                  {categories?.map((cat) => (
                    <option key={cat} value={cat}>
                      {cat}
                    </option>
                  ))}
                </select>
                <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-4 h-4 text-zinc-400 pointer-events-none" />
              </div>
            </div>

            {/* Proficiency Filter */}
            <div>
              <label className="block text-sm font-medium text-zinc-600 mb-2">
                Minimum Proficiency
              </label>
              <div className="flex gap-2">
                {[1, 2, 3, 4, 5].map((level) => (
                  <button
                    key={level}
                    onClick={() => handleProficiencyChange(level, 5)}
                    className={`w-8 h-8 rounded-lg text-sm transition-colors ${
                      currentParams.minProficiency === level
                        ? 'bg-[#1D9E75] text-white'
                        : 'bg-zinc-50 text-zinc-900 hover:bg-zinc-100 border border-zinc-200'
                    }`}
                  >
                    {level}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Clear Filters */}
          {hasActiveFilters && (
            <button
              onClick={() => onSearch({ skillType: undefined, category: undefined, minProficiency: undefined, maxProficiency: undefined })}
              className="text-sm text-zinc-500 hover:text-zinc-900 transition-colors"
            >
              Clear all filters
            </button>
          )}
        </div>
      )}
    </div>
  );
}
