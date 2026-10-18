import React from 'react';
import { Button } from '../common/Button';
import { SaleResponse } from '../../types/pos';

interface SuccessScreenProps {
  sale: SaleResponse | null;
  onNewSale: () => void;
  onPrint: () => void;
}

export const SuccessScreen: React.FC<SuccessScreenProps> = ({ sale, onNewSale, onPrint }) => {
  if (!sale) return null;

  return (
    <div className="flex items-center justify-center min-h-[500px]">
      <div className="bg-white rounded-lg shadow-lg p-8 max-w-md w-full text-center">
        <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h2 className="text-2xl font-bold text-gray-900 mb-2">¡Venta Exitosa!</h2>
        <p className="text-gray-600 mb-6">La venta se ha procesado correctamente</p>
        <div className="bg-gray-50 rounded-md p-4 mb-6 text-left space-y-2">
          <div className="flex justify-between">
            <span className="text-gray-600">ID Venta:</span>
            <span className="font-mono text-sm">{sale.saleId.substring(0, 8)}...</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600">Comprobante:</span>
            <span>{sale.documentType} - {sale.documentCorrelative}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600">Total:</span>
            <span className="font-bold">S/. {sale.totalAmount.toFixed(2)}</span>
          </div>
          {sale.sunatStatus && (
            <div className="flex justify-between">
              <span className="text-gray-600">SUNAT:</span>
              <span className="text-green-600">{sale.sunatStatus}</span>
            </div>
          )}
        </div>
        <div className="flex gap-2">
          <Button
            variant="secondary"
            className="flex-1"
            onClick={onPrint}
          >
            Imprimir Ticket/Boleta PDF
          </Button>
          <Button
            className="flex-1"
            onClick={onNewSale}
          >
            Nueva Venta
          </Button>
        </div>
      </div>
    </div>
  );
};
