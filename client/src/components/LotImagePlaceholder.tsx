import { CoinSVG } from './illustrations/CoinSVG';
import { BanknoteSVG } from './illustrations/BanknoteSVG';
import { MedalSVG } from './illustrations/MedalSVG';

type Kind = 'coin' | 'banknote' | 'medal';

interface LotImagePlaceholderProps {
  kind?: Kind;
  variant?: number;
}

const variants = {
  coin: [
    { metal: '#C8B380', dark: '#8A6A2A', label: '5', year: '1992', era: 'tryzub' as const },
    { metal: '#D9CFAF', dark: '#7A6E3A', label: '1', year: '1898', era: 'imperial' as const },
    { metal: '#B58E5C', dark: '#6F4F26', label: '50', year: '1994', era: 'tryzub' as const },
    { metal: '#E0CC8C', dark: '#7C5A1F', label: '10', year: '1996', era: 'modern' as const },
    { metal: '#9C8252', dark: '#5A4520', label: '25', year: '1992', era: 'tryzub' as const },
    { metal: '#D6C29C', dark: '#806430', label: '2', year: '1924', era: 'imperial' as const },
  ],
  banknote: [
    { value: '10', year: '1994', tone: 'sepia' as const },
    { value: '5', year: '1961', tone: 'blue' as const },
    { value: '100', year: '1992', tone: 'green' as const },
    { value: '50', year: '2004', tone: 'rose' as const },
    { value: '20', year: '1991', tone: 'purple' as const },
    { value: '1', year: '1992', tone: 'sepia' as const },
  ],
  medal: [
    { ribbon: '#7B1F1F', accent: '#C9A24E', dark: '#7C5A1F' },
    { ribbon: '#1B3F7A', accent: '#D8C078', dark: '#5A4520' },
    { ribbon: '#1F5A2A', accent: '#B89540', dark: '#6F4F26' },
  ],
};

/** Used when no real image is available — never on real lots that have a publicUrl. */
export function LotImagePlaceholder({ kind = 'coin', variant = 0 }: LotImagePlaceholderProps) {
  const v = variants[kind][variant % variants[kind].length];
  return (
    <div
      className="w-full h-full flex items-center justify-center overflow-hidden relative"
      style={{ background: 'linear-gradient(180deg, #F5F1E8 0%, #EAE3D0 100%)' }}
    >
      {kind === 'coin' && <CoinSVG {...(v as Parameters<typeof CoinSVG>[0])} size={'85%' as unknown as number} />}
      {kind === 'banknote' && <BanknoteSVG {...(v as Parameters<typeof BanknoteSVG>[0])} size={'90%' as unknown as number} />}
      {kind === 'medal' && <MedalSVG {...(v as Parameters<typeof MedalSVG>[0])} size={'85%' as unknown as number} />}
    </div>
  );
}
