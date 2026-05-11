import { Link, useParams } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';

/** Generic placeholder used by every stub route until its real component lands. */
export function Stub({ name }: { name: string }) {
  const params = useParams();
  const paramText = Object.entries(params)
    .map(([k, v]) => `${k}=${v}`)
    .join(' · ');

  const isNotFound = name.toLowerCase() === 'not found';

  return (
    <div>
      <TopNav />
      <main className="max-w-[720px] mx-auto px-7 py-20 text-center">
        <h1 className="text-3xl font-bold m-0">{name}</h1>
        {paramText && <p className="text-text-3 text-[12.5px] mono mt-2">{paramText}</p>}
        <p className="text-text-2 text-[14.5px] mt-4 leading-relaxed">
          {isNotFound
            ? 'The page you’re looking for doesn’t exist or has been moved.'
            : 'This section is not implemented yet — it ships in a later sprint.'}
        </p>
        <Link
          to="/"
          className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline mt-6"
        >
          Back to home
        </Link>
      </main>
      <Footer />
    </div>
  );
}
