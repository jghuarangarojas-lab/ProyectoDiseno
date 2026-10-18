import { useCallback, useEffect, useRef, useState } from 'react';
import { INVENTORY_HUB_URL } from '../config/api';
import type { LowStockAlertPayload } from '../types/inventory';

export type InventorySocketStatus =
  | 'connecting'
  | 'connected'
  | 'reconnecting'
  | 'disconnected';

export type LowStockHandler = (payload: LowStockAlertPayload) => void;

interface UseInventoryWebSocketsOptions {
  url?: string;
}

export interface UseInventoryWebSocketsResult {
  status: InventorySocketStatus;
  lastEventAt: string | null;
  onLowStockAlert: (handler: LowStockHandler) => () => void;
}

export function useInventoryWebSockets(
  options: UseInventoryWebSocketsOptions = {},
): UseInventoryWebSocketsResult {
  const { url = INVENTORY_HUB_URL } = options;

  const [status, setStatus] = useState<InventorySocketStatus>('connecting');
  const [lastEventAt, setLastEventAt] = useState<string | null>(null);

  const lowStockHandlersRef = useRef<Set<LowStockHandler>>(new Set());

  const onLowStockAlert = useCallback((handler: LowStockHandler) => {
    lowStockHandlersRef.current.add(handler);

    return () => {
      lowStockHandlersRef.current.delete(handler);
    };
  }, []);

  useEffect(() => {
    let disposed = false;
    let connection: { stop: () => Promise<void> } | undefined;

    setStatus('connecting');

    const connect = async () => {
      const signalr = await import('@microsoft/signalr');

      const hubConnection = new signalr.HubConnectionBuilder()
        .withUrl(url)
        .withAutomaticReconnect()
        .configureLogging(signalr.LogLevel.Warning)
        .build();

      connection = hubConnection;

      hubConnection.on('LowStockAlert', (payload: LowStockAlertPayload) => {
        if (disposed) {
          return;
        }

        setLastEventAt(new Date().toISOString());
        lowStockHandlersRef.current.forEach((handler) => handler(payload));
      });

      hubConnection.onreconnecting(() => {
        if (!disposed) {
          setStatus('reconnecting');
        }
      });

      hubConnection.onreconnected(() => {
        if (!disposed) {
          setStatus('connected');
        }
      });

      hubConnection.onclose(() => {
        if (!disposed) {
          setStatus('disconnected');
        }
      });

      await hubConnection.start();

      if (disposed) {
        await hubConnection.stop();
        return;
      }

      setStatus('connected');
    };

    connect().catch(() => {
      if (!disposed) {
        setStatus('disconnected');
      }
    });

    return () => {
      disposed = true;
      void connection?.stop();
    };
  }, [url]);

  return {
    status,
    lastEventAt,
    onLowStockAlert,
  };
}
