import axios, {
  type AxiosError,
  type AxiosInstance,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios';
import { authStorage } from './authStorage';

/**
 * Endpoint de autenticación: un 401 aquí significa credenciales inválidas,
 * no una sesión expirada, por lo que NO debe provocar logout ni redirección.
 */
const AUTH_URL_MARKER = '/api/auth/';

export function isAuthEndpoint(url?: string): boolean {
  return (url ?? '').includes(AUTH_URL_MARKER);
}

/**
 * Logout forzado: borra el JWT almacenado y redirige al Login.
 * Usado por el interceptor de Axios, por fetch y por el control de inactividad.
 */
export function forceLogoutAndRedirect(): void {
  authStorage.clear();

  if (!window.location.pathname.startsWith('/login')) {
    window.location.replace(`${window.location.origin}/login`);
  }
}

/**
 * Inyecta `Authorization: Bearer <token>` en TODAS las peticiones del instance
 * y fuerza el logout ante cualquier 401 de los microservicios.
 */
export function attachAuthInterceptors(instance: AxiosInstance): AxiosInstance {
  instance.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    const token = authStorage.getToken();
    if (token) {
      config.headers.set('Authorization', `Bearer ${token}`);
    }
    return config;
  });

  instance.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
      if (error.response?.status === 401 && !isAuthEndpoint(error.config?.url)) {
        forceLogoutAndRedirect();
      }
      return Promise.reject(error);
    },
  );

  return instance;
}

/**
 * Crea un cliente Axios con los interceptores globales de autenticación ya aplicados.
 * Todos los microservicios deben obtener su cliente a través de esta fábrica.
 */
export function createHttpClient(config?: AxiosRequestConfig): AxiosInstance {
  return attachAuthInterceptors(axios.create(config));
}

/** Cliente genérico sin baseURL (las rutas relativas se resuelven con config por servicio). */
export const httpClient = createHttpClient();
