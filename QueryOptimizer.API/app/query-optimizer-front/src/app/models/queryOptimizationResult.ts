import { NormalizedExecutionPlan } from "./normalizedExecutionPlan";
import { OptimizationCandidateResult } from "./optimizationCandidateResult";
import { QueryOptimizationFinding } from "./queryOptimizationFinding";
import { QueryPerformanceMetrics } from "./queryPerformanceMetrics";

export interface QueryOptimizationResult {
  analysisRunId: number;

  originalMetrics: QueryPerformanceMetrics;

  normalizedPlan?: NormalizedExecutionPlan | null;

  findings: QueryOptimizationFinding[];

  candidateResults: OptimizationCandidateResult[];

  bestCandidate?: OptimizationCandidateResult | null;
}