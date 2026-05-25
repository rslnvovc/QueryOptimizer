import { QueryPerformanceMetrics } from "./queryPerformanceMetrics";

export interface OptimizationCandidateResult {
  candidateId?: number | null;

  ruleCode: string;

  candidateSql: string;

  description?: string | null;

  wasTested: boolean;

  isBest: boolean;

  improvementPercent?: number | null;

  metrics?: QueryPerformanceMetrics | null;
}