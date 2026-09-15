import { request, toQuery } from "@/lib/api"
import { CreateDryCleanOrderRequest, DryCleanOrder, DryCleanProductivitySummary } from "@/types/domain/domain.types"

export function listDryCleanOrders(page = 1, pageSize = 20, status?: string, q?: string) {
  return request<{ items: DryCleanOrder[]; total: number }>(`/api/dry-clean/orders${toQuery({ page, pageSize, status, q })}`)
}

export function createDryCleanOrder(payload: CreateDryCleanOrderRequest) {
  return request<DryCleanOrder>("/api/dry-clean/orders", { method: "POST", body: JSON.stringify(payload) })
}

export function startDryCleanProcessing(id: string) {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}/processing`, { method: "POST" })
}

export function completeDryCleanOrder(id: string, ironerEmployeeId: string) {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}/complete`, { method: "POST", body: JSON.stringify({ ironerEmployeeId }) })
}

export function addDryCleanPayment(id: string, amount: number, paymentMethod: "cash" | "wallet" | "instapay") {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}/payments`, { method: "POST", body: JSON.stringify({ amount, paymentMethod }) })
}

export function deliverDryCleanOrder(id: string, deliveryEmployeeId: string) {
  return request<DryCleanOrder>(`/api/dry-clean/orders/${id}/deliver`, { method: "POST", body: JSON.stringify({ deliveryEmployeeId }) })
}

export function printDryCleanOrder(id: string, readyCopy: boolean) {
  return request<{ success: boolean; message: string }>(`/api/printing/dry-clean/${id}`, { method: "POST", body: JSON.stringify({ readyCopy }) })
}

export function getDryCleanProductivity(from?: string, to?: string) {
  return request<DryCleanProductivitySummary[]>(`/api/dry-clean/orders/productivity${toQuery({ from, to })}`)
}
