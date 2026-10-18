import { useState } from 'react';
import { isAxiosError } from 'axios';
import { Link } from 'react-router-dom';
import { Button } from '../../components/common/Button';
import { ArrowLeftIcon, SpinnerIcon } from '../../components/icons';
import { ViveroLogo } from '../../components/layout/ViveroLogo';
import { authService } from '../../services/auth.service';

export const ForgotPasswordPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSent, setIsSent] = useState(false);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    const trimmedEmail = email.trim();

    if (!trimmedEmail || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)) {
      setError('Ingresa un correo electrónico válido.');
      return;
    }

    setIsLoading(true);

    try {
      await authService.forgotPassword({ email: trimmedEmail });
      setIsSent(true);
    } catch (err) {
      if (isAxiosError(err) && !err.response) {
        setError('No se pudo conectar con el servicio de autenticación. Verifica que ms-auth esté disponible.');
      } else {
        setError('No se pudo procesar la solicitud. Intenta nuevamente.');
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
              ¿Olvidaste tu contraseña?
            </h1>
            <p className="text-sm text-slate-500 mt-1 text-center">
              Te enviaremos un código de recuperación a tu correo (válido por 15 minutos).
            </p>
          </div>

          {isSent ? (
            <div
              role="status"
              className="rounded-lg border border-green-200 bg-green-50 px-4 py-4 text-sm text-green-800"
            >
              <p className="font-semibold mb-1">Solicitud registrada</p>
              <p>
                Si el correo <b>{email.trim()}</b> está asociado a una cuenta, recibirás un
                mensaje con las instrucciones para restablecer tu contraseña.
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
                  <label htmlFor="email" className="block text-sm font-medium text-slate-700 mb-1">
                    Correo electrónico
                  </label>
                  <input
                    id="email"
                    name="email"
                    type="email"
                    autoComplete="email"
                    autoFocus
                    placeholder="correo@ejemplo.com"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-green-500 focus:outline-none focus:ring-2 focus:ring-green-200"
                  />
                </div>

                <Button type="submit" size="lg" className="w-full" disabled={isLoading}>
                  {isLoading ? (
                    <span className="inline-flex items-center justify-center gap-2">
                      <SpinnerIcon className="h-4 w-4 animate-spin" />
                      Enviando...
                    </span>
                  ) : (
                    'Enviar código'
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
