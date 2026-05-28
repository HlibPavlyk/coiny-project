import { Icon } from './Icon';

interface PageErrorProps {
  title?: string;
  description?: string;
  /** Optional retry callback — shows a "Try again" button when provided. */
  onRetry?: () => void;
  retryLabel?: string;
}

/**
 * Inline error banner — for use inside a page when a single data fetch fails but the rest of the
 * page chrome is fine (e.g. a list block fails to load). For unrecoverable navigation errors use
 * <c>InternalErrorPage</c> instead.
 */
export function PageError({
  title = 'Could not load this section',
  description = 'Please check your connection or try again in a moment.',
  onRetry,
  retryLabel = 'Try again',
}: PageErrorProps) {
  return (
    <div
      role="alert"
      className="bg-surface border border-dashed rounded-lg py-10 px-6 text-center"
      style={{ borderColor: 'var(--color-border)' }}
    >
      <div
        className="w-12 h-12 mx-auto mb-3.5 rounded-full flex items-center justify-center"
        style={{ background: 'var(--color-warning-soft)' }}
      >
        <Icon name="info" size={22} color="#92400E" stroke={1.7} />
      </div>
      <div className="text-[14.5px] font-semibold text-text">{title}</div>
      <p className="text-[13px] text-text-3 mt-1.5 max-w-[360px] mx-auto leading-relaxed m-0">{description}</p>
      {onRetry && (
        <button
          type="button"
          onClick={onRetry}
          className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-4 py-2 text-[13px] mt-4"
        >
          {retryLabel}
        </button>
      )}
    </div>
  );
}
