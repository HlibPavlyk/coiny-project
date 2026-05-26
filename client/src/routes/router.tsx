import { createBrowserRouter } from 'react-router-dom';
import { Stub } from './Stub';
import { RequireAuth } from '@/components/RequireAuth';
import SignInPage from './SignInPage';
import SignUpPage from './SignUpPage';
import VerifyEmailPage from './VerifyEmailPage';
import AuthCallbackPage from './AuthCallbackPage';
import MyProfilePage from './MyProfilePage';
import HomePage from './HomePage';
import CategoryPage from './CategoryPage';
import LotPage from './LotPage';
import CreateLotPage from './CreateLotPage';
import EditLotPage from './EditLotPage';
import MyLotsPage from './MyLotsPage';
import MyBidsPage from './MyBidsPage';
import SellerOnboardingPage from './SellerOnboardingPage';
import SellerOnboardedPage from './SellerOnboardedPage';
import PayLotPage from './PayLotPage';
import MyPurchasesPage from './MyPurchasesPage';
import PublicProfilePage from './PublicProfilePage';
import SearchPage from './SearchPage';
import AdminLandingPage from './AdminLandingPage';
import AdminReportsPage from './AdminReportsPage';

/**
 * Stub routing tree mirroring /docs/03-frontend-structure.md.
 * Real components ship in tasks 22–25; admin page comes in sprint 4.
 */
export const router = createBrowserRouter([
  // Public
  { path: '/', element: <HomePage /> },
  { path: '/category/:slug', element: <CategoryPage /> },
  { path: '/lot/:id', element: <LotPage /> },
  { path: '/search', element: <SearchPage /> },

  // Auth
  { path: '/sign-in', element: <SignInPage /> },
  { path: '/sign-up', element: <SignUpPage /> },
  { path: '/auth/callback', element: <AuthCallbackPage /> },
  { path: '/verify-email', element: <VerifyEmailPage /> },

  // Public profile
  { path: '/profile/:userId', element: <PublicProfilePage /> },

  // Authenticated
  {
    path: '/profile',
    element: (
      <RequireAuth>
        <MyProfilePage />
      </RequireAuth>
    ),
  },
  {
    path: '/my-lots',
    element: (
      <RequireAuth>
        <MyLotsPage />
      </RequireAuth>
    ),
  },
  {
    path: '/my-bids',
    element: (
      <RequireAuth>
        <MyBidsPage />
      </RequireAuth>
    ),
  },
  {
    path: '/my-purchases',
    element: (
      <RequireAuth>
        <MyPurchasesPage />
      </RequireAuth>
    ),
  },
  {
    path: '/my-purchases/:lotId/pay',
    element: (
      <RequireAuth>
        <PayLotPage />
      </RequireAuth>
    ),
  },
  {
    path: '/seller/onboarding',
    element: (
      <RequireAuth>
        <SellerOnboardingPage />
      </RequireAuth>
    ),
  },
  {
    path: '/seller/onboarded',
    element: (
      <RequireAuth>
        <SellerOnboardedPage />
      </RequireAuth>
    ),
  },
  {
    path: '/lots/new',
    element: (
      <RequireAuth>
        <CreateLotPage />
      </RequireAuth>
    ),
  },
  {
    path: '/lots/:id/edit',
    element: (
      <RequireAuth>
        <EditLotPage />
      </RequireAuth>
    ),
  },

  // Admin / Moderator
  {
    path: '/admin',
    element: (
      <RequireAuth roles={['Admin', 'Moderator']}>
        <AdminLandingPage />
      </RequireAuth>
    ),
  },
  {
    path: '/admin/reports',
    element: (
      <RequireAuth roles={['Admin', 'Moderator']}>
        <AdminReportsPage />
      </RequireAuth>
    ),
  },

  // 404
  { path: '*', element: <Stub name="Not found" /> },
]);
