import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { Logo } from '@/components/Logo';

interface AuthShellProps {
  children: ReactNode;
}

/**
 * Shared shell for /sign-in, /sign-up, /verify-email, /auth/callback.
 * Minimal header (logo + support link), warm radial-gradient background, card centered below.
 */
export function AuthShell({ children }: AuthShellProps) {
  return (
    <div
      className="min-h-screen flex flex-col"
      style={{
        background: 'var(--color-bg)',
        backgroundImage:
          'radial-gradient(ellipse 80% 60% at 50% 0%, #F5EFE2 0%, transparent 60%)',
      }}
    >
      <header className="py-5">
        <div className="max-w-[1280px] mx-auto px-7 flex items-center justify-between">
          <Link to="/" className="no-underline">
            <Logo />
          </Link>
          <div className="text-sm text-text-3 hidden sm:block">
            Need help?{' '}
            <a className="text-accent-deep font-medium cursor-pointer hover:underline">
              Contact support
            </a>
          </div>
        </div>
      </header>
      <div className="flex-1 flex items-start justify-center px-4 sm:px-7 pb-16 pt-4 sm:pt-10">
        {children}
      </div>
    </div>
  );
}
