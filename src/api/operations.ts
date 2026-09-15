import { request, toQuery } from "@/lib/api"
import { PagedAuditLogs } from "@/types/domain/domain.types"

export function listAuditLogs(page = 1, pageSize = 20, action?: string, entityType?: string) { return request<PagedAuditLogs>(`/api/audit-logs${toQuery({ page, pageSize, action, entityType })}`) }
