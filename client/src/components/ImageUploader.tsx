import { useCallback, useRef, useState } from 'react';
import { Icon } from './Icon';
import { lots, type LotImage } from '@/api/lots';
import { ApiError } from '@/api/fetch';

interface ImageUploaderProps {
  lotId: string;
  images: LotImage[];
  onChange: (next: LotImage[]) => void;
  max?: number;
}

const ACCEPT = 'image/jpeg,image/png,image/webp';

export function ImageUploader({ lotId, images, onChange, max = 5 }: ImageUploaderProps) {
  const [uploading, setUploading] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [dragOverIdx, setDragOverIdx] = useState<number | null>(null);
  const draggedIdxRef = useRef<number | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const remaining = max - images.length;

  const uploadFiles = useCallback(
    async (files: File[]) => {
      setError(null);
      const allowed = files.slice(0, remaining);
      if (files.length > allowed.length) {
        setError(`Only ${max} images per lot. ${files.length - allowed.length} skipped.`);
      }
      for (const file of allowed) {
        const tag = `${file.name}-${file.size}-${Date.now()}`;
        setUploading((u) => [...u, tag]);
        try {
          const uploaded = await lots.uploadImage(lotId, file);
          const newImage: LotImage = {
            id: uploaded.id,
            publicUrl: uploaded.publicUrl,
            displayOrder: uploaded.displayOrder,
            width: 0,
            height: 0,
          };
          onChange([...(images as LotImage[]), newImage].sort((a, b) => a.displayOrder - b.displayOrder));
        } catch (err) {
          const msg = err instanceof ApiError ? err.detail ?? err.message : 'Upload failed';
          setError(msg);
        } finally {
          setUploading((u) => u.filter((t) => t !== tag));
        }
      }
    },
    [lotId, images, onChange, remaining, max],
  );

  const onPick = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files ? Array.from(e.target.files) : [];
    if (files.length) void uploadFiles(files);
    e.target.value = '';
  };

  const onDropZone = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    const files = Array.from(e.dataTransfer.files).filter((f) =>
      ACCEPT.split(',').includes(f.type),
    );
    if (files.length) void uploadFiles(files);
  };

  const onDelete = async (imageId: string) => {
    setError(null);
    try {
      await lots.deleteImage(lotId, imageId);
      onChange(images.filter((i) => i.id !== imageId));
    } catch (err) {
      const msg = err instanceof ApiError ? err.detail ?? err.message : 'Delete failed';
      setError(msg);
    }
  };

  const onThumbDragStart = (idx: number) => () => {
    draggedIdxRef.current = idx;
  };

  const onThumbDragOver = (idx: number) => (e: React.DragEvent) => {
    e.preventDefault();
    if (draggedIdxRef.current !== null && draggedIdxRef.current !== idx) {
      setDragOverIdx(idx);
    }
  };

  const onThumbDrop = (idx: number) => async (e: React.DragEvent) => {
    e.preventDefault();
    setDragOverIdx(null);
    const from = draggedIdxRef.current;
    draggedIdxRef.current = null;
    if (from === null || from === idx) return;

    const next = [...images];
    const [moved] = next.splice(from, 1);
    next.splice(idx, 0, moved);

    const reordered: LotImage[] = next.map((img, i) => ({ ...img, displayOrder: i }));
    onChange(reordered);

    try {
      await lots.reorderImages(
        lotId,
        reordered.map((i) => i.id),
      );
    } catch (err) {
      const msg = err instanceof ApiError ? err.detail ?? err.message : 'Reorder failed';
      setError(msg);
    }
  };

  return (
    <div>
      <div className="flex items-baseline justify-between mb-2">
        <h3 className="text-[13px] font-semibold text-text m-0">
          Images <span className="mono text-text-3 font-medium">{images.length} / {max}</span>
        </h3>
        {error && (
          <span className="text-[12px] text-danger">{error}</span>
        )}
      </div>

      <div
        onDragOver={(e) => e.preventDefault()}
        onDrop={onDropZone}
        className="border-2 border-dashed border-border-strong rounded-lg p-6 text-center bg-bg-soft hover:bg-bg-softer transition cursor-pointer"
        onClick={() => inputRef.current?.click()}
      >
        <input
          ref={inputRef}
          type="file"
          accept={ACCEPT}
          multiple
          onChange={onPick}
          className="hidden"
        />
        <Icon name="plus" size={20} color="var(--color-text-3)" />
        <div className="text-[13px] text-text-2 mt-2 font-medium">
          {remaining > 0
            ? 'Drag images here or click to pick'
            : `Maximum ${max} images reached`}
        </div>
        <div className="text-[11px] text-text-3 mt-1">JPEG, PNG, WebP · up to 10 MB each</div>
      </div>

      {(images.length > 0 || uploading.length > 0) && (
        <div className="grid gap-2 mt-3" style={{ gridTemplateColumns: 'repeat(5, 1fr)' }}>
          {images.map((img, i) => (
            <div
              key={img.id}
              draggable
              onDragStart={onThumbDragStart(i)}
              onDragOver={onThumbDragOver(i)}
              onDrop={onThumbDrop(i)}
              className="relative bg-bg-soft rounded-md overflow-hidden border"
              style={{
                aspectRatio: '1 / 1',
                borderColor: dragOverIdx === i ? 'var(--color-accent)' : 'var(--color-border)',
                cursor: 'grab',
              }}
            >
              <img
                src={img.publicUrl}
                alt=""
                className="absolute inset-0 w-full h-full object-cover pointer-events-none"
              />
              {i === 0 && (
                <span
                  className="absolute top-1.5 left-1.5 rounded-sm font-semibold uppercase"
                  style={{
                    padding: '2px 6px',
                    fontSize: 9,
                    letterSpacing: '0.06em',
                    background: 'rgba(255,255,255,0.92)',
                    color: 'var(--color-accent-deep)',
                  }}
                >
                  Cover
                </span>
              )}
              <button
                type="button"
                aria-label="Remove image"
                onClick={(e) => {
                  e.stopPropagation();
                  void onDelete(img.id);
                }}
                className="absolute top-1 right-1 w-6 h-6 rounded-full flex items-center justify-center text-white font-bold text-sm"
                style={{ background: 'rgba(0,0,0,0.55)', cursor: 'pointer' }}
              >
                ×
              </button>
            </div>
          ))}
          {uploading.map((tag) => (
            <div
              key={tag}
              className="bg-bg-soft rounded-md flex items-center justify-center text-[11px] text-text-3 border border-border"
              style={{ aspectRatio: '1 / 1' }}
            >
              Uploading…
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
