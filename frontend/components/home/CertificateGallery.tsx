"use client";

import Image from "next/image";
import { useEffect, useRef, useState } from "react";
import { X } from "lucide-react";

const certificates = [
  { src: "/images/rashecertificate1.jpg", title: "WHO-GMP", alt: "WHO-GMP certificate" },
  { src: "/images/rashecertificate2.jpg", title: "ISO 9001:2015", alt: "ISO 9001:2015 certificate" },
];

export function CertificateGallery() {
  const [selected, setSelected] = useState<number | null>(null);
  const previousFocus = useRef<HTMLElement | null>(null);

  useEffect(() => {
    if (selected === null) return;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setSelected(null);
    };
    document.addEventListener("keydown", onKeyDown);
    document.body.style.overflow = "hidden";
    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = "";
      previousFocus.current?.focus();
    };
  }, [selected]);

  return (
    <>
      <div className="relative mx-auto h-[380px] w-full max-w-md rounded-[2rem] bg-primary-light sm:h-[440px]">
        {certificates.map((certificate, index) => (
          <figure key={certificate.title} className={index === 0 ? "absolute left-[7%] top-[9%] w-[52%] -rotate-6" : "absolute bottom-[8%] right-[7%] w-[52%] rotate-3"}>
            <button type="button" onClick={(event) => { previousFocus.current = event.currentTarget; setSelected(index); }} aria-label={`View ${certificate.title} certificate`}
              className="block w-full rounded-lg bg-white p-2 shadow-[0_18px_40px_rgba(7,63,50,0.22)] transition hover:scale-[1.03] focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-primary/30">
              <Image src={certificate.src} alt={certificate.alt} width={350} height={489} sizes="(max-width: 640px) 45vw, 220px" className="h-auto w-full rounded-sm" />
            </button>
            <figcaption className={`absolute -bottom-3 ${index === 0 ? "left-3 bg-brand-dark" : "right-3 bg-primary"} rounded-full px-3 py-1 text-xs font-semibold text-white`}>
              {certificate.title}
            </figcaption>
          </figure>
        ))}
        <p className="absolute bottom-3 left-1/2 -translate-x-1/2 whitespace-nowrap rounded-full bg-white/80 px-3 py-1 text-xs font-medium text-brand-dark">Select a certificate to view it</p>
      </div>

      {selected !== null && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/80 p-4 backdrop-blur-sm" onMouseDown={(event) => { if (event.target === event.currentTarget) setSelected(null); }}>
          <section role="dialog" aria-modal="true" aria-labelledby="certificate-dialog-title" className="relative max-h-[92vh] w-full max-w-3xl overflow-auto rounded-2xl bg-white p-4 shadow-2xl sm:p-6">
            <div className="mb-4 flex items-center justify-between gap-4">
              <h2 id="certificate-dialog-title" className="text-lg font-bold text-brand-dark">{certificates[selected].title} certificate</h2>
              <button type="button" autoFocus onClick={() => setSelected(null)} aria-label="Close certificate preview" className="flex size-10 shrink-0 items-center justify-center rounded-full border border-slate-200 text-slate-700 hover:bg-slate-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary">
                <X className="size-5" />
              </button>
            </div>
            <Image src={certificates[selected].src} alt={certificates[selected].alt} width={1000} height={1400} sizes="(max-width: 768px) 95vw, 700px" className="mx-auto max-h-[75vh] w-auto rounded-lg object-contain" />
          </section>
        </div>
      )}
    </>
  );
}
