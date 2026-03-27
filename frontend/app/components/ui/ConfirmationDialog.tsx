"use client";

import { motion, AnimatePresence } from "framer-motion";
import { scaleIn } from "@/app/components/motion/variants";

interface ConfirmationDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: "danger" | "primary";
  loading?: boolean;
}

/**
 * Confirmation dialog with Framer Motion scaleIn animation.
 * Variant "danger" = red confirm button, "primary" = violet confirm button.
 */
export function ConfirmationDialog({
  open,
  onClose,
  onConfirm,
  title,
  description,
  confirmLabel = "Confirm",
  cancelLabel = "Cancel",
  variant = "primary",
  loading = false,
}: ConfirmationDialogProps) {
  const confirmColors =
    variant === "danger"
      ? "bg-red-600 hover:bg-red-700 text-white"
      : "bg-zinc-900 hover:bg-zinc-800 text-white";

  return (
    <AnimatePresence>
      {open && (
        <>
          {/* Backdrop */}
          <motion.div
            className="fixed inset-0 z-50 bg-black/40 backdrop-blur-sm"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
          />

          {/* Dialog */}
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div
              className="w-full max-w-sm bg-white rounded-2xl border border-zinc-200 p-6 shadow-xl"
              initial={scaleIn.initial}
              animate={scaleIn.animate}
              exit={scaleIn.initial}
              transition={scaleIn.transition}
              onClick={(e) => e.stopPropagation()}
            >
              <h3 className="font-display text-lg font-semibold text-zinc-900 mb-1">
                {title}
              </h3>
              {description && (
                <p className="text-sm text-gray-500 mb-6">{description}</p>
              )}

              <div className="flex gap-3">
                <button
                  onClick={onClose}
                  disabled={loading}
                  className="flex-1 border border-zinc-200 text-zinc-700 font-medium px-4 py-2 rounded-lg
                             hover:bg-zinc-50
                             transition-all duration-150 text-sm disabled:opacity-50"
                >
                  {cancelLabel}
                </button>
                <button
                  onClick={onConfirm}
                  disabled={loading}
                  className={`flex-1 font-medium px-4 py-2 rounded-lg
                             active:scale-[0.98] transition-all duration-150 text-sm
                             disabled:opacity-50 ${confirmColors}`}
                >
                  {loading ? "…" : confirmLabel}
                </button>
              </div>
            </motion.div>
          </div>
        </>
      )}
    </AnimatePresence>
  );
}
