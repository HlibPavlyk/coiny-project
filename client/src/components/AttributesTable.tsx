type SubcategoryKind = 'Coin' | 'Banknote' | 'Medal';

const KNOWN_KEYS: Record<SubcategoryKind, { key: string; label: string }[]> = {
  Coin: [
    { key: 'country', label: 'Country' },
    { key: 'year', label: 'Year' },
    { key: 'denomination', label: 'Denomination' },
    { key: 'metal', label: 'Metal' },
    { key: 'weight_g', label: 'Weight (g)' },
    { key: 'catalog_number', label: 'Catalog #' },
  ],
  Banknote: [
    { key: 'country', label: 'Country' },
    { key: 'year', label: 'Year' },
    { key: 'denomination', label: 'Denomination' },
    { key: 'series', label: 'Series' },
    { key: 'serial_number', label: 'Serial #' },
    { key: 'pick_number', label: 'Pick #' },
  ],
  Medal: [
    { key: 'country', label: 'Country' },
    { key: 'period', label: 'Period' },
    { key: 'metal', label: 'Metal' },
    { key: 'award_name', label: 'Award name' },
    { key: 'issuer', label: 'Issuer' },
    { key: 'serial_number', label: 'Serial #' },
  ],
};

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col sm:grid sm:grid-cols-[150px_1fr] gap-1 sm:gap-x-6 py-2.5 border-b border-border-soft text-[13px]">
      <span className="text-text-3">{label}</span>
      <span className="text-text font-medium break-words">{value}</span>
    </div>
  );
}

function isPresent(v: unknown): boolean {
  return v !== undefined && v !== null && v !== '';
}

export function AttributesTable({
  attributes,
  subcategoryKind,
  headerRows,
}: {
  attributes: Record<string, unknown>;
  subcategoryKind: SubcategoryKind | null;
  /** Optional rows rendered before the known/other attributes (e.g., Condition, Category). */
  headerRows?: { label: string; value: string }[];
}) {
  const hasAttributes = attributes && Object.keys(attributes).length > 0;
  const known = subcategoryKind ? KNOWN_KEYS[subcategoryKind] : [];
  const knownKeySet = new Set(known.map((k) => k.key));
  const knownRows = hasAttributes ? known.filter((k) => isPresent(attributes[k.key])) : [];
  const otherKeys = hasAttributes
    ? Object.keys(attributes).filter((k) => !knownKeySet.has(k) && isPresent(attributes[k]))
    : [];

  const safeHeaderRows = headerRows?.filter((r) => isPresent(r.value)) ?? [];

  if (safeHeaderRows.length === 0 && knownRows.length === 0 && otherKeys.length === 0) {
    return <p className="text-[13px] text-text-3">No attributes provided.</p>;
  }

  return (
    <div>
      {safeHeaderRows.length > 0 && (
        <div>
          {safeHeaderRows.map((r) => (
            <Row key={r.label} label={r.label} value={r.value} />
          ))}
        </div>
      )}
      {knownRows.length > 0 && (
        <div>
          {knownRows.map((k) => (
            <Row key={k.key} label={k.label} value={String(attributes[k.key])} />
          ))}
        </div>
      )}
      {otherKeys.length > 0 && (
        <div className="mt-5">
          <h4 className="text-[11px] font-semibold uppercase tracking-wider text-text-3 mb-1">
            Other
          </h4>
          {otherKeys.map((k) => (
            <Row key={k} label={k} value={String(attributes[k])} />
          ))}
        </div>
      )}
    </div>
  );
}
