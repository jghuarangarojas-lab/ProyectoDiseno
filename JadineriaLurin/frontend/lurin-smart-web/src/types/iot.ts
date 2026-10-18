export type DeviceStatus = 'ONLINE' | 'OFFLINE' | 'MAINTENANCE' | string;

export type AlertIssueType =
  | 'LOW_BATTERY'
  | 'HARDWARE_DISCONNECTED'
  | 'IRRIGATION_REQUEST_FAILED'
  | string;

export interface BotanicalThreshold {
  deviceCode: string;
  minHumidityPercentage: number;
  maxHumidityPercentage: number;
  minTemperatureCelsius: number;
  maxTemperatureCelsius: number;
  autoIrrigationSeconds: number;
  updatedAtUtc: string;
}

export interface IotDevice {
  deviceCode: string;
  macAddress: string;
  currentSectorArea: string;
  status: DeviceStatus;
  batteryPercentage: number;
  isBatteryLow: boolean;
  lastSeenAtUtc: string | null;
  lastIrrigationRequestedAtUtc: string | null;
  updatedAtUtc: string;
  threshold: BotanicalThreshold | null;
}

export interface TelemetryRecord {
  id: string;
  deviceCode: string;
  recordedAtUtc: string;
  humidityPercentage: number;
  temperatureCelsius: number;
}

export interface DeviceAlert {
  id: string;
  deviceCode: string;
  issueType: AlertIssueType;
  message: string;
  isResolved: boolean;
  createdAtUtc: string;
  resolvedAtUtc: string | null;
  resolutionNote: string | null;
}

export interface RelocateSensorRequest {
  currentSectorArea: string;
}

export interface UpdateThresholdRequest {
  minHumidityPercentage?: number | null;
  maxHumidityPercentage?: number | null;
  minTemperatureCelsius?: number | null;
  maxTemperatureCelsius?: number | null;
  autoIrrigationSeconds?: number | null;
}

export interface ResolveAlertRequest {
  resolutionNote?: string;
}

export type PotWaterState =
  | 'empty'
  | 'disconnected'
  | 'normal'
  | 'danger'
  | 'critical';

export interface PotStateMeta {
  state: PotWaterState;
  label: string;
  cardClassName: string;
  potFill: string;
  potRimFill: string;
  plantFill: string;
  soilFill: string;
  accentText: string;
}

export interface BatterySample {
  recordedAtMs: number;
  label: string;
  batteryPercentage: number;
}

export type BatteryChartRow = Record<string, number | null> & { time: string };

export interface HumidityLevelSlice {
  name: string;
  value: number;
  color: string;
}
