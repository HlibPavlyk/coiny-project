import { FieldInput, FieldLabel } from './auth/FieldHint';
import { COUNTRIES } from '@/lib/countries';

export type SubcategoryKind = 'Coin' | 'Banknote' | 'Medal';

const METAL_OPTIONS = ['Gold', 'Silver', 'Copper', 'Nickel', 'Bronze', 'Bimetal', 'Other'] as const;

interface CategoryAttributeFormProps {
  subcategoryKind: SubcategoryKind;
  value: Record<string, unknown>;
  onChange: (next: Record<string, unknown>) => void;
}

function set(value: Record<string, unknown>, key: string, v: unknown): Record<string, unknown> {
  if (v === '' || v === null || v === undefined) {
    const { [key]: _omit, ...rest } = value;
    return rest;
  }
  return { ...value, [key]: v };
}

function getString(value: Record<string, unknown>, key: string): string {
  const v = value[key];
  return v === undefined || v === null ? '' : String(v);
}

function MetalSelect({
  id,
  value,
  onChange,
}: {
  id: string;
  value: string;
  onChange: (v: string) => void;
}) {
  return (
    <select
      id={id}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="w-full rounded-md border border-border-strong bg-surface px-3 py-2.5 text-sm transition focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/15"
    >
      <option value="">Select metal…</option>
      {METAL_OPTIONS.map((m) => (
        <option key={m} value={m}>
          {m}
        </option>
      ))}
    </select>
  );
}

function CountryField({
  value,
  onChange,
}: {
  value: string;
  onChange: (v: string) => void;
}) {
  return (
    <>
      <FieldLabel htmlFor="attr-country">Country</FieldLabel>
      <FieldInput
        id="attr-country"
        list="country-list"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="Start typing…"
        maxLength={60}
      />
      <datalist id="country-list">
        {COUNTRIES.map((c) => (
          <option key={c} value={c} />
        ))}
      </datalist>
    </>
  );
}

export function CategoryAttributeForm({
  subcategoryKind,
  value,
  onChange,
}: CategoryAttributeFormProps) {
  const v = (key: string) => getString(value, key);
  const upd = (key: string) => (next: string) => onChange(set(value, key, next));

  if (subcategoryKind === 'Coin') {
    return (
      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2">
        <div>
          <CountryField value={v('country')} onChange={upd('country')} />
        </div>
        <div>
          <FieldLabel htmlFor="attr-year">Year</FieldLabel>
          <FieldInput
            id="attr-year"
            value={v('year')}
            onChange={(e) => upd('year')(e.target.value)}
            placeholder="1894 or 1894–1917"
            maxLength={20}
          />
        </div>
        <div>
          <FieldLabel htmlFor="attr-denomination">Denomination</FieldLabel>
          <FieldInput
            id="attr-denomination"
            value={v('denomination')}
            onChange={(e) => upd('denomination')(e.target.value)}
            placeholder="1 hryvnia, 5 kopecks…"
            maxLength={40}
          />
        </div>
        <div>
          <FieldLabel htmlFor="attr-metal">Metal</FieldLabel>
          <MetalSelect id="attr-metal" value={v('metal')} onChange={upd('metal')} />
        </div>
        <div>
          <FieldLabel htmlFor="attr-weight">Weight (g)</FieldLabel>
          <FieldInput
            id="attr-weight"
            type="number"
            min={0}
            max={10000}
            step="0.01"
            mono
            value={v('weight_g')}
            onChange={(e) => upd('weight_g')(e.target.value)}
            placeholder="e.g. 19.99"
          />
        </div>
        <div>
          <FieldLabel htmlFor="attr-catalog">Catalog #</FieldLabel>
          <FieldInput
            id="attr-catalog"
            value={v('catalog_number')}
            onChange={(e) => upd('catalog_number')(e.target.value)}
            placeholder="KM#Y-59.3"
            maxLength={60}
          />
        </div>
      </div>
    );
  }

  if (subcategoryKind === 'Banknote') {
    return (
      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2">
        <div>
          <CountryField value={v('country')} onChange={upd('country')} />
        </div>
        <div>
          <FieldLabel htmlFor="attr-year">Year</FieldLabel>
          <FieldInput
            id="attr-year"
            value={v('year')}
            onChange={(e) => upd('year')(e.target.value)}
            maxLength={20}
          />
        </div>
        <div>
          <FieldLabel htmlFor="attr-denomination">Denomination</FieldLabel>
          <FieldInput
            id="attr-denomination"
            value={v('denomination')}
            onChange={(e) => upd('denomination')(e.target.value)}
            placeholder="100 hryvnia"
            maxLength={40}
          />
        </div>
        <div>
          <FieldLabel htmlFor="attr-series">Series</FieldLabel>
          <FieldInput
            id="attr-series"
            value={v('series')}
            onChange={(e) => upd('series')(e.target.value)}
            maxLength={30}
          />
        </div>
        <div>
          <FieldLabel htmlFor="attr-serial">Serial #</FieldLabel>
          <FieldInput
            id="attr-serial"
            mono
            value={v('serial_number')}
            onChange={(e) => upd('serial_number')(e.target.value)}
            placeholder="As printed"
            maxLength={30}
          />
        </div>
        <div>
          <FieldLabel htmlFor="attr-pick">Pick #</FieldLabel>
          <FieldInput
            id="attr-pick"
            mono
            value={v('pick_number')}
            onChange={(e) => upd('pick_number')(e.target.value)}
            placeholder="P-123"
            maxLength={30}
          />
        </div>
      </div>
    );
  }

  // Medal
  return (
    <div className="grid gap-4 grid-cols-1 sm:grid-cols-2">
      <div>
        <CountryField value={v('country')} onChange={upd('country')} />
      </div>
      <div>
        <FieldLabel htmlFor="attr-period">Period</FieldLabel>
        <FieldInput
          id="attr-period"
          value={v('period')}
          onChange={(e) => upd('period')(e.target.value)}
          placeholder="1939–1945, Cold War…"
          maxLength={40}
        />
      </div>
      <div>
        <FieldLabel htmlFor="attr-metal">Metal</FieldLabel>
        <MetalSelect id="attr-metal" value={v('metal')} onChange={upd('metal')} />
      </div>
      <div>
        <FieldLabel htmlFor="attr-award-name">Award name</FieldLabel>
        <FieldInput
          id="attr-award-name"
          value={v('award_name')}
          onChange={(e) => upd('award_name')(e.target.value)}
          placeholder="Order of the Patriotic War"
          maxLength={100}
        />
      </div>
      <div>
        <FieldLabel htmlFor="attr-issuer">Issuer</FieldLabel>
        <FieldInput
          id="attr-issuer"
          value={v('issuer')}
          onChange={(e) => upd('issuer')(e.target.value)}
          placeholder="USSR Ministry of Defense"
          maxLength={100}
        />
      </div>
      <div>
        <FieldLabel htmlFor="attr-serial">
          <span>Serial #</span>
        </FieldLabel>
        <FieldInput
          id="attr-serial"
          mono
          value={v('serial_number')}
          onChange={(e) => upd('serial_number')(e.target.value)}
          maxLength={30}
        />
      </div>
    </div>
  );
}
