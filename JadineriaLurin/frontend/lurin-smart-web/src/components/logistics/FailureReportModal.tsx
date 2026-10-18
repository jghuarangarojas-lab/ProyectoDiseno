import { useState } from 'react';
import toast from 'react-hot-toast';
import { toDeliveryUserMessage } from '../../services/logistics.service';

interface FailureReportModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (failureReason: string) => Promise<void>;
}

const failureReasons = ['Cliente ausente', 'Dirección incorrecta', 'Vehículo averiado'];

export const FailureReportModal: React.FC<FailureReportModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
}) => {
  const [reason, setReason] = useState<string>('');
  const [submitting, setSubmitting] = useState(false);

  if (!isOpen) return null;

  const handleClose = () => {
    setReason('');
    onClose();
  };

  const handleConfirm = async () => {
    if (!reason) return;
    setSubmitting(true);
    try {
      await onConfirm(reason);
      setReason('');
    } catch (error) {
      toast.error(toDeliveryUserMessage(error));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 px-2">
      <div className="w-full max-w-md rounded-xl bg-white shadow-xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-4 py-3">
          <h3 className="text-base font-semibold text-slate-800">Reportar Problema</h3>
          <button onClick={handleClose} className="text-sm text-slate-500 hover:text-slate-700">
            Cerrar
          </button>
        </div>
        <div className="space-y-3 p-4">
          <label className="block text-sm font-medium text-slate-700">
            Motivo de falla <span className="text-red-600">*</span>
          </label>
          <select
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            className="w-full rounded-md border border-slate-300 px-3 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-red-500"
          >
            <option value="">Seleccione un motivo</option>
            {failureReasons.map((r) => (
              <option key={r} value={r}>
                {r}
              </option>
            ))}
          </select>
          <button
            onClick={handleConfirm}
            disabled={!reason || submitting}
            className="w-full rounded-md bg-red-600 px-4 py-3 text-base font-semibold text-white disabled:opacity-50 active:bg-red-700"
          >
            {submitting ? 'Guardando...' : 'Confirmar y Marcar como FALLIDO'}
          </button>
        </div>
      </div>
    </div>
  );
};
