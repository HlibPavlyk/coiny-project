import { Link } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';

/**
 * 404 page. Mounted by React Router for any unmatched route (<c>path: '*'</c>). Also reused by
 * feature routes when their target resource is missing (e.g. <c>LotPage</c> when the lot id is unknown).
 */
export default function NotFoundPage() {
  return (
    <div>
      <TopNav />
      <main className="max-w-[720px] mx-auto px-7 py-24 text-center">
        <div className="mono text-[64px] font-bold text-text-3" style={{ letterSpacing: '-0.04em' }}>
          404
        </div>
        <h1 className="text-[28px] font-bold m-0 mt-2 text-text">Page not found</h1>
        <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed max-w-[460px] mx-auto">
          The page you are looking for does not exist or has been moved. Check the URL, or head back
          to the home page to browse lots.
        </p>
        <div className="flex justify-center gap-2.5 mt-6">
          <Link
            to="/"
            className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline"
          >
            Back to home
          </Link>
          <Link
            to="/search"
            className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-5 py-3 text-sm no-underline"
          >
            Browse lots
          </Link>
        </div>
      </main>
      <Footer />
    </div>
  );
}
