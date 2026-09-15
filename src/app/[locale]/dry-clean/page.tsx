"use client"

import { useState } from "react"
import { useActiveClients } from "@/hooks/use-clients"
import { useActiveEmployees } from "@/hooks/use-employees"
import { useAddDryCleanPayment, useCompleteDryCleanOrder, useCreateDryCleanOrder, useDeliverDryCleanOrder, useDryCleanOrders, useDryCleanProductivity, usePrintDryCleanOrder, useStartDryCleanProcessing } from "@/hooks/use-dry-clean"
import { CreateDryCleanOrderRequest, DryCleanLineInput, DryCleanOrder, DryCleanPaymentMethod } from "@/types/domain/domain.types"
import { useAuth } from "@/components/common/auth-context"
import { PERMISSIONS } from "@/lib/constants"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { toast } from "sonner"
import { useTranslations } from "next-intl"

const emptyLine = (): DryCleanLineInput => ({ garmentType: "", serviceDescription: "", quantity: 1, unitPrice: 0, countsForProductivity: true, isManualEntry: true })
export default function DryCleanPage() {
  const t = useTranslations("DryClean")
  const statusLabels: Record<DryCleanOrder["status"], string> = { received: t("status.received"), processing: t("status.processing"), needs_review: t("status.needsReview"), ready: t("status.ready"), delivered: t("status.delivered"), cancelled: t("status.cancelled") }
  const { hasPermission } = useAuth()
  const { data: clients = [] } = useActiveClients()
  const { data: employees = [] } = useActiveEmployees()
  const { data } = useDryCleanOrders(1, 50)
  const { data: productivity = [] } = useDryCleanProductivity()
  const create = useCreateDryCleanOrder()
  const start = useStartDryCleanProcessing()
  const complete = useCompleteDryCleanOrder()
  const payment = useAddDryCleanPayment()
  const deliver = useDeliverDryCleanOrder()
  const print = usePrintDryCleanOrder()
  const [clientId, setClientId] = useState("")
  const [receivedByEmployeeId, setReceivedByEmployeeId] = useState("")
  const [lines, setLines] = useState<DryCleanLineInput[]>([emptyLine()])
  const [ironerByOrder, setIronerByOrder] = useState<Record<string, string>>({})
  const [deliveryByOrder, setDeliveryByOrder] = useState<Record<string, string>>({})
  const [paymentByOrder, setPaymentByOrder] = useState<Record<string, { amount: string; method: DryCleanPaymentMethod }>>({})
  const canCreate = hasPermission(PERMISSIONS.DRY_CLEAN_ORDERS_CREATE)

  const updateLine = (index: number, patch: Partial<DryCleanLineInput>) => setLines((current) => current.map((line, i) => i === index ? { ...line, ...patch } : line))
  const submit = async () => {
    const payload: CreateDryCleanOrderRequest = { clientId, receivedByEmployeeId, lines }
    try { const order = await create.mutateAsync(payload); await print.mutateAsync({ id: order.id, readyCopy: false }); toast.success("تم إنشاء الطلب وطباعة إيصال الاستلام") ; setClientId(""); setReceivedByEmployeeId(""); setLines([emptyLine()]) } catch { toast.error("تم إنشاء الطلب لكن تعذر الطباعة أو الحفظ") }
  }
  const run = async (action: () => Promise<unknown>, success: string) => { try { await action(); toast.success(success) } catch { toast.error("تعذر تنفيذ العملية") } }
  const paymentDraft = (id: string) => paymentByOrder[id] ?? { amount: "", method: "cash" as DryCleanPaymentMethod }

  return <div className="space-y-6">
    <div><h1 className="text-2xl font-bold">{t("title")}</h1><p className="text-sm text-muted-foreground">{t("subtitle")}</p></div>
    {canCreate && <Card><CardHeader><CardTitle>{t("newOrder")}</CardTitle></CardHeader><CardContent className="space-y-4">
      <div className="grid gap-3 md:grid-cols-2">
        <Select value={clientId} onValueChange={(value) => value && setClientId(value)}><SelectTrigger><SelectValue placeholder="اختر العميل" /></SelectTrigger><SelectContent>{clients.map((client) => <SelectItem key={client.id} value={client.id}>{client.name} - {client.phone}</SelectItem>)}</SelectContent></Select>
        <Select value={receivedByEmployeeId} onValueChange={(value) => value && setReceivedByEmployeeId(value)}><SelectTrigger><SelectValue placeholder="عامل الاستلام" /></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select>
      </div>
      {lines.map((line, index) => <div className="grid gap-2 rounded-md border p-3 md:grid-cols-6" key={index}><Input placeholder="نوع القطعة" value={line.garmentType} onChange={(e) => updateLine(index, { garmentType: e.target.value })} /><Input placeholder="الخدمة" value={line.serviceDescription} onChange={(e) => updateLine(index, { serviceDescription: e.target.value })} /><Input dir="ltr" type="number" min="1" value={line.quantity} onChange={(e) => updateLine(index, { quantity: Number(e.target.value) })} /><Input dir="ltr" type="number" min="0" value={line.unitPrice} onChange={(e) => updateLine(index, { unitPrice: Number(e.target.value) })} /><Input placeholder="وصف/لون" value={line.colorOrDescription ?? ""} onChange={(e) => updateLine(index, { colorOrDescription: e.target.value })} /><label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={line.countsForProductivity} onChange={(e) => updateLine(index, { countsForProductivity: e.target.checked })} /> إنتاجية</label></div>)}
      <div className="flex gap-2"><Button type="button" variant="outline" onClick={() => setLines((current) => [...current, emptyLine()])}>{t("addLine")}</Button><Button disabled={create.isPending || !clientId || !receivedByEmployeeId} onClick={submit}>{t("saveOrder")}</Button></div>
    </CardContent></Card>}
    <Card><CardHeader><CardTitle>الطلبات</CardTitle></CardHeader><CardContent className="space-y-3">{data?.items.map((order) => { const draft = paymentDraft(order.id); return <div className="space-y-3 rounded border p-3" key={order.id}>
      <div className="flex flex-wrap items-center justify-between gap-2"><span>#{order.orderNumber} · {order.totalAmount.toFixed(2)}</span><span>{statusLabels[order.status]} · المتبقي {order.remainingAmount.toFixed(2)}</span></div>
      <div className="flex flex-wrap gap-2">{order.status === "received" && <Button size="sm" onClick={() => run(() => start.mutateAsync(order.id), "بدأ التجهيز")}>بدء التجهيز</Button>}{order.status === "processing" && <><Select value={ironerByOrder[order.id] ?? ""} onValueChange={(value) => value && setIronerByOrder((current) => ({ ...current, [order.id]: value }))}><SelectTrigger className="w-48"><SelectValue placeholder="المكوجي" /></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select><Button size="sm" disabled={!ironerByOrder[order.id]} onClick={() => run(async () => { await complete.mutateAsync({ id: order.id, ironerEmployeeId: ironerByOrder[order.id] }); await print.mutateAsync({ id: order.id, readyCopy: true }) }, "تم الإنهاء وطباعة ورقة الجاهز")}>إنهاء وطباعة</Button></>}{order.status === "ready" && <><Select value={deliveryByOrder[order.id] ?? ""} onValueChange={(value) => value && setDeliveryByOrder((current) => ({ ...current, [order.id]: value }))}><SelectTrigger className="w-48"><SelectValue placeholder="مندوب التسليم" /></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select><Button size="sm" disabled={!deliveryByOrder[order.id]} onClick={() => run(() => deliver.mutateAsync({ id: order.id, deliveryEmployeeId: deliveryByOrder[order.id] }), "تم التسليم")}>تسليم</Button><Button size="sm" variant="outline" onClick={() => run(() => print.mutateAsync({ id: order.id, readyCopy: true }), "تمت إعادة طباعة ورقة الجاهز")}>طباعة الجاهز</Button></>}</div>
      {order.remainingAmount > 0 && order.status !== "cancelled" && <div className="flex flex-wrap gap-2"><Input className="w-32" dir="ltr" type="number" min="0" max={order.remainingAmount} placeholder="المبلغ" value={draft.amount} onChange={(e) => setPaymentByOrder((current) => ({ ...current, [order.id]: { ...draft, amount: e.target.value } }))} /><Select value={draft.method} onValueChange={(value) => setPaymentByOrder((current) => ({ ...current, [order.id]: { ...draft, method: value as DryCleanPaymentMethod } }))}><SelectTrigger className="w-32"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="cash">كاش</SelectItem><SelectItem value="wallet">محفظة</SelectItem><SelectItem value="instapay">InstaPay</SelectItem></SelectContent></Select><Button size="sm" disabled={!Number(draft.amount)} onClick={() => run(() => payment.mutateAsync({ id: order.id, amount: Number(draft.amount), paymentMethod: draft.method }), "تم تسجيل الدفع")}>تسجيل دفع</Button></div>}
    </div> })}</CardContent></Card>
    <Card><CardHeader><CardTitle>إنتاجية المكوجي</CardTitle></CardHeader><CardContent>{productivity.length === 0 ? <p className="text-sm text-muted-foreground">لا توجد إنتاجية مسجلة بعد</p> : <div className="grid gap-2 md:grid-cols-3">{productivity.map((item) => <div className="rounded border p-3" key={item.employeeId}><p className="font-medium">{item.employeeName}</p><p className="text-sm text-muted-foreground">{item.ordersCompleted} طلب · {item.countedQuantity} قطعة</p></div>)}</div>}</CardContent></Card>
  </div>
}
