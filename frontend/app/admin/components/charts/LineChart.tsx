"use client";

import { useEffect, useRef } from "react";
import * as d3 from "d3";
import type { UserGrowthPoint } from "@/lib/types/admin";

interface LineChartProps {
  data: UserGrowthPoint[];
}

export default function LineChart({ data }: LineChartProps) {
  const svgRef = useRef<SVGSVGElement>(null);

  useEffect(() => {
    if (!svgRef.current || data.length === 0) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const margin = { top: 20, right: 30, bottom: 40, left: 50 };
    const width = svgRef.current.clientWidth - margin.left - margin.right;
    const height = 280 - margin.top - margin.bottom;

    const g = svg
      .attr("viewBox", `0 0 ${width + margin.left + margin.right} ${height + margin.top + margin.bottom}`)
      .append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`);

    // Scales
    const x = d3
      .scalePoint<string>()
      .domain(data.map((d) => d.month))
      .range([0, width])
      .padding(0.5);

    const y = d3
      .scaleLinear()
      .domain([0, d3.max(data, (d) => d.count) ?? 10])
      .nice()
      .range([height, 0]);

    // Gradient fill
    const defs = svg.append("defs");
    const gradient = defs
      .append("linearGradient")
      .attr("id", "area-gradient")
      .attr("x1", "0%").attr("y1", "0%")
      .attr("x2", "0%").attr("y2", "100%");
    gradient.append("stop").attr("offset", "0%").attr("stop-color", "#6366f1").attr("stop-opacity", 0.3);
    gradient.append("stop").attr("offset", "100%").attr("stop-color", "#6366f1").attr("stop-opacity", 0.02);

    // Area
    const area = d3
      .area<UserGrowthPoint>()
      .x((d) => x(d.month)!)
      .y0(height)
      .y1((d) => y(d.count))
      .curve(d3.curveMonotoneX);

    g.append("path")
      .datum(data)
      .attr("fill", "url(#area-gradient)")
      .attr("d", area);

    // Line
    const line = d3
      .line<UserGrowthPoint>()
      .x((d) => x(d.month)!)
      .y((d) => y(d.count))
      .curve(d3.curveMonotoneX);

    const path = g
      .append("path")
      .datum(data)
      .attr("fill", "none")
      .attr("stroke", "#6366f1")
      .attr("stroke-width", 2.5)
      .attr("d", line);

    // Animate line drawing
    const totalLength = path.node()?.getTotalLength() || 0;
    path
      .attr("stroke-dasharray", `${totalLength} ${totalLength}`)
      .attr("stroke-dashoffset", totalLength)
      .transition()
      .duration(1200)
      .ease(d3.easeCubicOut)
      .attr("stroke-dashoffset", 0);

    // Dots
    g.selectAll(".dot")
      .data(data)
      .join("circle")
      .attr("class", "dot")
      .attr("cx", (d) => x(d.month)!)
      .attr("cy", (d) => y(d.count))
      .attr("r", 4)
      .attr("fill", "#6366f1")
      .attr("stroke", "#fff")
      .attr("stroke-width", 2)
      .style("opacity", 0)
      .transition()
      .delay(1200)
      .duration(300)
      .style("opacity", 1);

    // Axes
    const formatMonth = (m: string) => {
      const [, month] = m.split("-");
      const months = ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];
      return months[parseInt(month) - 1] || m;
    };

    g.append("g")
      .attr("transform", `translate(0,${height})`)
      .call(d3.axisBottom(x).tickFormat(formatMonth))
      .selectAll("text")
      .attr("fill", "#6b7280")
      .style("font-size", "11px");

    g.append("g")
      .call(d3.axisLeft(y).ticks(5).tickFormat(d3.format("d")))
      .selectAll("text")
      .attr("fill", "#6b7280")
      .style("font-size", "11px");

    // Remove axis lines
    g.selectAll(".domain").attr("stroke", "#e5e7eb");
    g.selectAll(".tick line").attr("stroke", "#f3f4f6");
  }, [data]);

  return <svg ref={svgRef} className="w-full" style={{ height: 280 }} />;
}
