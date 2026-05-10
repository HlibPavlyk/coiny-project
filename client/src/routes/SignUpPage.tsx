import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '@/state/useAuthStore';
import { auth } from '@/api/auth';
import { ApiError } from '@/api/fetch';
import { AuthShell } from '@/components/auth/AuthShell';
import { AuthCard } from '@/components/auth/AuthCard';
import { GoogleButton } from '@/components/auth/GoogleButton';
import { ErrorBand } from '@/components/auth/ErrorBand';
import { OrDivider } from '@/components/auth/Divider';
import { FieldLabel, FieldInput, FieldHint } from '@/components/auth/FieldHint';
import { Icon } from '@/components/Icon';

export default function SignUpPage() {
  const user = useAuthStore((s) => s.user);
  const register = useAuthStore((s) => s.register);
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const next = params.get('next') ?? '/';

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [error, setError] = useState<{ title: string; body?: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (user) return <Navigate to={next} replace />;

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await register({
        email,
        password,
        displayName: displayName.trim() || undefined,
      });
      navigate(next, { replace: true });
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 409) {
          setError({ title: 'Email already in use', body: 'Try signing in instead.' });
        } else {
          setError({ title: err.detail ?? err.message });
        }
      } else {
        setError({ title: 'Network error', body: 'Please try again.' });
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <AuthShell>
      <AuthCard width={420}>
        <h1 className="text-[26px] font-bold tracking-tight m-0 mb-1.5">Create your account</h1>
        <p className="text-sm text-text-3 m-0 mb-6">
          Start collecting, bidding, and selling on Coiny
        </p>

        {error && <ErrorBand title={error.title} body={error.body} />}

        <GoogleButton onClick={() => auth.googleStart(next)} disabled={submitting} />

        <OrDivider />

        <form onSubmit={onSubmit} noValidate>
          <div className="mb-3.5">
            <FieldLabel htmlFor="email">Email</FieldLabel>
            <FieldInput
              id="email"
              type="email"
              autoComplete="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
            />
            <FieldHint>We&apos;ll send a verification link to this address.</FieldHint>
          </div>

          <div className="mb-3.5">
            <FieldLabel htmlFor="password">Password</FieldLabel>
            <FieldInput
              id="password"
              type="password"
              autoComplete="new-password"
              required
              minLength={8}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Create a strong password"
            />
            <FieldHint>≥8 characters, with 1 digit, 1 uppercase, 1 lowercase.</FieldHint>
          </div>

          <div className="mb-5">
            <FieldLabel htmlFor="displayName" optional>
              Display name
            </FieldLabel>
            <FieldInput
              id="displayName"
              type="text"
              autoComplete="nickname"
              maxLength={50}
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="john_collector"
              mono
            />
            <FieldHint>Shown on your bids and lots. You can change it later.</FieldHint>
          </div>

          <button
            type="submit"
            disabled={submitting}
            className="w-full rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm disabled:opacity-60 disabled:cursor-not-allowed transition"
          >
            {submitting ? 'Creating…' : 'Create account'}
          </button>
        </form>

        <div className="mt-5 px-3 py-2.5 bg-bg-soft rounded-md flex items-start gap-2">
          <div className="mt-0.5 flex-shrink-0">
            <Icon name="shield" size={14} color="var(--color-success)" />
          </div>
          <div className="text-[11.5px] text-text-3 leading-relaxed">
            By creating an account you agree to our{' '}
            <a className="text-text-2 underline cursor-pointer">Terms</a> and{' '}
            <a className="text-text-2 underline cursor-pointer">Privacy Policy</a>. Payments
            protected by Stripe escrow.
          </div>
        </div>

        <p className="text-center text-sm text-text-3 mt-5 mb-0">
          Already have an account?{' '}
          <Link
            to={`/sign-in${next ? `?next=${encodeURIComponent(next)}` : ''}`}
            className="text-accent-deep font-semibold hover:underline"
          >
            Sign in
          </Link>
        </p>
      </AuthCard>
    </AuthShell>
  );
}
