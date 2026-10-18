import React, { useState } from 'react';
import { Modal } from '../common/Modal';
import { Button } from '../common/Button';
import { ToggleSwitch } from '../common/ToggleSwitch';
import { DeliveryForm } from './DeliveryForm';
import { PaymentMethod, DocumentType } from '../../types/pos';
import { useCart } from '../../contexts/CartContext';

interface CheckoutModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (data: CheckoutData) => void;
  isProcessing?: boolean;
}

export interface CheckoutData {
  paymentMethod: PaymentMethod;
  documentType: DocumentType;
  isDelivery: boolean;
  deliveryAddress?: string;
  deliveryDate?: string;
  deliveryTime?: string;
  deliveryPhone?: string;
  deliveryReference?: string;
}

export const CheckoutModal: React.FC<CheckoutModalProps> = ({ isOpen, onClose, onConfirm, isProcessing = false }) => {
  const { total } = useCart();
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Efectivo');
  const [documentType, setDocumentType] = useState<DocumentType>('Boleta');
  const [isDelivery, setIsDelivery] = useState(false);
  const [address, setAddress] = useState('');
  const [date, setDate] = useState('');
  const [time, setTime] = useState('');
  const [phone, setPhone] = useState('');
  const [reference, setReference] = useState('');

  const handleConfirm = () => {
    if (isDelivery) {
      if (!address.trim() || !date || !time || !phone.trim()) {
        alert('Complete todos los campos obligatorios de entrega');
        return;
      }
    }
    onConfirm({
      paymentMethod,
      documentType,
      isDelivery,
      deliveryAddress: address.trim() || undefined,
      deliveryDate: date || undefined,
      deliveryTime: time || undefined,
      deliveryPhone: phone.trim() || undefined,
      deliveryReference: reference.trim() || undefined,
    });
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Proceso de Cobro" maxWidth="max-w-lg">
      <div className="space-y-4">
        <div className="bg-gray-50 p-3 rounded-md">
          <p className="text-sm text-gray-600">Total a pagar</p>
          <p className="text-2xl font-bold text-green-600">S/. {total.toFixed(2)}</p>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Método de Pago
          </label>
          <select
            value={paymentMethod}
            onChange={(e) => setPaymentMethod(e.target.value as PaymentMethod)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
          >
            <option value="Efectivo">Efectivo</option>
            <option value="Tarjeta">Tarjeta</option>
            <option value="Yape">Yape</option>
            <option value="Plin">Plin</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Comprobante SUNAT
          </label>
          <select
            value={documentType}
            onChange={(e) => setDocumentType(e.target.value as DocumentType)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
          >
            <option value="Boleta">Boleta</option>
            <option value="Factura">Factura</option>
          </select>
        </div>
        <ToggleSwitch
          checked={isDelivery}
          onChange={setIsDelivery}
          label="¿Es para Delivery?"
        />
        {isDelivery && (
          <DeliveryForm
            address={address}
            date={date}
            time={time}
            phone={phone}
            reference={reference}
            onAddressChange={setAddress}
            onDateChange={setDate}
            onTimeChange={setTime}
            onPhoneChange={setPhone}
            onReferenceChange={setReference}
          />
        )}
        <div className="flex gap-2 justify-end pt-4">
          <Button variant="secondary" onClick={onClose} disabled={isProcessing}>Cancelar</Button>
          <Button onClick={handleConfirm} disabled={isProcessing}>
            {isProcessing ? 'Procesando...' : 'Confirmar Venta'}
          </Button>
        </div>
      </div>
    </Modal>
  );
};
