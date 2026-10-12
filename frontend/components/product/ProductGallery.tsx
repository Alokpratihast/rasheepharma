"use client";

import { useState } from "react";
import { Pill, Sparkles } from "lucide-react";

import { cn } from "@/lib/utils";

export interface GalleryImage {
  id: number;
  url: string;
  alt: string;
}

interface ProductGalleryProps {
  images: GalleryImage[];
  productName: string;
  featured?: boolean;
}

export function ProductGallery({
  images,
  productName,
  featured = false,
}: ProductGalleryProps) {
  const [selected, setSelected] = useState(0);
  const [origin, setOrigin] = useState("50% 50%");
  const [zoom, setZoom] = useState(false);

  const current = images[selected];

  return (
    <div>
      <div
        className="group relative overflow-hidden rounded-[32px] border border-border bg-gradient-to-br from-primary-light via-muted to-background shadow-[0_20px_60px_rgba(7,63,50,0.08)]"
        onMouseEnter={() => setZoom(true)}
        onMouseLeave={() => setZoom(false)}
        onMouseMove={(e) => {
          const rect = e.currentTarget.getBoundingClientRect();
          setOrigin(
            `${((e.clientX - rect.left) / rect.width) * 100}% ${
              ((e.clientY - rect.top) / rect.height) * 100
            }%`,
          );
        }}
      >
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -right-16 -top-16 size-56 rounded-full bg-primary/10 blur-3xl"
        />
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -bottom-20 -left-16 size-56 rounded-full bg-sky-400/10 blur-3xl"
        />

        {featured && (
          <span className="absolute left-4 top-4 z-20 inline-flex items-center gap-1.5 rounded-full bg-background/90 px-3.5 py-1.5 text-xs font-bold text-primary shadow-sm backdrop-blur">
            <Sparkles className="size-3.5" />
            Featured
          </span>
        )}

        <div className="relative flex aspect-square items-center justify-center p-8 sm:p-12">
          {current ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={current.url}
              alt={current.alt || productName}
              className="relative z-10 h-full w-full object-contain drop-shadow-[0_24px_30px_rgba(7,63,50,0.15)] transition-transform duration-300 ease-out"
              style={{
                transformOrigin: origin,
                transform: zoom ? "scale(1.45)" : "scale(1)",
              }}
            />
          ) : (
            <div className="relative z-10 flex flex-col items-center text-center">
              <div className="flex size-28 items-center justify-center rounded-[28px] bg-background shadow-lg">
                <Pill className="size-12 text-primary" />
              </div>
              <p className="mt-5 text-sm font-semibold text-foreground">
                Product image
              </p>
              <p className="mt-1 text-xs text-muted-foreground">
                Image not available yet
              </p>
            </div>
          )}
        </div>
      </div>

      {images.length > 1 && (
        <ul className="mt-4 grid grid-cols-5 gap-3" aria-label="Product images">
          {images.map((image, index) => (
            <li key={image.id}>
              <button
                type="button"
                onClick={() => setSelected(index)}
                aria-label={`Show image ${index + 1}`}
                aria-current={index === selected}
                className={cn(
                  "relative aspect-square w-full overflow-hidden rounded-2xl border-2 bg-muted transition-all",
                  index === selected
                    ? "border-primary shadow-[0_8px_20px_rgba(8,127,91,0.2)]"
                    : "border-transparent opacity-70 hover:opacity-100",
                )}
              >
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={image.url}
                  alt=""
                  loading="lazy"
                  className="h-full w-full object-contain p-2"
                />
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
