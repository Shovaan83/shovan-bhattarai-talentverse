"use client";

import { useState, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { X, Zap, Target, Loader2 } from "lucide-react";
import type { AddSkillPayload } from "@/lib/types/skills";

const skillSchema = z.object({
  skillName: z
    .string()
    .min(2, "Skill name must be at least 2 characters")
    .max(50, "Skill name must be less than 50 characters"),
  category: z
    .string()
    .min(2, "Category must be at least 2 characters")
    .max(30, "Category must be less than 30 characters"),
  description: z
    .string()
    .max(200, "Description must be less than 200 characters")
    .optional(),
});

type SkillFormData = z.infer<typeof skillSchema>;

interface AddSkillModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: AddSkillPayload) => void;
  initialType?: "Offer" | "Want";
  isSubmitting?: boolean;
}

export function AddSkillModal({
  isOpen,
  onClose,
  onSubmit,
  initialType = "Offer",
  isSubmitting = false,
}: AddSkillModalProps) {
  const [skillType, setSkillType] = useState<"Offer" | "Want">(initialType);

  useEffect(() => {
    setSkillType(initialType);
  }, [initialType]);

  const isOffer = skillType === "Offer";

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<SkillFormData>({
    resolver: zodResolver(skillSchema),
  });

  const handleFormSubmit = (data: SkillFormData) => {
    onSubmit({
      skillName: data.skillName,
      category: data.category,
      type: isOffer ? 0 : 1,
      description: data.description || "",
    });
    reset();
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  // Categories suggestions
  const categories = [
    "Programming",
    "Design",
    "Music",
    "Languages",
    "Marketing",
    "Photography",
    "Writing",
    "Business",
    "Cooking",
    "Fitness",
  ];

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={handleClose}
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50"
          />

          {/* Modal */}
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 20 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 20 }}
            transition={{ type: "spring", damping: 25, stiffness: 300 }}
            className="fixed inset-0 z-50 flex items-center justify-center p-4"
          >
            <div
              className={`w-full max-w-md bg-white rounded-3xl shadow-2xl overflow-hidden transition-colors duration-300 ${
                isOffer ? "ring-2 ring-emerald-200" : "ring-2 ring-orange-200"
              }`}
              onClick={(e) => e.stopPropagation()}
            >
              {/* Header */}
              <div
                className={`p-6 transition-colors duration-300 ${
                  isOffer
                    ? "bg-gradient-to-r from-emerald-500 to-emerald-600"
                    : "bg-gradient-to-r from-orange-500 to-orange-600"
                }`}
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-xl bg-white/20 backdrop-blur-sm flex items-center justify-center">
                      {isOffer ? (
                        <Zap className="w-5 h-5 text-white" />
                      ) : (
                        <Target className="w-5 h-5 text-white" />
                      )}
                    </div>
                    <h2 className="font-heading text-xl font-bold text-white">
                      Add New Skill
                    </h2>
                  </div>
                  <button
                    onClick={handleClose}
                    className="w-8 h-8 rounded-full bg-white/20 hover:bg-white/30 flex items-center justify-center transition-colors"
                  >
                    <X className="w-4 h-4 text-white" />
                  </button>
                </div>
              </div>

              {/* Toggle */}
              <div className="p-6 pb-0">
                <div className="flex items-center justify-center gap-2 p-1 bg-gray-100 rounded-xl">
                  <button
                    type="button"
                    onClick={() => setSkillType("Offer")}
                    className={`flex-1 py-2.5 px-4 rounded-lg font-medium text-sm transition-all duration-200 flex items-center justify-center gap-2 ${
                      isOffer
                        ? "bg-emerald-500 text-white shadow-md"
                        : "text-gray-600 hover:text-gray-900"
                    }`}
                  >
                    <Zap className="w-4 h-4" />I Offer This
                  </button>
                  <button
                    type="button"
                    onClick={() => setSkillType("Want")}
                    className={`flex-1 py-2.5 px-4 rounded-lg font-medium text-sm transition-all duration-200 flex items-center justify-center gap-2 ${
                      !isOffer
                        ? "bg-orange-500 text-white shadow-md"
                        : "text-gray-600 hover:text-gray-900"
                    }`}
                  >
                    <Target className="w-4 h-4" />I Want This
                  </button>
                </div>
              </div>

              {/* Form */}
              <form onSubmit={handleSubmit(handleFormSubmit)} className="p-6">
                <div className="space-y-4">
                  {/* Skill Name */}
                  <div>
                    <label
                      htmlFor="skillName"
                      className="block text-sm font-medium text-gray-700 mb-1.5"
                    >
                      Skill Name
                    </label>
                    <input
                      id="skillName"
                      type="text"
                      {...register("skillName")}
                      placeholder="e.g., Python, Guitar, Spanish"
                      className={`w-full px-4 py-3 rounded-xl border-2 transition-all duration-200 outline-none ${
                        errors.skillName
                          ? "border-red-300 focus:border-red-500"
                          : isOffer
                          ? "border-gray-200 focus:border-emerald-500"
                          : "border-gray-200 focus:border-orange-500"
                      }`}
                    />
                    {errors.skillName && (
                      <p className="mt-1.5 text-sm text-red-500">
                        {errors.skillName.message}
                      </p>
                    )}
                  </div>

                  {/* Category */}
                  <div>
                    <label
                      htmlFor="category"
                      className="block text-sm font-medium text-gray-700 mb-1.5"
                    >
                      Category
                    </label>
                    <input
                      id="category"
                      type="text"
                      {...register("category")}
                      placeholder="e.g., Programming, Music"
                      list="categories"
                      className={`w-full px-4 py-3 rounded-xl border-2 transition-all duration-200 outline-none ${
                        errors.category
                          ? "border-red-300 focus:border-red-500"
                          : isOffer
                          ? "border-gray-200 focus:border-emerald-500"
                          : "border-gray-200 focus:border-orange-500"
                      }`}
                    />
                    <datalist id="categories">
                      {categories.map((cat) => (
                        <option key={cat} value={cat} />
                      ))}
                    </datalist>
                    {errors.category && (
                      <p className="mt-1.5 text-sm text-red-500">
                        {errors.category.message}
                      </p>
                    )}
                  </div>

                  {/* Description */}
                  <div>
                    <label
                      htmlFor="description"
                      className="block text-sm font-medium text-gray-700 mb-1.5"
                    >
                      Description{" "}
                      <span className="text-gray-400">(Optional)</span>
                    </label>
                    <textarea
                      id="description"
                      {...register("description")}
                      placeholder="Briefly describe your skill level or what you want to learn..."
                      rows={3}
                      className={`w-full px-4 py-3 rounded-xl border-2 transition-all duration-200 outline-none resize-none ${
                        isOffer
                          ? "border-gray-200 focus:border-emerald-500"
                          : "border-gray-200 focus:border-orange-500"
                      }`}
                    />
                    {errors.description && (
                      <p className="mt-1.5 text-sm text-red-500">
                        {errors.description.message}
                      </p>
                    )}
                  </div>
                </div>

                {/* Submit Button */}
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className={`w-full mt-6 py-3.5 px-4 rounded-xl font-semibold text-white flex items-center justify-center gap-2 transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed ${
                    isOffer
                      ? "bg-emerald-500 hover:bg-emerald-600 shadow-lg shadow-emerald-500/30"
                      : "bg-orange-500 hover:bg-orange-600 shadow-lg shadow-orange-500/30"
                  }`}
                >
                  {isSubmitting ? (
                    <>
                      <Loader2 className="w-5 h-5 animate-spin" />
                      Adding...
                    </>
                  ) : (
                    <>
                      {isOffer ? (
                        <Zap className="w-5 h-5" />
                      ) : (
                        <Target className="w-5 h-5" />
                      )}
                      Add {isOffer ? "Offer" : "Want"}
                    </>
                  )}
                </button>
              </form>
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
}
