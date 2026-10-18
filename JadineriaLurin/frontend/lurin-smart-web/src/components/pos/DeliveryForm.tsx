import React from 'react';

interface DeliveryFormProps {
  address: string;
  date: string;
  time: string;
  phone: string;
  reference: string;
  onAddressChange: (value: string) => void;
  onDateChange: (value: string) => void;
  onTimeChange: (value: string) => void;
  onPhoneChange: (value: string) => void;
  onReferenceChange: (value: string) => void;
}

export const DeliveryForm: React.FC<DeliveryFormProps> = ({
  address,
  date,
  time,
  phone,
  reference,
  onAddressChange,
  onDateChange,
  onTimeChange,
  onPhoneChange,
  onReferenceChange,
}) => {
  return (
    <div className="space-y-3 mt-4 p-4 bg-gray-50 rounded-md border border-gray-200">
      <h3 className="font-medium text-gray-900">Datos de Entrega</h3>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Dirección <span className="text-red-600">*</span>
        </label>
        <input
          type="text"
          value={address}
          onChange={(e) => onAddressChange(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
          placeholder="Dirección completa"
        />
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Fecha de Entrega <span className="text-red-600">*</span>
          </label>
          <input
            type="date"
            value={date}
            onChange={(e) => onDateChange(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Hora de Entrega <span className="text-red-600">*</span>
          </label>
          <input
            type="time"
            value={time}
            onChange={(e) => onTimeChange(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
          />
        </div>
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Teléfono <span className="text-red-600">*</span>
        </label>
        <input
          type="tel"
          value={phone}
          onChange={(e) => onPhoneChange(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
          placeholder="987654321"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Referencia
        </label>
        <input
          type="text"
          value={reference}
          onChange={(e) => onReferenceChange(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-green-500"
          placeholder="Referencia de la dirección"
        />
      </div>
    </div>
  );
};
