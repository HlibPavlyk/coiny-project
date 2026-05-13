import { Link } from 'react-router-dom';
import { Logo } from './Logo';

export function Footer() {
  return (
    // Outer <footer> is transparent — it carries `mt-auto` (sticky-to-bottom in #root's
    // flex column) plus `pt-16` so there is always 4rem of breathing room above the
    // visible footer panel, even when page content is tall and mt-auto collapses to 0.
    <footer className="mt-auto pt-16">
      <div className="bg-surface border-t border-border px-7 pt-10 pb-7">
        <div
          className="max-w-[1280px] mx-auto grid gap-10"
          style={{ gridTemplateColumns: '1.5fr 1fr 1fr 1fr' }}
        >
          <div>
            <Logo />
            <p className="text-[13px] text-text-3 leading-relaxed mt-3 max-w-[320px]">
              Trusted Ukrainian numismatic auction with payment escrow via Stripe and delivery handled
              by Nova Poshta. Funds are released to sellers only after delivery is confirmed.
            </p>
          </div>
          <div>
            <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">Marketplace</div>
            <ul className="space-y-1.5 text-[13px]">
              <li><Link to="/category/coins" className="text-text-2 hover:text-text">Coins</Link></li>
              <li><Link to="/category/banknotes" className="text-text-2 hover:text-text">Banknotes</Link></li>
              <li><Link to="/category/medals" className="text-text-2 hover:text-text">Medals & orders</Link></li>
              <li><Link to="/search" className="text-text-2 hover:text-text">Browse all</Link></li>
            </ul>
          </div>
          <div>
            <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">Sell</div>
            <ul className="space-y-1.5 text-[13px]">
              <li><Link to="/lots/new" className="text-text-2 hover:text-text">List a lot</Link></li>
              <li><Link to="/seller/onboarding" className="text-text-2 hover:text-text">Become a seller</Link></li>
              <li><a className="text-text-2 hover:text-text cursor-pointer">Pricing</a></li>
            </ul>
          </div>
          <div>
            <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">Help</div>
            <ul className="space-y-1.5 text-[13px]">
              <li><a className="text-text-2 hover:text-text cursor-pointer">How it works</a></li>
              <li><a className="text-text-2 hover:text-text cursor-pointer">Buyer protection</a></li>
              <li><a className="text-text-2 hover:text-text cursor-pointer">Contact support</a></li>
            </ul>
          </div>
        </div>
        <div className="max-w-[1280px] mx-auto mt-8 pt-5 border-t border-border-soft flex justify-between items-center text-[12px] text-text-3">
          <span>© 2026 Coiny. All rights reserved.</span>
          <div className="flex gap-4">
            <a className="hover:text-text-2 cursor-pointer">Terms</a>
            <a className="hover:text-text-2 cursor-pointer">Privacy</a>
          </div>
        </div>
      </div>
    </footer>
  );
}
