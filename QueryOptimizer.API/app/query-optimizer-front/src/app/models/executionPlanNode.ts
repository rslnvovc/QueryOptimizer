export interface ExecutionPlanNode {
  nodeType?: string | null;

  normalizedNodeType?: string | null;

  objectName?: string | null;

  indexName?: string | null;

  predicate?: string | null;

  joinType?: string | null;

  estimatedCost?: number | null;

  estimatedRows?: number | null;

  actualRows?: number | null;

  actualTimeMs?: number | null;

  logicalReads?: number | null;

  physicalReads?: number | null;

  children?: ExecutionPlanNode[];
}