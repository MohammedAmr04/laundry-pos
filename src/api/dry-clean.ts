import { request, toQuery } from "@/lib/api"
 import { CreateDryCleanOrderRequest, DryCleanCatalogItem, DryCleanCatalogItemInput, DryCleanOrder, DryCleanProductivitySummary, DryCleanServiceDefinition, DryCleanServiceInput } from "@/types/domain/domain.types"

export function listDryCleanOrders(page = 1, pageSize = 20, status?: string, q?: string, clientId?: string, paymentStatus?: string) {
  return request<{ items: DryCleanOrder[]; total: number }>(`/api/dry-clean/orders${toQuery({ page, pageSize, status, q, clientId, paymentStatus })}`)
}

export function getDryCleanOrder(id: string) {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}`)
}

export function createDryCleanOrder(payload: CreateDryCleanOrderRequest) {
  return request<DryCleanOrder>("/api/dry-clean/orders", { method: "POST", body: JSON.stringify(payload) })
}

export function prepareDryCleanOrder(id: string, ironerEmployeeId: string) {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}/prepare`, { method: "POST", body: JSON.stringify({ ironerEmployeeId }) })
}

export function assignDryCleanIroner(id: string, ironerEmployeeId: string) {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}/ironer`, { method: "PUT", body: JSON.stringify({ ironerEmployeeId }) })
}

export function addDryCleanPayment(id: string, amount: number, paymentMethod: "cash" | "wallet" | "instapay") {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}/payments`, { method: "POST", body: JSON.stringify({ amount, paymentMethod }) })
}

export function deliverDryCleanOrder(id: string, deliveryEmployeeId: string) {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}/deliver`, { method: "POST", body: JSON.stringify({ deliveryEmployeeId }) })
}

export function printDryCleanOrder(id: string, readyCopy: boolean, ironerCopy = false) {
  return request<{ success: boolean; message: string }>(`/api/printing/dry-clean/${id}`, { method: "POST", body: JSON.stringify({ readyCopy, ironerCopy }) })
}

export function getDryCleanProductivity(from?: string, to?: string) {
  return request<DryCleanProductivitySummary[]>(`/api/dry-clean/orders/productivity${toQuery({ from, to })}`)
}

  export function listDryCleanCatalog(includeInactive = false) { return request<DryCleanCatalogItem[]>(`/api/dry-clean/catalog${toQuery({ includeInactive })}`) }
  export function listDryCleanServices(includeInactive = false) { return request<DryCleanServiceDefinition[]>(`/api/dry-clean/services${toQuery({ includeInactive })}`) }
  export function createDryCleanService(payload: DryCleanServiceInput) { return request<DryCleanServiceDefinition>("/api/dry-clean/services", { method: "POST", body: JSON.stringify(payload) }) }
  export function updateDryCleanService(id: string, payload: DryCleanServiceInput) { return request<DryCleanServiceDefinition>(`/api/dry-clean/services/${id}`, { method: "PUT", body: JSON.stringify(payload) }) }
  export function deleteDryCleanService(id: string) { return request<void>(`/api/dry-clean/services/${id}`, { method: "DELETE" }) }
export function createDryCleanCatalogItem(payload: DryCleanCatalogItemInput) { return request<DryCleanCatalogItem>("/api/dry-clean/catalog", { method: "POST", body: JSON.stringify(payload) }) }
export function updateDryCleanCatalogItem(id: string, payload: DryCleanCatalogItemInput) { return request<DryCleanCatalogItem>(`/api/dry-clean/catalog/${id}`, { method: "PUT", body: JSON.stringify(payload) }) }
export function deleteDryCleanCatalogItem(id: string) { return request<void>(`/api/dry-clean/catalog/${id}`, { method: "DELETE" }) }
