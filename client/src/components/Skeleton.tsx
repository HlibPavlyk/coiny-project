import type { CSSProperties } from 'react';

interface SkeletonProps {
  /** Tailwind classes for sizing/positioning. */
  className?: string;
  /** Inline style for one-off dimensions (e.g. aspectRatio). */
  style?: CSSProperties;
}

/**
 * Generic shimmer placeholder. Used on every TanStack Query <c>isLoading</c> path to give the page
 * its eventual shape during the initial fetch — no layout shift when real data arrives.
 *
 * Style baseline matches the existing palette: <c>bg-bg-soft</c> with a soft border so the
 * placeholders read as "loading" rather than empty containers.
 */
export function Skeleton({ className = '', style }: SkeletonProps) {
  return (
    <div
      aria-hidden="true"
      className={`bg-bg-soft border border-border-soft rounded animate-pulse ${className}`}
      style={style}
    />
  );
}

/**
 * Convenience for a single line of skeleton text. Width is a Tailwind class so callers can keep
 * the placeholder lines varied (<c>w-1/3</c>, <c>w-2/3</c>) for a more natural look.
 */
export function SkeletonLine({ width = 'w-full', className = '' }: { width?: string; className?: string }) {
  return <Skeleton className={`h-3.5 ${width} ${className}`} />;
}

/**
 * A vertical lot-card placeholder for grid views (HomePage panels, SearchPage results).
 * Aspect ratio matches the production <c>LotCard</c> footprint so the grid doesn't reflow.
 */
export function SkeletonLotCard() {
  return <Skeleton style={{ aspectRatio: '0.78' }} />;
}

/**
 * A horizontal row placeholder for list views (My lots, My bids, reports table).
 */
export function SkeletonRow({ height = 'h-20' }: { height?: string }) {
  return <Skeleton className={`${height} w-full`} />;
}
