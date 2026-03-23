"use client";

import { useEffect, useRef } from "react";
import * as d3 from "d3";
import type { TopSkillPoint } from "@/lib/types/admin";

interface BarChartProps {
  data: TopSkillPoint[];
}

const COLORS = [
  "#6366f1", "#8b5cf6", "#a78bfa", "#c084fc",
  "#818cf8", "#7c3aed", "#6d28d9", "#5b21b6",
  "#4f46e5", "#4338ca",
];

export default function BarChart({ data }: BarChartProps) {
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || data.length === 0) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const margin = { top: 15, right: 30, bottom: 5, left: 120 };
    const width = svgRef.current.clientWidth - margin.left - margin.right;
    const barHeight = 28;
    const gap = 8;
    const height = data.length * (barHeight + gap);

    const g = svg
      .attr("viewBox", `0 0 ${width + margin.left + margin.right} ${height + margin.top + margin.bottom}`)
      .append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`);

    const x = d3
      .scaleLinear()
      .domain([0, d3.max(data, (d) => d.userCount) ?? 10])
      .nice()
      .range([0, width]);

    const y = d3
      .scaleBand()
      .domain(data.map((d) => d.skillName))
      .range([0, height])
      .padding(0.2);

    // Bars with animation
    g.selectAll(".bar")
      .data(data)
      .join("rect")
      .attr("class", "bar")
      .attr("y", (d) => y(d.skillName)!)
      .attr("height", y.bandwidth())
      .attr("rx", 6)
      .attr("fill", (_, i) => COLORS[i % COLORS.length])
      .attr("x", 0)
      .attr("width", 0)
      .transition()
      .duration(800)
      .delay((_, i) => i * 60)
      .ease(d3.easeCubicOut)
      .attr("width", (d) => x(d.userCount));

    // Labels (skill name on left)
    g.selectAll(".label")
      .data(data)
      .join("text")
      .attr("class", "label")
      .attr("x", -8)
      .attr("y", (d) => y(d.skillName)! + y.bandwidth() / 2)
      .attr("dy", "0.35em")
      .attr("text-anchor", "end")
      .attr("fill", "#374151")
      .style("font-size", "12px")
      .style("font-weight", "500")
      .text((d) => d.skillName.length > 16 ? d.skillName.slice(0, 16) + "…" : d.skillName);

    // Count labels on bar
    g.selectAll(".count")
      .data(data)
      .join("text")
      .attr("class", "count")
      .attr("x", (d) => x(d.userCount) + 6)
      .attr("y", (d) => y(d.skillName)! + y.bandwidth() / 2)
      .attr("dy", "0.35em")
      .attr("fill", "#6b7280")
      .style("font-size", "11px")
      .style("font-weight", "600")
      .style("opacity", 0)
      .text((d) => d.userCount)
      .transition()
      .delay(800)
      .duration(300)
      .style("opacity", 1);
  }, [data]);

  const chartHeight = Math.max(data.length * 36 + 20, 200);
  return <svg ref={svgRef} className="w-full" style={{ height: chartHeight }} />;
}
