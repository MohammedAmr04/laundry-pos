"use client"

import { useState } from "react"
import { Link, useRouter } from "@/i18n/navigation"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Popover, PopoverPopup, PopoverPositioner, PopoverPortal, PopoverTrigger } from "@/components/ui/popover"
import { Command, CommandEmpty, CommandInput, CommandItem, CommandList } from "@/components/ui/command"
import { ChevronDown, Trash2 } from "lucide-react"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { useClientsPage } from "@/hooks/use-clients"
import { useDebouncedCallback } from "@/hooks/use-debounced-callback"
import { useActiveEmployees } from "@/hooks/use-employees"
import { useCreateDryCleanOrder, useDryCleanCatalog, usePrintDryCleanOrder } from "@/hooks/use-dry-clean"
import { DryCleanCatalogItem, DryCleanLineInput } from "@/types/domain/domain.types"

type DraftLine = { catalogItemId: string; quantity: number; colorOrDescription: string; unitPrice: string }
const emptyLine = (): DraftLine => ({ catalogItemId: "", quantity: 1, colorOrDescription: "", unitPrice: "" })

export default function NewDryCleanOrderPage() {
  const t = useTranslations("DryClean")
  const router = useRouter()
  const [clientSearchInput, setClientSearchInput] = useState("")
  const [clientSearch, setClientSearch] = useState("")
  const [clientPickerOpen, setClientPickerOpen] = useState(false)
  const debouncedClientSearch = useDebouncedCallback((value: string) => setClientSearch(value), 300)
  const { data: clientsData } = useClientsPage(1, 20, { q: clientSearch, balance: "all" })
  const clients = clientsData?.items ?? []
  const { data: employees = [] } = useActiveEmployees()
  const { data: catalog = [] } = useDryCleanCatalog()
  const create = useCreateDryCleanOrder()
  const print = usePrintDryCleanOrder()
  const [clientId, setClientId] = useState("")
  const [receivedByEmployeeId, setReceivedByEmployeeId] = useState("")
  const [lines, setLines] = useState<DraftLine[]>([emptyLine()])
  const updateLine = (index: number, patch: Partial<DraftLine>) => setLines((current) => current.map((line, i) => i === index ? { ...line, ...patch } : line))
  const removeLine = (index: number) => setLines((current) => current.length === 1 ? current : current.filter((_, i) => i !== index))
  const submit = async () => {
    const selected = lines.map((line) => catalog.find((item) => item.id === line.catalogItemId)).filter((item): item is DryCleanCatalogItem => Boolean(item))
    if (!clientId || !receivedByEmployeeId || selected.length !== lines.length || lines.some((line) => !Number(line.unitPrice))) return
    const requestLines: DryCleanLineInput[] = lines.map((line, index) => ({ ...selected[index], quantity: line.quantity, unitPrice: Number(line.unitPrice), colorOrDescription: line.colorOrDescription }))
    try { const order = await create.mutateAsync({ clientId, receivedByEmployeeId, lines: requestLines }); await print.mutateAsync({ id: order.id, readyCopy: false }); toast.success(t("messages.createdAndPrinted")); router.push("/dry-clean/orders") } catch { toast.error(t("messages.createFailed")) }
  }
  return <div className="space-y-6"><div className="flex items-center justify-between gap-3"><div><h1 className="text-2xl font-bold">{t("createOrder")}</h1><p className="text-sm text-muted-foreground">{t("newOrderDescription")}</p></div><Link className="inline-flex h-9 items-center justify-center rounded-md border px-4 text-sm font-medium" href="/dry-clean/orders">{t("openOrders")}</Link></div><Card><CardHeader><CardTitle>{t("newOrder")}</CardTitle></CardHeader><CardContent className="space-y-4"><Popover open={clientPickerOpen} onOpenChange={setClientPickerOpen}><PopoverTrigger render={<Button type="button" variant="outline" className="w-full justify-between"><span>{clients.find((client) => client.id === clientId)?.name ?? t("selectClient")}</span><ChevronDown className="size-4 text-muted-foreground" /></Button>} /><PopoverPortal><PopoverPositioner side="bottom" align="start" sideOffset={6} className="w-[var(--anchor-width)]"><PopoverPopup dir="rtl" className="w-full p-0"><Command><CommandInput autoFocus value={clientSearchInput} onChange={(event) => { setClientSearchInput(event.target.value); debouncedClientSearch(event.target.value) }} placeholder={t("clientSearchPlaceholder")} /><CommandList>{clients.length === 0 ? <CommandEmpty>{t("noClientsFound")}</CommandEmpty> : clients.map((client) => <CommandItem key={client.id} onClick={() => { setClientId(client.id); setClientSearchInput(""); setClientSearch(""); setClientPickerOpen(false) }}><span>{client.name}</span><span className="text-muted-foreground">{client.phone}</span></CommandItem>)}</CommandList></Command></PopoverPopup></PopoverPositioner></PopoverPortal></Popover><div className="grid gap-3 md:grid-cols-2"><Select value={receivedByEmployeeId} onValueChange={(value) => value && setReceivedByEmployeeId(value)}><SelectTrigger><SelectValue placeholder={t("receivingEmployee")}>{employees.find((employee) => employee.id === receivedByEmployeeId)?.name}</SelectValue></SelectTrigger><SelectContent>{employees.map((employee) => <SelectItem key={employee.id} value={employee.id}>{employee.name}</SelectItem>)}</SelectContent></Select></div>{lines.map((line, index) => { const selectedItem = catalog.find((item) => item.id === line.catalogItemId); const manual = selectedItem?.isManualEntry; return <div className="grid gap-2 rounded-md border p-3 md:grid-cols-[minmax(0,1fr)_8rem_10rem_minmax(0,1fr)_auto]" key={index}><Select value={line.catalogItemId} onValueChange={(value) => { if (value) { const item = catalog.find((entry) => entry.id === value); updateLine(index, { catalogItemId: value, unitPrice: item?.unitPrice.toString() ?? "" }) } }}><SelectTrigger className="w-full"><SelectValue placeholder={t("catalog.selectItem")}>{selectedItem ? `${selectedItem.garmentType} · ${selectedItem.serviceDescription}` : undefined}</SelectValue></SelectTrigger><SelectContent>{catalog.map((item) => <SelectItem key={item.id} value={item.id}>{item.garmentType} · {item.serviceDescription} · {item.unitPrice.toFixed(2)}</SelectItem>)}</SelectContent></Select><Input dir="ltr" type="number" min="1" value={line.quantity} onChange={(event) => updateLine(index, { quantity: Number(event.target.value) })} placeholder={t("quantity")} /><Input dir="ltr" type="number" min="0" value={line.unitPrice} onChange={(event) => updateLine(index, { unitPrice: event.target.value })} placeholder={manual ? t("manualPrice") : t("price")} /><Input value={line.colorOrDescription} onChange={(event) => updateLine(index, { colorOrDescription: event.target.value })} placeholder={t("colorOrDescription")} /><Button type="button" variant="outline" size="icon" disabled={lines.length === 1} aria-label={t("removeLine")} onClick={() => removeLine(index)}><Trash2 className="size-4" /></Button></div> })}<div className="flex gap-2"><Button type="button" variant="outline" onClick={() => setLines((current) => [...current, emptyLine()])}>{t("addLine")}</Button><Button disabled={create.isPending || !clientId || !receivedByEmployeeId || lines.some((line) => !line.catalogItemId || !Number(line.unitPrice))} onClick={submit}>{t("saveOrder")}</Button></div></CardContent></Card></div>
}





