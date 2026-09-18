"use client"

import { useState } from "react"
import { useSearchParams } from "next/navigation"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { Link } from "@/i18n/navigation"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Pagination, PaginationContent, PaginationItem, PaginationNext, PaginationPrevious } from "@/components/ui/pagination"
import { Badge } from "@/components/ui/badge"
import { useClient } from "@/hooks/use-clients"
import { useAddDryCleanPayment, useDryCleanOrders } from "@/hooks/use-dry-clean"
import { DryCleanPaymentMethod } from "@/types/domain/domain.types"

export default function ClientDetailsPage() {
  const t = useTranslations("Clients")
  const td = useTranslations("DryClean")
  const searchParams = useSearchParams()
  const id = searchParams.get("id") ?? ""
  const [page, setPage] = useState(1)
  const { data: client, isLoading: clientLoading } = useClient(id, !!id)
  const { data, isLoading: ordersLoading } = useDryCleanOrders(page, 20, undefined, undefined, id)
  const addPayment = useAddDryCleanPayment()
  const [amount, setAmount] = useState<Record<string, string>>({})
  const [method, setMethod] = useState<Record<string, DryCleanPaymentMethod>>({})
  const pay = async (orderId: string) => { try { await addPayment.mutateAsync({ id: orderId, amount: Number(amount[orderId]), paymentMethod: method[orderId] ?? "cash" }); setAmount((current) => ({ ...current, [orderId]: "" })); toast.success(t("paymentAdded")) } catch { toast.error(t("paymentFailed")) } }
  const totalPages = Math.max(1, Math.ceil((data?.total ?? 0) / 20))
  return <div className="space-y-6"><div className="flex items-center justify-between"><div><h1 className="text-2xl font-bold">{client?.name ?? t("title")}</h1><p className="text-sm text-muted-foreground">{client?.phone ?? ""}</p></div><Link href="/clients" className="inline-flex h-9 items-center rounded-md border px-4 text-sm">{t("backToClients")}</Link></div>{clientLoading ? <p>{td("loading")}</p> : <Card><CardContent className="grid gap-3 pt-6 sm:grid-cols-3"><div><span className="text-xs text-muted-foreground">{t("name")}</span><p>{client?.name ?? "—"}</p></div><div><span className="text-xs text-muted-foreground">{t("phone")}</span><p dir="ltr">{client?.phone ?? "—"}</p></div><div><span className="text-xs text-muted-foreground">{t("balance")}</span><p dir="ltr" className="font-semibold">{(client?.balance ?? 0).toFixed(2)}</p></div></CardContent></Card>}<Card><CardHeader><CardTitle>{t("orders")}</CardTitle></CardHeader><CardContent><div className="overflow-x-auto rounded-md border"><Table><TableHeader><TableRow><TableHead>{t("orderNumber")}</TableHead><TableHead>{t("orderStatus")}</TableHead><TableHead>{t("orderTotal")}</TableHead><TableHead>{t("orderRemaining")}</TableHead><TableHead>{t("recordOrderPayment")}</TableHead></TableRow></TableHeader><TableBody>{ordersLoading ? <TableRow><TableCell colSpan={5}>{td("loading")}</TableCell></TableRow> : data?.items.length ? data.items.map((order) => <TableRow key={order.id}><TableCell dir="ltr">#{order.orderNumber}</TableCell><TableCell><Badge>{td(`status.${order.status}` as "status.received")}</Badge></TableCell><TableCell dir="ltr">{order.totalAmount.toFixed(2)}</TableCell><TableCell dir="ltr">{order.remainingAmount.toFixed(2)}</TableCell><TableCell>{order.remainingAmount > 0 ? <div className="flex min-w-72 flex-wrap gap-2"><Input className="w-24" dir="ltr" type="number" min="0" max={order.remainingAmount} value={amount[order.id] ?? ""} onChange={(event) => setAmount((current) => ({ ...current, [order.id]: event.target.value }))} placeholder={t("paymentAmount")} /><Select value={method[order.id] ?? "cash"} onValueChange={(value) => value && setMethod((current) => ({ ...current, [order.id]: value as DryCleanPaymentMethod }))}><SelectTrigger className="w-24"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="cash">{t("cash")}</SelectItem><SelectItem value="wallet">{td("payment.wallet")}</SelectItem><SelectItem value="instapay">{td("payment.instapay")}</SelectItem></SelectContent></Select><Button size="sm" disabled={!Number(amount[order.id])} onClick={() => pay(order.id)}>{t("recordPayment")}</Button></div> : <span className="text-emerald-600">{t("paid")}</span>}</TableCell></TableRow>) : <TableRow><TableCell colSpan={5}>{t("noOrders")}</TableCell></TableRow>}</TableBody></Table></div><div className="mt-4 flex items-center justify-between"><span className="text-sm text-muted-foreground">{data?.total ?? 0}</span><Pagination className="mx-0 w-auto"><PaginationContent><PaginationItem><PaginationPrevious text={t("previous")} aria-disabled={page <= 1} className={page <= 1 ? "pointer-events-none opacity-50" : undefined} href="#" onClick={(event) => { event.preventDefault(); if (page > 1) setPage(page - 1) }} /></PaginationItem><PaginationItem><span className="px-3 text-sm">{page} / {totalPages}</span></PaginationItem><PaginationItem><PaginationNext text={t("next")} aria-disabled={page >= totalPages} className={page >= totalPages ? "pointer-events-none opacity-50" : undefined} href="#" onClick={(event) => { event.preventDefault(); if (page < totalPages) setPage(page + 1) }} /></PaginationItem></PaginationContent></Pagination></div></CardContent></Card></div>
}
