using PosCs.Application.Ports;
using PosCs.Application.Services;
using PosCs.Infrastructure.Devices;
using PosCs.Infrastructure.Persistence;
using PosCs.Infrastructure.Printing;
using PosCs.Infrastructure.Security;
using PosCs.Infrastructure.SystemTime;

namespace PosCs
{
    public static class CompositionRoot
    {
        private static readonly object Sync = new object();
        private static IClock _clock;
        private static IPasswordHasher _hasher;
        private static ITokenService _tokens;
        private static IMachineIdProvider _machineId;
        private static LoginThrottle _throttle;
        private static IClientRepository _clients;
        private static IEmployeeRepository _employees;
        private static IShiftRepository _shifts;
        private static IExpenseRepository _expenses;
        private static IPrinterSettingsRepository _printerSettings;
        private static IAuthRepository _auth;
        private static IUsersRepository _users;
        private static IRolesRepository _roles;
        private static IPermissionsRepository _permissions;
        private static ITenantFeatureRepository _tenantFeatures;
        private static ILicenseRepository _license;
        private static IAccessControl _access;
        private static IReceiptPrinter _receiptPrinter;
        private static IBarcodeLabelPrinter _labelPrinter;
        private static PrinterSettingsService _printerSettingsService;
        private static IOperationsRepository _operations;
        private static IDryCleanOrderRepository _dryCleanOrders;
        private static AuthService _authService;
        private static ClientService _clientService;
        private static EmployeeService _employeeService;
        private static ShiftService _shiftService;
        private static ExpenseService _expenseService;
        private static UsersService _usersService;
        private static RolesService _rolesService;
        private static TenantFeaturesService _tenantFeaturesService;
        private static LicenseService _licenseService;
        private static PrintingService _printingService;
        private static BackupService _backupService;
        private static OperationsService _operationsService;
        private static DryCleanService _dryCleanService;

        public static IClock Clock => Lazy(ref _clock, () => new SystemClock());
        public static IPasswordHasher Hasher => Lazy(ref _hasher, () => new PasswordHasher());
        public static ITokenService Tokens => Lazy(ref _tokens, () => new TokenService());
        public static IMachineIdProvider MachineId => Lazy(ref _machineId, () => new MachineIdProvider());
        public static LoginThrottle Throttle => Lazy(ref _throttle, () => new LoginThrottle(Clock));
        public static IClientRepository ClientsRepo => Lazy(ref _clients, () => new ClientRepository());
        public static IEmployeeRepository EmployeesRepo => Lazy(ref _employees, () => new EmployeeRepository());
        public static IShiftRepository ShiftsRepo => Lazy(ref _shifts, () => new ShiftRepository());
        public static IExpenseRepository ExpensesRepo => Lazy(ref _expenses, () => new ExpenseRepository());
        public static IPrinterSettingsRepository PrinterSettingsRepo => Lazy(ref _printerSettings, () => new PrinterSettingsRepository());
        public static IAuthRepository Auth => Lazy(ref _auth, () => new AuthRepository(MachineId));
        public static IUsersRepository UsersRepo => Lazy(ref _users, () => new UsersRepository());
        public static IRolesRepository RolesRepo => Lazy(ref _roles, () => new RolesRepository());
        public static IPermissionsRepository PermissionsRepo => Lazy(ref _permissions, () => new PermissionsRepository());
        public static ITenantFeatureRepository TenantFeaturesRepo => Lazy(ref _tenantFeatures, () => new TenantFeatureRepository());
        public static ILicenseRepository LicenseRepo => Lazy(ref _license, () => new SettingsRepository());
        public static IAccessControl Access => Lazy(ref _access, () => new AccessControl());
        public static IReceiptPrinter Receipts => Lazy(ref _receiptPrinter, () => new ReceiptPrinter(PrinterSettingsRepo));
        public static IBarcodeLabelPrinter BarcodeLabels => Lazy(ref _labelPrinter, () => new BarcodeLabelPrinter(PrinterSettingsRepo));
        public static PrinterSettingsService PrinterSettingsService => Lazy(ref _printerSettingsService, () => new PrinterSettingsService(PrinterSettingsRepo));
        public static IOperationsRepository OperationsRepo => Lazy(ref _operations, () => new OperationsRepository());
        public static IDryCleanOrderRepository DryCleanOrdersRepo => Lazy(ref _dryCleanOrders, () => new DryCleanOrderRepository());
        public static AuthService AuthService => Lazy(ref _authService, () => new AuthService(Auth, RolesRepo, Hasher, Tokens, Throttle));
        public static ClientService ClientService => Lazy(ref _clientService, () => new ClientService(ClientsRepo));
        public static EmployeeService EmployeeService => Lazy(ref _employeeService, () => new EmployeeService(EmployeesRepo));
        public static ShiftService ShiftService => Lazy(ref _shiftService, () => new ShiftService(ShiftsRepo));
        public static ExpenseService ExpenseService => Lazy(ref _expenseService, () => new ExpenseService(ExpensesRepo));
        public static UsersService UsersService => Lazy(ref _usersService, () => new UsersService(UsersRepo, Auth, RolesRepo, Hasher, Access));
        public static RolesService RolesService => Lazy(ref _rolesService, () => new RolesService(RolesRepo, PermissionsRepo));
        public static TenantFeaturesService TenantFeaturesService => Lazy(ref _tenantFeaturesService, () => new TenantFeaturesService(Auth, TenantFeaturesRepo, Access));
        public static LicenseService LicenseService => Lazy(ref _licenseService, () => new LicenseService(LicenseRepo, MachineId, Clock));
        public static PrintingService PrintingService => Lazy(ref _printingService, () => new PrintingService(Receipts, BarcodeLabels, DryCleanOrdersRepo));
        public static BackupService BackupService => Lazy(ref _backupService, () => new BackupService());
        public static OperationsService OperationsService => Lazy(ref _operationsService, () => new OperationsService(OperationsRepo));
        public static DryCleanService DryCleanService => Lazy(ref _dryCleanService, () => new DryCleanService(DryCleanOrdersRepo));

        private static T Lazy<T>(ref T field, System.Func<T> factory) where T : class
        {
            if (field != null) return field;
            lock (Sync) { if (field == null) field = factory(); return field; }
        }
    }
}
