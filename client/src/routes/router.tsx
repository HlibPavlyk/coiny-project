import { Outlet, createBrowserRouter } from 'react-router-dom';
import { RequireAuth } from '@/components/RequireAuth';
import NotFoundPage from './NotFoundPage';
import InternalErrorPage from './InternalErrorPage';

/**
 * Pathless root route — exists solely to attach a single <c>errorElement</c> that catches any
 * thrown render-time error or loader rejection in any descendant route. Without it, an uncaught
 * error in a leaf component shows a blank white page.
 */
const RootErrorBoundary = () => <Outlet />;
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
import ModerationOverviewPage from './ModerationOverviewPage';
import ModerationReportsPage from './ModerationReportsPage';
import ModerationUsersPage from './ModerationUsersPage';
import ModerationLotsPage from './ModerationLotsPage';
import ModerationDemoPage from './ModerationDemoPage';
import { MyAccountLayout } from './MyAccountLayout';
import { ModerationLayout } from './ModerationLayout';

/**
 * Stub routing tree mirroring /docs/03-frontend-structure.md.
 * Real components ship in tasks 22–25; admin page comes in sprint 4.
 */
export const router = createBrowserRouter([{
  element: <RootErrorBoundary />,
  errorElement: <InternalErrorPage />,
  children: [
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

  // My account — single layout route so the sidebar stays mounted across child navigations.
  {
    element: (
      <RequireAuth>
        <MyAccountLayout />
      </RequireAuth>
    ),
    children: [
      { path: '/profile', element: <MyProfilePage /> },
      { path: '/my-lots', element: <MyLotsPage /> },
      { path: '/my-bids', element: <MyBidsPage /> },
      { path: '/my-purchases', element: <MyPurchasesPage /> },
    ],
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

  // Moderation (Moderator + Admin) — layout-route pattern so the sidebar persists.
  {
    element: (
      <RequireAuth roles={['Admin', 'Moderator']}>
        <ModerationLayout />
      </RequireAuth>
    ),
    children: [
      { path: '/moderation', element: <ModerationOverviewPage /> },
      { path: '/moderation/reports', element: <ModerationReportsPage /> },
      { path: '/moderation/users', element: <ModerationUsersPage /> },
      { path: '/moderation/lots', element: <ModerationLotsPage /> },
      // Admin-only — Demo tab is hidden from Moderators in the sidebar, and the route is also
      // wrapped in a tighter role guard so a direct URL hit by a Moderator falls back to a 403.
      {
        path: '/moderation/demo',
        element: (
          <RequireAuth roles={['Admin']}>
            <ModerationDemoPage />
          </RequireAuth>
        ),
      },
    ],
  },

  // 404 — any unmatched route lands here.
  { path: '*', element: <NotFoundPage /> },
  ],
}]);
