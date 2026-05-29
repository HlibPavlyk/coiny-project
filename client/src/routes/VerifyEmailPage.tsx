import { useEffect, useState, type ReactNode } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { auth } from '@/api/auth';
import { ApiError } from '@/api/fetch';
import { AuthShell } from '@/components/auth/AuthShell';
import { AuthCard } from '@/components/auth/AuthCard';
import { Icon } from '@/components/Icon';

type VerifyState = 'verifying' | 'verified' | 'expired' | 'invalid' | 'missing';

interface Variant {
  icon: ReactNode;
  title: string;
  body: ReactNode;
  ctas: ReactNode;
}

function Spinner() {
  return (
    <div
      style={{
        width: 56,
        height: 56,
        borderRadius: '50%',
        border: '3px solid var(--color-bg-soft)',
        borderTopColor: 'var(--color-accent)',
        animation: 'coiny-spin 0.9s linear infinite',
      }}
    />
  );
}

function StatusCircle({
  kind,
  name,
  stroke = 2.4,
}: {
  kind: 'success' | 'warning' | 'danger';
  name: 'check' | 'clock' | 'x';
  stroke?: number;
}) {
  const styles = {
    success: { bg: 'var(--color-success-soft)', fg: '#166534' },
    warning: { bg: 'var(--color-warning-soft)', fg: '#92400E' },
    danger: { bg: 'var(--color-danger-soft)', fg: '#991B1B' },
  };
  const s = styles[kind];
  return (
    <div
      style={{
        width: 56,
        height: 56,
        borderRadius: '50%',
        background: s.bg,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <Icon name={name} size={28} color={s.fg} stroke={stroke} />
    </div>
  );
}

export default function VerifyEmailPage() {
  const [params] = useSearchParams();
  const token = params.get('token');
  const [state, setState] = useState<VerifyState>(() => (token ? 'verifying' : 'missing'));

  useEffect(() => {
    if (!token) return;
    let cancelled = false;
    (async () => {
      try {
        await auth.verifyEmail(token);
        if (!cancelled) setState('verified');
      } catch (err) {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 409) setState('expired');
        else setState('invalid');
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [token]);

  const variants: Record<VerifyState, Variant> = {
    verifying: {
      icon: <Spinner />,
      title: 'Verifying your email…',
      body: 'This will only take a moment.',
      ctas: null,
    },
    verified: {
      icon: <StatusCircle kind="success" name="check" stroke={2.5} />,
      title: 'Email verified',
      body: <>You&apos;re all set. Your account is ready to bid and sell.</>,
      ctas: (
        <>
          <Link
            to="/profile"
            className="block w-full text-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm transition"
          >
            Go to my profile
          </Link>
          <Link
            to="/"
            className="block w-full text-center rounded-md hover:bg-bg-soft text-text font-medium px-5 py-2.5 text-sm mt-2 transition"
          >
            Browse lots
          </Link>
        </>
      ),
    },
    expired: {
      icon: <StatusCircle kind="warning" name="clock" stroke={2} />,
      title: 'Link expired or already used',
      body:
        'Verification links are valid for 24 hours and one click only. Sign in and request a new link from your profile.',
      ctas: (
        <Link
          to="/sign-in?next=/profile"
          className="block w-full text-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm transition"
        >
          Sign in
        </Link>
      ),
    },
    invalid: {
      icon: <StatusCircle kind="danger" name="x" />,
      title: 'Invalid verification link',
      body:
        'This link is malformed or no longer recognized. If you copied it from email, make sure you got the whole URL.',
      ctas: (
        <Link
          to="/sign-in"
          className="block w-full text-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm transition"
        >
          Sign in
        </Link>
      ),
    },
    missing: {
      icon: <StatusCircle kind="danger" name="x" />,
      title: 'Missing token',
      body: 'Open the link from your verification email to continue.',
      ctas: null,
    },
  };

  const v = variants[state];
  return (
    <AuthShell>
      <AuthCard width={460}>
        <div className="text-center mb-5">
          <div className="inline-flex">{v.icon}</div>
        </div>
        <h1 className="text-2xl font-bold tracking-tight text-center m-0 mb-2.5">{v.title}</h1>
        <p className="text-sm text-text-3 text-center mb-6 leading-relaxed">{v.body}</p>
        {v.ctas}
        <style>{`@keyframes coiny-spin { to { transform: rotate(360deg); } }`}</style>
      </AuthCard>
    </AuthShell>
  );
}
