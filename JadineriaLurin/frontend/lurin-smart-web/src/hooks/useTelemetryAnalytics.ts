import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { iotService } from '../services/iot.service';
import type {
  BatteryChartRow,
  BatterySample,
  HumidityLevelSlice,
  IotDevice,
  TelemetryRecord,
} from '../types/iot';
import {
  HUMIDITY_LEVEL_COLORS,
  formatTimeLabel,
  humidityLevelOf,
} from '../utils/iot';

const MAX_SAMPLES_PER_DEVICE = 60;
const READINGS_POLL_MS = 60000;

export const ALL_AREAS = 'TODAS';

interface UseTelemetryAnalyticsResult {
  latestReadings: Record<string, TelemetryRecord>;
  isLoadingReadings: boolean;
  batteryRows: BatteryChartRow[];
  areas: string[];
  selectedArea: string;
  humiditySlices: HumidityLevelSlice[];
  selectArea: (area: string) => void;
  pushTelemetry: (record: TelemetryRecord) => void;
  refreshReadings: () => Promise<void>;
}

export function useTelemetryAnalytics(
  devices: IotDevice[],
): UseTelemetryAnalyticsResult {
  const [latestReadings, setLatestReadings] = useState<
    Record<string, TelemetryRecord>
  >({});
  const [batterySeries, setBatterySeries] = useState<
    Record<string, BatterySample[]>
  >({});
  const [isLoadingReadings, setIsLoadingReadings] = useState(false);
  const [selectedArea, setSelectedArea] = useState<string>(ALL_AREAS);

  const devicesRef = useRef<IotDevice[]>(devices);

  useEffect(() => {
    devicesRef.current = devices;
  }, [devices]);

  const refreshReadings = useCallback(async () => {
    const currentDevices = devicesRef.current;

    if (currentDevices.length === 0) {
      setLatestReadings({});
      return;
    }

    setIsLoadingReadings(true);

    try {
      const results = await Promise.all(
        currentDevices.map(async (device) => {
          try {
            const records = await iotService.getTelemetry(device.deviceCode, 1);
            return records[0] ?? null;
          } catch {
            return null;
          }
        }),
      );

      const next: Record<string, TelemetryRecord> = {};

      for (const record of results) {
        if (record) {
          next[record.deviceCode] = record;
        }
      }

      setLatestReadings((previous) => ({ ...previous, ...next }));
    } finally {
      setIsLoadingReadings(false);
    }
  }, []);

  useEffect(() => {
    if (devices.length === 0) {
      return;
    }

    const hasUnreadSensor = devices.some(
      (device) => !(device.deviceCode in latestReadings),
    );

    if (hasUnreadSensor) {
      void refreshReadings();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [devices, refreshReadings]);

  useEffect(() => {
    if (devices.length === 0) {
      return;
    }

    const intervalId = window.setInterval(() => {
      void refreshReadings();
    }, READINGS_POLL_MS);

    return () => window.clearInterval(intervalId);
  }, [devices.length > 0, refreshReadings]);

  useEffect(() => {
    if (devices.length === 0) {
      return;
    }

    setBatterySeries((previous) => {
      const next = { ...previous };
      let changed = false;

      for (const device of devices) {
        const samples = next[device.deviceCode] ?? [];
        const lastSample = samples[samples.length - 1];

        if (lastSample?.batteryPercentage === device.batteryPercentage) {
          continue;
        }

        const recordedAtMs = Date.now();
        const sample: BatterySample = {
          recordedAtMs,
          label: formatTimeLabel(recordedAtMs),
          batteryPercentage: device.batteryPercentage,
        };

        next[device.deviceCode] = [
          ...samples,
          sample,
        ].slice(-MAX_SAMPLES_PER_DEVICE);
        changed = true;
      }

      return changed ? next : previous;
    });
  }, [devices]);

  const pushTelemetry = useCallback((record: TelemetryRecord) => {
    setLatestReadings((previous) => {
      const existing = previous[record.deviceCode];

      if (
        existing &&
        new Date(existing.recordedAtUtc).getTime() >=
          new Date(record.recordedAtUtc).getTime()
      ) {
        return previous;
      }

      return { ...previous, [record.deviceCode]: record };
    });
  }, []);

  const batteryRows = useMemo<BatteryChartRow[]>(() => {
    const codes = Object.keys(batterySeries);

    if (codes.length === 0) {
      return [];
    }

    const rowsByLabel = new Map<string, BatteryChartRow>();

    for (const code of codes) {
      for (const sample of batterySeries[code]) {
        const row = rowsByLabel.get(sample.label) ?? ({ time: sample.label } as BatteryChartRow);

        for (const other of codes) {
          if (!(other in row)) {
            (row as any)[other] = null;
          }
        }

        (row as any)[code] = sample.batteryPercentage;
        rowsByLabel.set(sample.label, row);
      }
    }

    return Array.from(rowsByLabel.values()).slice(-30);
  }, [batterySeries]);

  const areas = useMemo(
    () => [...new Set(devices.map((device) => device.currentSectorArea))].sort(),
    [devices],
  );

  const humiditySlices = useMemo<HumidityLevelSlice[]>(() => {
    const scope = devices.filter(
      (device) =>
        selectedArea === ALL_AREAS ||
        device.currentSectorArea === selectedArea,
    );

    const counts: Record<string, number> = {
      'Crítico': 0,
      Seco: 0,
      'Óptimo': 0,
      Excedido: 0,
    };

    for (const device of scope) {
      const record = latestReadings[device.deviceCode];

      if (!record) {
        continue;
      }

      const level = humidityLevelOf(record.humidityPercentage, device);
      counts[level] += 1;
    }

    return Object.entries(counts)
      .filter(([, value]) => value > 0)
      .map(([name, value]) => ({
        name,
        value,
        color: HUMIDITY_LEVEL_COLORS[name as keyof typeof HUMIDITY_LEVEL_COLORS],
      }));
  }, [devices, latestReadings, selectedArea]);

  return {
    latestReadings,
    isLoadingReadings,
    batteryRows,
    areas,
    selectedArea,
    humiditySlices,
    selectArea: setSelectedArea,
    pushTelemetry,
    refreshReadings,
  };
}
