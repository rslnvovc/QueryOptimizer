export interface QueryOptimizationFinding {
  ruleCode: string;

  title: string;

  description: string;

  recommendation: string;

  suggestedSql?: string | null;

  suggestedIndexSql?: string | null;

  affectedObject?: string | null;

  affectedNodeType?: string | null;

  severity: number;

  confidence?: number | null;

  adaptiveConfidence?: number | null;
}