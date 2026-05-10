import { createBrowserRouter } from 'react-router-dom';
import { Stub } from './Stub';
import { RequireAuth } from '@/components/RequireAuth';

/**
 * Stub routing tree mirroring /docs/03-frontend-structure.md.
 * Real components ship in tasks 22–25; admin page comes in sprint 4.
 */
export const router = createBrowserRouter([
  // Public
  { path: '/', element: <Stub name="Home" /> },
  { path: '/category/:slug', element: <Stub name="Category" /> },
  { path: '/lot/:id', element: <Stub name="Lot detail" /> },
  { path: '/search', element: <Stub name="Search" /> },

  // Auth
  { path: '/sign-in', element: <Stub name="Sign in" /> },
  { path: '/sign-up', element: <Stub name="Sign up" /> },
  { path: '/auth/callback', element: <Stub name="Auth callback" /> },
  { path: '/verify-email', element: <Stub name="Verify email" /> },

  // Public profile
  { path: '/profile/:userId', element: <Stub name="Public profile" /> },

  // Authenticated
  {
    path: '/profile',
    element: (
      <RequireAuth>
        <Stub name="My profile" />
      </RequireAuth>
    ),
  },
  {
    path: '/my-lots',
    element: (
      <RequireAuth>
        <Stub name="My lots" />
      </RequireAuth>
    ),
  },
  {
    path: '/my-bids',
    element: (
      <RequireAuth>
        <Stub name="My bids" />
      </RequireAuth>
    ),
  },
  {
    path: '/my-purchases',
    element: (
      <RequireAuth>
        <Stub name="My purchases" />
      </RequireAuth>
    ),
  },
  {
    path: '/seller/onboarding',
    element: (
      <RequireAuth>
        <Stub name="Seller onboarding" />
      </RequireAuth>
    ),
  },
  {
    path: '/seller/onboarded',
    element: (
      <RequireAuth>
        <Stub name="Seller onboarded" />
      </RequireAuth>
    ),
  },
  {
    path: '/lots/new',
    element: (
      <RequireAuth>
        <Stub name="Create lot" />
      </RequireAuth>
    ),
  },
  {
    path: '/lots/:id/edit',
    element: (
      <RequireAuth>
        <Stub name="Edit lot" />
      </RequireAuth>
    ),
  },

  // Admin / Moderator
  {
    path: '/admin',
    element: (
      <RequireAuth roles={['Admin', 'Moderator']}>
        <Stub name="Admin landing" />
      </RequireAuth>
    ),
  },
  {
    path: '/admin/reports',
    element: (
      <RequireAuth roles={['Admin', 'Moderator']}>
        <Stub name="Reports" />
      </RequireAuth>
    ),
  },

  // 404
  { path: '*', element: <Stub name="Not found" /> },
]);
