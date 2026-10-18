import { useCallback, useEffect, useRef, useState } from 'react';
import toast from 'react-hot-toast';
import { iotService, toUserMessage } from '../services/iot.service';
import type { DeviceAlert } from '../types/iot';

const DEFAULT_POLL_MS = 30000;

interface UseIotAlertsOptions {
  pollMs?: number;
  enabled?: boolean;
}

export interface UseIotAlertsResult {
  alerts: DeviceAlert[];
  isLoading: boolean;
  resolvingId: string | null;
  refresh: (silent?: boolean) => Promise<void>;
  resolveAlert: (alert: DeviceAlert, resolutionNote?: string) => Promise<boolean>;
  upsertAlert: (alert: DeviceAlert) => void;
  removeAlert: (alert: DeviceAlert) => void;
}

export function useIotAlerts(options: UseIotAlertsOptions = {}): UseIotAlertsResult {
  const { pollMs = DEFAULT_POLL_MS, enabled = true } = options;

  const [alerts, setAlerts] = useState<DeviceAlert[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [resolvingId, setResolvingId] = useState<string | null>(null);

  const mountedRef = useRef(true);

  const refresh = useCallback(async (silent = false) => {
    if (!silent) {
      setIsLoading(true);
    }

    try {
      const result = await iotService.getAlerts({ includeResolved: false });

      if (mountedRef.current) {
        setAlerts(result);
      }
    } catch (error) {
      if (!silent) {
        toast.error(toUserMessage(error));
      }
    } finally {
      if (mountedRef.current) {
        setIsLoading(false);
      }
    }
  }, []);

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

  const upsertAlert = useCallback((alert: DeviceAlert) => {
    if (alert.isResolved) {
      setAlerts((previous) => previous.filter((item) => item.id !== alert.id));
      return;
    }

    setAlerts((previous) => {
      const exists = previous.some((item) => item.id === alert.id);

      return exists
        ? previous.map((item) => (item.id === alert.id ? alert : item))
        : [alert, ...previous];
    });
  }, []);

  const removeAlert = useCallback((alert: DeviceAlert) => {
    setAlerts((previous) => previous.filter((item) => item.id !== alert.id));
  }, []);

  const resolveAlert = useCallback(
    async (alert: DeviceAlert, resolutionNote?: string): Promise<boolean> => {
      setResolvingId(alert.id);
      setAlerts((previous) => previous.filter((item) => item.id !== alert.id));

      try {
        const resolved = await iotService.resolveAlert(alert.id, {
          resolutionNote,
        });

        if (mountedRef.current && !resolved.isResolved) {
          setAlerts((previous) => [resolved, ...previous]);
        }

        toast.success(`Alerta de ${alert.deviceCode} marcada como resuelta.`);
        return true;
      } catch (error) {
        if (mountedRef.current) {
          setAlerts((previous) => {
            const exists = previous.some((item) => item.id === alert.id);
            return exists ? previous : [alert, ...previous];
          });
        }

        toast.error(toUserMessage(error));
        return false;
      } finally {
        if (mountedRef.current) {
          setResolvingId(null);
        }
      }
    },
    [],
  );

  return {
    alerts,
    isLoading,
    resolvingId,
    refresh,
    resolveAlert,
    upsertAlert,
    removeAlert,
  };
}
