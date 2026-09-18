"use client"

import { useState, type Dispatch, type SetStateAction } from "react"
import { Link, useRouter } from "@/i18n/navigation"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Pagination, PaginationContent, PaginationItem, PaginationNext, PaginationPrevious } from "@/components/ui/pagination"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { useActiveEmployees } from "@/hooks/use-employees"
import { useAddDryCleanPayment, useDeliverDryCleanOrder, useDryCleanOrders, usePrepareDryCleanOrder, usePrintDryCleanOrder } from "@/hooks/use-dry-clean"
import { DryCleanOrder, DryCleanPaymentMethod, Employee } from "@/types/domain/domain.types"

const PAGE_SIZE = 20
const statuses: DryCleanOrder["status"][] = ["received", "prepared", "delivered"]
type PaymentDraft = { amount: string; method: DryCleanPaymentMethod }

export default function DryCleanOrdersPage() {
  const t = useTranslations("DryClean")
  const router = useRouter()
  const { data: employees = [] } = useActiveEmployees()
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState("")
  const [paymentStatus, setPaymentStatus] = useState("")
  const [query, setQuery] = useState("")
  const [payments, setPayments] = useState<Record<string, PaymentDraft>>({})
  const { data, isLoading } = useDryCleanOrders(page, PAGE_SIZE, status || undefined, query.trim() || undefined, undefined, paymentStatus || undefined)
  const prepare = usePrepareDryCleanOrder()
  const payment = useAddDryCleanPayment()
  const deliver = useDeliverDryCleanOrder()
  const print = usePrintDryCleanOrder()
  const labels: Record<DryCleanOrder["status"], string> = { received: t("status.received"), prepared: t("status.prepared"), delivered: t("status.delivered"), cancelled: t("status.cancelled") }
  const totalPages = Math.max(1, Math.ceil((data?.total ?? 0) / PAGE_SIZE))
  const run = async (action: () => Promise<unknown>, message: string) => {
    try { await action(); toast.success(message) } catch (error) { toast.error(error instanceof Error ? error.message : t("messages.operationFailed")) }
  }

  return <div className="space-y-6"><div className="flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-bold">{t("orders")}</h1><p className="text-sm text-muted-foreground">{t("ordersDescription")}</p></div><Link className="inline-flex h-9 items-center justify-center rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground" href="/dry-clean/new">{t("createOrder")}</Link></div><Card><CardContent className="flex flex-wrap items-end gap-3 pt-6"><div className="min-w-64 flex-1"><label className="mb-1 block text-sm font-medium">{t("search")}</label><Input value={query} onChange={(event) => { setQuery(event.target.value); setPage(1) }} placeholder={t("searchPlaceholder")} /></div><div className="w-56"><label className="mb-1 block text-sm font-medium">{t("filterStatus")}</label><Select value={status || "all"} onValueChange={(value) => { setStatus(value === null || value === "all" ? "" : value); setPage(1) }}><SelectTrigger><SelectValue>{status ? labels[status as DryCleanOrder["status"]] : t("allStatuses")}</SelectValue></SelectTrigger><SelectContent><SelectItem value="all">{t("allStatuses")}</SelectItem>{statuses.map((item) => <SelectItem key={item} value={item}>{labels[item]}</SelectItem>)}</SelectContent></Select></div><div className="w-40"><label className="mb-1 block text-sm font-medium">{t("paymentStatus")}</label><Select value={paymentStatus || "all"} onValueChange={(value) => { setPaymentStatus(value === null || value === "all" ? "" : value); setPage(1) }}><SelectTrigger><SelectValue>{paymentStatus === "paid" ? t("paid") : paymentStatus === "credit" ? t("credit") : t("allPayments")}</SelectValue></SelectTrigger><SelectContent><SelectItem value="all">{t("allPayments")}</SelectItem><SelectItem value="paid">{t("paid")}</SelectItem><SelectItem value="credit">{t("credit")}</SelectItem></SelectContent></Select></div></CardContent></Card><Card><CardContent className="pt-6"><div className="overflow-x-auto rounded-md border"><Table><TableHeader><TableRow><TableHead>{t("orderNumber")}</TableHead><TableHead>{t("client")}</TableHead><TableHead>{t("receivedAt")}</TableHead><TableHead>{t("statusLabel")}</TableHead><TableHead>{t("total")}</TableHead><TableHead>{t("remaining")}</TableHead><TableHead>{t("actionsLabel")}</TableHead></TableRow></TableHeader><TableBody>{isLoading ? <TableRow><TableCell colSpan={7}>{t("loading")}</TableCell></TableRow> : data?.items.length ? data.items.map((order) => <OrderRow key={order.id} order={order} t={t} labels={labels} employees={employees} draft={payments[order.id] ?? { amount: "", method: "cash" }} setPayments={setPayments} onOpen={() => router.push(`/dry-clean/orders/details?id=${order.id}`)} run={run} prepare={prepare} payment={payment} deliver={deliver} print={print} />) : <TableRow><TableCell colSpan={7}>{t("noOrders")}</TableCell></TableRow>}</TableBody></Table></div><div className="mt-4 flex items-center justify-between text-sm text-muted-foreground"><span>{t("resultsCount", { count: data?.total ?? 0 })}</span><Pagination className="mx-0 w-auto"><PaginationContent><PaginationItem><PaginationPrevious text={t("previous")} href="#" onClick={(event) => { event.preventDefault(); if (page > 1) setPage(page - 1) }} /></PaginationItem><PaginationItem><span className="px-3">{page} / {totalPages}</span></PaginationItem><PaginationItem><PaginationNext text={t("next")} href="#" onClick={(event) => { event.preventDefault(); if (page < totalPages) setPage(page + 1) }} /></PaginationItem></PaginationContent></Pagination></div></CardContent></Card></div>
}

function OrderRow({ order, t, labels, employees, draft, setPayments, onOpen, run, prepare, payment, deliver, print }: { order: DryCleanOrder; t: ReturnType<typeof useTranslations>; labels: Record<DryCleanOrder["status"], string>; employees: Employee[]; draft: PaymentDraft; setPayments: Dispatch<SetStateAction<Record<string, PaymentDraft>>>; onOpen: () => void; run: (action: () => Promise<unknown>, success: string) => Promise<void>; prepare: ReturnType<typeof usePrepareDryCleanOrder>; payment: ReturnType<typeof useAddDryCleanPayment>; deliver: ReturnType<typeof useDeliverDryCleanOrder>; print: ReturnType<typeof usePrintDryCleanOrder> }) {
  return <TableRow className="cursor-pointer" onClick={onOpen}><TableCell dir="ltr">#{order.orderNumber}</TableCell><TableCell><div className="font-medium">{order.clientName ?? t("unknownClient")}</div><div className="text-xs text-muted-foreground">{order.deliveryAddress}</div></TableCell><TableCell dir="ltr">{new Date(order.receivedAt).toLocaleString("ar-EG")}</TableCell><TableCell><Badge variant={order.status === "cancelled" ? "destructive" : order.status === "delivered" ? "secondary" : "default"}>{labels[order.status]}</Badge></TableCell><TableCell dir="ltr">{order.totalAmount.toFixed(2)}</TableCell><TableCell dir="ltr">{order.remainingAmount.toFixed(2)}</TableCell><TableCell onClick={(event) => event.stopPropagation()}><div className="flex min-w-80 flex-wrap justify-center gap-2"><Button size="sm" variant="outline" onClick={() => run(() => print.mutateAsync({ id: order.id, readyCopy: order.status === "prepared" }), t("actions.printed"))}>{t("actions.printCustomer")}</Button><Button size="sm" variant="outline" onClick={() => run(() => print.mutateAsync({ id: order.id, readyCopy: false, ironerCopy: true }), t("actions.printed"))}>{t("actions.printIroner")}</Button>{order.status === "received" && <Select value={order.assignedIronerId ?? ""} onValueChange={(value) => value && run(() => prepare.mutateAsync({ id: order.id, ironerEmployeeId: value }), t("actions.prepared"))}><SelectTrigger className="w-36"><SelectValue placeholder={t("ironer")} /></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select>}{order.status === "prepared" && <Select value={order.deliveredByEmployeeId ?? ""} onValueChange={(value) => value && run(() => deliver.mutateAsync({ id: order.id, deliveryEmployeeId: value }), t("actions.delivered"))}><SelectTrigger className="w-36"><SelectValue placeholder={t("deliveryEmployee")} /></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select>}{order.remainingAmount > 0 && order.status !== "cancelled" && <><Input className="w-24" dir="ltr" type="number" min="0" max={order.remainingAmount} placeholder={t("paymentAmount")} value={draft.amount} onChange={(event) => setPayments((current) => ({ ...current, [order.id]: { ...draft, amount: event.target.value } }))} /><Button size="sm" disabled={!Number(draft.amount)} onClick={() => run(() => payment.mutateAsync({ id: order.id, amount: Number(draft.amount), paymentMethod: draft.method }), t("actions.paymentAdded"))}>{t("actions.addPayment")}</Button></>}</div></TableCell></TableRow>
}
