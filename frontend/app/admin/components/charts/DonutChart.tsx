"use client";

import { useEffect, useRef } from "react";
import * as d3 from "d3";
import type { ProposalStatsDto } from "@/lib/types/admin";

interface DonutChartProps {
  data: ProposalStatsDto;
}

const STATUS_CONFIG = [
  { key: "pending" as const, label: "Pending", color: "#f59e0b" },
  { key: "accepted" as const, label: "Accepted", color: "#10b981" },
  { key: "declined" as const, label: "Declined", color: "#ef4444" },
  { key: "completed" as const, label: "Completed", color: "#6366f1" },
];

export default function DonutChart({ data }: DonutChartProps) {
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const pieces = STATUS_CONFIG.map((s) => ({
      label: s.label,
      value: data[s.key],
      color: s.color,
    })).filter((p) => p.value > 0);

    const total = pieces.reduce((s, p) => s + p.value, 0);
    if (total === 0) return;

    const size = 220;
    const radius = size / 2;
    const innerRadius = radius * 0.58;

    const g = svg
      .attr("viewBox", `0 0 ${size} ${size}`)
      .append("g")
      .attr("transform", `translate(${radius},${radius})`);

    const pie = d3.pie<(typeof pieces)[0]>().value((d) => d.value).sort(null).padAngle(0.02);
    const arc = d3.arc<d3.PieArcDatum<(typeof pieces)[0]>>().innerRadius(innerRadius).outerRadius(radius - 4);

    // Slices with animation
    g.selectAll("path")
      .data(pie(pieces))
      .join("path")
      .attr("fill", (d) => d.data.color)
      .attr("stroke", "#fff")
      .attr("stroke-width", 2)
      .transition()
      .duration(800)
      .attrTween("d", function (d) {
        const i = d3.interpolate({ startAngle: d.startAngle, endAngle: d.startAngle } as d3.PieArcDatum<(typeof pieces)[0]>, d);
        return (t: number) => arc(i(t)) || "";
      });

    // Center text
    g.append("text")
      .attr("text-anchor", "middle")
      .attr("dy", "-0.2em")
      .attr("fill", "#111827")
      .style("font-size", "28px")
      .style("font-weight", "700")
      .text(total);

    g.append("text")
      .attr("text-anchor", "middle")
      .attr("dy", "1.2em")
      .attr("fill", "#6b7280")
      .style("font-size", "12px")
      .text("Total");
  }, [data]);

  const total = STATUS_CONFIG.reduce((s, c) => s + data[c.key], 0);

  return (
    <div className="flex flex-col items-center gap-4">
      <svg ref={svgRef} className="w-full max-w-[220px]" style={{ height: 220 }} />
      <div className="flex flex-wrap justify-center gap-x-4 gap-y-2">
        {STATUS_CONFIG.map((s) => (
          <div key={s.key} className="flex items-center gap-1.5 text-sm">
            <span
              className="inline-block w-3 h-3 rounded-full"
              style={{ backgroundColor: s.color }}
            />
            <span className="text-gray-600">{s.label}</span>
            <span className="font-semibold text-gray-900">
              {data[s.key]}
              {total > 0 && (
                <span className="text-gray-400 font-normal ml-0.5 text-xs">
                  ({Math.round((data[s.key] / total) * 100)}%)
                </span>
              )}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
