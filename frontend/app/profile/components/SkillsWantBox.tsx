"use client";

import { motion } from "framer-motion";
import { Plus, X, Target } from "lucide-react";
import type { UserSkill } from "@/lib/types/skills";

interface SkillsWantBoxProps {
  skills: UserSkill[];
  onAdd: () => void;
  onDelete: (id: number) => void;
  isLoading?: boolean;
}

export function SkillsWantBox({
  skills,
  onAdd,
  onDelete,
  isLoading,
}: SkillsWantBoxProps) {
  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.05,
      },
    },
  };

  const itemVariants = {
    hidden: { opacity: 0, scale: 0.8 },
    visible: { opacity: 1, scale: 1 },
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay: 0.4 }}
      className="bg-orange-50/50 rounded-3xl p-6 border border-orange-100 relative overflow-hidden"
    >
      {/* Decorative background */}
      <div className="absolute top-0 right-0 w-64 h-64 bg-gradient-to-bl from-orange-100/50 to-transparent rounded-full blur-3xl pointer-events-none"></div>

      {/* Header */}
      <div className="flex items-center justify-between mb-6 relative">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-orange-500 flex items-center justify-center">
            <Target className="w-5 h-5 text-white" />
          </div>
          <div>
            <h3 className="font-heading text-lg font-bold text-gray-900">
              What I&apos;m Looking For
            </h3>
            <p className="text-sm text-orange-600">
              {skills.length} skill{skills.length !== 1 ? "s" : ""} wanted
            </p>
          </div>
        </div>
        <button
          onClick={onAdd}
          className="w-10 h-10 rounded-xl bg-orange-500 hover:bg-orange-600 text-white flex items-center justify-center transition-all duration-200 hover:scale-105 hover:shadow-lg hover:shadow-orange-500/30"
        >
          <Plus className="w-5 h-5" />
        </button>
      </div>

      {/* Skills List */}
      <motion.div
        variants={containerVariants}
        initial="hidden"
        animate="visible"
        className="flex flex-wrap gap-2 relative min-h-[120px]"
      >
        {isLoading ? (
          <div className="w-full flex items-center justify-center py-8">
            <div className="w-8 h-8 border-3 border-orange-200 border-t-orange-500 rounded-full animate-spin"></div>
          </div>
        ) : skills.length === 0 ? (
          <div className="w-full text-center py-8">
            <p className="text-orange-600/70 text-sm">
              No skills wanted yet. Click + to add what you want to learn!
            </p>
          </div>
        ) : (
          skills.map((skill) => (
            <motion.div
              key={skill.userSkillId}
              variants={itemVariants}
              layout
              className="group relative"
            >
              <div className="flex items-center gap-2 px-4 py-2 bg-white rounded-full border border-orange-200 hover:border-orange-300 hover:shadow-md transition-all duration-200">
                <span className="font-medium text-orange-700">
                  {skill.skillName}
                </span>
                <span className="text-xs text-orange-500 bg-orange-100 px-2 py-0.5 rounded-full">
                  {skill.category}
                </span>
                <button
                  onClick={() => onDelete(skill.userSkillId)}
                  className="w-5 h-5 rounded-full bg-red-100 text-red-500 hover:bg-red-500 hover:text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all duration-200"
                >
                  <X className="w-3 h-3" />
                </button>
              </div>
            </motion.div>
          ))
        )}
      </motion.div>
    </motion.div>
  );
}
