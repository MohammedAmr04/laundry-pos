namespace PosCs.Domain.Rules
{
    public static class DryCleanOrderFlow
    {
        public const string Received = "received";
        public const string Prepared = "prepared";
        public const string Delivered = "delivered";

        public static bool CanPrepare(string status) { return status == Received; }
        public static bool CanDeliver(string status) { return status == Prepared; }
    }
}
