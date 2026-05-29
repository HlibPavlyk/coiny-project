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

interface FieldErrors {
  email?: string;
  password?: string;
}

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function validateClient(email: string, password: string): FieldErrors {
  const e: FieldErrors = {};
  if (!email.trim()) e.email = 'Email is required.';
  else if (!EMAIL_RE.test(email)) e.email = 'Enter a valid email address.';
  if (!password) e.password = 'Password is required.';
  return e;
}

export default function SignInPage() {
  const user = useAuthStore((s) => s.user);
  const signIn = useAuthStore((s) => s.signIn);
  const pushToast = useToastStore((s) => s.push);
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const next = params.get('next') ?? '/';

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<{ title: string; body?: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (user) return <Navigate to={next} replace />;

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const errors = validateClient(email, password);
    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) return;

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
          setFormError({
            title: 'Wrong email or password',
            body: 'Check your credentials and try again.',
          });
        } else {
          setFormError({ title: err.detail ?? err.message });
        }
      } else {
        setFormError({ title: 'Network error', body: 'Please try again.' });
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

        {formError && <ErrorBand title={formError.title} body={formError.body} />}

        <GoogleButton onClick={() => auth.googleStart(next)} disabled={submitting} />

        <OrDivider />

        <form onSubmit={onSubmit} noValidate>
          <div className="mb-3.5">
            <FieldLabel htmlFor="email">Email</FieldLabel>
            <FieldInput
              id="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                if (fieldErrors.email) setFieldErrors({ ...fieldErrors, email: undefined });
              }}
              placeholder="you@example.com"
              aria-invalid={!!fieldErrors.email}
              aria-describedby="email-hint"
            />
            {fieldErrors.email && (
              <p id="email-hint" className="text-xs mt-1.5" style={{ color: '#B91C1C' }}>
                {fieldErrors.email}
              </p>
            )}
          </div>

          <div className="mb-5">
            <div className="flex justify-between items-baseline mb-1.5">
              <label htmlFor="password" className="block text-sm font-medium">
                Password
              </label>
              <a className="text-xs text-accent-deep cursor-pointer hover:underline">
                Forgot password?
              </a>
            </div>
            <FieldInput
              id="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                if (fieldErrors.password) setFieldErrors({ ...fieldErrors, password: undefined });
              }}
              aria-invalid={!!fieldErrors.password}
              aria-describedby="password-hint"
            />
            {fieldErrors.password && (
              <p id="password-hint" className="text-xs mt-1.5" style={{ color: '#B91C1C' }}>
                {fieldErrors.password}
              </p>
            )}
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
