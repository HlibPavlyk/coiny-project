import { Link } from 'react-router-dom';

export interface BreadcrumbPart {
  label: string;
  href?: string;
}

export function Breadcrumb({ parts }: { parts: BreadcrumbPart[] }) {
  return (
    <nav aria-label="Breadcrumb" className="flex items-center flex-wrap gap-1.5 text-[12px] text-text-3">
      {parts.map((p, i) => {
        const isLast = i === parts.length - 1;
        return (
          <span key={i} className="inline-flex items-center gap-1.5">
            {p.href && !isLast ? (
              <Link to={p.href} className="text-text-3 hover:text-text-2 no-underline">
                {p.label}
              </Link>
            ) : (
              <span className={isLast ? 'text-text-2 font-medium' : ''}>{p.label}</span>
            )}
            {!isLast && (
              <span className="text-text-3 select-none" aria-hidden="true">
                ›
              </span>
            )}
          </span>
        );
      })}
    </nav>
  );
}
