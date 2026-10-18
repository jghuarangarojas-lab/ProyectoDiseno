import { useCallback, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import type { DeliveryOrder, Driver } from '../types/logistics';
import { logisticsService, toDeliveryUserMessage } from '../services/logistics.service';
import { DeliveryPODModal } from '../components/logistics/DeliveryPODModal';
import { FailureReportModal } from '../components/logistics/FailureReportModal';

const DRIVER_STORAGE_KEY = 'lurin.delivery.driverId';

const capLabel: Record<string, string> = {
  SMALL: 'Pequeño',
  MEDIUM: 'Mediano',
  LARGE: 'Grande',
};

const buildNavUrl = (order: DeliveryOrder): string => {
  const query = encodeURIComponent(order.address);
  const isMobile = /android|iphone|ipad|ipod/i.test(navigator.userAgent);
  if (isMobile) return `geo:0,0?q=${query}`;
  return `https://www.google.com/maps/search/?api=1&query=${query}`;
};

export const DriverMobileApp: React.FC = () => {
  const [driverId, setDriverId] = useState<string | null>(
    () => localStorage.getItem(DRIVER_STORAGE_KEY),
  );
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [orders, setOrders] = useState<DeliveryOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState<DeliveryOrder | null>(null);
  const [podOpen, setPodOpen] = useState(false);
  const [failureOpen, setFailureOpen] = useState(false);

  const currentDriver = drivers.find((d) => d.id === driverId) ?? null;

  useEffect(() => {
    logisticsService
      .getDrivers(true)
      .then(setDrivers)
      .catch((error) => toast.error(toDeliveryUserMessage(error)));
  }, []);

  const loadOrders = useCallback(async (id: string) => {
    setLoading(true);
    try {
      const data = await logisticsService.getOrdersByDriver(id);
      setOrders(data.filter((o) => o.status === 'PENDING' || o.status === 'DISPATCHED'));
    } catch (error) {
      toast.error(toDeliveryUserMessage(error));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (driverId) loadOrders(driverId);
    else setLoading(false);
  }, [driverId, loadOrders]);

  const selectDriver = (id: string) => {
    localStorage.setItem(DRIVER_STORAGE_KEY, id);
    setDriverId(id);
  };

  const signOut = () => {
    localStorage.removeItem(DRIVER_STORAGE_KEY);
    setDriverId(null);
    setOrders([]);
  };

  const openNav = (order: DeliveryOrder) => {
    window.open(buildNavUrl(order), '_blank');
  };

  const handleDeliver = (order: DeliveryOrder) => {
    setSelectedOrder(order);
    setPodOpen(true);
  };

  const handleConfirmDelivery = async (data: { photo: File | null; signature: File | null }) => {
    if (!selectedOrder) return;
    await logisticsService.updateStatus(selectedOrder.id, {
      status: 'DELIVERED',
      proofPhoto: data.photo,
      digitalSignature: data.signature,
    });
    toast.success('Entrega confirmada.');
    setPodOpen(false);
    setSelectedOrder(null);
    if (driverId) await loadOrders(driverId);
  };

  const handleReportFailure = (order: DeliveryOrder) => {
    setSelectedOrder(order);
    setFailureOpen(true);
  };

  const handleConfirmFailure = async (reason: string) => {
    if (!selectedOrder) return;
    await logisticsService.updateStatus(selectedOrder.id, {
      status: 'FAILED',
      failureReason: reason,
    });
    toast.success('Problema reportado.');
    setFailureOpen(false);
    setSelectedOrder(null);
    if (driverId) await loadOrders(driverId);
  };

  if (!driverId) {
    return (
      <div className="flex min-h-screen flex-col bg-slate-900 px-4 py-8 text-white">
        <h1 className="text-2xl font-bold">Jardinería Lurín</h1>
        <p className="mt-1 text-slate-300">Selecciona tu usuario de repartidor para comenzar</p>
        <div className="mt-6 flex-1 space-y-3">
          {drivers.length === 0 && <p className="text-center text-slate-400">Cargando repartidores...</p>}
          {drivers.map((d) => (
            <button
              key={d.id}
              onClick={() => selectDriver(d.id)}
              className="w-full rounded-xl border border-slate-700 bg-slate-800 p-4 text-left active:bg-slate-700"
            >
              <p className="text-lg font-semibold">{d.name}</p>
              <p className="text-sm text-slate-400">
                {d.vehicleType} | Capacidad {capLabel[d.maxCapacity]} | {d.activeOrders} pedidos activos
              </p>
            </button>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-900 text-white">
      <div className="sticky top-0 z-10 border-b border-slate-700 bg-slate-900/95 backdrop-blur">
        <div className="flex items-center justify-between px-4 py-4">
          <div>
            <h1 className="text-xl font-bold">Mis Entregas</h1>
            <p className="text-sm text-slate-300">Repartidor: {currentDriver?.name ?? '...'}</p>
          </div>
          <button
            onClick={signOut}
            className="rounded-md px-3 py-2 text-sm font-medium text-slate-300 ring-1 ring-slate-600 active:bg-slate-800"
          >
            Cambiar
          </button>
        </div>
      </div>

      <main className="px-3 py-4">
        {loading ? (
          <p className="text-center text-slate-300">Cargando entregas...</p>
        ) : orders.length === 0 ? (
          <div className="mt-8 rounded-lg bg-slate-800 p-6 text-center">
            <p className="text-lg font-semibold">Sin entregas asignadas</p>
            <p className="mt-1 text-sm text-slate-400">Cuando recibas entregas aparecerán aquí</p>
          </div>
        ) : (
          <div className="space-y-3">
            {orders.map((order) => (
              <div key={order.id} className="rounded-xl border border-slate-700 bg-slate-800 shadow-lg">
                <div className="space-y-2 p-4">
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="text-sm font-semibold text-emerald-400">Venta: {order.saleId}</p>
                      <p className="text-xs text-slate-400">
                        {order.deliveryDate} {order.deliveryTime}
                      </p>
                    </div>
                    <span className="rounded-full bg-blue-900/60 px-2 py-1 text-xs font-medium text-blue-300">
                      {order.status === 'DISPATCHED' ? 'Despachado' : 'Pendiente'}
                    </span>
                  </div>
                  <div>
                    <p className="text-base font-semibold text-white">{order.address}</p>
                    <p className="mt-1 text-sm text-slate-300">Referencia: {order.reference}</p>
                    <p className="mt-1 text-sm text-slate-300">Teléfono: {order.phone}</p>
                    <p className="mt-1 text-sm text-slate-300">Capacidad: {capLabel[order.requiredCapacity]}</p>
                  </div>
                </div>

                <div className="grid grid-cols-1 gap-2 border-t border-slate-700 p-3 sm:grid-cols-3">
                  <button
                    onClick={() => openNav(order)}
                    className="w-full rounded-lg bg-blue-600 px-4 py-4 text-lg font-bold text-white active:bg-blue-700"
                  >
                    Navegar
                  </button>
                  <button
                    onClick={() => handleDeliver(order)}
                    className="w-full rounded-lg bg-emerald-600 px-4 py-4 text-lg font-bold text-white active:bg-emerald-700"
                  >
                    Entregar
                  </button>
                  <button
                    onClick={() => handleReportFailure(order)}
                    className="w-full rounded-lg bg-red-600 px-4 py-4 text-lg font-bold text-white active:bg-red-700"
                  >
                    Reportar Problema
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>

      <DeliveryPODModal
        isOpen={podOpen}
        onClose={() => setPodOpen(false)}
        onConfirm={handleConfirmDelivery}
      />
      <FailureReportModal
        isOpen={failureOpen}
        onClose={() => setFailureOpen(false)}
        onConfirm={handleConfirmFailure}
      />
    </div>
  );
};
