export interface QueryAnalysisRun {
  id: number;
  userId: number;
  provider: number;
  originalSql: string;
  normalizedSqlHash: string;
  startedAt: string;
  finishedAt?: string | null;
  status: string;
  errorMessage?: string | null;
}