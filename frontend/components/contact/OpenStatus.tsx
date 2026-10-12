"use client";

import { useSyncExternalStore } from "react";

import { cn } from "@/lib/utils";

/** Business hours: Monday - Saturday, 9:30 AM - 6:30 PM (India time). */
const OPEN_MINUTES = 9 * 60 + 30;
const CLOSE_MINUTES = 18 * 60 + 30;

function getStatus(): "open" | "closed" {
  const parts = new Intl.DateTimeFormat("en-US", {
    timeZone: "Asia/Kolkata",
    weekday: "short",
    hour: "numeric",
    minute: "numeric",
    hourCycle: "h23",
  }).formatToParts(new Date());

  const get = (type: string) =>
    parts.find((part) => part.type === type)?.value ?? "";

  const minutes = (Number(get("hour")) % 24) * 60 + Number(get("minute"));
  const isSunday = get("weekday") === "Sun";

  return !isSunday && minutes >= OPEN_MINUTES && minutes < CLOSE_MINUTES
    ? "open"
    : "closed";
}

function subscribe(onChange: () => void) {
  const id = setInterval(onChange, 60_000);
  return () => clearInterval(id);
}

export function OpenStatus({ className }: { className?: string }) {
  // null on the server -> nothing rendered until hydrated (no mismatch)
  const status = useSyncExternalStore(subscribe, getStatus, () => null);

  if (!status) return null;

  const open = status === "open";

  return (
    <span
      className={cn(
        "inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-bold",
        open
          ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
          : "bg-muted text-muted-foreground",
        className,
      )}
    >
      <span className="relative flex size-2">
        {open && (
          <span className="absolute inline-flex size-full animate-ping rounded-full bg-emerald-400 opacity-60" />
        )}
        <span
          className={cn(
            "relative inline-flex size-2 rounded-full",
            open ? "bg-emerald-500" : "bg-muted-foreground/50",
          )}
        />
      </span>
      {open ? "Open now" : "Closed now"}
    </span>
  );
}
