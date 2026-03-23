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
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-emerald-500" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search by name or skill..."
            className="w-full pl-12 pr-4 py-3 rounded-2xl bg-emerald-900/30 border border-emerald-800/50 text-white placeholder-emerald-600 focus:outline-none focus:border-emerald-600 transition-colors"
          />
          {searchQuery && (
            <button
              type="button"
              onClick={() => {
                setSearchQuery('');
                onSearch({ query: undefined });
              }}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-emerald-500 hover:text-emerald-300"
            >
              <X className="w-4 h-4" />
            </button>
          )}
        </div>
        <button
          type="submit"
          className="px-6 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-500 transition-colors font-medium"
        >
          Search
        </button>
        <button
          type="button"
          onClick={() => setShowFilters(!showFilters)}
          className={`px-4 py-3 rounded-2xl border transition-colors ${
            showFilters
              ? 'bg-emerald-600 border-emerald-600'
              : 'bg-emerald-900/30 border-emerald-800/50 hover:border-emerald-600'
          }`}
        >
          <SlidersHorizontal className="w-5 h-5" />
        </button>
      </form>

      {/* Filter Panel */}
      {showFilters && (
        <div className="bg-emerald-900/30 rounded-2xl p-4 border border-emerald-800/50 space-y-4">
          <div className="flex flex-wrap gap-4">
            {/* Skill Type Filter */}
            <div>
              <label className="block text-sm font-medium text-emerald-400 mb-2">
                Skill Type
              </label>
              <div className="flex gap-2">
                <button
                  onClick={() => handleSkillTypeChange(undefined)}
                  className={`px-3 py-1.5 rounded-lg text-sm transition-colors ${
                    !currentParams.skillType
                      ? 'bg-emerald-600 text-white'
                      : 'bg-emerald-900/50 text-emerald-300 hover:bg-emerald-900'
                  }`}
                >
                  All
                </button>
                <button
                  onClick={() => handleSkillTypeChange('Offered')}
                  className={`px-3 py-1.5 rounded-lg text-sm transition-colors ${
                    currentParams.skillType === 'Offered'
                      ? 'bg-emerald-600 text-white'
                      : 'bg-emerald-900/50 text-emerald-300 hover:bg-emerald-900'
                  }`}
                >
                  Offering
                </button>
                <button
                  onClick={() => handleSkillTypeChange('Wanted')}
                  className={`px-3 py-1.5 rounded-lg text-sm transition-colors ${
                    currentParams.skillType === 'Wanted'
                      ? 'bg-orange-600 text-white'
                      : 'bg-orange-900/50 text-orange-300 hover:bg-orange-900'
                  }`}
                >
                  Seeking
                </button>
              </div>
            </div>

            {/* Category Filter */}
            <div>
              <label className="block text-sm font-medium text-emerald-400 mb-2">
                Category
              </label>
              <div className="relative">
                <select
                  value={currentParams.category || ''}
                  onChange={(e) => handleCategoryChange(e.target.value || undefined)}
                  className="appearance-none pl-3 pr-8 py-1.5 rounded-lg text-sm bg-emerald-900/50 text-emerald-300 border border-emerald-800/50 hover:border-emerald-600 focus:outline-none focus:border-emerald-600 transition-colors cursor-pointer min-w-[160px]"
                >
                  <option value="">All Categories</option>
                  {categories?.map((cat) => (
                    <option key={cat} value={cat}>
                      {cat}
                    </option>
                  ))}
                </select>
                <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-4 h-4 text-emerald-500 pointer-events-none" />
              </div>
            </div>

            {/* Proficiency Filter */}
            <div>
              <label className="block text-sm font-medium text-emerald-400 mb-2">
                Minimum Proficiency
              </label>
              <div className="flex gap-2">
                {[1, 2, 3, 4, 5].map((level) => (
                  <button
                    key={level}
                    onClick={() => handleProficiencyChange(level, 5)}
                    className={`w-8 h-8 rounded-lg text-sm transition-colors ${
                      currentParams.minProficiency === level
                        ? 'bg-emerald-600 text-white'
                        : 'bg-emerald-900/50 text-emerald-300 hover:bg-emerald-900'
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
              className="text-sm text-emerald-400 hover:text-emerald-300 transition-colors"
            >
              Clear all filters
            </button>
          )}
        </div>
      )}
    </div>
  );
}
