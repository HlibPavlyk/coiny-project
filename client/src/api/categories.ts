import { useQuery } from '@tanstack/react-query';
import { api } from './fetch';

export interface CategoryNode {
  id: number;
  slug: string;
  name: string;
  level: 0 | 1 | 2;
  isLeaf: boolean;
  subcategoryKind: 'Coin' | 'Banknote' | 'Medal' | null;
  lotCountActive: number;
  children: CategoryNode[];
}

export interface CategoryTree {
  roots: CategoryNode[];
}

export const categories = {
  getTree: () => api<CategoryTree>('/api/v1/categories'),
};

export function useCategoryTree() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: categories.getTree,
    staleTime: 5 * 60 * 1000,
  });
}

/** Walk the tree and return the node matching slug, or undefined. */
export function findCategoryBySlug(tree: CategoryTree | undefined, slug: string): CategoryNode | undefined {
  if (!tree) return undefined;
  function walk(node: CategoryNode): CategoryNode | undefined {
    if (node.slug === slug) return node;
    for (const child of node.children) {
      const found = walk(child);
      if (found) return found;
    }
    return undefined;
  }
  for (const root of tree.roots) {
    const found = walk(root);
    if (found) return found;
  }
  return undefined;
}
