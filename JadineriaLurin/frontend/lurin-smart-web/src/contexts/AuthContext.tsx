import React, { createContext, useCallback, useContext, useMemo, useState } from 'react';
import { authStorage } from '../lib/authStorage';
import { authService } from '../services/auth.service';
import type { AuthUser, LoginRequest, UserRole } from '../types/auth';

interface AuthContextType {
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (request: LoginRequest) => Promise<AuthUser>;
  logout: () => void;
  hasRole: (...roles: UserRole[]) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthUser | null>(() => authStorage.getUser());

  const login = useCallback(async (request: LoginRequest): Promise<AuthUser> => {
    const response = await authService.login(request);

    const session: AuthUser = {
      username: response.username,
      email: response.email,
      role: response.role,
      expiresAtUtc: response.expiresAtUtc,
    };

    authStorage.save(response.token, session);
    setUser(session);
    return session;
  }, []);

  const logout = useCallback(() => {
    authStorage.clear();
    setUser(null);
  }, []);

  const hasRole = useCallback(
    (...roles: UserRole[]) => {
      if (!user) return false;
      if (roles.length === 0) return true;
      return roles.includes(user.role);
    },
    [user],
  );

  const value = useMemo<AuthContextType>(
    () => ({
      user,
      isAuthenticated: user !== null,
      login,
      logout,
      hasRole,
    }),
    [user, login, logout, hasRole],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth debe usarse dentro de <AuthProvider>');
  }
  return context;
}
