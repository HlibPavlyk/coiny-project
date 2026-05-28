import { useCallback, useEffect, useRef, useState } from 'react';
import type { LotImage } from '@/api/lots';

/**
 * Lot image gallery — horizontal swipe-snap carousel + thumbnail strip + fullscreen lightbox.
 *
 * Swipe / scroll behaviour:
 * - Native CSS `overflow-x-auto` + `scroll-snap` does the carousel work. Each slide is full-width
 *   and snap-aligned to its start so swipes land on a clean slide boundary.
 * - `touch-action: pan-x` on the scroller is the general blocker for vertical pan — iOS Safari
 *   treats the gallery as horizontal-only, so a finger on the gallery cannot start a vertical
 *   page-scroll. (Some edge-case iOS quirks during snap-animation remain; in practice they
 *   subside in production builds running over a stable connection.)
 * - `scroll-snap-stop: always` on slides prevents fast flicks from skipping past slides.
 *
 * State sync:
 * - IntersectionObserver watches which slide is centered and updates `active` so the counter
 *   badge and the thumbnail's accent border stay in sync without polling.
 * - Clicking a thumbnail scrolls the main carousel to that slide.
 * - Arrow buttons (desktop only) step ±1 and stop at the edges instead of wrapping; wrapping
 *   would trigger an animated scroll back through every slide, which feels jarring.
 */
export function ImageGallery({ images }: { images: LotImage[] }) {
  const sorted = [...images].sort((a, b) => a.displayOrder - b.displayOrder);
  const [active, setActive] = useState(0);
  const [lightbox, setLightbox] = useState(false);
  const scrollerRef = useRef<HTMLDivElement | null>(null);

  const cycleLightbox = useCallback(
    (dir: 1 | -1) => {
      if (sorted.length === 0) return;
      setActive((a) => (a + dir + sorted.length) % sorted.length);
    },
    [sorted.length],
  );

  // Scroll the main carousel to the slide at `index`. Used by thumbnail clicks and arrows.
  const scrollToSlide = useCallback((index: number) => {
    const scroller = scrollerRef.current;
    if (!scroller) return;
    const slide = scroller.children[index] as HTMLElement | undefined;
    slide?.scrollIntoView({ behavior: 'smooth', inline: 'start', block: 'nearest' });
  }, []);

  // Sync `active` to whichever slide is mostly visible. Threshold 0.6 / 0.9 picks the slide that
  // owns the viewport rather than the one that's transitioning out.
  useEffect(() => {
    const scroller = scrollerRef.current;
    if (!scroller || sorted.length <= 1) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const best = entries
          .filter((e) => e.isIntersecting && e.intersectionRatio >= 0.6)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];
        if (!best) return;
        const idx = Number((best.target as HTMLElement).dataset.idx);
        if (!Number.isNaN(idx)) setActive(idx);
      },
      { root: scroller, threshold: [0.6, 0.9] },
    );

    Array.from(scroller.children).forEach((slide) => observer.observe(slide as Element));
    return () => observer.disconnect();
  }, [sorted.length]);

  // Lightbox keyboard navigation — wraps around for power-user fullscreen browsing.
  useEffect(() => {
    if (!lightbox) return;
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') setLightbox(false);
      if (e.key === 'ArrowRight') cycleLightbox(1);
      if (e.key === 'ArrowLeft') cycleLightbox(-1);
    }
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [lightbox, cycleLightbox]);

  if (sorted.length === 0) {
    return (
      <div
        className="bg-bg-soft border border-border rounded-lg"
        style={{ aspectRatio: '1 / 1' }}
      />
    );
  }

  const goToThumb = (i: number) => {
    setActive(i);
    scrollToSlide(i);
  };

  // Arrow click: ±1, stop at edges (no wrap-around scroll-back animation).
  const onArrowClick = (dir: 1 | -1) => {
    const next = active + dir;
    if (next < 0 || next >= sorted.length) return;
    goToThumb(next);
  };
  const atStart = active === 0;
  const atEnd = active === sorted.length - 1;

  return (
    <div>
      {/* Main carousel: horizontal swipe-snap on both touch and pointer devices. */}
      <div className="relative">
        <div
          ref={scrollerRef}
          className="flex w-full overflow-x-auto snap-x snap-mandatory bg-bg-soft border border-border rounded-lg"
          style={{
            aspectRatio: '1 / 1',
            scrollbarWidth: 'none',
            touchAction: 'pan-x',
            overscrollBehavior: 'contain',
          } as React.CSSProperties}
        >
          {sorted.map((img, i) => (
            <button
              key={img.id}
              type="button"
              data-idx={i}
              onClick={() => setLightbox(true)}
              aria-label={`Open lightbox · image ${i + 1}`}
              className="relative flex-shrink-0 w-full h-full snap-start p-0 bg-transparent overflow-hidden"
              style={{ cursor: 'zoom-in', scrollSnapStop: 'always' }}
            >
              <img
                src={img.publicUrl}
                alt=""
                loading={i === 0 ? 'eager' : 'lazy'}
                decoding="async"
                draggable={false}
                className="absolute inset-0 w-full h-full object-contain pointer-events-none"
                style={{
                  WebkitUserDrag: 'none',
                  WebkitUserSelect: 'none',
                  userSelect: 'none',
                  WebkitTouchCallout: 'none',
                } as React.CSSProperties}
              />
            </button>
          ))}
        </div>

        <span
          className="absolute top-3 right-3 rounded-full px-2 py-0.5 text-[11px] font-semibold text-text-2 pointer-events-none"
          style={{ background: 'rgba(255,255,255,0.95)' }}
        >
          {active + 1} / {sorted.length}
        </span>

        {sorted.length > 1 && (
          <>
            <button
              type="button"
              aria-label="Previous image"
              onClick={() => onArrowClick(-1)}
              disabled={atStart}
              className="hidden md:flex absolute left-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full items-center justify-center text-text font-bold text-lg shadow-md transition hover:bg-white disabled:opacity-40 disabled:cursor-not-allowed"
              style={{ background: 'rgba(255,255,255,0.92)' }}
            >
              ‹
            </button>
            <button
              type="button"
              aria-label="Next image"
              onClick={() => onArrowClick(1)}
              disabled={atEnd}
              className="hidden md:flex absolute right-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full items-center justify-center text-text font-bold text-lg shadow-md transition hover:bg-white disabled:opacity-40 disabled:cursor-not-allowed"
              style={{ background: 'rgba(255,255,255,0.92)' }}
            >
              ›
            </button>
          </>
        )}
      </div>

      {/* Thumbnail strip — horizontally scrollable when there are many images. Click jumps the
          main carousel to the corresponding slide. */}
      {sorted.length > 1 && (
        <div
          role="tablist"
          aria-label="Image thumbnails"
          className="flex gap-1.5 mt-2.5 overflow-x-auto"
          style={{ scrollbarWidth: 'none' }}
          onKeyDown={(e) => {
            if (e.key === 'ArrowRight') {
              onArrowClick(1);
              e.preventDefault();
            }
            if (e.key === 'ArrowLeft') {
              onArrowClick(-1);
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
              onClick={() => goToThumb(i)}
              className="bg-bg-soft rounded overflow-hidden p-0 cursor-pointer transition flex-shrink-0"
              style={{
                width: 80,
                height: 80,
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

      {/* Fullscreen lightbox overlay — click backdrop or press Esc to close, ← / → to navigate. */}
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
            src={sorted[active].publicUrl}
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
