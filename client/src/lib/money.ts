/**
 * Format kopiykas as UAH. Uses en-US locale per project convention (English UI strings),
 * but currency code stays UAH — output looks like "UAH 1,234.50".
 */
export function formatKopiykasAsUah(kopiykas: number, options?: { integer?: boolean }): string {
  const uah = kopiykas / 100;
  const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'UAH',
    minimumFractionDigits: options?.integer ? 0 : 2,
    maximumFractionDigits: options?.integer ? 0 : 2,
  });
  return formatter.format(uah);
}

/**
 * Format raw UAH (not kopiykas) for inputs and display where the value is already in hryvnias.
 */
export function formatUah(uah: number, options?: { integer?: boolean }): string {
  return formatKopiykasAsUah(uah * 100, options);
}
