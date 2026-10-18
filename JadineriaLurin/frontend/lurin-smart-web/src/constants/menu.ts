import type { UserRole } from '../types/auth';

export interface MenuItem {
  label: string;
  path: string;
  roles: UserRole[];
}

/** Fuente única de verdad: rutas visibles y permitidas por rol. */
export const MENU_ITEMS: MenuItem[] = [
  { label: 'POS', path: '/pos', roles: ['ADMIN', 'CASHIER'] },
  { label: 'Inventario', path: '/inventory', roles: ['ADMIN', 'WAREHOUSE', 'GARDENER'] },
  { label: 'Monitoreo IoT', path: '/iot', roles: ['ADMIN', 'BOTANIST'] },
  { label: 'Riego', path: '/irrigation', roles: ['ADMIN', 'GARDENER', 'BOTANIST'] },
  { label: 'Logística Admin', path: '/logistics/admin', roles: ['ADMIN', 'WAREHOUSE'] },
  { label: 'Repartidor (Móvil)', path: '/logistics/driver', roles: ['ADMIN', 'DELIVERY'] },
];

/** Ruta inicial a la que se redirige a cada rol tras el login. */
export const DEFAULT_PATH_BY_ROLE: Record<UserRole, string> = {
  ADMIN: '/pos',
  CASHIER: '/pos',
  WAREHOUSE: '/inventory',
  GARDENER: '/irrigation',
  BOTANIST: '/iot',
  DELIVERY: '/logistics/driver',
};

export function menuItemsForRole(role: UserRole | null | undefined): MenuItem[] {
  if (!role) return [];
  return MENU_ITEMS.filter((item) => item.roles.includes(role));
}

export function rolesForPath(path: string): UserRole[] {
  return MENU_ITEMS.find((item) => item.path === path)?.roles ?? [];
}
