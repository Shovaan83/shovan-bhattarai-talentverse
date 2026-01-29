'use client';

import React from 'react';
import { motion } from 'framer-motion';
import { UploadCloud, Users, TrendingUp } from 'lucide-react';

const steps = [
  {
    icon: UploadCloud,
    title: "Post a Skill",
    description: "List what you're good at and what you want to learn. It takes less than 2 minutes."
  },
  {
    icon: Users,
    title: "Connect & Swap",
    description: "Browse matches or let our AI pair you. Schedule a session and start trading knowledge."
  },
  {
    icon: TrendingUp,
    title: "Earn & Repeat",
    description: "Complete the session, rate your partner, and earn credits to unlock more skills."
  }
];

export const HowItWorks: React.FC = () => {
  return (
    <section className="py-24 bg-gray-50 relative overflow-hidden" id="how-it-works">
      {/* Background decoration lines */}
      <div className="absolute top-1/2 left-0 w-full h-px bg-gray-200 -translate-y-1/2 hidden md:block" />
      
      <div className="container mx-auto px-6 relative z-10">
        <div className="text-center mb-16">
          <span className="text-orange-600 font-semibold tracking-wider uppercase text-sm">Simple Process</span>
          <h2 className="font-heading text-3xl md:text-5xl font-bold text-gray-900 mt-2">How TalentVerse Works</h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
          {steps.map((step, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 30 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.2, duration: 0.6 }}
              className="flex flex-col items-center text-center bg-white p-8 rounded-3xl shadow-sm border border-gray-100 relative group hover:-translate-y-2 transition-transform duration-300"
            >
              <div className="w-20 h-20 bg-emerald-50 rounded-full flex items-center justify-center mb-6 text-emerald-600 group-hover:bg-emerald-600 group-hover:text-white transition-colors duration-300">
                <step.icon size={32} />
              </div>
              <div className="absolute top-6 right-6 font-heading font-bold text-6xl text-gray-100 -z-10 group-hover:text-emerald-600 transition-colors">
                {index + 1}
              </div>
              <h3 className="font-heading text-xl font-bold text-gray-900 mb-3">{step.title}</h3>
              <p className="text-gray-500 leading-relaxed">
                {step.description}
              </p>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  );
};
