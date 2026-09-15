# Dry Clean Permissions

الصلاحيات تُفحص في الـ backend بواسطة `RequirePermission`، وتستخدمها الواجهة لإظهار القوائم والأزرار. يجب أن تكون صلاحية المستخدم موجودة وأن تكون capability `dry_clean` مفعلة للـ tenant.

## الطلبات

| Key | الاستخدام |
|---|---|
| `dry_clean.orders.view` | عرض الطلبات والتفاصيل |
| `dry_clean.orders.create` | إنشاء طلب واستلام القطع |
| `dry_clean.orders.update` | تعديل بيانات الطلب قبل الإنهاء |
| `dry_clean.orders.process` | بدء التجهيز |
| `dry_clean.orders.complete` | إنهاء المكوجي وتسجيل الإنتاجية |
| `dry_clean.orders.deliver` | تسجيل التسليم |
| `dry_clean.orders.cancel` | إلغاء الطلب |

## الدفع والتقارير والطباعة

| Key | الاستخدام |
|---|---|
| `dry_clean.payments.create` | تسجيل دفعة كاملة أو جزئية |
| `dry_clean.reports.view` | قراءة التقارير التشغيلية |
| `dry_clean.productivity.view` | قراءة إنتاجية المكوجي |
| `dry_clean.print.receipt` | طباعة إيصال الاستلام وورقة الجاهز |

## التشغيل المشترك

| Key | الاستخدام |
|---|---|
| `clients.view` | اختيار وعرض العملاء |
| `clients.create` | إنشاء عميل |
| `clients.update` | تعديل بيانات العميل وعنوانه |
| `employees.view` | اختيار موظف الاستلام والمكوجي والمندوب |
| `employees.manage` | إدارة الموظفين |
| `shifts.view` | عرض الشيفتات |
| `shifts.open` | فتح شيفت |
| `shifts.close` | إغلاق شيفت |
| `expenses.view` | عرض المصروفات |
| `expenses.create` | تسجيل مصروف |
| `audit.view` | قراءة سجل التغييرات |
| `settings.view` | عرض الإعدادات |
| `settings.update` | تعديل الإعدادات |
| `users.manage` | إدارة المستخدمين |
| `roles.manage` | إدارة الأدوار والصلاحيات |

## الأدوار الافتراضية

- `Admin`: كل الصلاحيات.
- `Manager`: كل صلاحيات التشغيل والإدارة ما عدا إدارة المستخدمين والأدوار.
- `Cashier`: العملاء، الموظفون للعرض، إنشاء وتشغيل وتسليم الطلبات، الدفع، الطباعة، الشيفت والمصروفات.

## قواعد الحماية

- لا تعتمد الواجهة وحدها على إخفاء الزر؛ الـ API يعيد `403` عند غياب الصلاحية.
- لا يتم إضافة صلاحيات خاصة بالبيع بالتجزئة إلى أي role جديد.
- كل انتقال حالة ودفع يجب أن يسجل actor في الـ audit log.
