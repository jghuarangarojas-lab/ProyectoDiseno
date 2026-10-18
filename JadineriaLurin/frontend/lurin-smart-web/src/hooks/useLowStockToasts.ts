import { useEffect } from 'react';
import toast from 'react-hot-toast';
import { useInventoryWebSockets } from './useInventoryWebSockets';
import type { InventorySocketStatus } from './useInventoryWebSockets';

export interface UseLowStockToastsResult {
  status: InventorySocketStatus;
  lastEventAt: string | null;
}

export function useLowStockToasts(): UseLowStockToastsResult {
  const { status, lastEventAt, onLowStockAlert } = useInventoryWebSockets();

  useEffect(
    () =>
      onLowStockAlert((payload) => {
        toast.error(
          `El producto ${payload.sku} requiere reposición inmediata`,
          {
            id: `low-stock-${payload.productId}`,
            duration: 6000,
            style: {
              backgroundColor: '#dc2626',
              color: '#ffffff',
              fontWeight: 600,
            },
          },
        );
      }),
    [onLowStockAlert],
  );

  return { status, lastEventAt };
}
