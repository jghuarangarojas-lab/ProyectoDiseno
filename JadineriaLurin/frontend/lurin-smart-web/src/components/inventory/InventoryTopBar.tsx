import { useEffect, useRef, useState, type KeyboardEvent } from 'react';
import { PRODUCT_CATEGORIES } from '../../constants/mermaReasons';
import { RefreshIcon, SearchIcon, SpinnerIcon } from '../icons';
import type { Product } from '../../types/inventory';

interface InventoryTopBarProps {
  searchTerm: string;
  suggestions: Product[];
  category: string | null;
  isRefreshing: boolean;
  onSearchTermChange: (value: string) => void;
  onSelectSuggestion: (product: Product) => void;
  onCategoryChange: (category: string | null) => void;
  onRefresh: () => void;
}

export function InventoryTopBar({
  searchTerm,
  suggestions,
  category,
  isRefreshing,
  onSearchTermChange,
  onSelectSuggestion,
  onCategoryChange,
  onRefresh,
}: InventoryTopBarProps) {
  const [isSuggestionOpen, setIsSuggestionOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    setActiveIndex(-1);
  }, [suggestions]);

  useEffect(() => {
    const handlePointerDown = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setIsSuggestionOpen(false);
      }
    };

    document.addEventListener('mousedown', handlePointerDown);
    return () => document.removeEventListener('mousedown', handlePointerDown);
  }, []);

  const showSuggestions = isSuggestionOpen && suggestions.length > 0;

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (!showSuggestions) {
      if (event.key === 'ArrowDown') {
        setIsSuggestionOpen(true);
      }
      return;
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      setActiveIndex((previous) => (previous + 1) % suggestions.length);
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      setActiveIndex((previous) =>
        previous <= 0 ? suggestions.length - 1 : previous - 1,
      );
    } else if (event.key === 'Enter' && activeIndex >= 0) {
      event.preventDefault();
      onSelectSuggestion(suggestions[activeIndex]);
      setIsSuggestionOpen(false);
    } else if (event.key === 'Escape') {
      setIsSuggestionOpen(false);
    }
  };

  return (
    <div className="sticky top-[49px] z-20 border-b border-slate-200 bg-white/95 backdrop-blur">
      <div className="mx-auto flex max-w-7xl flex-col gap-3 px-4 py-3 lg:flex-row lg:items-center">
        <div ref={containerRef} className="relative w-full lg:max-w-md">
          <SearchIcon className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="search"
            value={searchTerm}
            onChange={(event) => {
              onSearchTermChange(event.target.value);
              setIsSuggestionOpen(true);
            }}
            onFocus={() => setIsSuggestionOpen(true)}
            onKeyDown={handleKeyDown}
            placeholder="Buscar por nombre o SKU (ej. V001)..."
            aria-label="Buscar productos"
            className="w-full rounded-full border border-slate-200 bg-slate-50 py-2.5 pl-9 pr-4 text-sm outline-none transition placeholder:text-slate-400 focus:border-emerald-400 focus:bg-white focus:ring-2 focus:ring-emerald-100"
          />

          {showSuggestions && (
            <ul className="absolute left-0 right-0 top-full z-30 mt-2 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-xl">
              {suggestions.map((product, index) => (
                <li key={product.id}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => {
                      onSelectSuggestion(product);
                      setIsSuggestionOpen(false);
                    }}
                    onMouseEnter={() => setActiveIndex(index)}
                    className={`flex w-full items-center gap-3 px-3 py-2 text-left transition ${
                      activeIndex === index ? 'bg-emerald-50' : 'bg-white'
                    }`}
                  >
                    <span className="rounded-md bg-slate-100 px-1.5 py-0.5 font-mono text-[11px] font-bold text-slate-600">
                      {product.sku}
                    </span>
                    <span className="flex-1 truncate text-sm text-slate-700">
                      {product.commonName}
                    </span>
                    <span
                      className={`text-xs font-semibold ${
                        product.isBelowMinimum ? 'text-rose-600' : 'text-emerald-600'
                      }`}
                    >
                      {product.stockAvailable}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => onCategoryChange(null)}
            className={`rounded-full px-3.5 py-1.5 text-xs font-semibold transition ${
              category === null
                ? 'bg-emerald-600 text-white shadow-sm'
                : 'border border-slate-200 text-slate-600 hover:bg-slate-100'
            }`}
          >
            Todas
          </button>

          {PRODUCT_CATEGORIES.map((option) => (
            <button
              key={option}
              type="button"
              onClick={() =>
                onCategoryChange(category === option ? null : option)
              }
              className={`rounded-full px-3.5 py-1.5 text-xs font-semibold transition ${
                category === option
                  ? 'bg-emerald-600 text-white shadow-sm'
                  : 'border border-slate-200 text-slate-600 hover:bg-slate-100'
              }`}
            >
              {option}
            </button>
          ))}

          <button
            type="button"
            onClick={onRefresh}
            aria-label="Actualizar catálogo"
            className="ml-auto rounded-full border border-slate-200 p-2 text-slate-500 transition hover:bg-slate-100 lg:ml-2"
          >
            {isRefreshing ? (
              <SpinnerIcon className="h-4 w-4 animate-spin" />
            ) : (
              <RefreshIcon className="h-4 w-4" />
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
