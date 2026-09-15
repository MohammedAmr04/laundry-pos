import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { addDryCleanPayment, completeDryCleanOrder, createDryCleanOrder, deliverDryCleanOrder, getDryCleanProductivity, listDryCleanOrders, printDryCleanOrder, startDryCleanProcessing } from "@/api/dry-clean"
import { CreateDryCleanOrderRequest } from "@/types/domain/domain.types"

export const dryCleanKeys = {
  all: ["dry-clean"] as const,
  orders: (page: number, pageSize: number, status?: string, q?: string) => ["dry-clean", "orders", page, pageSize, status, q] as const,
}

export function useDryCleanOrders(page: number, pageSize: number, status?: string, q?: string) {
  return useQuery({ queryKey: dryCleanKeys.orders(page, pageSize, status, q), queryFn: () => listDryCleanOrders(page, pageSize, status, q), placeholderData: keepPreviousData })
}

export function useCreateDryCleanOrder() {
  const queryClient = useQueryClient()
  return useMutation({ mutationFn: (payload: CreateDryCleanOrderRequest) => createDryCleanOrder(payload), onSuccess: () => queryClient.invalidateQueries({ queryKey: dryCleanKeys.all }) })
}

function useDryCleanMutation<T>(mutationFn: (value: T) => Promise<unknown>) {
  const queryClient = useQueryClient()
  return useMutation({ mutationFn, onSuccess: () => queryClient.invalidateQueries({ queryKey: dryCleanKeys.all }) })
}

export function useStartDryCleanProcessing() { return useDryCleanMutation((id: string) => startDryCleanProcessing(id)) }
export function useCompleteDryCleanOrder() { return useDryCleanMutation(({ id, ironerEmployeeId }: { id: string; ironerEmployeeId: string }) => completeDryCleanOrder(id, ironerEmployeeId)) }
export function useAddDryCleanPayment() { return useDryCleanMutation(({ id, amount, paymentMethod }: { id: string; amount: number; paymentMethod: "cash" | "wallet" | "instapay" }) => addDryCleanPayment(id, amount, paymentMethod)) }
export function useDeliverDryCleanOrder() { return useDryCleanMutation(({ id, deliveryEmployeeId }: { id: string; deliveryEmployeeId: string }) => deliverDryCleanOrder(id, deliveryEmployeeId)) }
export function usePrintDryCleanOrder() { return useMutation({ mutationFn: ({ id, readyCopy }: { id: string; readyCopy: boolean }) => printDryCleanOrder(id, readyCopy) }) }
export function useDryCleanProductivity(from?: string, to?: string) { return useQuery({ queryKey: [...dryCleanKeys.all, "productivity", from, to], queryFn: () => getDryCleanProductivity(from, to) }) }
