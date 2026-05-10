import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '@/state/useAuthStore';
import { useToastStore } from '@/state/useToastStore';
import { auth } from '@/api/auth';
import { ApiError } from '@/api/fetch';
import { AuthShell } from '@/components/auth/AuthShell';
import { AuthCard } from '@/components/auth/AuthCard';
import { GoogleButton } from '@/components/auth/GoogleButton';
import { ErrorBand } from '@/components/auth/ErrorBand';
import { OrDivider } from '@/components/auth/Divider';
import { FieldLabel, FieldInput } from '@/components/auth/FieldHint';

export default function SignInPage() {
  const user = useAuthStore((s) => s.user);
  const signIn = useAuthStore((s) => s.signIn);
  const pushToast = useToastStore((s) => s.push);
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const next = params.get('next') ?? '/';

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<{ title: string; body?: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (user) return <Navigate to={next} replace />;

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await signIn({ email, password });
      navigate(next, { replace: true });
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 403) {
          pushToast({
            kind: 'danger',
            title: 'Account banned',
            description: err.detail ?? 'Your account has been suspended.',
          });
        } else if (err.status === 401) {
          setError({
            title: 'Wrong email or password',
            body: 'Check your credentials and try again.',
          });
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
        <h1 className="text-[26px] font-bold tracking-tight m-0 mb-1.5">Welcome back</h1>
        <p className="text-sm text-text-3 m-0 mb-6">Sign in to your Coiny account</p>

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
          </div>

          <div className="mb-5">
            <div className="flex justify-between items-baseline mb-1.5">
              <label htmlFor="password" className="block text-sm font-medium">Password</label>
              <a className="text-xs text-accent-deep cursor-pointer hover:underline">Forgot password?</a>
            </div>
            <FieldInput
              id="password"
              type="password"
              autoComplete="current-password"
              required
              minLength={8}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          <button
            type="submit"
            disabled={submitting}
            className="w-full rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm disabled:opacity-60 disabled:cursor-not-allowed transition"
          >
            {submitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>

        <p className="text-center text-sm text-text-3 mt-5 mb-0">
          Don&apos;t have an account?{' '}
          <Link
            to={`/sign-up${next ? `?next=${encodeURIComponent(next)}` : ''}`}
            className="text-accent-deep font-semibold hover:underline"
          >
            Create one
          </Link>
        </p>
      </AuthCard>
    </AuthShell>
  );
}
