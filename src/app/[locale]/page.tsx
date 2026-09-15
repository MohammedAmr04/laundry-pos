"use client"

import { ClipboardList, Shirt, UsersRound } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Link } from "@/i18n/navigation"
import { useDryCleanOrders } from "@/hooks/use-dry-clean"
import { useActiveClients } from "@/hooks/use-clients"

export default function DashboardPage() {
  const { data: orders } = useDryCleanOrders(1, 1)
  const { data: clients = [] } = useActiveClients()
  const { data: ready } = useDryCleanOrders(1, 1, "ready")
  const { data: processing } = useDryCleanOrders(1, 1, "processing")

  return <div className="space-y-6 pt-6">
    <div><h1 className="text-3xl font-bold">لوحة تحكم التنظيف والكي</h1><p className="text-muted-foreground">متابعة الطلبات والإنتاجية والتسليم</p></div>
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      <Card><CardHeader><CardTitle className="flex items-center gap-2 text-sm"><ClipboardList className="h-4 w-4" /> كل الطلبات</CardTitle></CardHeader><CardContent><div className="text-2xl font-bold">{orders?.total ?? 0}</div></CardContent></Card>
      <Card><CardHeader><CardTitle className="flex items-center gap-2 text-sm"><Shirt className="h-4 w-4" /> قيد التشغيل</CardTitle></CardHeader><CardContent><div className="text-2xl font-bold">{processing?.total ?? 0}</div></CardContent></Card>
      <Card><CardHeader><CardTitle className="flex items-center gap-2 text-sm"><Shirt className="h-4 w-4" /> جاهز للتسليم</CardTitle></CardHeader><CardContent><div className="text-2xl font-bold">{ready?.total ?? 0}</div></CardContent></Card>
      <Card><CardHeader><CardTitle className="flex items-center gap-2 text-sm"><UsersRound className="h-4 w-4" /> العملاء</CardTitle></CardHeader><CardContent><div className="text-2xl font-bold">{clients.length}</div></CardContent></Card>
    </div>
    <Link href="/dry-clean" className="inline-flex rounded-md bg-primary px-4 py-2 text-primary-foreground">إنشاء طلب تنظيف جديد</Link>
  </div>
}
