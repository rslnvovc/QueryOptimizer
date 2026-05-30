import { DatabaseTypes } from "./databaseType";

export interface ConnectionStringModel{
    provider: DatabaseTypes;
    serverName?: string | null;
    userName?: string | null;
    password?: string | null;
    databaseName?: string | null;
    host?: string | null;
    port?: number | null;
    serviceName?: string | null;
}