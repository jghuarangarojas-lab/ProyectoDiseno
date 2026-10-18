import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import toast from 'react-hot-toast';
import { inventoryService, toInventoryUserMessage } from '../services/inventory.service';
import type { Product } from '../types/inventory';

const DEBOUNCE_MS = 300;
const MAX_SUGGESTIONS = 6;

interface UseProductSearchOptions {
  pageSize?: number;
}

export interface UseProductSearchResult {
  products: Product[];
  suggestions: Product[];
  searchTerm: string;
  category: string | null;
  isLoading: boolean;
  isRefreshing: boolean;
  setSearchTerm: (value: string) => void;
  setCategory: (category: string | null) => void;
  refresh: (silent?: boolean) => Promise<void>;
}

export function useProductSearch(
  options: UseProductSearchOptions = {},
): UseProductSearchResult {
  const { pageSize = 60 } = options;

  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedTerm, setDebouncedTerm] = useState('');
  const [category, setCategory] = useState<string | null>(null);
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);

  const mountedRef = useRef(true);

  useEffect(() => {
    mountedRef.current = true;

    return () => {
      mountedRef.current = false;
    };
  }, []);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setDebouncedTerm(searchTerm.trim());
    }, DEBOUNCE_MS);

    return () => window.clearTimeout(timer);
  }, [searchTerm]);

  const refresh = useCallback(
    async (silent = false) => {
      if (!silent) {
        setIsRefreshing(true);
      }

      try {
        const result = await inventoryService.searchProducts({
          name: debouncedTerm || undefined,
          category: category ?? undefined,
          pageSize,
        });

        if (!mountedRef.current) {
          return;
        }

        setProducts(result);
      } catch (error) {
        if (!silent) {
          toast.error(toInventoryUserMessage(error));
        }
      } finally {
        if (mountedRef.current) {
          setIsLoading(false);

          if (!silent) {
            setIsRefreshing(false);
          }
        }
      }
    },
    [category, debouncedTerm, pageSize],
  );

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const suggestions = useMemo(() => {
    const term = searchTerm.trim().toLowerCase();

    if (!term) {
      return [];
    }

    return products
      .filter(
        (product) =>
          product.sku.toLowerCase().includes(term) ||
          product.commonName.toLowerCase().includes(term),
      )
      .slice(0, MAX_SUGGESTIONS);
  }, [products, searchTerm]);

  return {
    products,
    suggestions,
    searchTerm,
    category,
    isLoading,
    isRefreshing,
    setSearchTerm,
    setCategory,
    refresh,
  };
}
