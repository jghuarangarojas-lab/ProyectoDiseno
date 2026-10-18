import { useCallback, useEffect, useRef, useState } from 'react';
import toast from 'react-hot-toast';
import { inventoryService, toInventoryUserMessage } from '../services/inventory.service';
import type { ShoppingListItem } from '../types/inventory';

const DEFAULT_POLL_MS = 30000;

interface UseRestockListOptions {
  pollMs?: number;
  enabled?: boolean;
}

export interface UseRestockListResult {
  items: ShoppingListItem[];
  isLoading: boolean;
  isRefreshing: boolean;
  lastLoadedAt: string | null;
  refresh: (silent?: boolean) => Promise<void>;
}

export function useRestockList(
  options: UseRestockListOptions = {},
): UseRestockListResult {
  const { pollMs = DEFAULT_POLL_MS, enabled = true } = options;

  const [items, setItems] = useState<ShoppingListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [lastLoadedAt, setLastLoadedAt] = useState<string | null>(null);

  const mountedRef = useRef(true);

  const refresh = useCallback(
    async (silent = false) => {
      if (!silent) {
        setIsRefreshing(true);
      }

      try {
        const result = await inventoryService.getShoppingList();

        if (!mountedRef.current) {
          return;
        }

        setItems(result);
        setLastLoadedAt(new Date().toISOString());
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
    [],
  );

  useEffect(() => {
    mountedRef.current = true;

    return () => {
      mountedRef.current = false;
    };
  }, []);

  useEffect(() => {
    if (!enabled) {
      return;
    }

    void refresh();

    const intervalId = window.setInterval(() => {
      void refresh(true);
    }, pollMs);

    return () => window.clearInterval(intervalId);
  }, [enabled, pollMs, refresh]);

  return {
    items,
    isLoading,
    isRefreshing,
    lastLoadedAt,
    refresh,
  };
}
