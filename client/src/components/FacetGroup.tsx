import type { FacetValue } from '@/api/lots';

interface FacetGroupProps {
  title: string;
  values: FacetValue[];
  selected: string[];
  onToggle: (value: string) => void;
}

function FacetItem({
  facet,
  checked,
  onToggle,
}: {
  facet: FacetValue;
  checked: boolean;
  onToggle: (value: string) => void;
}) {
  return (
    <li>
      <label className="flex items-center justify-between gap-2 text-sm py-1 cursor-pointer text-text-2 hover:text-text">
        <span className="flex items-center gap-2 min-w-0">
          <input type="checkbox" checked={checked} onChange={() => onToggle(facet.value)} className="accent-accent shrink-0" />
          <span className="truncate" title={facet.value}>
            {facet.value}
          </span>
        </span>
        <span className="mono text-xs text-text-3 shrink-0">{facet.count}</span>
      </label>
    </li>
  );
}

/**
 * A sidebar facet. Values are split across two columns to save vertical space; counts are right-aligned
 * within each column and the columns are separated by a divider, so each count stays clearly tied to
 * its own value. Uses CSS Grid (not flex) so a single-value section stays at half-column width instead
 * of stretching across the full sidebar. Renders nothing when there are no values.
 */
export function FacetGroup({ title, values, selected, onToggle }: FacetGroupProps) {
  if (values.length === 0) return null;

  const mid = Math.ceil(values.length / 2);
  const left = values.slice(0, mid);
  const right = values.slice(mid);

  return (
    <div className="mb-6">
      <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">{title}</div>
      <div className="grid grid-cols-2 gap-x-4">
        <ul className="min-w-0 space-y-1 m-0 p-0 list-none">
          {left.map((facet) => (
            <FacetItem key={facet.value} facet={facet} checked={selected.includes(facet.value)} onToggle={onToggle} />
          ))}
        </ul>
        {right.length > 0 && (
          <ul className="min-w-0 space-y-1 pl-4 border-l border-border m-0 list-none">
            {right.map((facet) => (
              <FacetItem key={facet.value} facet={facet} checked={selected.includes(facet.value)} onToggle={onToggle} />
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
