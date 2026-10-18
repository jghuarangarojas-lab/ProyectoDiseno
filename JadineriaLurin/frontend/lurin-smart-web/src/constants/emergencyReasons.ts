import type { EmergencyReasonOption } from '../types/irrigation';

export const EMERGENCY_REASONS: EmergencyReasonOption[] = [
  {
    code: 'SOBRE_RIEGO',
    label: 'Sobreriego',
    description: 'Se aplicó más agua de la que el cultivo necesitaba.',
  },
  {
    code: 'ERROR_HUMANO',
    label: 'Error humano',
    description: 'Configuración o uso incorrecto por parte del operador.',
  },
  {
    code: 'FALLO_DISPOSITIVO',
    label: 'Fallo del dispositivo',
    description: 'La bomba o el dispositivo respondieron de forma anómala.',
  },
  {
    code: 'OTROS',
    label: 'Otros',
    description: 'Otro motivo que no figura en la lista.',
  },
];
