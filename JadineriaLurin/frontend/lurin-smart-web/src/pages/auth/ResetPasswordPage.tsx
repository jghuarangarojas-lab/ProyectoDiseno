import { useState } from 'react';
import { isAxiosError } from 'axios';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Button } from '../../components/common/Button';
import { ArrowLeftIcon, SpinnerIcon } from '../../components/icons';
import { ViveroLogo } from '../../components/layout/ViveroLogo';
import { authService } from '../../services/auth.service';

export const ResetPasswordPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const tokenFromUrl = searchParams.get('token')?.trim() ?? '';

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    if (newPassword.length < 8) {
      setError('La contraseña debe tener al menos 8 caracteres.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Las contraseñas no coinciden.');
      return;
    }

    setIsLoading(true);

    try {
      await authService.resetPassword({ token: tokenFromUrl, newPassword });
      toast.success('Contraseña actualizada. Inicia sesión con tu nueva contraseña.');
      navigate('/login', { replace: true });
    } catch (err) {
      if (isAxiosError(err) && err.response?.status === 400) {
        const payload = err.response.data as { error?: string } | undefined;
        setError(payload?.error ?? 'El token es inválido o ha expirado. Solicita uno nuevo.');
      } else if (isAxiosError(err) && !err.response) {
        setError('No se pudo conectar con el servicio de autenticación. Verifica que ms-auth esté disponible.');
      } else {
        setError('No se pudo restablecer la contraseña. Intenta nuevamente.');
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
              Restablecer contraseña
            </h1>
            <p className="text-sm text-slate-500 mt-1 text-center">
              Crea una nueva contraseña para tu cuenta
            </p>
          </div>

          {!tokenFromUrl ? (
            <div
              role="alert"
              className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-800"
            >
              <p className="font-semibold mb-1">Token no encontrado</p>
              <p>
                El enlace de recuperación no incluye un token válido. Solicita un nuevo código
                de recuperación.
              </p>
            </div>
          ) : (
            <>
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
                  <label htmlFor="newPassword" className="block text-sm font-medium text-slate-700 mb-1">
                    Nueva contraseña
                  </label>
                  <input
                    id="newPassword"
                    name="newPassword"
                    type="password"
                    autoComplete="new-password"
                    autoFocus
                    placeholder="Mínimo 8 caracteres"
                    value={newPassword}
                    onChange={(event) => setNewPassword(event.target.value)}
                    className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-green-500 focus:outline-none focus:ring-2 focus:ring-green-200"
                  />
                </div>

                <div>
                  <label htmlFor="confirmPassword" className="block text-sm font-medium text-slate-700 mb-1">
                    Confirmar contraseña
                  </label>
                  <input
                    id="confirmPassword"
                    name="confirmPassword"
                    type="password"
                    autoComplete="new-password"
                    placeholder="Repite la nueva contraseña"
                    value={confirmPassword}
                    onChange={(event) => setConfirmPassword(event.target.value)}
                    className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-green-500 focus:outline-none focus:ring-2 focus:ring-green-200"
                  />
                </div>

                <Button type="submit" size="lg" className="w-full" disabled={isLoading}>
                  {isLoading ? (
                    <span className="inline-flex items-center justify-center gap-2">
                      <SpinnerIcon className="h-4 w-4 animate-spin" />
                      Actualizando...
                    </span>
                  ) : (
                    'Restablecer contraseña'
                  )}
                </Button>
              </form>
            </>
          )}

          <div className="mt-5 text-center">
            <Link
              to="/login"
              className="inline-flex items-center gap-1 text-sm font-medium text-green-700 hover:text-green-800 hover:underline"
            >
              <ArrowLeftIcon className="h-4 w-4" />
              Volver al inicio de sesión
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};
