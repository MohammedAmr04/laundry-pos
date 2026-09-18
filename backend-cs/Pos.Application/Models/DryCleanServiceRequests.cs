namespace PosCs.Application.Models
{
    public sealed class CreateDryCleanServiceRequest
    {
        public string Name { get; set; }
    }

    public sealed class UpdateDryCleanServiceRequest
    {
        public string Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
