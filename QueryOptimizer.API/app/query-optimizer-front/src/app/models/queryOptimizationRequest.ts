import { ConnectionStringModel } from "./connectionStringModel";
import { DatabaseTypes } from "./databaseType";

export interface QueryOptimizationRequest {
  userId: number;
  connectionStringModel: ConnectionStringModel;
  sql: string;
}