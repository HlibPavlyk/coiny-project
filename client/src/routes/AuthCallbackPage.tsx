import { useEffect, useState, type ReactNode } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '@/state/useAuthStore';
import { AuthShell } from '@/components/auth/AuthShell';
import { AuthCard } from '@/components/auth/AuthCard';
import { Icon } from '@/components/Icon';

const reasonMessages: Record<string, string> = {
  auth_failed: 'Google authentication did not complete. Please try again.',
  missing_claims: 'Google did not return the required profile data.',
  email_not_verified: 'Your Google account email is not verified. Use a verified address.',
  'Auth.GoogleEmailNotVerified': 'Your Google account email is not verified. Use a verified address.',
  'Auth.Banned': 'This account has been suspended.',
};

function Spinner() {
  return (
    <div
      style={{
        width: 44,
        height: 44,
        borderRadius: '50%',
        border: '2.5px solid var(--color-bg-soft)',
        borderTopColor: 'var(--color-accent)',
        animation: 'coiny-spin 0.9s linear infinite',
      }}
    />
  );
}

export default function AuthCallbackPage() {
  const [params] = useSearchParams();
  const refresh = useAuthStore((s) => s.refresh);
  const navigate = useNavigate();
  const ok = params.get('ok') === '1';
  const reason = params.get('reason');
  const [error, setError] = useState<ReactNode | null>(null);

  useEffect(() => {
    if (!ok) {
      setError(reason ? reasonMessages[reason] ?? `Sign-in failed: ${reason}` : 'Sign-in failed.');
      return;
    }
    let cancelled = false;
    (async () => {
      await refresh();
      if (!cancelled) navigate('/', { replace: true });
    })();
    return () => {
      cancelled = true;
    };
  }, [ok, reason, refresh, navigate]);

  return (
    <AuthShell>
      <AuthCard width={420}>
        <div className="text-center mb-4">
          <div className="inline-flex">
            {error ? (
              <div
                style={{
                  width: 56,
                  height: 56,
                  borderRadius: '50%',
                  background: 'var(--color-danger-soft)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <Icon name="info" size={28} color="#991B1B" />
              </div>
            ) : (
              <Spinner />
            )}
          </div>
        </div>
        <h1 className="text-[22px] font-bold tracking-tight text-center m-0 mb-2">
          {error ? "Couldn't sign you in" : 'Signing you in…'}
        </h1>
        <p className="text-[13.5px] text-text-3 text-center mb-5 leading-relaxed">
          {error ?? <>Hold on while we finish setting up your session.</>}
        </p>
        {error && (
          <Link
            to="/sign-in"
            className="block w-full text-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm transition"
          >
            Back to sign in
          </Link>
        )}
        <style>{`@keyframes coiny-spin { to { transform: rotate(360deg); } }`}</style>
      </AuthCard>
    </AuthShell>
  );
}
