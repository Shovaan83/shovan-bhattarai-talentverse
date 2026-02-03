"use client";

import { Star, User } from "lucide-react";
import Image from "next/image";
import { format } from "date-fns";
import type { ReviewDto } from "@/lib/types/reviews";

interface ReviewCardProps {
  review: ReviewDto;
}

export default function ReviewCard({ review }: ReviewCardProps) {
  return (
    <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100 hover:shadow-md transition-shadow">
      <div className="flex items-start gap-4">
        {/* Reviewer Profile Picture */}
        <div className="flex-shrink-0">
          {review.reviewerProfilePictureUrl ? (
            <Image
              src={review.reviewerProfilePictureUrl}
              alt={review.reviewerUsername}
              width={48}
              height={48}
              className="rounded-full object-cover"
            />
          ) : (
            <div className="w-12 h-12 rounded-full bg-emerald-100 flex items-center justify-center">
              <User className="w-6 h-6 text-emerald-600" />
            </div>
          )}
        </div>

        {/* Review Content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center justify-between gap-2 mb-2">
            <div>
              <h4 className="font-semibold text-gray-900">
                {review.reviewerUsername}
              </h4>
              <p className="text-sm text-gray-500">
                {format(new Date(review.createdAt), "MMM dd, yyyy")}
              </p>
            </div>
            {/* Star Rating */}
            <div className="flex items-center gap-0.5">
              {[1, 2, 3, 4, 5].map((star) => (
                <Star
                  key={star}
                  className={`w-4 h-4 ${
                    star <= review.rating
                      ? "fill-yellow-400 text-yellow-400"
                      : "fill-gray-200 text-gray-200"
                  }`}
                />
              ))}
            </div>
          </div>

          {/* Review Comment */}
          {review.comment && (
            <p className="text-gray-700 leading-relaxed">{review.comment}</p>
          )}
        </div>
      </div>
    </div>
  );
}
