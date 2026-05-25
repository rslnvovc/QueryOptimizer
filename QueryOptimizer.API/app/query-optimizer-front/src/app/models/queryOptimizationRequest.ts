import { DatabaseTypes } from "./databaseType";

export interface QueryOptimizationRequest {
  userId: number;
  provider: DatabaseTypes;
  connectionString: string;
  sql: string;
}