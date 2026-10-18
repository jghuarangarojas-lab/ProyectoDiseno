import type { AuthUser } from '../types/auth';

const TOKEN_KEY = 'lurin_auth_token';
const USER_KEY = 'lurin_auth_user';

function readJson<T>(key: string): T | null {
  try {
    const raw = window.localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : null;
  } catch {
    window.localStorage.removeItem(key);
    return null;
  }
}

export const authStorage = {
  getToken(): string | null {
    return window.localStorage.getItem(TOKEN_KEY);
  },

  getUser(): AuthUser | null {
    const user = readJson<AuthUser>(USER_KEY);
    if (!user) return null;

    // Si el JWT ya venció, se limpia la sesión huérfana.
    if (new Date(user.expiresAtUtc).getTime() <= Date.now()) {
      authStorage.clear();
      return null;
    }

    return user;
  },

  save(token: string, user: AuthUser): void {
    window.localStorage.setItem(TOKEN_KEY, token);
    window.localStorage.setItem(USER_KEY, JSON.stringify(user));
  },

  clear(): void {
    window.localStorage.removeItem(TOKEN_KEY);
    window.localStorage.removeItem(USER_KEY);
  },
};
