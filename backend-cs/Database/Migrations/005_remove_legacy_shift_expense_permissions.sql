DELETE FROM RolePermission WHERE permissionId IN ('shifts.view', 'shifts.open', 'shifts.close', 'expenses.view', 'expenses.create');
DELETE FROM Permission WHERE key IN ('shifts.view', 'shifts.open', 'shifts.close', 'expenses.view', 'expenses.create');
