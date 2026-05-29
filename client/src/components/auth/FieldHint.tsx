import type { ReactNode } from 'react';

export function FieldHint({ children }: { children: ReactNode }) {
  return <div className="text-xs text-text-3 mt-1.5">{children}</div>;
}

export function FieldLabel({ children, htmlFor, optional }: { children: ReactNode; htmlFor: string; optional?: boolean }) {
  return (
    <label htmlFor={htmlFor} className="block text-sm font-medium mb-1.5">
      {children}
      {optional && <span className="text-text-3 font-normal"> · optional</span>}
    </label>
  );
}

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  mono?: boolean;
}

export function FieldInput({ mono, className, ...rest }: InputProps) {
  return (
    <input
      {...rest}
      className={[
        'w-full rounded-md border border-border-strong bg-surface px-3 py-2.5 text-sm transition',
        'focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/15',
        mono ? 'mono' : '',
        className ?? '',
      ].join(' ')}
    />
  );
}
