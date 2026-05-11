import { Link, useParams } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { useLot } from '@/api/lots';
import { ApiError } from '@/api/fetch';
import CreateLotPage from './CreateLotPage';

function ErrorView({ title, body, cta }: { title: string; body: string; cta: string }) {
  return (
    <div>
      <TopNav />
      <div className="max-w-[640px] mx-auto px-7 py-20 text-center">
        <h1 className="text-3xl font-bold m-0">{title}</h1>
        <p className="text-text-2 mt-3">{body}</p>
        <Link
          to="/my-lots"
          className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline mt-6"
        >
          {cta}
        </Link>
      </div>
      <Footer />
    </div>
  );
}

export default function EditLotPage() {
  const { id } = useParams<{ id: string }>();
  const { data: lot, isLoading, error } = useLot(id);

  if (isLoading) {
    return (
      <div>
        <TopNav />
        <div className="max-w-[920px] mx-auto px-7 py-20 text-center text-text-3">Loading…</div>
        <Footer />
      </div>
    );
  }

  if (error instanceof ApiError && error.status === 404) {
    return <ErrorView title="Lot not found" body="This draft no longer exists." cta="Back to My Lots" />;
  }
  if (error || !lot) {
    return <ErrorView title="Could not load lot" body="Try again in a moment." cta="Back to My Lots" />;
  }

  if (lot.status !== 'Draft') {
    return (
      <ErrorView
        title="This lot is no longer editable"
        body={`Lots in status "${lot.status}" cannot be edited. Cancel and recreate if needed.`}
        cta="Back to My Lots"
      />
    );
  }

  return <CreateLotPage draft={lot} />;
}
