"use client";

import { useState } from "react";
import { X, Star } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { motion, AnimatePresence } from "framer-motion";
import { toast } from "react-hot-toast";
import { useCreateReview } from "@/lib/hooks/useReviews";
import type { CreateReviewDto } from "@/lib/types/reviews";

const reviewSchema = z.object({
  rating: z.number().min(1, "Please select a rating").max(5),
  comment: z
    .string()
    .max(500, "Comment cannot exceed 500 characters")
    .optional(),
});

type ReviewFormData = z.infer<typeof reviewSchema>;

interface ReviewModalProps {
  proposalId: number;
  revieweeUsername: string;
  isOpen: boolean;
  onClose: () => void;
}

export default function ReviewModal({
  proposalId,
  revieweeUsername,
  isOpen,
  onClose,
}: ReviewModalProps) {
  const [hoveredRating, setHoveredRating] = useState(0);
  const createReviewMutation = useCreateReview();

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
    reset,
  } = useForm<ReviewFormData>({
    resolver: zodResolver(reviewSchema),
    defaultValues: {
      rating: 0,
      comment: "",
    },
  });

  const rating = watch("rating");

  const handleClose = () => {
    reset();
    onClose();
  };

  const onSubmit = async (data: ReviewFormData) => {
    try {
      const payload: CreateReviewDto = {
        proposalId,
        rating: data.rating,
        comment: data.comment || undefined,
      };

      const response = await createReviewMutation.mutateAsync(payload);

      if (response.success) {
        toast.success(response.message || "Review submitted successfully!");
        handleClose();
      } else {
        toast.error(response.message || "Failed to submit review");
      }
    } catch (error: any) {
      toast.error(
        error?.response?.data?.message || "Failed to submit review"
      );
    }
  };

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
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="bg-white rounded-2xl shadow-2xl max-w-lg w-full max-h-[90vh] overflow-y-auto"
            >
              {/* Header */}
              <div className="flex items-center justify-between p-6 border-b border-zinc-200">
                <div>
                  <h2 className="text-2xl font-bold text-zinc-900">
                    Leave a Review
                  </h2>
                  <p className="text-zinc-600 mt-1">
                    How was your experience with{" "}
                    <span className="font-semibold">{revieweeUsername}</span>?
                  </p>
                </div>
                <button
                  onClick={handleClose}
                  className="p-2 hover:bg-zinc-100 rounded-lg transition-colors"
                >
                  <X className="w-5 h-5 text-zinc-500" />
                </button>
              </div>

              {/* Form */}
              <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
                {/* Star Rating */}
                <div>
                  <label className="block text-sm font-semibold text-zinc-900 mb-3">
                    Rating <span className="text-red-500">*</span>
                  </label>
                  <div className="flex items-center gap-2">
                    {[1, 2, 3, 4, 5].map((star) => (
                      <button
                        key={star}
                        type="button"
                        onClick={() => setValue("rating", star)}
                        onMouseEnter={() => setHoveredRating(star)}
                        onMouseLeave={() => setHoveredRating(0)}
                        className="transition-transform hover:scale-110 focus:outline-none focus:ring-2 focus:ring-zinc-400 rounded"
                      >
                        <Star
                          className={`w-10 h-10 ${
                            star <= (hoveredRating || rating)
                              ? "fill-yellow-400 text-yellow-400"
                              : "fill-zinc-200 text-zinc-200"
                          } transition-colors`}
                        />
                      </button>
                    ))}
                    {rating > 0 && (
                      <span className="ml-2 text-zinc-600 font-medium">
                        {rating} / 5
                      </span>
                    )}
                  </div>
                  {errors.rating && (
                    <p className="text-red-500 text-sm mt-2">
                      {errors.rating.message}
                    </p>
                  )}
                </div>

                {/* Comment */}
                <div>
                  <label
                    htmlFor="comment"
                    className="block text-sm font-semibold text-zinc-900 mb-2"
                  >
                    Comment (Optional)
                  </label>
                  <textarea
                    id="comment"
                    {...register("comment")}
                    rows={4}
                    placeholder="Share your experience with this swap..."
                    className="w-full px-4 py-3 border border-zinc-200 rounded-lg focus:ring-2 focus:ring-zinc-400 focus:border-zinc-400 resize-none text-zinc-900 bg-white"
                    maxLength={500}
                  />
                  <div className="flex justify-between mt-2">
                    {errors.comment && (
                      <p className="text-red-500 text-sm">
                        {errors.comment.message}
                      </p>
                    )}
                    <p className="text-sm text-zinc-500 ml-auto">
                      {watch("comment")?.length || 0} / 500
                    </p>
                  </div>
                </div>

                {/* Actions */}
                <div className="flex gap-3 pt-4">
                  <button
                    type="button"
                    onClick={handleClose}
                    className="flex-1 px-6 py-3 bg-zinc-100 text-zinc-700 font-medium rounded-lg hover:bg-zinc-200 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={createReviewMutation.isPending}
                    className="flex-1 px-6 py-3 bg-zinc-900 text-white font-medium rounded-lg hover:bg-zinc-800 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {createReviewMutation.isPending
                      ? "Submitting..."
                      : "Submit Review"}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        </>
      )}
    </AnimatePresence>
  );
}
