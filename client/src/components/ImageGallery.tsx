import { useCallback, useEffect, useState } from 'react';
import type { LotImage } from '@/api/lots';

export function ImageGallery({ images }: { images: LotImage[] }) {
  const sorted = [...images].sort((a, b) => a.displayOrder - b.displayOrder);
  const [active, setActive] = useState(0);
  const [lightbox, setLightbox] = useState(false);

  const cycle = useCallback(
    (dir: 1 | -1) => {
      if (sorted.length === 0) return;
      setActive((a) => (a + dir + sorted.length) % sorted.length);
    },
    [sorted.length],
  );

  useEffect(() => {
    if (!lightbox) return;
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') setLightbox(false);
      if (e.key === 'ArrowRight') cycle(1);
      if (e.key === 'ArrowLeft') cycle(-1);
    }
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [lightbox, cycle]);

  if (sorted.length === 0) {
    return (
      <div
        className="bg-bg-soft border border-border rounded-lg"
        style={{ aspectRatio: '1 / 1' }}
      />
    );
  }

  const main = sorted[active];

  return (
    <div>
      <button
        type="button"
        onClick={() => setLightbox(true)}
        aria-label="Open lightbox"
        className="relative w-full bg-bg-soft border border-border rounded-lg overflow-hidden p-0"
        style={{ aspectRatio: '1 / 1', cursor: 'zoom-in' }}
      >
        <img
          src={main.publicUrl}
          alt=""
          loading="eager"
          decoding="async"
          className="absolute inset-0 w-full h-full object-contain"
        />
        <span
          className="absolute top-3 right-3 rounded-full px-2 py-0.5 text-[11px] font-semibold text-text-2"
          style={{ background: 'rgba(255,255,255,0.95)' }}
        >
          {active + 1} / {sorted.length}
        </span>
      </button>

      {sorted.length > 1 && (
        <div
          role="tablist"
          aria-label="Image thumbnails"
          className="flex flex-wrap gap-1.5 mt-2.5"
          onKeyDown={(e) => {
            if (e.key === 'ArrowRight') {
              cycle(1);
              e.preventDefault();
            }
            if (e.key === 'ArrowLeft') {
              cycle(-1);
              e.preventDefault();
            }
          }}
        >
          {sorted.map((img, i) => (
            <button
              key={img.id}
              type="button"
              role="tab"
              aria-selected={i === active}
              aria-label={`Image ${i + 1}`}
              onClick={() => setActive(i)}
              className="bg-bg-soft rounded overflow-hidden p-0 cursor-pointer transition flex-shrink-0"
              style={{
                width: 100,
                height: 100,
                border:
                  i === active ? '2px solid var(--color-accent)' : '1px solid var(--color-border)',
              }}
            >
              <img
                src={img.publicUrl}
                alt=""
                loading="lazy"
                decoding="async"
                className="w-full h-full object-cover"
              />
            </button>
          ))}
        </div>
      )}

      {lightbox && (
        <div
          role="dialog"
          aria-modal="true"
          aria-label="Image lightbox"
          className="fixed inset-0 z-50 flex items-center justify-center"
          style={{ background: 'rgba(0,0,0,0.88)', cursor: 'zoom-out' }}
          onClick={() => setLightbox(false)}
        >
          <img
            src={main.publicUrl}
            alt=""
            className="max-w-[92vw] max-h-[92vh] object-contain"
            onClick={(e) => e.stopPropagation()}
            style={{ cursor: 'default' }}
          />
          <button
            type="button"
            aria-label="Close"
            onClick={(e) => {
              e.stopPropagation();
              setLightbox(false);
            }}
            className="absolute top-5 right-5 w-10 h-10 rounded-full text-white text-xl font-bold flex items-center justify-center"
            style={{ background: 'rgba(255,255,255,0.18)', cursor: 'pointer' }}
          >
            ×
          </button>
        </div>
      )}
    </div>
  );
}
