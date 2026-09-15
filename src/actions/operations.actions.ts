import { queryClient } from "@/lib/query-client"
import { operationsKeys } from "@/hooks/use-operations"
import { createBackup, restoreBackup } from "@/api/backups"

export async function invalidateAuditLogs() { await queryClient.invalidateQueries({ queryKey: operationsKeys.all }) }
export async function createDatabaseBackup() { const result = await createBackup(); await queryClient.invalidateQueries({ queryKey: operationsKeys.all }); return result }
export async function restoreDatabaseBackup(fileName: string) { const result = await restoreBackup(fileName); await queryClient.invalidateQueries({ queryKey: operationsKeys.all }); return result }
