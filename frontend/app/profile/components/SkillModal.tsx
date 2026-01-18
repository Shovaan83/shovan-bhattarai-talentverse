"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { motion, AnimatePresence } from "framer-motion";
import { ArrowRightLeft, X } from "lucide-react";
import type { AddSkillPayload } from "@/lib/types/skills";

export const SkillType = {
  Offer: 0,
  Want: 1,
} as const;

export type SkillType = (typeof SkillType)[keyof typeof SkillType];

type SkillFormValues = AddSkillPayload;

interface SkillModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: AddSkillPayload) => void;
  isSubmitting: boolean;
  defaultType?: SkillType;
}

export default function SkillModal({
  isOpen,
  onClose,
  onSubmit,
  isSubmitting,
  defaultType = SkillType.Offer,
}: SkillModalProps) {
  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors },
  } = useForm<SkillFormValues>({
    defaultValues: {
      type: defaultType,
      category: "General",
      skillName: "",
      description: "",
    },
  });

  const skillType = watch("type");
  const isOffer = Number(skillType) === SkillType.Offer;

  useEffect(() => {
    if (isOpen) {
      setValue("type", defaultType);
    } else {
      const t = setTimeout(() => reset(), 300);
      return () => clearTimeout(t);
    }
  }, [isOpen, defaultType, reset, setValue]);

  const toggleType = () => {
    setValue("type", isOffer ? SkillType.Want : SkillType.Offer);
  };

  const bgColor = isOffer ? "bg-emerald-50" : "bg-orange-50";
  const buttonColor = isOffer
    ? "bg-emerald-600 hover:bg-emerald-700"
    : "bg-orange-600 hover:bg-orange-700";
  const ringColor = isOffer ? "focus:ring-emerald-500" : "focus:ring-orange-500";

  return (
    <AnimatePresence>
      {isOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="absolute inset-0 bg-black/40 backdrop-blur-sm"
          />

          <motion.div
            initial={{ scale: 0.95, opacity: 0, y: 20 }}
            animate={{ scale: 1, opacity: 1, y: 0 }}
            exit={{ scale: 0.95, opacity: 0, y: 20 }}
            className="relative w-full max-w-lg overflow-hidden rounded-2xl bg-white shadow-2xl ring-1 ring-gray-200"
            onClick={(e) => e.stopPropagation()}
          >
            <div
              className={`flex items-center justify-between px-6 py-4 border-b border-gray-100 ${bgColor}`}
            >
              <h2 className="text-xl font-heading font-semibold text-gray-800">
                {isOffer ? "Add a Skill to Offer" : "Add a Skill You Want"}
              </h2>
              <button
                onClick={onClose}
                className="p-2 text-gray-400 transition-colors rounded-full hover:bg-white/50 hover:text-gray-600"
                type="button"
              >
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
              <div className="flex justify-center mb-6">
                <button
                  type="button"
                  onClick={toggleType}
                  className={`flex items-center gap-2 px-4 py-2 text-sm font-medium transition-all rounded-full border shadow-sm ${
                    isOffer
                      ? "bg-emerald-100 text-emerald-800 border-emerald-200"
                      : "bg-orange-100 text-orange-800 border-orange-200"
                  }`}
                >
                  <ArrowRightLeft size={16} />
                  <span>Switch to {isOffer ? "Requesting" : "Offering"}</span>
                </button>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-gray-700">
                  Skill Name
                </label>
                <input
                  {...register("skillName", { required: "Skill name is required" })}
                  placeholder={isOffer ? "e.g., React Development" : "e.g., Piano Lessons"}
                  className={`w-full px-4 py-3 text-gray-800 transition-all border border-gray-200 rounded-xl bg-gray-50 focus:bg-white focus:outline-none focus:ring-2 ${ringColor}`}
                />
                {errors.skillName && (
                  <span className="text-xs text-red-500">
                    {errors.skillName.message}
                  </span>
                )}
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-gray-700">
                  Category
                </label>
                <select
                  {...register("category")}
                  className={`w-full px-4 py-3 text-gray-800 transition-all border border-gray-200 rounded-xl bg-gray-50 focus:bg-white focus:outline-none focus:ring-2 ${ringColor}`}
                >
                  <option value="Development">Development</option>
                  <option value="Design">Design</option>
                  <option value="Music">Music</option>
                  <option value="Language">Language</option>
                  <option value="Lifestyle">Lifestyle</option>
                  <option value="Business">Business</option>
                  <option value="General">General</option>
                </select>
              </div>

              <div className="space-y-2">
                <label className="text-sm font-medium text-gray-700">
                  Description
                </label>
                <textarea
                  {...register("description", {
                    required: "Please describe the skill",
                  })}
                  rows={3}
                  placeholder={
                    isOffer
                      ? "Describe what you can teach or build..."
                      : "Describe what you're looking to learn..."
                  }
                  className={`w-full px-4 py-3 text-gray-800 transition-all border border-gray-200 rounded-xl bg-gray-50 focus:bg-white focus:outline-none focus:ring-2 ${ringColor}`}
                />
                {errors.description && (
                  <span className="text-xs text-red-500">
                    {errors.description.message}
                  </span>
                )}
              </div>

              <div className="flex justify-end pt-2">
                <button
                  type="button"
                  onClick={onClose}
                  className="px-6 py-2.5 mr-3 text-sm font-medium text-gray-600 transition-colors rounded-xl hover:bg-gray-100"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className={`px-8 py-2.5 text-sm font-medium text-white transition-all transform rounded-xl shadow-lg hover:shadow-xl active:scale-95 disabled:opacity-70 disabled:cursor-not-allowed ${buttonColor}`}
                >
                  {isSubmitting ? "Saving..." : "Add Skill"}
                </button>
              </div>
            </form>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
}
