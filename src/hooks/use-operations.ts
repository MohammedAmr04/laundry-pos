import { keepPreviousData, useQuery } from "@tanstack/react-query"
import { listAuditLogs } from "@/api/operations"
import { listBackups } from "@/api/backups"

export const operationsKeys = { all: ["operations"] as const, audit: (page: number) => ["operations", "audit", page] as const, backups: () => ["operations", "backups"] as const }
export function useAuditLogs(page: number, action?: string, entityType?: string) { return useQuery({ queryKey: [...operationsKeys.audit(page), action, entityType], queryFn: () => listAuditLogs(page, 20, action, entityType), placeholderData: keepPreviousData }) }
export function useBackups() { return useQuery({ queryKey: operationsKeys.backups(), queryFn: listBackups }) }
