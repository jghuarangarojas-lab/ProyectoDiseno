import { useCallback, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import type { DeliveryOrder, Driver } from '../types/logistics';
import { logisticsService, toDeliveryUserMessage } from '../services/logistics.service';

const statusLabel: Record<string, string> = {
  PENDING: 'Pendiente',
  DISPATCHED: 'Despachado',
  DELIVERED: 'Entregado',
  FAILED: 'Fallido',
};

const statusBadge: Record<string, string> = {
  PENDING: 'bg-amber-50 text-amber-700 ring-amber-200',
  DISPATCHED: 'bg-blue-50 text-blue-700 ring-blue-200',
  DELIVERED: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  FAILED: 'bg-red-50 text-red-700 ring-red-200',
};

const capLabel: Record<string, string> = {
  SMALL: 'Pequeño',
  MEDIUM: 'Mediano',
  LARGE: 'Grande',
};

const vehicleLabel: Record<string, string> = {
  MOTORCYCLE: 'Motocicleta',
  VAN: 'Camioneta',
  TRUCK: 'Camión',
};

export const LogisticsDashboard: React.FC = () => {
  const [orders, setOrders] = useState<DeliveryOrder[]>([]);
  const [drivers, setDrivers] = useState<Driver[]>([]);
  const [loading, setLoading] = useState(true);
  const [reassigning, setReassigning] = useState<string | null>(null);

  const activeDrivers = drivers.filter((d) => d.isActive);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [activeOrders, driverList] = await Promise.all([
        logisticsService.getActiveOrders(),
        logisticsService.getDrivers(),
      ]);
      setOrders(activeOrders);
      setDrivers(driverList);
    } catch (error) {
      toast.error(toDeliveryUserMessage(error));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleReassign = async (orderId: string, driverId: string) => {
    if (!driverId) return;
    setReassigning(orderId);
    try {
      await logisticsService.reassignOrder(orderId, driverId);
      toast.success('Orden reasignada correctamente.');
      await loadData();
    } catch (error) {
      toast.error(toDeliveryUserMessage(error));
      await loadData();
    } finally {
      setReassigning(null);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 p-4 md:p-6">
      <div className="mx-auto max-w-7xl">
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-2xl font-bold text-slate-800">Dashboard Logístico - Administración</h1>
          <button
            onClick={loadData}
            disabled={loading}
            className="rounded-md bg-white px-4 py-2 text-sm font-medium text-slate-700 ring-1 ring-slate-300 hover:bg-slate-100 disabled:opacity-50"
          >
            Actualizar
          </button>
        </div>

        {loading ? (
          <p className="text-slate-600">Cargando órdenes y repartidores...</p>
        ) : (
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <div className="rounded-lg bg-white shadow-sm">
              <div className="border-b border-slate-200 px-4 py-3">
                <h2 className="text-lg font-semibold text-slate-800">Órdenes Pendientes/Despachadas</h2>
              </div>
              <div className="divide-y divide-slate-100">
                {orders.length === 0 ? (
                  <p className="px-4 py-6 text-sm text-slate-500">No hay órdenes pendientes o despachadas</p>
                ) : (
                  orders.map((order) => (
                    <div key={order.id} className="px-4 py-4">
                      <div className="mb-2 flex flex-wrap items-start justify-between gap-2">
                        <div>
                          <p className="text-sm font-semibold text-slate-800">Venta: {order.saleId}</p>
                          <p className="text-xs text-slate-500">
                            {order.deliveryDate} {order.deliveryTime}
                          </p>
                        </div>
                        <span
                          className={`rounded-full px-2 py-1 text-xs font-medium ring-1 ${statusBadge[order.status]}`}
                        >
                          {statusLabel[order.status]}
                        </span>
                      </div>
                      <p className="text-sm text-slate-700">{order.address}</p>
                      <p className="mt-1 text-xs text-slate-500">Referencia: {order.reference}</p>
                      <p className="mt-1 text-xs text-slate-500">
                        Capacidad requerida: {capLabel[order.requiredCapacity]}
                      </p>
                      <div className="mt-3 flex flex-wrap items-center gap-2">
                        <span className="text-xs text-slate-600">
                          Asignado a: {order.driverName || 'Sin asignar'}
                        </span>
                        <select
                          className="rounded-md border border-slate-300 px-2 py-1 text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500"
                          value=""
                          disabled={reassigning === order.id || activeDrivers.length === 0}
                          onChange={(e) => handleReassign(order.id, e.target.value)}
                        >
                          <option value="">Reasignar...</option>
                          {activeDrivers
                            .filter((d) => d.id !== order.driverId)
                            .map((d) => (
                              <option key={d.id} value={d.id}>
                                {d.name} ({capLabel[d.maxCapacity]})
                              </option>
                            ))}
                        </select>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>

            <div className="rounded-lg bg-white shadow-sm">
              <div className="border-b border-slate-200 px-4 py-3">
                <h2 className="text-lg font-semibold text-slate-800">Repartidores Disponibles</h2>
              </div>
              <div className="divide-y divide-slate-100">
                {activeDrivers.length === 0 ? (
                  <p className="px-4 py-6 text-sm text-slate-500">No hay repartidores activos</p>
                ) : (
                  activeDrivers.map((d) => (
                    <div key={d.id} className="flex items-center justify-between px-4 py-3">
                      <div>
                        <p className="text-sm font-semibold text-slate-800">{d.name}</p>
                        <p className="text-xs text-slate-500">
                          Vehículo: {vehicleLabel[d.vehicleType] ?? d.vehicleType} | Capacidad máx:{' '}
                          {capLabel[d.maxCapacity]} | Pedidos activos: {d.activeOrders}
                        </p>
                      </div>
                      <span className="rounded-full bg-emerald-100 px-2 py-1 text-xs font-medium text-emerald-800">
                        Activo
                      </span>
                    </div>
                  ))
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
