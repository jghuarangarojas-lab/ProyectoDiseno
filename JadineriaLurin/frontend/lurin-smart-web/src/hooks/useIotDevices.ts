import { useCallback, useEffect, useRef, useState } from 'react';
import toast from 'react-hot-toast';
import { iotService, toUserMessage } from '../services/iot.service';
import type { BotanicalThreshold, IotDevice, UpdateThresholdRequest } from '../types/iot';

const DEFAULT_POLL_MS = 20000;

interface UseIotDevicesOptions {
  pollMs?: number;
  enabled?: boolean;
}

export interface UseIotDevicesResult {
  devices: IotDevice[];
  isLoading: boolean;
  isRefreshing: boolean;
  lastLoadedAt: string | null;
  refresh: (silent?: boolean) => Promise<void>;
  replaceDevice: (device: IotDevice) => void;
  relocateSensor: (deviceCode: string, targetArea: string) => Promise<boolean>;
  updateThreshold: (
    deviceCode: string,
    payload: UpdateThresholdRequest,
  ) => Promise<BotanicalThreshold>;
}

export function useIotDevices(
  options: UseIotDevicesOptions = {},
): UseIotDevicesResult {
  const { pollMs = DEFAULT_POLL_MS, enabled = true } = options;

  const [devices, setDevices] = useState<IotDevice[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [lastLoadedAt, setLastLoadedAt] = useState<string | null>(null);

  const mountedRef = useRef(true);
  const devicesRef = useRef<IotDevice[]>([]);

  useEffect(() => {
    devicesRef.current = devices;
  }, [devices]);

  const refresh = useCallback(
    async (silent = false) => {
      if (!silent) {
        setIsRefreshing(true);
      }

      try {
        const result = await iotService.getDevices();

        if (!mountedRef.current) {
          return;
        }

        setDevices(result);
        setLastLoadedAt(new Date().toISOString());
      } catch (error) {
        if (!silent) {
          toast.error(toUserMessage(error));
        }
      } finally {
        if (mountedRef.current && !silent) {
          setIsRefreshing(false);
        }

        if (mountedRef.current) {
          setIsLoading(false);
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

  const replaceDevice = useCallback((device: IotDevice) => {
    setDevices((previous) => {
      const exists = previous.some(
        (item) => item.deviceCode === device.deviceCode,
      );

      return exists
        ? previous.map((item) =>
            item.deviceCode === device.deviceCode ? device : item,
          )
        : [...previous, device];
    });

    setLastLoadedAt(new Date().toISOString());
  }, []);

  const relocateSensor = useCallback(
    async (deviceCode: string, targetArea: string): Promise<boolean> => {
      const previousDevices = devicesRef.current;

      setDevices((current) =>
        current.map((item) =>
          item.deviceCode === deviceCode
            ? { ...item, currentSectorArea: targetArea }
            : item,
        ),
      );

      try {
        const updated = await iotService.relocate(deviceCode, targetArea);
        replaceDevice(updated);
        toast.success(`${deviceCode} reubicado en ${targetArea}.`);
        return true;
      } catch (error) {
        setDevices(previousDevices);
        toast.error(toUserMessage(error));
        return false;
      }
    },
    [replaceDevice],
  );

  const updateThreshold = useCallback(
    async (deviceCode: string, payload: UpdateThresholdRequest) => {
      const threshold = await iotService.updateThreshold(deviceCode, payload);

      setDevices((previous) =>
        previous.map((item) =>
          item.deviceCode === deviceCode ? { ...item, threshold } : item,
        ),
      );

      toast.success(`Umbrales actualizados para ${deviceCode}.`);
      return threshold;
    },
    [],
  );

  return {
    devices,
    isLoading,
    isRefreshing,
    lastLoadedAt,
    refresh,
    replaceDevice,
    relocateSensor,
    updateThreshold,
  };
}
