import { useEffect } from 'react';
import { RouterProvider } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { router } from '@/routes/router';
import { useAuthStore } from '@/state/useAuthStore';
import { ToastViewport } from '@/components/Toast';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      retry: (failureCount, err: unknown) => {
        // Don't retry on 4xx — only on transient network/5xx
        if (err && typeof err === 'object' && 'status' in err) {
          const s = (err as { status: number }).status;
          if (s >= 400 && s < 500) return false;
        }
        return failureCount < 2;
      },
    },
  },
});

export default function App() {
  useEffect(() => {
    void useAuthStore.getState().refresh();
  }, []);

  return (
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
      <ToastViewport />
    </QueryClientProvider>
  );
}
