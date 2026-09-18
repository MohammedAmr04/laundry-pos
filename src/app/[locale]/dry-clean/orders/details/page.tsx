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
import { useClient } from "@/hooks/use-clients"
import { useActiveEmployees } from "@/hooks/use-employees"
import { useAddDryCleanPayment, useAssignDryCleanIroner, useDeliverDryCleanOrder, useDryCleanOrder, usePrepareDryCleanOrder, usePrintDryCleanOrder } from "@/hooks/use-dry-clean"
import { DryCleanOrder, DryCleanPaymentMethod } from "@/types/domain/domain.types"

export default function DryCleanOrderDetailsPage() {
  const t = useTranslations("DryClean")
  const searchParams = useSearchParams()
  const id = searchParams.get("id") ?? ""
  const { data: order, isLoading, isError } = useDryCleanOrder(id)
  const { data: employees = [] } = useActiveEmployees()
  const { data: client } = useClient(order?.clientId ?? "", !!order?.clientId)
  const assign = useAssignDryCleanIroner()
  const prepare = usePrepareDryCleanOrder()
  const deliver = useDeliverDryCleanOrder()
  const payment = useAddDryCleanPayment()
  const print = usePrintDryCleanOrder()
  const [ironerId, setIronerId] = useState("")
  const [deliveryId, setDeliveryId] = useState("")
  const [amount, setAmount] = useState("")
  const [method, setMethod] = useState<DryCleanPaymentMethod>("cash")
  const run = async (action: () => Promise<unknown>, message: string) => { try { await action(); toast.success(message) } catch (error) { toast.error(error instanceof Error ? error.message : t("messages.operationFailed")) } }
  if (isLoading) return <div className="p-6">{t("loading")}</div>
  if (isError || !order) return <div className="space-y-4 p-6"><p className="text-destructive">{t("orderLoadFailed")}</p><Link href="/dry-clean/orders" className="inline-flex h-9 items-center rounded-md border px-4 text-sm">{t("backToOrders")}</Link></div>
  const labels: Record<DryCleanOrder["status"], string> = { received: t("status.received"), prepared: t("status.prepared"), delivered: t("status.delivered"), cancelled: t("status.cancelled") }
  const selectedIroner = ironerId || order.assignedIronerId || ""
  const selectedIronerName = employees.find((employee) => employee.id === selectedIroner)?.name
  const selectedDeliveryName = employees.find((employee) => employee.id === deliveryId)?.name
  return <div className="space-y-6"><div className="flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-bold">{t("orderDetails")} #{order.orderNumber}</h1><p className="text-sm text-muted-foreground">{client?.name ?? t("unknownClient")}</p></div><Link href="/dry-clean/orders" className="inline-flex h-9 items-center rounded-md border px-4 text-sm">{t("backToOrders")}</Link></div><Card><CardHeader><CardTitle>{t("orderControls")}</CardTitle></CardHeader><CardContent className="flex flex-wrap gap-2"><Button variant="outline" onClick={() => run(() => print.mutateAsync({ id: order.id, readyCopy: order.status === "prepared" }), t("actions.printed"))}>{t("actions.printCustomer")}</Button><Button variant="outline" onClick={() => run(() => print.mutateAsync({ id: order.id, readyCopy: false, ironerCopy: true }), t("actions.printed"))}>{t("actions.printIroner")}</Button>{order.status === "received" && <><Select value={selectedIroner} onValueChange={(value) => value && setIronerId(value)}><SelectTrigger className="w-48"><SelectValue placeholder={t("ironer")}>{selectedIronerName}</SelectValue></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select><Button disabled={!selectedIroner} onClick={() => run(() => prepare.mutateAsync({ id: order.id, ironerEmployeeId: selectedIroner }), t("actions.prepared"))}>{t("actions.prepare")}</Button></>}{order.status === "prepared" && <><Select value={selectedIroner} onValueChange={(value) => { if (value) { setIronerId(value); run(() => assign.mutateAsync({ id: order.id, ironerEmployeeId: value }), t("actions.ironerChanged")) } }}><SelectTrigger className="w-48"><SelectValue placeholder={t("ironer")}>{selectedIronerName}</SelectValue></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select><Select value={deliveryId} onValueChange={(value) => value && setDeliveryId(value)}><SelectTrigger className="w-48"><SelectValue placeholder={t("deliveryEmployee")}>{selectedDeliveryName}</SelectValue></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select><Button disabled={!deliveryId} onClick={() => run(() => deliver.mutateAsync({ id: order.id, deliveryEmployeeId: deliveryId }), t("actions.delivered"))}>{t("actions.deliver")}</Button></>}{order.remainingAmount > 0 && <><Input className="w-28" dir="ltr" type="number" min="0" max={order.remainingAmount} value={amount} onChange={(event) => setAmount(event.target.value)} placeholder={t("paymentAmount")} /><Select value={method} onValueChange={(value) => value && setMethod(value as DryCleanPaymentMethod)}><SelectTrigger className="w-32"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="cash">{t("payment.cash")}</SelectItem><SelectItem value="wallet">{t("payment.wallet")}</SelectItem><SelectItem value="instapay">{t("payment.instapay")}</SelectItem></SelectContent></Select><Button disabled={!Number(amount)} onClick={() => run(() => payment.mutateAsync({ id: order.id, amount: Number(amount), paymentMethod: method }), t("actions.paymentAdded"))}>{t("actions.addPayment")}</Button></>}</CardContent></Card><Card><CardContent className="grid gap-3 pt-6 sm:grid-cols-2 lg:grid-cols-4"><Info label={t("client")} value={client?.name ?? t("unknownClient")} /><Info label={t("phone")} value={client?.phone ?? t("notAvailable")} /><Info label={t("statusLabel")} value={labels[order.status]} /><Info label={t("deliveryAddress")} value={order.deliveryAddress} /><Info label={t("receivedAt")} value={new Date(order.receivedAt).toLocaleString("ar-EG")} /><Info label={t("ironer")} value={employees.find((employee) => employee.id === order.assignedIronerId)?.name ?? t("notAssigned")} /><Info label={t("total")} value={order.totalAmount.toFixed(2)} /><Info label={t("remaining")} value={order.remainingAmount.toFixed(2)} /></CardContent></Card><Card><CardHeader><CardTitle>{t("orderLines")}</CardTitle></CardHeader><CardContent><div className="overflow-x-auto rounded-md border"><Table><TableHeader><TableRow><TableHead>{t("garmentType")}</TableHead><TableHead>{t("service")}</TableHead><TableHead>{t("quantity")}</TableHead><TableHead>{t("colorOrDescription")}</TableHead><TableHead>{t("lineTotal")}</TableHead></TableRow></TableHeader><TableBody>{order.lines.map((line) => <TableRow key={line.id}><TableCell>{line.garmentType}</TableCell><TableCell>{line.serviceDescription}</TableCell><TableCell>{line.quantity}</TableCell><TableCell>{line.colorOrDescription || t("notAvailable")}</TableCell><TableCell dir="ltr">{line.lineTotal.toFixed(2)}</TableCell></TableRow>)}</TableBody></Table></div></CardContent></Card></div>
}

function Info({ label, value }: { label: string; value: string }) { return <div><dt className="text-xs text-muted-foreground">{label}</dt><dd className="font-medium">{value}</dd></div> }
