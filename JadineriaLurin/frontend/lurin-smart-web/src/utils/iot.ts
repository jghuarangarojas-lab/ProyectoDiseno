import type { IotDevice, PotStateMeta, PotWaterState, TelemetryRecord } from '../types/iot';

export const CRITICAL_HUMIDITY_RATIO = 0.5;

export const POT_STATE_META: Record<PotWaterState, PotStateMeta> = {
  empty: {
    state: 'empty',
    label: 'Sin sensor',
    cardClassName: 'border-dashed border-slate-300 bg-slate-50',
    potFill: '#e2e8f0',
    potRimFill: '#cbd5e1',
    plantFill: '#cbd5e1',
    soilFill: '#f1f5f9',
    accentText: 'text-slate-500',
  },
  disconnected: {
    state: 'disconnected',
    label: 'Desconectado',
    cardClassName: 'border-slate-400 bg-slate-200',
    potFill: '#cbd5e1',
    potRimFill: '#94a3b8',
    plantFill: '#94a3b8',
    soilFill: '#e2e8f0',
    accentText: 'text-slate-700',
  },
  normal: {
    state: 'normal',
    label: 'Humedad óptima',
    cardClassName: 'border-sky-400 bg-sky-100',
    potFill: '#38bdf8',
    potRimFill: '#0284c7',
    plantFill: '#4ade80',
    soilFill: '#bae6fd',
    accentText: 'text-sky-800',
  },
  danger: {
    state: 'danger',
    label: 'Peligro por umbral',
    cardClassName: 'border-yellow-400 bg-yellow-100',
    potFill: '#facc15',
    potRimFill: '#ca8a04',
    plantFill: '#a3e635',
    soilFill: '#fef08a',
    accentText: 'text-yellow-800',
  },
  critical: {
    state: 'critical',
    label: 'Seco crítico',
    cardClassName: 'border-red-500 bg-red-100',
    potFill: '#ef4444',
    potRimFill: '#b91c1c',
    plantFill: '#f87171',
    soilFill: '#fecaca',
    accentText: 'text-red-800',
  },
};

export function resolvePotState(
  device: IotDevice | null | undefined,
  reading: TelemetryRecord | null | undefined,
): PotStateMeta {
  if (!device) {
    return POT_STATE_META.empty;
  }

  if (device.status !== 'ONLINE') {
    return POT_STATE_META.disconnected;
  }

  if (!reading) {
    return POT_STATE_META.disconnected;
  }

  const humidity = reading.humidityPercentage;
  const minimum = device.threshold?.minHumidityPercentage ?? null;
  const maximum = device.threshold?.maxHumidityPercentage ?? null;

  if (minimum === null || maximum === null) {
    return POT_STATE_META.normal;
  }

  if (humidity <= 0 || humidity <= minimum * CRITICAL_HUMIDITY_RATIO) {
    return POT_STATE_META.critical;
  }

  if (humidity < minimum || humidity > maximum) {
    return POT_STATE_META.danger;
  }

  return POT_STATE_META.normal;
}

export type HumidityLevel = 'Crítico' | 'Seco' | 'Óptimo' | 'Excedido';

export function humidityLevelOf(
  humidity: number,
  device: IotDevice | null | undefined,
): HumidityLevel {
  const minimum = device?.threshold?.minHumidityPercentage ?? 0;
  const maximum = device?.threshold?.maxHumidityPercentage ?? 100;

  if (humidity <= 0 || (minimum > 0 && humidity <= minimum * CRITICAL_HUMIDITY_RATIO)) {
    return 'Crítico';
  }

  if (humidity < minimum) {
    return 'Seco';
  }

  if (humidity > maximum) {
    return 'Excedido';
  }

  return 'Óptimo';
}

export const HUMIDITY_LEVEL_COLORS: Record<HumidityLevel, string> = {
  'Crítico': '#ef4444',
  Seco: '#facc15',
  'Óptimo': '#38bdf8',
  Excedido: '#8b5cf6',
};

export const deviceStatusLabel = (status: string): string => {
  if (status === 'ONLINE') {
    return 'En línea';
  }

  if (status === 'MAINTENANCE') {
    return 'Mantenimiento';
  }

  return 'Desconectado';
};

export interface AlertIssueMeta {
  label: string;
  badgeClassName: string;
  dotClassName: string;
}

export const ALERT_ISSUE_META: Record<string, AlertIssueMeta> = {
  LOW_BATTERY: {
    label: 'Batería baja',
    badgeClassName: 'bg-red-50 text-red-700 ring-red-200',
    dotClassName: 'bg-red-500',
  },
  HARDWARE_DISCONNECTED: {
    label: 'Hardware desconectado · Last Will',
    badgeClassName: 'bg-slate-100 text-slate-700 ring-slate-300',
    dotClassName: 'bg-slate-500',
  },
  IRRIGATION_REQUEST_FAILED: {
    label: 'Fallo al solicitar riego',
    badgeClassName: 'bg-amber-50 text-amber-700 ring-amber-200',
    dotClassName: 'bg-amber-500',
  },
};

export const alertIssueMeta = (issueType: string): AlertIssueMeta =>
  ALERT_ISSUE_META[issueType] ?? {
    label: issueType,
    badgeClassName: 'bg-rose-50 text-rose-700 ring-rose-200',
    dotClassName: 'bg-rose-500',
  };

export function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return '—';
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return '—';
  }

  return new Intl.DateTimeFormat('es-PE', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date);
}

export function formatTimeLabel(valueMs: number): string {
  return new Intl.DateTimeFormat('es-PE', {
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(valueMs));
}

export const batteryBarClassName = (battery: number, isLow: boolean): string => {
  if (isLow || battery < 15) {
    return 'bg-red-500';
  }

  if (battery < 50) {
    return 'bg-amber-500';
  }

  return 'bg-emerald-500';
};
