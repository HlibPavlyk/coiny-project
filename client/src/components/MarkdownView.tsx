import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

interface MarkdownViewProps {
  source: string;
  className?: string;
}

/**
 * Read-only markdown renderer. Uses react-markdown with remark-gfm (tables, strikethrough,
 * task lists, autolinks). HTML in source is NOT rendered — react-markdown ignores raw HTML
 * by default, which prevents XSS injection through stored descriptions.
 */
export function MarkdownView({ source, className }: MarkdownViewProps) {
  return (
    <div className={['markdown-body', className ?? ''].join(' ')}>
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          h1: ({ children }) => (
            <h1 className="text-[20px] font-bold mt-5 mb-2 text-text">{children}</h1>
          ),
          h2: ({ children }) => (
            <h2 className="text-[17px] font-semibold mt-5 mb-2 text-text">{children}</h2>
          ),
          h3: ({ children }) => (
            <h3 className="text-[15px] font-semibold mt-4 mb-1.5 text-text">{children}</h3>
          ),
          p: ({ children }) => (
            <p className="my-3 text-text" style={{ lineHeight: 1.7 }}>
              {children}
            </p>
          ),
          ul: ({ children }) => (
            <ul className="list-disc pl-6 my-3 text-text" style={{ lineHeight: 1.7 }}>
              {children}
            </ul>
          ),
          ol: ({ children }) => (
            <ol className="list-decimal pl-6 my-3 text-text" style={{ lineHeight: 1.7 }}>
              {children}
            </ol>
          ),
          li: ({ children }) => <li className="my-1">{children}</li>,
          blockquote: ({ children }) => (
            <blockquote
              className="border-l-4 pl-4 my-4 text-text-2"
              style={{ borderColor: 'var(--color-accent)', fontStyle: 'italic' }}
            >
              {children}
            </blockquote>
          ),
          code: ({ children, ...props }) => {
            const isInline = !('inline' in props && props.inline === false);
            return isInline ? (
              <code
                className="mono text-[0.9em] px-1.5 py-0.5 rounded"
                style={{ background: 'var(--color-bg-soft)', color: 'var(--color-accent-deep)' }}
              >
                {children}
              </code>
            ) : (
              <code className="mono">{children}</code>
            );
          },
          pre: ({ children }) => (
            <pre
              className="mono text-[12.5px] rounded-md p-3 my-3 overflow-x-auto"
              style={{ background: 'var(--color-bg-soft)', border: '1px solid var(--color-border)' }}
            >
              {children}
            </pre>
          ),
          a: ({ children, href }) => (
            <a
              href={href}
              target="_blank"
              rel="noopener noreferrer"
              className="text-accent-deep hover:underline"
            >
              {children}
            </a>
          ),
          hr: () => <hr className="my-5 border-border" />,
          strong: ({ children }) => <strong className="font-semibold text-text">{children}</strong>,
          em: ({ children }) => <em className="italic">{children}</em>,
          table: ({ children }) => (
            <div className="overflow-x-auto my-3">
              <table className="w-full text-[13px] border-collapse">{children}</table>
            </div>
          ),
          thead: ({ children }) => <thead className="text-left">{children}</thead>,
          th: ({ children }) => (
            <th className="px-3 py-2 font-semibold text-text-2 border-b border-border">
              {children}
            </th>
          ),
          td: ({ children }) => (
            <td className="px-3 py-2 border-b border-border-soft text-text">{children}</td>
          ),
        }}
      >
        {source}
      </ReactMarkdown>
    </div>
  );
}
