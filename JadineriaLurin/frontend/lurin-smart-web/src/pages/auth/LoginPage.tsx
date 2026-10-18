import { useState } from 'react';
import { isAxiosError } from 'axios';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Button } from '../../components/common/Button';
import { SpinnerIcon } from '../../components/icons';
import { ViveroLogo } from '../../components/layout/ViveroLogo';
import { useAuth } from '../../contexts/AuthContext';
import { DEFAULT_PATH_BY_ROLE } from '../../constants/menu';

const USERNAME_PATTERN = /^[a-zA-Z]+@JL(admin|botanist|gardener|cashier|warehouse|delivery)$/i;

interface LocationState {
  from?: { pathname?: string };
}

export const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    const trimmedUsername = username.trim();

    if (!trimmedUsername || !password) {
      setError('Ingresa tu usuario y contraseña.');
      return;
    }

    if (!USERNAME_PATTERN.test(trimmedUsername)) {
      setError('Formato de usuario inválido. Ejemplo: jhuaranga@JLadmin');
      return;
    }

    setIsLoading(true);

    try {
      const user = await login({ username: trimmedUsername, password });
      const state = location.state as LocationState | null;
      const target = state?.from?.pathname ?? DEFAULT_PATH_BY_ROLE[user.role] ?? '/pos';
      toast.success(`Bienvenido, ${user.username}`);
      navigate(target, { replace: true });
    } catch (err) {
      if (isAxiosError(err) && err.response?.status === 401) {
        setError('Usuario o contraseña incorrectos.');
      } else if (isAxiosError(err) && !err.response) {
        setError('No se pudo conectar con el servicio de autenticación. Verifica que ms-auth esté disponible.');
      } else {
        setError('No se pudo iniciar sesión. Intenta nuevamente.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-green-50 via-emerald-50 to-lime-50 p-4">
      <div className="w-full max-w-md">
        <div className="bg-white rounded-2xl shadow-xl border border-slate-100 p-8">
          <div className="flex flex-col items-center mb-6">
            <ViveroLogo className="h-14 w-14 mb-3" />
            <h1 className="text-xl font-bold text-slate-900 text-center">
              Jardinería Lurín Smart
            </h1>
            <p className="text-sm text-slate-500 mt-1">
              Inicia sesión para continuar al sistema
            </p>
          </div>

          {error && (
            <div
              role="alert"
              className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
            >
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4" noValidate>
            <div>
              <label htmlFor="username" className="block text-sm font-medium text-slate-700 mb-1">
                Usuario
              </label>
              <input
                id="username"
                name="username"
                type="text"
                autoComplete="username"
                autoFocus
                placeholder="jhuaranga@JLadmin"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-green-500 focus:outline-none focus:ring-2 focus:ring-green-200"
              />
            </div>

            <div>
              <div className="flex items-center justify-between mb-1">
                <label htmlFor="password" className="block text-sm font-medium text-slate-700">
                  Contraseña
                </label>
                <button
                  type="button"
                  onClick={() => setShowPassword((previous) => !previous)}
                  className="text-xs font-medium text-green-700 hover:text-green-800"
                >
                  {showPassword ? 'Ocultar' : 'Mostrar'}
                </button>
              </div>
              <input
                id="password"
                name="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                placeholder="••••••••"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-green-500 focus:outline-none focus:ring-2 focus:ring-green-200"
              />
            </div>

            <Button type="submit" size="lg" className="w-full" disabled={isLoading}>
              {isLoading ? (
                <span className="inline-flex items-center justify-center gap-2">
                  <SpinnerIcon className="h-4 w-4 animate-spin" />
                  Verificando...
                </span>
              ) : (
                'Ingresar'
              )}
            </Button>
          </form>

          <div className="mt-5 text-center">
            <Link
              to="/forgot-password"
              className="text-sm font-medium text-green-700 hover:text-green-800 hover:underline"
            >
              ¿Olvidaste tu contraseña?
            </Link>
          </div>
        </div>

        <p className="mt-6 text-center text-xs text-slate-400">
          Sistema de monitoreo IoT y gestión de vivero
        </p>
      </div>
    </div>
  );
};
