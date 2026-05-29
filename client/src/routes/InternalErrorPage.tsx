import { Link, useRouteError, isRouteErrorResponse } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { ApiError } from '@/api/fetch';

/**
 * Full-page error fallback for unrecoverable navigation errors. Wired into React Router via
 * <c>errorElement</c> on the root route so a thrown exception or a non-404 response surfaces here
 * instead of a blank white screen. 404s are routed to <c>NotFoundPage</c> separately.
 */
export default function InternalErrorPage() {
  const error = useRouteError();
  const { title, description } = describe(error);

  return (
    <div>
      <TopNav />
      <main className="max-w-[720px] mx-auto px-7 py-24 text-center">
        <div className="mono text-[64px] font-bold text-text-3" style={{ letterSpacing: '-0.04em' }}>
          500
        </div>
        <h1 className="text-[28px] font-bold m-0 mt-2 text-text">{title}</h1>
        <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed max-w-[460px] mx-auto">{description}</p>
        <div className="flex justify-center gap-2.5 mt-6">
          <Link
            to="/"
            className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline"
          >
            Back to home
          </Link>
          <button
            type="button"
            onClick={() => window.location.reload()}
            className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-5 py-3 text-sm"
          >
            Reload page
          </button>
        </div>
      </main>
      <Footer />
    </div>
  );
}

/** Translate any error shape (Response, ApiError, plain Error) into UI-friendly copy. */
function describe(error: unknown): { title: string; description: string } {
  if (isRouteErrorResponse(error)) {
    return {
      title: `${error.status} — ${error.statusText || 'Something went wrong'}`,
      description: error.data?.message ?? 'The requested page could not be loaded.',
    };
  }
  if (error instanceof ApiError) {
    return {
      title: `Request failed (${error.status})`,
      description: error.detail ?? error.message,
    };
  }
  if (error instanceof Error) {
    return {
      title: 'Something went wrong',
      description: error.message,
    };
  }
  return {
    title: 'Something went wrong',
    description: 'An unexpected error occurred. Please try again or head back home.',
  };
}
