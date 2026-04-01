using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain
{
    public class Product : EntityBase, IAggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public ProductStatus ProductStatus { get; set; }
        public string CorrelationId { get; set; }
        public string WareHouseId { get; set; }

        //private readonly List<Inventory> _inventories = new();
        //public IReadOnlyCollection<Inventory> Inventories => _inventories.AsReadOnly();

        protected Product() { }
        public Product(string name, string description, int price, string correlationId, string wareHouseId)
        {
            Id = Ulid.NewUlid();
            Name = name;
            Description = description;
            Price = price;
            CreatedAt = DateTime.UtcNow;
            ProductStatus = ProductStatus.Pending;
            CorrelationId = correlationId;
            WareHouseId = wareHouseId;
        }

        public void UpdateInformation(string name, string description, int price)
        {
            Name = name;
            Description = description;
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProductStatus(ProductStatus status)
        {
            ProductStatus = status;
            UpdatedAt = DateTime.UtcNow;
        }

        // public void AddOrUpdateInventory(Ulid inventoryId, int stock, Ulid productId, string inventoryName)
        // {
        //     var existingItem = inventoryId != Ulid.Empty 
        //         ? _inventories.SingleOrDefault(x => x.Id == inventoryId) 
        //         : null;

        //     if (existingItem != null)
        //     {
        //         existingItem.UpdateStock(stock);
        //     }
        //     else
        //     {
        //         _inventories.Add(new Inventory(productId, stock, inventoryName));
        //     }
        // }

        // public void DeleteInventory(Ulid inventoryId)
        // {
        //     var inventory = _inventories.SingleOrDefault(x => x.Id == inventoryId);

        //     if (inventory != null)
        //     {
        //         inventory.DeleteInventory();
        //     }
        // }
    }
}