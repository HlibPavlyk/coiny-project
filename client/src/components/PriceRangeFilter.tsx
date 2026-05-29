import { useEffect, useState } from 'react';

interface PriceRangeFilterProps {
  /** Current bounds in whole UAH (not kopiykas); undefined = no bound. */
  fromUah: number | undefined;
  toUah: number | undefined;
  /** Debounced change with parsed UAH bounds (undefined when the field is empty). */
  onChange: (fromUah: number | undefined, toUah: number | undefined) => void;
}

const parse = (s: string): number | undefined => {
  const n = Number(s);
  return s.trim() === '' || Number.isNaN(n) || n < 0 ? undefined : n;
};

/**
 * Min/max price filter in UAH. The parent stores/queries in kopiykas; this widget speaks UAH and
 * debounces edits (400 ms) so typing doesn't refire the search on every keystroke.
 */
export function PriceRangeFilter({ fromUah, toUah, onChange }: PriceRangeFilterProps) {
  const [from, setFrom] = useState(fromUah?.toString() ?? '');
  const [to, setTo] = useState(toUah?.toString() ?? '');

  // Re-sync when the bounds change from outside (e.g. "Clear all").
  useEffect(() => setFrom(fromUah?.toString() ?? ''), [fromUah]);
  useEffect(() => setTo(toUah?.toString() ?? ''), [toUah]);

  // Debounce edits before pushing them up.
  useEffect(() => {
    const id = setTimeout(() => onChange(parse(from), parse(to)), 400);
    return () => clearTimeout(id);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [from, to]);

  return (
    <div className="mb-6">
      <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">Price (UAH)</div>
      <div className="flex items-center gap-2">
        <input
          type="number"
          min={0}
          inputMode="numeric"
          placeholder="Min"
          value={from}
          onChange={(e) => setFrom(e.target.value)}
          className="w-full rounded-md border border-border-strong bg-surface text-sm py-1.5 px-2.5"
          aria-label="Minimum price in UAH"
        />
        <span className="text-text-3 text-sm">–</span>
        <input
          type="number"
          min={0}
          inputMode="numeric"
          placeholder="Max"
          value={to}
          onChange={(e) => setTo(e.target.value)}
          className="w-full rounded-md border border-border-strong bg-surface text-sm py-1.5 px-2.5"
          aria-label="Maximum price in UAH"
        />
      </div>
    </div>
  );
}
