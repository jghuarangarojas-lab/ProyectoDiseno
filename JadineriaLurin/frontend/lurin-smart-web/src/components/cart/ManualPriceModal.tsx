import React, { useState } from 'react';
import { Modal } from '../common/Modal';
import { Button } from '../common/Button';
import { CartItem } from '../../types/pos';

interface ManualPriceModalProps {
  isOpen: boolean;
  item: CartItem | null;
  onClose: () => void;
  onConfirm: (itemId: string, newPrice: number, reason: string) => void;
}

export const ManualPriceModal: React.FC<ManualPriceModalProps> = ({ isOpen, item, onClose, onConfirm }) => {
  const [price, setPrice] = useState(item?.finalPrice.toString() || '');
  const [reason, setReason] = useState('');

  React.useEffect(() => {
    if (item) {
      setPrice(item.finalPrice.toString());
      setReason('');
    }
  }, [item]);

  const handleConfirm = () => {
    const numPrice = parseFloat(price);
    if (isNaN(numPrice) || numPrice < 0) {
      alert('Ingrese un precio válido');
      return;
    }
    if (!reason.trim()) {
      alert('OBLIGATORIO: Escriba la razón del ajuste manual');
      return;
    }
    if (item) {
      onConfirm(item.id, numPrice, reason.trim());
      onClose();
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Ajuste Manual de Precio">
      <div className="space-y-4">
        {item && (
          <div className="bg-gray-50 p-3 rounded-md">
            <p className="font-medium">{item.product.name}</p>
            <p className="text-sm text-gray-600">Precio actual: S/. {item.finalPrice.toFixed(2)}</p>
          </div>
        )}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Nuevo Precio (S/.)
          </label>
          <input
            type="number"
            step="0.01"
            min="0"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
            placeholder="0.00"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Razón del ajuste <span className="text-red-600">*</span>
          </label>
          <input
            type="text"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
            placeholder="Ej. Descuento pre-acordado por el gerente"
          />
          <p className="text-xs text-red-600 mt-1">Este campo es OBLIGATORIO</p>
        </div>
        <div className="flex gap-2 justify-end pt-4">
          <Button variant="secondary" onClick={onClose}>Cancelar</Button>
          <Button onClick={handleConfirm}>Confirmar</Button>
        </div>
      </div>
    </Modal>
  );
};
