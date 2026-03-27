interface ProficiencyDotsProps {
  level: number;
  color: "teal" | "violet";
  maxDots?: number;
}

const filledColor = {
  teal: "bg-[#1D9E75]",
  violet: "bg-[#534AB7]",
} as const;

export function ProficiencyDots({
  level,
  color,
  maxDots = 5,
}: ProficiencyDotsProps) {
  return (
    <div className="flex gap-1 justify-center mt-1">
      {Array.from({ length: maxDots }).map((_, i) => (
        <div
          key={i}
          className={`w-1.5 h-1.5 rounded-full ${
            i < level ? filledColor[color] : "bg-gray-200"
          }`}
        />
      ))}
    </div>
  );
}
