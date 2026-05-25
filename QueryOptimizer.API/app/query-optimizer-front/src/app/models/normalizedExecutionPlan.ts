import { ExecutionPlanNode } from './executionPlanNode';

export interface NormalizedExecutionPlan {
  provider: number;

  rawPlan?: string | null;

  nodes?: ExecutionPlanNode[];

  totalCost?: number | null;

  totalExecutionTimeMs?: number | null;

  totalLogicalReads?: number | null;

  totalPhysicalReads?: number | null;

  requestedMemoryKb?: number | null;

  grantedMemoryKb?: number | null;

  maxUsedMemoryKb?: number | null;

  additionalMetrics?: Record<string, string>;
}