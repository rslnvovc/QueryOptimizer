export interface QueryPerformanceMetrics {
  id?: string | null;

  provider: number;

  originalSql?: string | null;

  startedAt?: string | null;
  finishedAt?: string | null;

  clientElapsedMs?: number | null;
  databaseElapsedMs?: number | null;
  cpuTimeMs?: number | null;

  logicalReads?: number | null;
  physicalReads?: number | null;

  rowsReturned?: number | null;
  rowsAffected?: number | null;

  estimatedCost?: number | null;
  planningTimeMs?: number | null;

  executionPlan?: string | null;
  executionPlanFormat?: number | string | null;

  warnings?: string[];
  rawMetrics?: Record<string, string>;
}