import type { ReactNode } from 'react';

interface AuthCardProps {
  children: ReactNode;
  width?: number;
}

export function AuthCard({ children, width = 420 }: AuthCardProps) {
  return (
    <div
      className="w-full bg-surface border border-border rounded-xl p-9"
      style={{ maxWidth: width, boxShadow: 'var(--shadow-2)' }}
    >
      {children}
    </div>
  );
}
