import { useRef, useState } from 'react';
import toast from 'react-hot-toast';
import SignatureCanvas from 'react-signature-canvas';
import { toDeliveryUserMessage } from '../../services/logistics.service';

interface DeliveryPODModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (data: { photo: File | null; signature: File | null }) => Promise<void>;
}

type Tab = 'camera' | 'signature';

export const DeliveryPODModal: React.FC<DeliveryPODModalProps> = ({ isOpen, onClose, onConfirm }) => {
  const [activeTab, setActiveTab] = useState<Tab>('camera');
  const [photo, setPhoto] = useState<File | null>(null);
  const [photoPreview, setPhotoPreview] = useState<string | null>(null);
  const [signatureData, setSignatureData] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const sigPadRef = useRef<SignatureCanvas | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  if (!isOpen) return null;

  const reset = () => {
    setPhoto(null);
    setPhotoPreview(null);
    setSignatureData(null);
    setActiveTab('camera');
    sigPadRef.current?.clear();
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  const handleCapturePhoto = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setPhoto(file);
      const reader = new FileReader();
      reader.onloadend = () => setPhotoPreview(reader.result as string);
      reader.readAsDataURL(file);
    }
  };

  const handleClearSignature = () => {
    sigPadRef.current?.clear();
    setSignatureData(null);
  };

  const handleSaveSignature = () => {
    if (sigPadRef.current && !sigPadRef.current.isEmpty()) {
      setSignatureData(sigPadRef.current.toDataURL('image/png'));
    }
  };

  const dataUrlToFile = (dataUrl: string, filename: string): File => {
    const arr = dataUrl.split(',');
    const mime = arr[0].match(/:(.*?);/)?.[1] || 'image/png';
    const bstr = atob(arr[1]);
    let n = bstr.length;
    const u8arr = new Uint8Array(n);
    while (n--) u8arr[n] = bstr.charCodeAt(n);
    return new File([u8arr], filename, { type: mime });
  };

  const handleConfirm = async () => {
    const sigFile = signatureData ? dataUrlToFile(signatureData, 'firma.png') : null;
    setSubmitting(true);
    try {
      await onConfirm({ photo, signature: sigFile });
      reset();
    } catch (error) {
      toast.error(toDeliveryUserMessage(error));
    } finally {
      setSubmitting(false);
    }
  };

  const canConfirm = Boolean(photo || signatureData);

  const tabClass = (tab: Tab) =>
    `flex-1 px-3 py-2 text-sm font-medium ${
      activeTab === tab
        ? 'border-b-2 border-indigo-600 text-indigo-600'
        : 'text-slate-500 hover:text-slate-700'
    }`;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 px-2">
      <div className="w-full max-w-md rounded-xl bg-white shadow-xl">
        <div className="flex items-center justify-between border-b border-slate-200 px-4 py-3">
          <h3 className="text-base font-semibold text-slate-800">Prueba de Entrega (POD)</h3>
          <button onClick={handleClose} className="text-sm text-slate-500 hover:text-slate-700">
            Cerrar
          </button>
        </div>

        <div className="flex border-b border-slate-200">
          <button onClick={() => setActiveTab('camera')} className={tabClass('camera')}>
            Cámara
          </button>
          <button onClick={() => setActiveTab('signature')} className={tabClass('signature')}>
            Firma
          </button>
        </div>

        <div className="p-4">
          {activeTab === 'camera' && (
            <div className="space-y-3">
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                capture="environment"
                onChange={handleCapturePhoto}
                className="block w-full text-sm text-slate-600 file:mr-3 file:rounded-md file:border-0 file:bg-indigo-600 file:px-3 file:py-2 file:text-sm file:font-medium file:text-white hover:file:bg-indigo-700"
              />
              {photoPreview && (
                <div className="overflow-hidden rounded-lg border border-slate-200">
                  <img src={photoPreview} alt="Foto de entrega" className="h-64 w-full object-cover" />
                </div>
              )}
            </div>
          )}

          {activeTab === 'signature' && (
            <div className="space-y-3">
              <div className="overflow-hidden rounded-lg border border-slate-300 bg-white">
                <SignatureCanvas
                  ref={sigPadRef}
                  canvasProps={{ className: 'w-full h-56 touch-none', width: 320, height: 224 }}
                />
              </div>
              <div className="flex gap-2">
                <button
                  onClick={handleSaveSignature}
                  className="flex-1 rounded-md bg-emerald-600 px-3 py-2 text-sm font-medium text-white active:bg-emerald-700"
                >
                  Guardar firma
                </button>
                <button
                  onClick={handleClearSignature}
                  className="flex-1 rounded-md bg-slate-200 px-3 py-2 text-sm font-medium text-slate-800 active:bg-slate-300"
                >
                  Limpiar
                </button>
              </div>
              {signatureData && (
                <div className="overflow-hidden rounded-lg border border-slate-200">
                  <img src={signatureData} alt="Firma guardada" className="h-32 w-full object-contain" />
                </div>
              )}
            </div>
          )}

          <button
            onClick={handleConfirm}
            disabled={!canConfirm || submitting}
            className="mt-4 w-full rounded-md bg-indigo-600 px-4 py-3 text-base font-semibold text-white disabled:opacity-50 active:bg-indigo-700"
          >
            {submitting ? 'Confirmando...' : 'Confirmar Entrega'}
          </button>
        </div>
      </div>
    </div>
  );
};
