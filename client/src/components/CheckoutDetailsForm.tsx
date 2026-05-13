import { useEffect, useMemo, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { np, type NpCity, type NpWarehouse } from '@/api/np';
import { payments, type CheckoutDetailsBody } from '@/api/payments';
import { useToastStore } from '@/state/useToastStore';
import { ApiError } from '@/api/fetch';

interface Props {
  lotId: string;
  defaultName?: string;
  onSubmitted: () => void;
}

const PHONE_RE = /^\+380\d{9}$/;

/**
 * Buyer's "where do you want this delivered" form. Cities + warehouses come from the
 * shipments lookup proxy (real NP API under the hood). Submitting persists a Shipment
 * row in PendingTtn — the next step is creating the Stripe PaymentIntent.
 */
export function CheckoutDetailsForm({ lotId, defaultName = '', onSubmitted }: Props) {
  const pushToast = useToastStore((s) => s.push);
  const [cityQuery, setCityQuery] = useState('');
  const [debouncedQuery, setDebouncedQuery] = useState('');
  const [pickedCity, setPickedCity] = useState<NpCity | null>(null);
  const [warehouseQuery, setWarehouseQuery] = useState('');
  const [warehouseFocused, setWarehouseFocused] = useState(false);
  const [pickedWarehouse, setPickedWarehouse] = useState<NpWarehouse | null>(null);
  const [name, setName] = useState(defaultName);
  const [phone, setPhone] = useState('+380');
  const [submitting, setSubmitting] = useState(false);
  const cityInputRef = useRef<HTMLInputElement>(null);
  const warehouseInputRef = useRef<HTMLInputElement>(null);

  // 250ms debounce on the city query keeps NP autocomplete responsive without hammering.
  useEffect(() => {
    const handle = setTimeout(() => setDebouncedQuery(cityQuery.trim()), 250);
    return () => clearTimeout(handle);
  }, [cityQuery]);

  const cities = useQuery({
    queryKey: ['np-cities', debouncedQuery],
    queryFn: () => np.searchCities(debouncedQuery),
    enabled: debouncedQuery.length >= 2,
    staleTime: 30_000,
  });

  const warehouses = useQuery({
    queryKey: ['np-warehouses', pickedCity?.ref],
    queryFn: () => np.getWarehouses(pickedCity!.ref),
    enabled: !!pickedCity?.ref,
    staleTime: 60_000,
  });

  // Local filter for the warehouse dropdown — NP returns ~50–200 branches per city in one
  // call, so client-side filtering is appropriate (no second debounced query needed).
  const filteredWarehouses = useMemo(() => {
    const all = warehouses.data?.warehouses ?? [];
    const q = warehouseQuery.trim().toLowerCase();
    if (!q) return all.slice(0, 50);
    return all
      .filter(
        (w) =>
          w.number.toLowerCase().includes(q) || w.address.toLowerCase().includes(q),
      )
      .slice(0, 50);
  }, [warehouses.data, warehouseQuery]);

  const phoneValid = useMemo(() => PHONE_RE.test(phone), [phone]);
  const canSubmit =
    !!pickedCity && !!pickedWarehouse && name.trim().length > 0 && phoneValid && !submitting;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!canSubmit) return;

    const body: CheckoutDetailsBody = {
      recipientCityRef: pickedCity!.ref,
      recipientCityLabel: `${pickedCity!.name}, ${pickedCity!.area}`,
      recipientWarehouseRef: pickedWarehouse!.ref,
      recipientWarehouseLabel: pickedWarehouse!.address,
      recipientName: name.trim(),
      recipientPhone: phone.trim(),
    };

    setSubmitting(true);
    try {
      await payments.checkoutDetails(lotId, body);
      onSubmitted();
    } catch (err) {
      const msg =
        err instanceof ApiError
          ? err.detail ?? err.message
          : err instanceof Error
            ? err.message
            : 'Could not save delivery details.';
      pushToast({ kind: 'danger', title: 'Save failed', description: msg });
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <div>
        <label className="text-[12px] font-semibold text-text-3 uppercase tracking-wider">
          Delivery city
        </label>
        {pickedCity ? (
          <div className="mt-2 flex items-center justify-between rounded-md border border-border bg-bg-soft px-3 py-2 text-[14px]">
            <span>
              <span className="font-medium">{pickedCity.name}</span>
              <span className="text-text-3 ml-2">{pickedCity.area}</span>
            </span>
            <button
              type="button"
              onClick={() => {
                setPickedCity(null);
                setPickedWarehouse(null);
                setCityQuery('');
                cityInputRef.current?.focus();
              }}
              className="text-[12.5px] text-accent-deep hover:underline"
            >
              Change
            </button>
          </div>
        ) : (
          <div className="relative mt-2">
            <input
              ref={cityInputRef}
              type="text"
              value={cityQuery}
              onChange={(e) => setCityQuery(e.target.value)}
              placeholder="Start typing the city name (e.g. Київ)"
              className="w-full rounded-md border border-border-strong bg-surface px-3 py-2 text-[14px]"
              autoComplete="off"
            />
            {debouncedQuery.length >= 2 && (
              <div className="absolute z-10 left-0 right-0 mt-1 max-h-60 overflow-auto rounded-md border border-border bg-surface shadow-md">
                {cities.isLoading && (
                  <div className="px-3 py-2 text-[13px] text-text-3">Searching…</div>
                )}
                {cities.data?.cities.length === 0 && (
                  <div className="px-3 py-2 text-[13px] text-text-3">No matches.</div>
                )}
                {cities.data?.cities.map((c) => (
                  <button
                    type="button"
                    key={c.ref}
                    onClick={() => setPickedCity(c)}
                    className="block w-full text-left px-3 py-2 text-[14px] hover:bg-bg-soft"
                  >
                    <span className="font-medium">{c.name}</span>
                    <span className="text-text-3 ml-2 text-[12.5px]">{c.area}</span>
                  </button>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      <div>
        <label className="text-[12px] font-semibold text-text-3 uppercase tracking-wider">
          Nova Poshta branch
        </label>
        {pickedWarehouse ? (
          <div className="mt-2 flex items-center justify-between rounded-md border border-border bg-bg-soft px-3 py-2 text-[14px]">
            <span className="min-w-0 truncate pr-2">
              <span className="font-medium">№{pickedWarehouse.number}</span>
              <span className="text-text-3 ml-2">{pickedWarehouse.address}</span>
            </span>
            <button
              type="button"
              onClick={() => {
                setPickedWarehouse(null);
                setWarehouseQuery('');
                setTimeout(() => warehouseInputRef.current?.focus(), 0);
              }}
              className="text-[12.5px] text-accent-deep hover:underline flex-shrink-0"
            >
              Change
            </button>
          </div>
        ) : (
          <div className="relative mt-2">
            <input
              ref={warehouseInputRef}
              type="text"
              value={warehouseQuery}
              onChange={(e) => setWarehouseQuery(e.target.value)}
              onFocus={() => setWarehouseFocused(true)}
              onBlur={() => setTimeout(() => setWarehouseFocused(false), 150)}
              disabled={!pickedCity}
              placeholder={
                !pickedCity
                  ? 'Pick a city first'
                  : warehouses.isLoading
                    ? 'Loading branches…'
                    : 'Search by branch number or street'
              }
              className="w-full rounded-md border border-border-strong bg-surface px-3 py-2 text-[14px] disabled:opacity-60 disabled:cursor-not-allowed"
              autoComplete="off"
            />
            {pickedCity && warehouseFocused && (
              <div className="absolute z-10 left-0 right-0 mt-1 max-h-60 overflow-auto rounded-md border border-border bg-surface shadow-md">
                {warehouses.isLoading && (
                  <div className="px-3 py-2 text-[13px] text-text-3">Loading…</div>
                )}
                {!warehouses.isLoading && filteredWarehouses.length === 0 && (
                  <div className="px-3 py-2 text-[13px] text-text-3">No matches.</div>
                )}
                {filteredWarehouses.map((w) => (
                  <button
                    type="button"
                    key={w.ref}
                    onMouseDown={(e) => e.preventDefault()} // keep input from blurring before click
                    onClick={() => {
                      setPickedWarehouse(w);
                      setWarehouseQuery('');
                      setWarehouseFocused(false);
                    }}
                    className="block w-full text-left px-3 py-2 text-[14px] hover:bg-bg-soft"
                  >
                    <span className="font-medium">№{w.number}</span>
                    <span className="text-text-3 ml-2 text-[12.5px]">{w.address}</span>
                  </button>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      <div>
        <label
          htmlFor="recipientName"
          className="text-[12px] font-semibold text-text-3 uppercase tracking-wider"
        >
          Recipient name
        </label>
        <input
          id="recipientName"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Full name as printed on ID"
          className="mt-2 w-full rounded-md border border-border-strong bg-surface px-3 py-2 text-[14px]"
        />
      </div>

      <div>
        <label
          htmlFor="recipientPhone"
          className="text-[12px] font-semibold text-text-3 uppercase tracking-wider"
        >
          Phone
        </label>
        <input
          id="recipientPhone"
          type="tel"
          inputMode="tel"
          value={phone}
          onChange={(e) => setPhone(e.target.value)}
          className={`mt-2 w-full rounded-md border bg-surface px-3 py-2 text-[14px] mono ${
            phone === '+380' || phoneValid ? 'border-border-strong' : 'border-danger'
          }`}
        />
        <p className="text-[12px] text-text-3 mt-1.5">Format: +380XXXXXXXXX (12 digits total).</p>
      </div>

      <button
        type="submit"
        disabled={!canSubmit}
        className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm disabled:opacity-60 disabled:cursor-not-allowed"
      >
        {submitting ? 'Saving…' : 'Continue to payment'}
      </button>
    </form>
  );
}
