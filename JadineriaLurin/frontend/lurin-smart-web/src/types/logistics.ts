export type VehicleType = 'MOTORCYCLE' | 'VAN' | 'TRUCK';

export type MaxCapacity = 'SMALL' | 'MEDIUM' | 'LARGE';

export type DeliveryStatus = 'PENDING' | 'DISPATCHED' | 'DELIVERED' | 'FAILED';

export interface Driver {
  id: string;
  name: string;
  phone: string;
  vehicleType: VehicleType;
  maxCapacity: MaxCapacity;
  isActive: boolean;
  activeOrders: number;
  createdAt: string;
  updatedAt: string;
}

export interface DeliveryOrder {
  id: string;
  saleId: string;
  address: string;
  deliveryDate: string;
  deliveryTime: string;
  phone: string;
  reference: string;
  requiredCapacity: MaxCapacity;
  status: DeliveryStatus;
  driverId: string | null;
  driverName: string | null;
  vehicleType: string | null;
  failureReason: string | null;
  proofPhotoUrl: string | null;
  digitalSignatureUrl: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface DeliveryOrderPage {
  items: DeliveryOrder[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface DeliveryOrderQuery {
  status?: DeliveryStatus;
  driverId?: string;
  deliveryDate?: string;
  page?: number;
  pageSize?: number;
}

export interface UpdateDeliveryStatusDto {
  status: DeliveryStatus;
  failureReason?: string | null;
  proofPhoto?: File | null;
  digitalSignature?: File | null;
}
