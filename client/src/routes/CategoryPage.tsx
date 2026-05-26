import { Navigate, useParams } from 'react-router-dom';

/**
 * Category browsing is now part of the unified `/search` page (category is just a filter). This route
 * is kept so existing `/category/:slug` links (nav, footer, lot breadcrumbs, bookmarks) deep-link into
 * the search page with the category preselected.
 */
export default function CategoryPage() {
  const { slug = '' } = useParams<{ slug: string }>();
  return <Navigate to={`/search?category=${encodeURIComponent(slug)}`} replace />;
}
