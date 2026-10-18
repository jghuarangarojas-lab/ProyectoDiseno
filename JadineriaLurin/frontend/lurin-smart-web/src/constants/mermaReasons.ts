import type { MermaReasonOption } from '../types/inventory';

export const MERMA_REASONS: MermaReasonOption[] = [
  {
    code: 'PLAGA',
    label: 'Plaga',
    description: 'Presencia de plagas que dañaron el producto.',
  },
  {
    code: 'DESHIDRATACION',
    label: 'Deshidratación',
    description: 'Pérdida de humedad por falta de riego o exposición.',
  },
  {
    code: 'DANO_FISICO',
    label: 'Daño físico',
    description: 'Roturas, golpes o mal manejo durante el almacenamiento.',
  },
  {
    code: 'ENFERMEDAD',
    label: 'Enfermedad',
    description: 'Infección o enfermedad fitosanitaria detectada.',
  },
  {
    code: 'VENCIMIENTO',
    label: 'Vencimiento',
    description: 'El insumo superó su fecha de vencimiento.',
  },
  {
    code: 'OTROS',
    label: 'Otros',
    description: 'Otro motivo que no figura en la lista.',
  },
];

export const PRODUCT_CATEGORIES = ['Plantas', 'Fertilizantes', 'Semillas'] as const;
