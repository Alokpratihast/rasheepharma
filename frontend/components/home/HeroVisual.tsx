"use client";

import Image from "next/image";
import { useRef } from "react";
import {
  Globe2,
  Handshake,
  Pill,
  ShieldCheck,
} from "lucide-react";

/** Depth layer: moves slightly with the mouse (parallax). */
function layer(depth: number): React.CSSProperties {
  return {
    transform: `translate3d(calc(var(--px, 0) * ${depth}px), calc(var(--py, 0) * ${depth}px), 0)`,
    transition: "transform 0.3s ease-out",
  };
}

const glass =
  "flex items-center gap-3 rounded-2xl border border-white/20 bg-white/10 px-4 py-3 text-white shadow-[0_18px_40px_rgba(0,0,0,0.28)] backdrop-blur-xl";

export function HeroVisual() {
  const ref = useRef<HTMLDivElement>(null);

  function onPointerMove(e: React.PointerEvent<HTMLDivElement>) {
    if (e.pointerType !== "mouse" || !ref.current) return;
    const rect = ref.current.getBoundingClientRect();
    ref.current.style.setProperty(
      "--px",
      String((e.clientX - rect.left) / rect.width - 0.5),
    );
    ref.current.style.setProperty(
      "--py",
      String((e.clientY - rect.top) / rect.height - 0.5),
    );
  }

  function onPointerLeave() {
    ref.current?.style.setProperty("--px", "0");
    ref.current?.style.setProperty("--py", "0");
  }

  return (
    <div
      ref={ref}
      onPointerMove={onPointerMove}
      onPointerLeave={onPointerLeave}
      className="relative mx-auto aspect-[4/4.4] w-full max-w-[540px] sm:aspect-[5/5.4]"
    >
      {/* glow */}
      <div
        aria-hidden="true"
        className="absolute inset-[8%] rounded-full bg-teal-400/30 blur-3xl"
      />

      {/* MAIN PHOTO */}
      <div
        className="absolute right-0 top-0 h-[86%] w-[76%]"
        style={layer(-8)}
      >
        <div className="relative h-full w-full overflow-hidden rounded-[36px] border border-white/20 shadow-[0_30px_80px_rgba(0,0,0,0.45)] sm:rounded-[44px]">
          <Image
            src="/images/header1.jpg"
            alt="Pharmaceutical research and quality"
            fill
            priority
            sizes="(max-width: 1024px) 70vw, 410px"
            className="object-cover object-[50%_35%]"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-brand-dark/70 via-transparent to-transparent" />
        </div>
      </div>

      {/* PRODUCT CARD */}
      <div
        className="absolute bottom-0 left-0 h-[42%] w-[58%]"
        style={layer(14)}
      >
        <div className="hero-float-alt relative h-full w-full overflow-hidden rounded-[26px] border-[5px] border-white shadow-[0_28px_60px_rgba(0,0,0,0.4)]">
          <Image
            src="/images/rasheheader2.png"
            alt="Pharmaceutical products"
            fill
            sizes="(max-width: 640px) 40vw, 300px"
            className="object-cover"
          />
          <div className="absolute bottom-2 left-2 flex items-center gap-2 rounded-xl bg-white/95 px-2.5 py-1.5 shadow-lg">
            <span className="flex size-7 items-center justify-center rounded-lg bg-primary-light">
              <Pill className="size-4 text-primary" />
            </span>
            <span className="hidden sm:block">
              <span className="block text-[11px] font-bold leading-tight text-brand-dark">
                Pharmaceutical Range
              </span>
              <span className="block text-[9px] leading-tight text-muted-foreground">
                Quality-focused solutions
              </span>
            </span>
          </div>
        </div>
      </div>

      {/* BADGE: QUALITY */}
      <div
        className="absolute left-[-2%] top-[10%] z-20"
        style={layer(22)}
      >
        <div className={`${glass} hero-float`}>
          <span className="flex size-10 items-center justify-center rounded-xl bg-emerald-400/20">
            <ShieldCheck className="size-5 text-emerald-300" />
          </span>
          <span>
            <span className="block text-sm font-bold">Quality Assured</span>
            <span className="block text-[11px] text-white/70">
              Consistent standards
            </span>
          </span>
        </div>
      </div>

      {/* BADGE: GLOBAL */}
      <div
        className="absolute right-[-3%] top-[44%] z-20"
        style={layer(26)}
      >
        <div className={`${glass} hero-float-b`}>
          <span className="flex size-10 items-center justify-center rounded-xl bg-sky-400/20">
            <Globe2 className="size-5 text-sky-300" />
          </span>
          <span>
            <span className="block text-sm font-bold">Global Supply</span>
            <span className="block text-[11px] text-white/70">
              Healthcare partnerships
            </span>
          </span>
        </div>
      </div>

      {/* BADGE: B2B (hidden on small screens) */}
      <div
        className="absolute bottom-[10%] right-[4%] z-20 hidden sm:block"
        style={layer(18)}
      >
        <div className={`${glass} hero-float-c`}>
          <span className="flex size-10 items-center justify-center rounded-xl bg-amber-400/20">
            <Handshake className="size-5 text-amber-300" />
          </span>
          <span>
            <span className="block text-sm font-bold">B2B Focused</span>
            <span className="block text-[11px] text-white/70">
              Reliable partnerships
            </span>
          </span>
        </div>
      </div>

      {/* ROTATING DASHED RING */}
      <div
        aria-hidden="true"
        className="hero-spin pointer-events-none absolute -right-3 -top-3 size-24 rounded-full border border-dashed border-white/30 sm:size-28"
      />
    </div>
  );
}
