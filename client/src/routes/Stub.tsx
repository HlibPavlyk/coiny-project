import { useParams } from 'react-router-dom';

/** Generic placeholder used by every stub route until its real component lands. */
export function Stub({ name }: { name: string }) {
  const params = useParams();
  const paramText = Object.entries(params)
    .map(([k, v]) => `${k}=${v}`)
    .join(' · ');

  return (
    <main className="min-h-screen p-8 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-2">{name}</h1>
      {paramText && <p className="text-text-3 text-sm mono">{paramText}</p>}
      <p className="mt-6 text-text-2">
        Stub. Real implementation lands in a later sprint-1 task.
      </p>
    </main>
  );
}
