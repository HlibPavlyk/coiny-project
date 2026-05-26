import { useEffect, useRef, useState } from 'react';
import type { CategoryNode, CategoryTree } from '@/api/categories';
import { Icon } from './Icon';

interface CategoryFilterProps {
  tree: CategoryTree | undefined;
  /** Currently selected category slug, or null for "all". */
  selectedSlug: string | null;
  /** Match counts for the current search, keyed by leaf categoryId (from the Meilisearch facet). */
  counts: Map<number, number>;
  onSelect: (slug: string | null) => void;
}

/** Total matches under a node = its own count plus every descendant's (lots live on leaf ids). */
function aggregateCount(node: CategoryNode, counts: Map<number, number>): number {
  let sum = counts.get(node.id) ?? 0;
  for (const child of node.children) sum += aggregateCount(child, counts);
  return sum;
}

/** Ids of every node on the path from a root down to the selected slug (inclusive). */
function selectedPathIds(roots: CategoryNode[], slug: string | null): Set<number> {
  const ids = new Set<number>();
  if (!slug) return ids;
  const walk = (node: CategoryNode, trail: number[]): boolean => {
    const next = [...trail, node.id];
    if (node.slug === slug) {
      next.forEach((id) => ids.add(id));
      return true;
    }
    return node.children.some((child) => walk(child, next));
  };
  roots.some((root) => walk(root, []));
  return ids;
}

/**
 * Collapsible category tree with a single, persistent open-state. Everything starts collapsed; a caret
 * toggles a branch, and selecting a category opens (and keeps open) its whole path. Opened branches
 * stay open until explicitly collapsed — switching sections never auto-collapses the others. Counts
 * come from the search's `categoryId` facet, so they reflect the active query and filters.
 */
export function CategoryFilter({ tree, selectedSlug, counts, onSelect }: CategoryFilterProps) {
  const [expanded, setExpanded] = useState<Set<number>>(new Set());

  // On first load only, open the path to a deep-linked category. After that, expansion is fully
  // manual (caret or double-click) — selecting/switching categories never moves the open state.
  const didInit = useRef(false);
  useEffect(() => {
    if (didInit.current || !tree) return;
    didInit.current = true;
    if (selectedSlug) setExpanded(selectedPathIds(tree.roots, selectedSlug));
  }, [tree, selectedSlug]);

  if (!tree) return null;

  const toggle = (id: number) =>
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  // Single click selects (clear via "All categories"); double click expands/collapses so you don't
  // have to aim at the caret. Selecting never auto-expands, so the two never fight.
  const select = (node: CategoryNode) => onSelect(node.slug);

  const renderNode = (node: CategoryNode) => {
    const hasChildren = node.children.length > 0;
    const open = expanded.has(node.id);
    const active = node.slug === selectedSlug;

    return (
      <li key={node.id}>
        <div className={`flex items-center rounded ${active ? 'bg-bg-soft' : 'hover:bg-bg-soft'}`}>
          {hasChildren ? (
            <button
              type="button"
              onClick={() => toggle(node.id)}
              aria-label={open ? `Collapse ${node.name}` : `Expand ${node.name}`}
              className="p-1 text-text-3 hover:text-text shrink-0"
            >
              <span className={`inline-block transition-transform ${open ? 'rotate-90' : ''}`}>
                <Icon name="arrowR" size={11} />
              </span>
            </button>
          ) : (
            <span className="w-[19px] shrink-0" />
          )}
          <button
            type="button"
            onClick={() => select(node)}
            onDoubleClick={hasChildren ? () => toggle(node.id) : undefined}
            className={`flex flex-1 items-center justify-between gap-2 text-sm py-1 pr-1.5 text-left min-w-0 select-none ${
              active ? 'text-text font-semibold' : 'text-text-2'
            }`}
          >
            <span className="truncate" title={node.name}>
              {node.name}
            </span>
            <span className="mono text-xs text-text-3 shrink-0">{aggregateCount(node, counts)}</span>
          </button>
        </div>

        {hasChildren && open && (
          <ul className="ml-3 pl-2 border-l border-border space-y-0.5">{node.children.map(renderNode)}</ul>
        )}
      </li>
    );
  };

  return (
    <div className="mb-6">
      <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">Category</div>
      <ul className="space-y-0.5">
        <li>
          <button
            type="button"
            onClick={() => onSelect(null)}
            className={`w-full text-sm py-1 px-1.5 rounded text-left ${
              selectedSlug === null ? 'text-text font-semibold bg-bg-soft' : 'text-text-2 hover:text-text hover:bg-bg-soft'
            }`}
          >
            All categories
          </button>
        </li>
        {tree.roots.map(renderNode)}
      </ul>
    </div>
  );
}
