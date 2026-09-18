"use client"

import { useState } from "react"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { useAuth } from "@/components/common/auth-context"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { PERMISSIONS } from "@/lib/constants"
import { useCreateDryCleanCatalogItem, useCreateDryCleanService, useDeleteDryCleanCatalogItem, useDeleteDryCleanService, useDryCleanCatalog, useDryCleanServices, useUpdateDryCleanCatalogItem, useUpdateDryCleanService } from "@/hooks/use-dry-clean"
import { DryCleanCatalogItem, DryCleanCatalogItemInput, DryCleanServiceDefinition } from "@/types/domain/domain.types"

const emptyCatalog: DryCleanCatalogItemInput = { garmentType: "", serviceDescription: "", serviceId: "", unitPrice: 0, countsForProductivity: true, isManualEntry: false }

export default function DryCleanCatalogPage() {
  const t = useTranslations("DryClean")
  const { hasPermission } = useAuth()
  const canManage = hasPermission(PERMISSIONS.DRY_CLEAN_CATALOG_MANAGE)
  const { data: items = [] } = useDryCleanCatalog(true)
  const { data: services = [] } = useDryCleanServices(true)
  const createItem = useCreateDryCleanCatalogItem()
  const updateItem = useUpdateDryCleanCatalogItem()
  const deleteItem = useDeleteDryCleanCatalogItem()
  const createService = useCreateDryCleanService()
  const updateService = useUpdateDryCleanService()
  const deleteService = useDeleteDryCleanService()
  const [catalogOpen, setCatalogOpen] = useState(false)
  const [serviceOpen, setServiceOpen] = useState(false)
  const [form, setForm] = useState<DryCleanCatalogItemInput>(emptyCatalog)
  const [editing, setEditing] = useState<DryCleanCatalogItem | null>(null)
  const [serviceName, setServiceName] = useState("")
  const [editingService, setEditingService] = useState<DryCleanServiceDefinition | null>(null)

  const openNewItem = () => { setEditing(null); setForm(emptyCatalog); setCatalogOpen(true) }
  const editItem = (item: DryCleanCatalogItem) => { setEditing(item); setForm({ garmentType: item.garmentType, serviceDescription: item.serviceDescription, serviceId: item.serviceId ?? "", unitPrice: item.unitPrice, countsForProductivity: item.countsForProductivity, isManualEntry: item.isManualEntry, isActive: item.isActive }); setCatalogOpen(true) }
  const openNewService = () => { setEditingService(null); setServiceName(""); setServiceOpen(true) }
  const editService = (service: DryCleanServiceDefinition) => { setEditingService(service); setServiceName(service.name); setServiceOpen(true) }
  const saveItem = async () => { const service = services.find((item) => item.id === form.serviceId); if (!form.garmentType.trim() || !service) return; try { const payload = { ...form, serviceDescription: service.name }; if (editing) await updateItem.mutateAsync({ id: editing.id, payload }); else await createItem.mutateAsync(payload); setCatalogOpen(false); setForm(emptyCatalog); setEditing(null); toast.success(t("catalog.saved")) } catch { toast.error(t("catalog.failed")) } }
  const saveService = async () => { if (!serviceName.trim()) return; try { if (editingService) await updateService.mutateAsync({ id: editingService.id, payload: { name: serviceName } }); else await createService.mutateAsync({ name: serviceName }); setServiceOpen(false); setServiceName(""); setEditingService(null); toast.success(t("services.saved")) } catch { toast.error(t("services.failed")) } }

  return <div className="space-y-6"><div><h1 className="text-2xl font-bold">{t("catalog.title")}</h1><p className="text-sm text-muted-foreground">{t("catalog.subtitle")}</p></div><Card><CardHeader className="flex flex-row items-center justify-between"><CardTitle>{t("services.title")}</CardTitle>{canManage && <Button onClick={openNewService}>{t("services.new")}</Button>}</CardHeader><CardContent><div className="overflow-x-auto rounded-md border"><Table><TableHeader><TableRow><TableHead>{t("services.name")}</TableHead><TableHead>{t("catalog.status")}</TableHead><TableHead>{t("actionsLabel")}</TableHead></TableRow></TableHeader><TableBody>{services.length ? services.map((service) => <TableRow key={service.id}><TableCell className="font-medium">{service.name}</TableCell><TableCell>{service.isActive ? t("catalog.active") : <Badge variant="secondary">{t("catalog.inactive")}</Badge>}</TableCell><TableCell><div className="flex gap-2">{canManage && <><Button size="sm" variant="outline" onClick={() => editService(service)}>{t("services.edit")}</Button>{service.isActive && <Button size="sm" variant="destructive" onClick={() => deleteService.mutateAsync(service.id)}>{t("services.delete")}</Button>}</>}</div></TableCell></TableRow>) : <TableRow><TableCell colSpan={3}>{t("services.empty")}</TableCell></TableRow>}</TableBody></Table></div></CardContent></Card><Card><CardHeader className="flex flex-row items-center justify-between"><CardTitle>{t("catalog.items")}</CardTitle>{canManage && <Button onClick={openNewItem}>{t("catalog.new")}</Button>}</CardHeader><CardContent><div className="overflow-x-auto rounded-md border"><Table><TableHeader><TableRow><TableHead>{t("catalog.garmentType")}</TableHead><TableHead>{t("catalog.service")}</TableHead><TableHead>{t("catalog.price")}</TableHead><TableHead>{t("catalog.productivity")}</TableHead><TableHead>{t("catalog.status")}</TableHead><TableHead>{t("actionsLabel")}</TableHead></TableRow></TableHeader><TableBody>{items.length ? items.map((item) => <TableRow key={item.id}><TableCell className="font-medium">{item.garmentType}</TableCell><TableCell>{item.serviceName ?? item.serviceDescription}</TableCell><TableCell dir="ltr">{item.unitPrice.toFixed(2)}</TableCell><TableCell>{item.countsForProductivity ? t("catalog.yes") : t("catalog.no")}</TableCell><TableCell>{item.isActive ? t("catalog.active") : <Badge variant="secondary">{t("catalog.inactive")}</Badge>}</TableCell><TableCell><div className="flex gap-2">{canManage && <><Button size="sm" variant="outline" onClick={() => editItem(item)}>{t("catalog.edit")}</Button>{item.isActive && <Button size="sm" variant="destructive" onClick={() => deleteItem.mutateAsync(item.id)}>{t("catalog.delete")}</Button>}</>}</div></TableCell></TableRow>) : <TableRow><TableCell colSpan={6}>{t("catalog.empty")}</TableCell></TableRow>}</TableBody></Table></div></CardContent></Card><Dialog open={serviceOpen} onOpenChange={setServiceOpen}><DialogContent dir="rtl" className="max-w-lg"><DialogHeader><DialogTitle>{editingService ? t("services.edit") : t("services.new")}</DialogTitle></DialogHeader><Input autoFocus value={serviceName} onChange={(event) => setServiceName(event.target.value)} placeholder={t("services.name")} /><DialogFooter><Button variant="outline" onClick={() => setServiceOpen(false)}>{t("catalog.cancel")}</Button><Button onClick={saveService} disabled={!serviceName.trim() || createService.isPending || updateService.isPending}>{t("services.save")}</Button></DialogFooter></DialogContent></Dialog><Dialog open={catalogOpen} onOpenChange={setCatalogOpen}><DialogContent dir="rtl" className="max-w-2xl"><DialogHeader><DialogTitle>{editing ? t("catalog.edit") : t("catalog.new")}</DialogTitle></DialogHeader><div className="grid gap-4 sm:grid-cols-2"><Input autoFocus placeholder={t("catalog.garmentType")} value={form.garmentType} onChange={(event) => setForm({ ...form, garmentType: event.target.value })} /><Select value={form.serviceId ?? ""} onValueChange={(value) => value && setForm({ ...form, serviceId: value })}><SelectTrigger><SelectValue placeholder={t("catalog.service")}>{services.find((service) => service.id === form.serviceId)?.name}</SelectValue></SelectTrigger><SelectContent>{services.filter((service) => service.isActive).map((service) => <SelectItem key={service.id} value={service.id}>{service.name}</SelectItem>)}</SelectContent></Select><Input dir="ltr" type="number" min="0" value={form.unitPrice} onChange={(event) => setForm({ ...form, unitPrice: Number(event.target.value) })} placeholder={t("catalog.price")} /><label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.countsForProductivity} onChange={(event) => setForm({ ...form, countsForProductivity: event.target.checked })} />{t("catalog.productivity")}</label><label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.isManualEntry} onChange={(event) => setForm({ ...form, isManualEntry: event.target.checked })} />{t("catalog.manual")}</label></div><DialogFooter><Button variant="outline" onClick={() => setCatalogOpen(false)}>{t("catalog.cancel")}</Button><Button onClick={saveItem} disabled={!form.garmentType.trim() || !form.serviceId || createItem.isPending || updateItem.isPending}>{t("catalog.save")}</Button></DialogFooter></DialogContent></Dialog></div>
}


