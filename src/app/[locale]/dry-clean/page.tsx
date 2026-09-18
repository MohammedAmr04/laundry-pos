"use client"

import { useState } from "react"
import { Link } from "@/i18n/navigation"
import { useTranslations } from "next-intl"
import { useDryCleanProductivity } from "@/hooks/use-dry-clean"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"

export default function DryCleanPage() {
  const t = useTranslations("DryClean")
  const today = new Date().toLocaleDateString("en-CA")
  const [productivityDate, setProductivityDate] = useState(today)
  const { data: productivity = [], isLoading: productivityLoading } = useDryCleanProductivity(productivityDate, productivityDate)
  return <div className="space-y-6">
    <div><h1 className="text-2xl font-bold">{t("title")}</h1><p className="text-sm text-muted-foreground">{t("subtitle")}</p></div>
    <div className="grid gap-4 md:grid-cols-3">
      <Link href="/dry-clean/orders"><Card className="h-full transition-colors hover:bg-muted"><CardHeader><CardTitle>{t("openOrders")}</CardTitle></CardHeader><CardContent><p className="text-sm text-muted-foreground">{t("ordersDescription")}</p></CardContent></Card></Link>
      <Link href="/dry-clean/new"><Card className="h-full transition-colors hover:bg-muted"><CardHeader><CardTitle>{t("createOrder")}</CardTitle></CardHeader><CardContent><p className="text-sm text-muted-foreground">{t("newOrderDescription")}</p></CardContent></Card></Link>
      <Link href="/dry-clean/catalog"><Card className="h-full transition-colors hover:bg-muted"><CardHeader><CardTitle>{t("catalog.open")}</CardTitle></CardHeader><CardContent><p className="text-sm text-muted-foreground">{t("catalog.description")}</p></CardContent></Card></Link>
    </div>
    <Card><CardHeader className="flex flex-row items-center justify-between gap-3"><CardTitle>{t("todayProductivity")}</CardTitle><div className="flex items-center gap-2"><label htmlFor="productivity-date" className="text-sm text-muted-foreground">{t("productivityDate")}</label><Input id="productivity-date" type="date" dir="ltr" value={productivityDate} max={today} onChange={(event) => setProductivityDate(event.target.value || today)} className="w-auto" /></div></CardHeader><CardContent>{productivityLoading ? <p className="text-sm text-muted-foreground">{t("loading")}</p> : productivity.length === 0 ? <p className="text-sm text-muted-foreground">{t("noProductivity")}</p> : <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">{productivity.map((item) => <div className="rounded-md border p-3" key={item.employeeId}><p className="font-medium">{item.employeeName}</p><p className="text-sm text-muted-foreground">{item.ordersCompleted} {t("ordersCompleted")} · {item.countedQuantity} {t("countedPieces")}</p></div>)}</div>}</CardContent></Card>
  </div>
}
