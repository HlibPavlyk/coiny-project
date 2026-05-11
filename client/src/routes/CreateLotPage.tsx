import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import MDEditor, { commands } from '@uiw/react-md-editor';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { FieldInput, FieldLabel } from '@/components/auth/FieldHint';
import { CategoryAttributeForm, type SubcategoryKind } from '@/components/CategoryAttributeForm';
import { ImageUploader } from '@/components/ImageUploader';
import { Icon } from '@/components/Icon';
import { useCategoryTree, findCategoryPath, type CategoryNode } from '@/api/categories';
import { lots, type LotCondition, type LotDetailModel, type LotImage } from '@/api/lots';
import { ApiError } from '@/api/fetch';
import { useAuthStore } from '@/state/useAuthStore';
import { auth } from '@/api/auth';
import { useToastStore } from '@/state/useToastStore';

type Step = 1 | 2 | 3;

const CONDITIONS: LotCondition[] = ['UNC', 'AU', 'XF', 'VF', 'F', 'VG', 'G', 'Poor', 'Ungraded'];

function toLocalDateTimeInput(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function defaultEndsAtLocal(): string {
  const d = new Date();
  d.setDate(d.getDate() + 3);
  return toLocalDateTimeInput(d);
}

function StepHeader({ step }: { step: Step }) {
  const items: { n: Step; label: string }[] = [
    { n: 1, label: 'Category' },
    { n: 2, label: 'Details' },
    { n: 3, label: 'Images & publish' },
  ];
  return (
    <div className="flex items-center gap-4">
      {items.map((it, i) => {
        const active = it.n === step;
        const done = it.n < step;
        return (
          <div key={it.n} className="flex items-center gap-3">
            <div
              className="rounded-full flex items-center justify-center font-bold text-[12px]"
              style={{
                width: 28,
                height: 28,
                background: active || done ? 'var(--color-accent-deep)' : 'var(--color-bg-soft)',
                color: active || done ? 'white' : 'var(--color-text-3)',
                border: active ? '2px solid var(--color-accent)' : 'none',
              }}
            >
              {done ? '✓' : it.n}
            </div>
            <span
              className="text-[13px] font-medium"
              style={{ color: active ? 'var(--color-text)' : 'var(--color-text-3)' }}
            >
              {it.label}
            </span>
            {i < items.length - 1 && (
              <div className="w-12 h-px" style={{ background: 'var(--color-border)' }} />
            )}
          </div>
        );
      })}
    </div>
  );
}

function CategoryPicker({
  tree,
  onPick,
}: {
  tree: ReturnType<typeof useCategoryTree>['data'];
  onPick: (node: CategoryNode) => void;
}) {
  const [lvl0Id, setLvl0Id] = useState<number | null>(null);
  const [lvl1Id, setLvl1Id] = useState<number | null>(null);

  if (!tree) return <div className="text-text-3 text-sm">Loading categories…</div>;

  const roots = tree.roots;
  const lvl0 = roots.find((r) => r.id === lvl0Id);
  const lvl1 = lvl0?.children.find((c) => c.id === lvl1Id);

  const Column = ({
    items,
    selectedId,
    onSelect,
    title,
  }: {
    items: CategoryNode[];
    selectedId: number | null;
    onSelect: (n: CategoryNode) => void;
    title: string;
  }) => (
    <div>
      <h3 className="text-[11px] font-semibold uppercase tracking-wider text-text-3 mb-2">
        {title}
      </h3>
      <div className="flex flex-col bg-surface border border-border rounded-md max-h-[400px] overflow-y-auto">
        {items.length === 0 ? (
          <div className="text-[13px] text-text-3 px-3 py-4">—</div>
        ) : (
          items.map((node) => {
            const selected = node.id === selectedId;
            return (
              <button
                key={node.id}
                type="button"
                onClick={() => onSelect(node)}
                className="flex items-center justify-between text-left px-3 py-2.5 text-[13px] border-b border-border-soft last:border-b-0 transition"
                style={{
                  background: selected ? 'var(--color-accent-tint)' : 'transparent',
                  color: selected ? 'var(--color-accent-deep)' : 'var(--color-text)',
                  fontWeight: selected ? 600 : 500,
                  cursor: 'pointer',
                }}
              >
                <span>{node.name}</span>
                {!node.isLeaf && <Icon name="arrowR" size={12} color="var(--color-text-3)" />}
              </button>
            );
          })
        )}
      </div>
    </div>
  );

  return (
    <div className="grid gap-5" style={{ gridTemplateColumns: '1fr 1fr 1fr' }}>
      <Column
        title="Section"
        items={roots}
        selectedId={lvl0Id}
        onSelect={(n) => {
          setLvl0Id(n.id);
          setLvl1Id(null);
          if (n.isLeaf) onPick(n);
        }}
      />
      <Column
        title="Group"
        items={lvl0?.children ?? []}
        selectedId={lvl1Id}
        onSelect={(n) => {
          setLvl1Id(n.id);
          if (n.isLeaf) onPick(n);
        }}
      />
      <Column
        title="Subcategory"
        items={lvl1?.children ?? []}
        selectedId={null}
        onSelect={(n) => {
          if (n.isLeaf) onPick(n);
        }}
      />
    </div>
  );
}

interface CreateLotPageProps {
  /** When set, the wizard is in edit mode: prefills state from the existing draft and skips Step 1. */
  draft?: LotDetailModel;
}

export default function CreateLotPage({ draft }: CreateLotPageProps = {}) {
  const user = useAuthStore((s) => s.user);
  const navigate = useNavigate();
  const pushToast = useToastStore((s) => s.push);
  const { data: tree } = useCategoryTree();

  const editMode = !!draft;

  const initialCategory: CategoryNode | null = useMemo(() => {
    if (!draft || !tree) return null;
    const path = findCategoryPath(tree, draft.category.id);
    return path?.[path.length - 1] ?? null;
  }, [draft, tree]);

  const [step, setStep] = useState<Step>(editMode ? 2 : 1);
  const [category, setCategory] = useState<CategoryNode | null>(initialCategory);
  const [title, setTitle] = useState(draft?.title ?? '');
  const [description, setDescription] = useState(draft?.description ?? '');
  const [startingPriceUah, setStartingPriceUah] = useState(
    draft ? (draft.startingPriceUahKopiykas / 100).toString() : '',
  );
  const [endsAtLocal, setEndsAtLocal] = useState(
    draft ? toLocalDateTimeInput(new Date(draft.endsAt)) : defaultEndsAtLocal(),
  );
  const [condition, setCondition] = useState<LotCondition>(draft?.condition ?? 'Ungraded');
  const [attributes, setAttributes] = useState<Record<string, unknown>>(draft?.attributes ?? {});
  const [lotId, setLotId] = useState<string | null>(draft?.id ?? null);
  const [images, setImages] = useState<LotImage[]>(draft?.images ?? []);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [resending, setResending] = useState(false);

  // Hydrate category once tree loads (edit mode only)
  useEffect(() => {
    if (editMode && !category && initialCategory) setCategory(initialCategory);
  }, [editMode, category, initialCategory]);

  const subcategoryKind: SubcategoryKind | null = useMemo(() => {
    if (!category) return null;
    return (category.subcategoryKind as SubcategoryKind | null) ?? null;
  }, [category]);

  // Pre-flight gates
  if (!user) return null;
  if (!user.emailVerified) {
    return (
      <div>
        <TopNav />
        <div className="max-w-[640px] mx-auto px-7 py-20 text-center">
          <h1 className="text-3xl font-bold m-0">Verify your email first</h1>
          <p className="text-text-2 mt-3">
            Before creating a lot, please confirm the email we sent to{' '}
            <span className="mono">{user.email}</span>.
          </p>
          <button
            type="button"
            disabled={resending}
            onClick={async () => {
              setResending(true);
              try {
                await auth.resendVerification();
                pushToast({ kind: 'success', title: 'Verification email sent' });
              } catch {
                pushToast({ kind: 'danger', title: 'Could not resend' });
              } finally {
                setResending(false);
              }
            }}
            className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm mt-6 disabled:opacity-60"
            style={{ cursor: resending ? 'not-allowed' : 'pointer' }}
          >
            {resending ? 'Sending…' : 'Resend verification email'}
          </button>
        </div>
        <Footer />
      </div>
    );
  }
  if (!user.stripeOnboarded) {
    return (
      <div>
        <TopNav />
        <div className="max-w-[640px] mx-auto px-7 py-20 text-center">
          <h1 className="text-3xl font-bold m-0">Complete seller onboarding</h1>
          <p className="text-text-2 mt-3">
            Coiny uses Stripe Connect Express to hold buyer funds in escrow and pay you out
            after delivery. Onboarding takes 2 minutes.
          </p>
          <Link
            to="/seller/onboarding"
            className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline mt-6"
          >
            Start onboarding
          </Link>
        </div>
        <Footer />
      </div>
    );
  }

  const goToStep2 = () => {
    if (!category || !category.isLeaf) {
      setError('Pick a leaf subcategory.');
      return;
    }
    setError(null);
    setStep(2);
  };

  const goToStep3 = async (e: FormEvent) => {
    e.preventDefault();
    if (!category) return;
    setError(null);

    const priceUah = Number(startingPriceUah);
    if (!Number.isFinite(priceUah) || priceUah < 1) {
      setError('Starting price must be at least 1 UAH.');
      return;
    }
    const endsAt = new Date(endsAtLocal);
    if (Number.isNaN(endsAt.getTime())) {
      setError('Invalid ends-at date.');
      return;
    }
    const minEnds = Date.now() + 60 * 60 * 1000;
    const maxEnds = Date.now() + 7 * 24 * 60 * 60 * 1000;
    if (endsAt.getTime() < minEnds || endsAt.getTime() > maxEnds) {
      setError('End time must be between 1 hour and 7 days from now.');
      return;
    }

    setSubmitting(true);
    try {
      if (lotId) {
        await lots.updateLot(lotId, {
          title,
          description,
          categoryId: category.id,
          condition,
          startingPriceUahKopiykas: Math.round(priceUah * 100),
          endsAt: endsAt.toISOString(),
          attributes,
        });
      } else {
        const created = await lots.createLot({
          title,
          description,
          categoryId: category.id,
          condition,
          startingPriceUahKopiykas: Math.round(priceUah * 100),
          endsAt: endsAt.toISOString(),
          attributes,
        });
        setLotId(created.id);
      }
      setStep(3);
    } catch (err) {
      setError(err instanceof ApiError ? err.detail ?? err.message : 'Could not save draft.');
    } finally {
      setSubmitting(false);
    }
  };

  const publish = async () => {
    if (!lotId) return;
    if (images.length === 0) {
      setError('Add at least one image before publishing.');
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      await lots.publishLot(lotId);
      navigate(`/lot/${lotId}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.detail ?? err.message : 'Publish failed.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <TopNav />

      <div className="max-w-[920px] mx-auto px-7 pt-8 pb-2">
        <h1 className="text-[28px] font-bold m-0">{editMode ? 'Edit draft' : 'Create a lot'}</h1>
        <p className="text-text-2 text-[14px] mt-1">
          {editMode
            ? 'Update your draft, then publish when ready.'
            : 'Three steps: pick the category, fill the details, upload images.'}
        </p>
      </div>

      <div className="max-w-[920px] mx-auto px-7 pt-5 pb-3">
        <StepHeader step={step} />
      </div>

      <div className="max-w-[920px] mx-auto px-7 pt-5 pb-16">
        {error && (
          <div
            className="rounded-md px-4 py-3 text-[13px] mb-5"
            style={{ background: '#FEF1EC', border: '1px solid #FCD9C9', color: '#7C2A11' }}
          >
            {error}
          </div>
        )}

        {step === 1 && (
          <div>
            <CategoryPicker tree={tree} onPick={(n) => setCategory(n)} />
            <div className="mt-6 flex items-center justify-between">
              <div className="text-[13px] text-text-3">
                {category ? (
                  <>
                    Selected: <span className="font-medium text-text">{category.name}</span>
                  </>
                ) : (
                  'Drill down to a leaf subcategory.'
                )}
              </div>
              <button
                type="button"
                onClick={goToStep2}
                disabled={!category}
                className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm disabled:opacity-50"
                style={{ cursor: !category ? 'not-allowed' : 'pointer' }}
              >
                Next →
              </button>
            </div>
          </div>
        )}

        {step === 2 && (
          <form onSubmit={goToStep3} className="flex flex-col gap-5">
            <div>
              <FieldLabel htmlFor="lot-title">Title</FieldLabel>
              <FieldInput
                id="lot-title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="e.g. 1 ruble 1898, SPB-AG, silver, XF"
                maxLength={120}
                required
              />
            </div>

            <div>
              <FieldLabel htmlFor="lot-description">Description</FieldLabel>
              <div data-color-mode="light">
                <MDEditor
                  value={description}
                  onChange={(val) => setDescription((val ?? '').slice(0, 10000))}
                  height={280}
                  preview="edit"
                  visibleDragbar={false}
                  commands={[
                    commands.bold,
                    commands.italic,
                    commands.strikethrough,
                    commands.divider,
                    commands.title2,
                    commands.title3,
                    commands.quote,
                    commands.divider,
                    commands.unorderedListCommand,
                    commands.orderedListCommand,
                    commands.divider,
                    commands.link,
                    commands.code,
                  ]}
                  extraCommands={[commands.codeEdit, commands.codeLive, commands.codePreview]}
                  textareaProps={{
                    id: 'lot-description',
                    placeholder: 'Origin, provenance, packaging, payment methods…',
                    maxLength: 10000,
                  }}
                />
              </div>
              <div className="flex items-center justify-between mt-1.5 text-[11px] text-text-3">
                <span>
                  Supports <span className="mono">**bold**</span>,{' '}
                  <span className="mono">*italic*</span>, lists, quotes, and links.
                </span>
                <span className="mono">{description.length} / 10000</span>
              </div>
            </div>

            <div className="grid gap-4" style={{ gridTemplateColumns: '1fr 1fr 1fr' }}>
              <div>
                <FieldLabel htmlFor="lot-price">Starting price (UAH)</FieldLabel>
                <FieldInput
                  id="lot-price"
                  type="number"
                  step="0.01"
                  min={1}
                  mono
                  value={startingPriceUah}
                  onChange={(e) => setStartingPriceUah(e.target.value)}
                  placeholder="250.00"
                  required
                />
              </div>
              <div>
                <FieldLabel htmlFor="lot-ends">Ends at</FieldLabel>
                <FieldInput
                  id="lot-ends"
                  type="datetime-local"
                  value={endsAtLocal}
                  onChange={(e) => setEndsAtLocal(e.target.value)}
                  required
                />
              </div>
              <div>
                <FieldLabel htmlFor="lot-condition">Condition</FieldLabel>
                <select
                  id="lot-condition"
                  value={condition}
                  onChange={(e) => setCondition(e.target.value as LotCondition)}
                  className="w-full rounded-md border border-border-strong bg-surface px-3 py-2.5 text-sm transition focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/15"
                >
                  {CONDITIONS.map((c) => (
                    <option key={c} value={c}>
                      {c}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {subcategoryKind && (
              <div className="mt-3">
                <h3 className="text-[11px] font-semibold uppercase tracking-wider text-text-3 mb-3">
                  Attributes · {subcategoryKind}
                </h3>
                <CategoryAttributeForm
                  subcategoryKind={subcategoryKind}
                  value={attributes}
                  onChange={setAttributes}
                />
              </div>
            )}

            <div className="flex items-center justify-between mt-3">
              {editMode ? (
                <span />
              ) : (
                <button
                  type="button"
                  onClick={() => setStep(1)}
                  className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-4 py-2.5 text-sm"
                  style={{ cursor: 'pointer' }}
                >
                  ← Back
                </button>
              )}
              <button
                type="submit"
                disabled={submitting}
                className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm disabled:opacity-60"
                style={{ cursor: submitting ? 'not-allowed' : 'pointer' }}
              >
                {submitting ? 'Saving…' : 'Save & continue →'}
              </button>
            </div>
          </form>
        )}

        {step === 3 && lotId && (
          <div className="flex flex-col gap-6">
            <ImageUploader lotId={lotId} images={images} onChange={setImages} />

            <div className="bg-surface border border-border rounded-lg p-5">
              <h3 className="text-[11px] font-semibold uppercase tracking-wider text-text-3 m-0 mb-3">
                Review
              </h3>
              <dl className="grid gap-y-2 text-[13px]" style={{ gridTemplateColumns: '160px 1fr' }}>
                <dt className="text-text-3">Title</dt>
                <dd className="m-0 font-medium">{title}</dd>
                <dt className="text-text-3">Category</dt>
                <dd className="m-0">{category?.name}</dd>
                <dt className="text-text-3">Condition</dt>
                <dd className="m-0 mono">{condition}</dd>
                <dt className="text-text-3">Starting price</dt>
                <dd className="m-0 mono">UAH {Number(startingPriceUah).toFixed(2)}</dd>
                <dt className="text-text-3">Ends at</dt>
                <dd className="m-0">{new Date(endsAtLocal).toLocaleString()}</dd>
              </dl>
            </div>

            <div className="flex items-center justify-between">
              <button
                type="button"
                onClick={() => setStep(2)}
                className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-4 py-2.5 text-sm"
                style={{ cursor: 'pointer' }}
              >
                ← Back
              </button>
              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={() => navigate('/my-lots')}
                  className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-4 py-2.5 text-sm"
                  style={{ cursor: 'pointer' }}
                >
                  Save draft
                </button>
                <button
                  type="button"
                  onClick={publish}
                  disabled={submitting || images.length === 0}
                  className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm disabled:opacity-50"
                  style={{ cursor: submitting || images.length === 0 ? 'not-allowed' : 'pointer' }}
                >
                  {submitting ? 'Publishing…' : 'Publish lot'}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      <Footer />
    </div>
  );
}
