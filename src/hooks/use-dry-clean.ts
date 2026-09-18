import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
 import { addDryCleanPayment, assignDryCleanIroner, createDryCleanCatalogItem, createDryCleanOrder, createDryCleanService, deleteDryCleanCatalogItem, deleteDryCleanService, deliverDryCleanOrder, getDryCleanOrder, getDryCleanProductivity, listDryCleanCatalog, listDryCleanOrders, listDryCleanServices, prepareDryCleanOrder, printDryCleanOrder, updateDryCleanCatalogItem, updateDryCleanService } from "@/api/dry-clean"
import { CreateDryCleanOrderRequest } from "@/types/domain/domain.types"

export const dryCleanKeys = {
  all: ["dry-clean"] as const,
  orders: (page: number, pageSize: number, status?: string, q?: string, clientId?: string, paymentStatus?: string) => ["dry-clean", "orders", page, pageSize, status, q, clientId, paymentStatus] as const,
}

export function useDryCleanOrders(page: number, pageSize: number, status?: string, q?: string, clientId?: string, paymentStatus?: string) {
  return useQuery({ queryKey: dryCleanKeys.orders(page, pageSize, status, q, clientId, paymentStatus), queryFn: () => listDryCleanOrders(page, pageSize, status, q, clientId, paymentStatus), placeholderData: keepPreviousData })
}

export function useDryCleanOrder(id: string | null) {
  return useQuery({ queryKey: [...dryCleanKeys.all, "detail", id], queryFn: () => getDryCleanOrder(id as string), enabled: !!id })
}

export function useCreateDryCleanOrder() {
  const queryClient = useQueryClient()
  return useMutation({ mutationFn: (payload: CreateDryCleanOrderRequest) => createDryCleanOrder(payload), onSuccess: () => queryClient.invalidateQueries({ queryKey: dryCleanKeys.all }) })
}

function useDryCleanMutation<T>(mutationFn: (value: T) => Promise<unknown>) {
  const queryClient = useQueryClient()
  return useMutation({ mutationFn, onSuccess: () => queryClient.invalidateQueries({ queryKey: dryCleanKeys.all }) })
}

export function usePrepareDryCleanOrder() { return useDryCleanMutation(({ id, ironerEmployeeId }: { id: string; ironerEmployeeId: string }) => prepareDryCleanOrder(id, ironerEmployeeId)) }
export function useAssignDryCleanIroner() { return useDryCleanMutation(({ id, ironerEmployeeId }: { id: string; ironerEmployeeId: string }) => assignDryCleanIroner(id, ironerEmployeeId)) }
export function useAddDryCleanPayment() { return useDryCleanMutation(({ id, amount, paymentMethod }: { id: string; amount: number; paymentMethod: "cash" | "wallet" | "instapay" }) => addDryCleanPayment(id, amount, paymentMethod)) }
export function useDeliverDryCleanOrder() { return useDryCleanMutation(({ id, deliveryEmployeeId }: { id: string; deliveryEmployeeId: string }) => deliverDryCleanOrder(id, deliveryEmployeeId)) }
export function usePrintDryCleanOrder() { return useMutation({ mutationFn: ({ id, readyCopy, ironerCopy }: { id: string; readyCopy: boolean; ironerCopy?: boolean }) => printDryCleanOrder(id, readyCopy, ironerCopy) }) }
export function useDryCleanProductivity(from?: string, to?: string) { return useQuery({ queryKey: [...dryCleanKeys.all, "productivity", from, to], queryFn: () => getDryCleanProductivity(from, to) }) }
  export function useDryCleanCatalog(includeInactive = false) { return useQuery({ queryKey: [...dryCleanKeys.all, "catalog", includeInactive], queryFn: () => listDryCleanCatalog(includeInactive) }) }
  export function useDryCleanServices(includeInactive = false) { return useQuery({ queryKey: [...dryCleanKeys.all, "services", includeInactive], queryFn: () => listDryCleanServices(includeInactive) }) }
  export function useCreateDryCleanService() { return useDryCleanMutation((payload: import("@/types/domain/domain.types").DryCleanServiceInput) => createDryCleanService(payload)) }
  export function useUpdateDryCleanService() { return useDryCleanMutation(({ id, payload }: { id: string; payload: import("@/types/domain/domain.types").DryCleanServiceInput }) => updateDryCleanService(id, payload)) }
  export function useDeleteDryCleanService() { return useDryCleanMutation((id: string) => deleteDryCleanService(id)) }
export function useCreateDryCleanCatalogItem() { return useDryCleanMutation((payload: import("@/types/domain/domain.types").DryCleanCatalogItemInput) => createDryCleanCatalogItem(payload)) }
export function useUpdateDryCleanCatalogItem() { return useDryCleanMutation(({ id, payload }: { id: string; payload: import("@/types/domain/domain.types").DryCleanCatalogItemInput }) => updateDryCleanCatalogItem(id, payload)) }
export function useDeleteDryCleanCatalogItem() { return useDryCleanMutation((id: string) => deleteDryCleanCatalogItem(id)) }
