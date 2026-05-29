import { useEffect, type RefObject } from 'react';

/**
 * Trap keyboard focus inside the element referenced by <c>ref</c> while <c>active</c> is true.
 * Tab cycles forward through focusable children; Shift+Tab cycles backward. The first focusable
 * element receives focus on activation; on deactivation, focus returns to the previously-focused
 * element (so closing a modal returns focus to the trigger button).
 *
 * Why: WCAG 2.1 SC 2.1.2 — no keyboard trap. The opposite case (escaping a modal accidentally
 * via Tab) makes screen-reader users lose context. This hook handles the standard pattern.
 *
 * Usage:
 * <pre>
 *   const dialogRef = useRef&lt;HTMLDivElement&gt;(null);
 *   useFocusTrap(dialogRef, isOpen);
 *   return &lt;div ref={dialogRef}&gt;...&lt;/div&gt;;
 * </pre>
 */
const FOCUSABLE = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',');

export function useFocusTrap(ref: RefObject<HTMLElement | null>, active: boolean) {
  useEffect(() => {
    if (!active || !ref.current) return;

    const container = ref.current;
    const previouslyFocused = document.activeElement as HTMLElement | null;

    // Focus the first focusable element so the keyboard user lands inside the dialog immediately.
    const focusables = () => Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE));
    const first = focusables()[0];
    if (first && !container.contains(document.activeElement)) {
      first.focus();
    }

    const onKey = (e: KeyboardEvent) => {
      if (e.key !== 'Tab') return;
      const list = focusables();
      if (list.length === 0) {
        e.preventDefault();
        return;
      }
      const firstEl = list[0];
      const lastEl = list[list.length - 1];
      const activeEl = document.activeElement as HTMLElement | null;

      if (e.shiftKey && activeEl === firstEl) {
        e.preventDefault();
        lastEl.focus();
      } else if (!e.shiftKey && activeEl === lastEl) {
        e.preventDefault();
        firstEl.focus();
      }
    };

    container.addEventListener('keydown', onKey);
    return () => {
      container.removeEventListener('keydown', onKey);
      // Return focus to the trigger so screen-reader users keep their place in the document.
      previouslyFocused?.focus?.();
    };
  }, [ref, active]);
}
