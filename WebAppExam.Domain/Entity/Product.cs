using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain
{
    public class Product : EntityBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Ulid InventoryId { get; set; }
        public Inventory Inventory {get; set; }
    }
}