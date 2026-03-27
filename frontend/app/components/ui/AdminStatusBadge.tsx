interface AdminStatusBadgeProps {
  status: "Active" | "Suspended" | "Banned" | "Skill" | "Review";
  className?: string;
}

const styles: Record<AdminStatusBadgeProps["status"], string> = {
  Active: "bg-brand-teal-50 text-brand-teal-700",
  Suspended: "bg-brand-gold-50 text-brand-gold-800",
  Banned: "bg-red-50 text-red-700",
  Skill: "bg-brand-teal-50 text-brand-teal-700",
  Review: "bg-zinc-100 text-zinc-700",
};

export function AdminStatusBadge({ status, className = "" }: AdminStatusBadgeProps) {
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${styles[status]} ${className}`.trim()}
    >
      <span className="h-1.5 w-1.5 rounded-full bg-current opacity-80" />
      {status}
    </span>
  );
}
