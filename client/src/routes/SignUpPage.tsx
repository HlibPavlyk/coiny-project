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

interface FieldErrors {
  email?: string;
  password?: string;
  displayName?: string;
}

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function validateClient(email: string, password: string, displayName: string): FieldErrors {
  const e: FieldErrors = {};
  if (!email.trim()) e.email = 'Email is required.';
  else if (!EMAIL_RE.test(email)) e.email = 'Enter a valid email address.';

  if (!password) e.password = 'Password is required.';
  else if (password.length < 8) e.password = 'At least 8 characters.';
  else if (!/[0-9]/.test(password)) e.password = 'Must contain a digit.';
  else if (!/[A-Z]/.test(password)) e.password = 'Must contain an uppercase letter.';
  else if (!/[a-z]/.test(password)) e.password = 'Must contain a lowercase letter.';

  if (displayName && displayName.length > 50) e.displayName = 'Up to 50 characters.';

  return e;
}

export default function SignUpPage() {
  const user = useAuthStore((s) => s.user);
  const register = useAuthStore((s) => s.register);
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const next = params.get('next') ?? '/';

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<{ title: string; body?: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (user) return <Navigate to={next} replace />;

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const errors = validateClient(email, password, displayName);
    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) return;

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
          setFieldErrors({ email: 'This email is already registered. Try signing in instead.' });
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
        <h1 className="text-[26px] font-bold tracking-tight m-0 mb-1.5">Create your account</h1>
        <p className="text-sm text-text-3 m-0 mb-6">
          Start collecting, bidding, and selling on Coiny
        </p>

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
            {fieldErrors.email ? (
              <p id="email-hint" className="text-xs mt-1.5" style={{ color: '#B91C1C' }}>
                {fieldErrors.email}
              </p>
            ) : (
              <FieldHint>We&apos;ll send a verification link to this address.</FieldHint>
            )}
          </div>

          <div className="mb-3.5">
            <FieldLabel htmlFor="password">Password</FieldLabel>
            <FieldInput
              id="password"
              type="password"
              autoComplete="new-password"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                if (fieldErrors.password) setFieldErrors({ ...fieldErrors, password: undefined });
              }}
              placeholder="Create a strong password"
              aria-invalid={!!fieldErrors.password}
              aria-describedby="password-hint"
            />
            {fieldErrors.password ? (
              <p id="password-hint" className="text-xs mt-1.5" style={{ color: '#B91C1C' }}>
                {fieldErrors.password}
              </p>
            ) : (
              <FieldHint>≥8 characters, with 1 digit, 1 uppercase, 1 lowercase.</FieldHint>
            )}
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
              onChange={(e) => {
                setDisplayName(e.target.value);
                if (fieldErrors.displayName) setFieldErrors({ ...fieldErrors, displayName: undefined });
              }}
              placeholder="john_collector"
              mono
              aria-invalid={!!fieldErrors.displayName}
              aria-describedby="displayName-hint"
            />
            {fieldErrors.displayName ? (
              <p id="displayName-hint" className="text-xs mt-1.5" style={{ color: '#B91C1C' }}>
                {fieldErrors.displayName}
              </p>
            ) : (
              <FieldHint>Shown on your bids and lots. You can change it later.</FieldHint>
            )}
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
