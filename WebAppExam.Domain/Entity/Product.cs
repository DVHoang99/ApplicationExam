using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain
{
    public class Product : EntityBase, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public int Price { get; private set; }
        public ProductStatus ProductStatus { get; private set; }
        public string CorrelationId { get; private set; }
        public string WareHouseId { get; private set; }

        private Product(string name, string? description, int price, string correlationId, string wareHouseId)
        {
            Id = Ulid.NewUlid();
            Name = name;
            Description = description ?? "";
            Price = price;
            CreatedAt = DateTime.UtcNow;
            ProductStatus = ProductStatus.Pending;
            CorrelationId = correlationId;
            WareHouseId = wareHouseId;
        }

        public static Product Init(string name, string? description, int price, string correlationId, string wareHouseId)
        {
            return new Product(name, description, price, correlationId, wareHouseId);
        }
        
        public void UpdateInformation(string name, string? description, int price)
        {
            Name = name;
            Description = description ?? "";
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProductStatus(ProductStatus status)
        {
            ProductStatus = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DeleteProduct()
        {
            DeletedAt = DateTime.UtcNow;
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