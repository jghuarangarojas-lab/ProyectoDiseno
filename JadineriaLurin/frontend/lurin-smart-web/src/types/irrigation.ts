export type IrrigationStatus =
  | 'PENDING'
  | 'IN_PROGRESS'
  | 'COMPLETED'
  | 'FAILED'
  | 'CANCELLED';

export type TriggerType = 'AUTOMATIC' | 'MANUAL';

export type CancelState = 'idle' | 'sending' | 'sent' | 'error';

export interface IrrigationAction {
  id: string;
  deviceId: string;
  sectorArea: string;
  triggerType: TriggerType | string;
  requestedDurationSeconds: number;
  maxPumpDurationSeconds: number;
  actualDurationSeconds: number | null;
  status: IrrigationStatus;
  cancellationReason: string | null;
  failureReason: string | null;
  createdAtUtc: string;
  startedAtUtc: string | null;
  finishedAtUtc: string | null;
  updatedAtUtc: string;
}

export interface Sector {
  id: string;
  group: string;
  row: number;
  column: number;
  deviceId: string;
  name: string;
}

export interface StartIrrigationRequest {
  deviceId: string;
  sectorArea: string;
  triggerType: TriggerType;
  requestedDurationSeconds: number;
}

export interface CancelIrrigationRequest {
  reason: string;
}

export type EmergencyReasonCode =
  | 'SOBRE_RIEGO'
  | 'ERROR_HUMANO'
  | 'FALLO_DISPOSITIVO'
  | 'OTROS';

export interface EmergencyReasonOption {
  code: EmergencyReasonCode;
  label: string;
  description: string;
}
